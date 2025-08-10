using DistriCatalogoAPI.Api.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

namespace DistriCatalogoAPI.Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // CORS debe ir primero
            app.UseCors();

            // Logging de CORS (opcional)
            app.UseCorsLogging(app.ApplicationServices.GetRequiredService<IConfiguration>());

            // Headers de seguridad
            app.UseSecurityHeaders();

            // Forwarded Headers (importante para proxies)
            app.UseForwardedHeaders();

            // Middleware de seguridad personalizado
            app.UseSecurityProtection();

            // Rate limiting
            app.UseRateLimiter();

            // Request logging middleware (antes del error handling)
            app.UseMiddleware<RequestLoggingMiddleware>();

            // Error handling middleware
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Company resolution middleware (solo para endpoints de catálogo)
            app.UseMiddleware<CompanyResolutionMiddleware>();

            // API Documentation (Scalar + Swagger)
            app.UseApiDocumentation(env);

            // HTTPS Redirection (solo en producción)
            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}