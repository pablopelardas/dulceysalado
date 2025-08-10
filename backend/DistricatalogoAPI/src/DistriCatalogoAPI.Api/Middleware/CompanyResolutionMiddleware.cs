using DistriCatalogoAPI.Api.Services;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Api.Middleware
{
    public class CompanyResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CompanyResolutionMiddleware> _logger;

        public CompanyResolutionMiddleware(RequestDelegate next, ILogger<CompanyResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, CompanyResolverService companyResolver)
        {
            try
            {
                // Solo procesar requests a endpoints de catálogo
                if (ShouldResolveCompany(context.Request.Path))
                {
                    var host = context.Request.Host.Value;
                    var origin = context.Request.Headers["Origin"].FirstOrDefault();
                    
                    _logger.LogInformation("MIDDLEWARE: Resolving company for host: {Host}, origin: {Origin}", host, origin);

                    // Primero intentar con el host, luego con el origin si el host no tiene subdominio
                    var company = await companyResolver.ResolveCompanyFromHostAsync(host);
                    
                    // Si no se resolvió por host y hay origin, intentar con origin
                    if (company != null && !string.IsNullOrEmpty(origin))
                    {
                        var originUri = new Uri(origin);
                        var originCompany = await companyResolver.ResolveCompanyFromHostAsync(originUri.Host);
                        
                        // Si el origin resuelve a una empresa diferente (no principal), usarla
                        if (originCompany != null && !originCompany.IsPrincipal)
                        {
                            _logger.LogInformation("MIDDLEWARE: Using company from origin instead of host");
                            company = originCompany;
                        }
                    }
                    
                    // Almacenar la empresa en el contexto para uso posterior
                    context.Items["CurrentCompany"] = company;
                    context.Items["CurrentCompanyId"] = company.Id;

                    _logger.LogInformation("MIDDLEWARE: Resolved company {CompanyId} ({CompanyName}) for host {Host}, origin {Origin}", 
                        company.Id, company.Nombre, host, origin);
                }
                else
                {
                    _logger.LogInformation("MIDDLEWARE: Skipping company resolution for path {Path}", context.Request.Path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MIDDLEWARE: Error resolving company for host {Host}", context.Request.Host.Value);
                // En caso de error, continuar sin empresa (el controller puede manejar este caso)
            }

            await _next(context);
        }

        private static bool ShouldResolveCompany(PathString path)
        {
            // Solo resolver empresa para endpoints de catálogo público
            return path.StartsWithSegments("/api/catalog", StringComparison.OrdinalIgnoreCase);
        }
    }
}