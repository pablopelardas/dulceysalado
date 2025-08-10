using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DistriCatalogoAPI.Api.Middleware
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;

        // Cache thread-safe para tracking
        private static readonly ConcurrentDictionary<string, IPTracker> _ipTracking = new();
        private static readonly ConcurrentSet<string> _blockedIPs = new();
        private static readonly ConcurrentDictionary<string, AttackPattern> _attackPatterns = new();

        // Patrones de endpoints sospechosos (SIN rutas /api/* para no interferir con la API legítima)
        private static readonly List<Regex> _suspiciousPatterns = new()
        {
            // Patrones de PHP y exploits comunes
            new Regex(@"\.php", RegexOptions.IgnoreCase),
            new Regex(@"phpunit", RegexOptions.IgnoreCase),
            new Regex(@"eval-stdin", RegexOptions.IgnoreCase),
            new Regex(@"/vendor/", RegexOptions.IgnoreCase),
            new Regex(@"\.git", RegexOptions.IgnoreCase),
            new Regex(@"\.env", RegexOptions.IgnoreCase),
            new Regex(@"/wp-", RegexOptions.IgnoreCase), // WordPress
            new Regex(@"/admin/", RegexOptions.IgnoreCase),
            new Regex(@"/backup/", RegexOptions.IgnoreCase),
            new Regex(@"/test/", RegexOptions.IgnoreCase),
            new Regex(@"/demo/", RegexOptions.IgnoreCase),
            new Regex(@"/cms/", RegexOptions.IgnoreCase),
            new Regex(@"/crm/", RegexOptions.IgnoreCase),
            new Regex(@"/panel/", RegexOptions.IgnoreCase),
            new Regex(@"/public/", RegexOptions.IgnoreCase),
            new Regex(@"/workspace/", RegexOptions.IgnoreCase),
            new Regex(@"/blog/", RegexOptions.IgnoreCase),
            new Regex(@"/www/", RegexOptions.IgnoreCase),
            new Regex(@"/laravel/", RegexOptions.IgnoreCase),
            new Regex(@"/yii/", RegexOptions.IgnoreCase),
            new Regex(@"/zend/", RegexOptions.IgnoreCase),
            new Regex(@"/containers/", RegexOptions.IgnoreCase),
            new Regex(@"index\.(php|asp|jsp|cgi)", RegexOptions.IgnoreCase),
            new Regex(@"hello\.world", RegexOptions.IgnoreCase),
            new Regex(@"/ab2[a-z]", RegexOptions.IgnoreCase)
        };

        // User agents sospechosos
        private static readonly List<Regex> _suspiciousUserAgents = new()
        {
            new Regex(@"bot", RegexOptions.IgnoreCase),
            new Regex(@"crawler", RegexOptions.IgnoreCase),
            new Regex(@"spider", RegexOptions.IgnoreCase),
            new Regex(@"scanner", RegexOptions.IgnoreCase),
            new Regex(@"curl", RegexOptions.IgnoreCase),
            new Regex(@"wget", RegexOptions.IgnoreCase),
            new Regex(@"python", RegexOptions.IgnoreCase),
            new Regex(@"java", RegexOptions.IgnoreCase),
            new Regex(@"^$") // User-Agent vacío
        };

        public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIP = GetClientIP(context);
            var path = context.Request.Path.Value ?? "";
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var method = context.Request.Method;

            // Excluir Swagger y endpoints de aplicaciones internas de validaciones de seguridad
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                IsInternalApplicationEndpoint(path))
            {
                await _next(context);
                return;
            }

            try
            {
                // 1. Verificar si la IP está bloqueada
                if (_blockedIPs.Contains(clientIP))
                {
                    _logger.LogWarning("🚨 BLOCKED IP ATTEMPT: {IP} -> {Method} {Path}",
                        clientIP, method, path);

                    await WriteBlockedResponse(context);
                    return;
                }

                // 2. Verificar patrones sospechosos
                if (IsSuspiciousRequest(path))
                {
                    LogSuspiciousActivity(clientIP, path, userAgent, "SUSPICIOUS_ENDPOINT");
                    TrackAttackPattern(clientIP, path);

                    await WriteNotFoundResponse(context);
                    return;
                }

                // 3. Verificar User-Agent sospechoso
                if (IsSuspiciousUserAgent(userAgent))
                {
                    LogSuspiciousActivity(clientIP, path, userAgent, "SUSPICIOUS_USER_AGENT");
                    TrackAttackPattern(clientIP, path);
                    
                    // Bloquear inmediatamente si es un bot conocido
                    await WriteNotFoundResponse(context);
                    return;
                }

                // 4. Tracking de actividad por IP
                TrackIPActivity(clientIP, path);

                // 5. Rate limiting check
                if (IsRateLimitExceeded(clientIP))
                {
                    _logger.LogWarning("🚨 RATE LIMIT EXCEEDED: {IP} -> {Method} {Path}",
                        clientIP, method, path);

                    await WriteRateLimitResponse(context);
                    return;
                }

                // Log request legítimo (solo en desarrollo)
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    _logger.LogInformation("✅ LEGITIMATE REQUEST: {IP} -> {Method} {Path}",
                        clientIP, method, path);
                }

                // Continuar con el pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SecurityMiddleware for IP {IP}", clientIP);
                await _next(context);
            }
        }

        private string GetClientIP(HttpContext context)
        {
            // Obtener IP real considerando proxies/load balancers
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIP))
            {
                return xRealIP;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsSuspiciousRequest(string path)
        {
            // Excluir rutas de API legítimas (empiezan con /api/)
            if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            return _suspiciousPatterns.Any(pattern => pattern.IsMatch(path));
        }

        private bool IsSuspiciousUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return true;
            return _suspiciousUserAgents.Any(pattern => pattern.IsMatch(userAgent));
        }

        private void LogSuspiciousActivity(string ip, string path, string userAgent, string reason)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                IP = ip,
                Path = path,
                UserAgent = userAgent,
                Reason = reason,
                Action = "BLOCKED"
            };

            _logger.LogWarning("🚨 SUSPICIOUS ACTIVITY DETECTED: {@SecurityLog}", logEntry);

            // TODO: Enviar a sistema de logging externo o base de datos
            SaveToSecurityLog(logEntry);
        }

        private void TrackAttackPattern(string ip, string path)
        {
            var pattern = _attackPatterns.GetOrAdd(ip, _ => new AttackPattern());

            lock (pattern)
            {
                pattern.Count++;
                pattern.Paths.Add(path);
                pattern.LastUpdate = DateTime.UtcNow;

                // Si más de 3 endpoints sospechosos en 5 minutos, bloquear IP
                var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
                if (pattern.Count > 3 && pattern.LastUpdate > fiveMinutesAgo)
                {
                    BlockIP(ip, "ATTACK_PATTERN_DETECTED");
                }
            }
        }

        private void TrackIPActivity(string ip, string path)
        {
            var tracker = _ipTracking.GetOrAdd(ip, _ => new IPTracker());

            lock (tracker)
            {
                var now = DateTime.UtcNow;

                // Reset contador cada 5 minutos
                if (now - tracker.LastReset > TimeSpan.FromMinutes(5))
                {
                    tracker.RequestCount = 0;
                    tracker.Suspicious404s = 0;
                    tracker.LastReset = now;
                }

                tracker.RequestCount++;
                tracker.LastActivity = now;

                // Contar 404s consecutivos (indicativo de bots escaneando) - EXCLUIR rutas /api/
                if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) &&
                    (path.Contains("php") || path.Contains(".env") || path.Contains(".git") || 
                     path.Contains("vendor") || path.Contains("admin") || path.Contains("wp-")))
                {
                    tracker.Suspicious404s++;
                    
                    // Bloquear después de 3 intentos sospechosos
                    if (tracker.Suspicious404s >= 3)
                    {
                        BlockIP(ip, "MULTIPLE_SUSPICIOUS_404s");
                        return;
                    }
                }

                // Verificar límites
                if (tracker.RequestCount > 1000) // Más de 1000 requests en 5 minutos
                {
                    BlockIP(ip, "EXCESSIVE_REQUESTS");
                }
            }
        }

        private bool IsRateLimitExceeded(string ip)
        {
            if (!_ipTracking.TryGetValue(ip, out var tracker)) return false;

            lock (tracker)
            {
                var now = DateTime.UtcNow;

                // Rate limit: 200 requests por minuto
                if (now - tracker.LastReset <= TimeSpan.FromMinutes(1) && tracker.RequestCount > 200)
                {
                    return true;
                }

                return false;
            }
        }

        private void BlockIP(string ip, string reason)
        {
            _blockedIPs.Add(ip);
            _logger.LogWarning("🚫 IP BLOCKED: {IP} - Reason: {Reason}", ip, reason);

            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                IP = ip,
                Action = "IP_BLOCKED",
                Reason = reason
            };

            SaveToSecurityLog(logEntry);

            // Tiempo de bloqueo basado en el tipo de ataque
            var blockDuration = reason switch
            {
                "ATTACK_PATTERN_DETECTED" => TimeSpan.FromHours(6), // Ataques detectados: 6 horas
                "MULTIPLE_SUSPICIOUS_404s" => TimeSpan.FromHours(2), // Escaneo de vulnerabilidades: 2 horas
                "EXCESSIVE_REQUESTS" => TimeSpan.FromMinutes(30), // Rate limit: 30 minutos
                _ => TimeSpan.FromHours(1) // Default: 1 hora
            };

            // Auto-desbloqueo
            _ = Task.Delay(blockDuration).ContinueWith(_ =>
            {
                _blockedIPs.TryRemove(ip);
                _logger.LogInformation("🔓 IP UNBLOCKED: {IP} after {Duration}", ip, blockDuration);
            });
        }

        private void SaveToSecurityLog(object logEntry)
        {
            // TODO: Implementar guardado en base de datos
            // Por ahora solo logging
            var json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("SECURITY_LOG: {SecurityLog}", json);
        }

        private async Task WriteBlockedResponse(HttpContext context)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Access denied",
                code = "IP_BLOCKED",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private async Task WriteNotFoundResponse(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Endpoint not found",
                code = "NOT_FOUND",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private async Task WriteRateLimitResponse(HttpContext context)
        {
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Too many requests. Please try again later.",
                code = "RATE_LIMIT_EXCEEDED",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        // Método estático para obtener estadísticas (para endpoint de admin)
        public static object GetSecurityStats()
        {
            return new
            {
                BlockedIPs = _blockedIPs.ToList(),
                TotalBlockedIPs = _blockedIPs.Count,
                ActiveAttackPatterns = _attackPatterns.Count,
                TotalRequestsTracked = _ipTracking.Count,
                LastUpdate = DateTime.UtcNow,
                RecentAttacks = _attackPatterns
                    .Where(kvp => DateTime.UtcNow - kvp.Value.LastUpdate < TimeSpan.FromHours(1))
                    .Select(kvp => new { IP = kvp.Key, Pattern = kvp.Value })
                    .ToList()
            };
        }

        /// <summary>
        /// Determina si el endpoint es usado por aplicaciones internas (GECOM processor, etc.)
        /// que deben ser excluidas de validaciones estrictas de User-Agent
        /// </summary>
        private static bool IsInternalApplicationEndpoint(string path)
        {
            // Endpoints de autenticación para aplicaciones internas
            if (path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase))
                return true;

            // Endpoints de sincronización (usados por GECOM processor)
            if (path.StartsWith("/api/sync", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }

    // Clases de soporte
    public class IPTracker
    {
        public int RequestCount { get; set; } = 0;
        public int Suspicious404s { get; set; } = 0;
        public DateTime LastReset { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }

    public class AttackPattern
    {
        public int Count { get; set; } = 0;
        public List<string> Paths { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }

    // Implementación thread-safe de ConcurrentSet
    public class ConcurrentSet<T> : IEnumerable<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary = new();

        public bool Add(T item) => _dictionary.TryAdd(item, 0);
        public bool Contains(T item) => _dictionary.ContainsKey(item);
        public bool TryRemove(T item) => _dictionary.TryRemove(item, out _);
        public int Count => _dictionary.Count;
        public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public List<T> ToList() => _dictionary.Keys.ToList();
    }
}
