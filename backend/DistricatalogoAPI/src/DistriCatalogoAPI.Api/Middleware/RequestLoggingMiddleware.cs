using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Context;

namespace DistriCatalogoAPI.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly HashSet<string> _sensitiveHeaders = new()
        {
            "authorization", "cookie", "x-api-key", "x-auth-token"
        };

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate or extract correlation ID
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                               ?? Guid.NewGuid().ToString();
            
            // Add correlation ID to response headers
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            
            // Push correlation ID to Serilog context
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
            {
                await LogRequestAsync(context, correlationId);
            }
        }

        private async Task LogRequestAsync(HttpContext context, string correlationId)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            
            // Capture request details
            var requestInfo = new
            {
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                Headers = GetSafeHeaders(request.Headers),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request.Headers["User-Agent"].ToString(),
                ContentType = request.ContentType,
                ContentLength = request.ContentLength,
                Scheme = request.Scheme,
                Host = request.Host.Value
            };

            // Extract user info if available
            var userId = context.User?.FindFirst("UserId")?.Value;
            var companyId = context.User?.FindFirst("CompanyId")?.Value ?? context.Items["CurrentCompanyId"]?.ToString();
            var userName = context.User?.FindFirst("UserName")?.Value;

            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("CompanyId", companyId))
            using (LogContext.PushProperty("UserName", userName))
            {
                _logger.LogInformation("HTTP Request started: {Method} {Path} by {UserName} (UserId: {UserId}, CompanyId: {CompanyId})",
                    request.Method, request.Path, userName ?? "Anonymous", userId, companyId);

                // Capture request body for POST/PUT/PATCH (excluding sensitive endpoints)
                string? requestBody = null;
                if (ShouldLogRequestBody(request))
                {
                    requestBody = await CaptureRequestBodyAsync(request);
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        _logger.LogDebug("Request body: {RequestBody}", requestBody);
                    }
                }

                Exception? exception = null;
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                    
                    // Log response
                    var response = context.Response;
                    var responseTime = stopwatch.ElapsedMilliseconds;

                    if (exception != null)
                    {
                        _logger.LogError(exception, 
                            "HTTP Request failed: {Method} {Path} responded {StatusCode} in {ResponseTime}ms",
                            request.Method, request.Path, response.StatusCode, responseTime);
                    }
                    else if (response.StatusCode >= 400)
                    {
                        _logger.LogWarning(
                            "HTTP Request completed with error: {Method} {Path} responded {StatusCode} in {ResponseTime}ms",
                            request.Method, request.Path, response.StatusCode, responseTime);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "HTTP Request completed: {Method} {Path} responded {StatusCode} in {ResponseTime}ms",
                            request.Method, request.Path, response.StatusCode, responseTime);
                    }

                    // Log performance warning for slow requests
                    if (responseTime > 5000) // 5 seconds
                    {
                        _logger.LogWarning("Slow request detected: {Method} {Path} took {ResponseTime}ms",
                            request.Method, request.Path, responseTime);
                    }
                }
            }
        }

        private Dictionary<string, string> GetSafeHeaders(IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> headers)
        {
            return headers
                .Where(h => !_sensitiveHeaders.Contains(h.Key.ToLowerInvariant()))
                .ToDictionary(h => h.Key, h => h.Value.ToString());
        }

        private bool ShouldLogRequestBody(HttpRequest request)
        {
            // Log body for POST, PUT, PATCH but exclude sensitive endpoints
            if (!new[] { "POST", "PUT", "PATCH" }.Contains(request.Method))
                return false;

            var path = request.Path.Value?.ToLowerInvariant() ?? "";
            
            // Exclude authentication endpoints and file uploads
            var excludedPaths = new[] { "/api/auth/login", "/api/auth/register", "/api/files" };
            if (excludedPaths.Any(excluded => path.StartsWith(excluded)))
                return false;

            // Only log if content type is JSON
            return request.ContentType?.Contains("application/json") == true;
        }


        private async Task<string?> CaptureRequestBodyAsync(HttpRequest request)
        {
            try
            {
                if (request.ContentLength > 1024 * 1024) // Skip bodies larger than 1MB
                    return "[Request body too large to log]";

                // Enable buffering so the body can be read multiple times
                request.EnableBuffering();
                
                // Read the body
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var bodyText = await reader.ReadToEndAsync();
                
                // Reset the position for the next middleware
                request.Body.Position = 0;
                
                // Mask sensitive data in JSON bodies
                return MaskSensitiveData(bodyText);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to capture request body");
                return "[Failed to capture request body]";
            }
        }

        private string MaskSensitiveData(string jsonContent)
        {
            try
            {
                var sensitiveFields = new[] { "password", "token", "secret", "key", "authorization" };
                
                // Simple string replacement for common sensitive fields
                foreach (var field in sensitiveFields)
                {
                    var pattern = $"\"{field}\"\\s*:\\s*\"[^\"]*\"";
                    jsonContent = System.Text.RegularExpressions.Regex.Replace(
                        jsonContent, pattern, $"\"{field}\": \"***\"", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                
                return jsonContent;
            }
            catch
            {
                return "[Unable to mask sensitive data]";
            }
        }
    }
}