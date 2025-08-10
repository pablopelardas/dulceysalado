using DistriCatalogoAPI.Api.Middleware;
using System.Threading.RateLimiting;

namespace DistriCatalogoAPI.Api.Extensions
{
    public static class SecurityExtensions
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services)
        {
            // Rate Limiting
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var clientIP = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(clientIP, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 300, // 300 requests por minuto por IP (5 por segundo)
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 50
                        });
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken: token);

                    // Log rate limit violation
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
                    logger.LogWarning("Rate limit exceeded for IP: {IP}", ip);
                };
            });

            return services;
        }

        public static IApplicationBuilder UseSecurityProtection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityMiddleware>();
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // Headers de seguridad (usando indexer para evitar warnings)
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                
                // Content-Security-Policy: permitir recursos necesarios para Scalar
                var isApiDocumentationPath = context.Request.Path.StartsWithSegments("/swagger") || 
                                           context.Request.Path.StartsWithSegments("/scalar") ||
                                           context.Request.Path.StartsWithSegments("/_content") ||
                                           context.Request.Path.StartsWithSegments("/_framework");
                
                if (isApiDocumentationPath)
                {
                    // CSP más permisiva para documentación API
                    context.Response.Headers["Content-Security-Policy"] = 
                        "default-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                        "style-src 'self' 'unsafe-inline'; " +
                        "img-src 'self' data: https:; " +
                        "font-src 'self' data:; " +
                        "connect-src 'self' ws: wss:;";
                }
                else
                {
                    // CSP estricta para el resto de la aplicación
                    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
                }

                // Ocultar información del servidor
                context.Response.Headers.Remove("Server");
                context.Response.Headers["Server"] = "DistriCatalogo";

                await next();
            });

            return app;
        }
    }
}
