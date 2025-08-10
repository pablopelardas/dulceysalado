using Microsoft.Extensions.Options;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Api.Services
{
    public class CompanyResolverService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<CompanyResolverService> _logger;
        private readonly CompanyResolverOptions _options;

        public CompanyResolverService(
            ICompanyRepository companyRepository,
            ILogger<CompanyResolverService> logger,
            IOptions<CompanyResolverOptions> options)
        {
            _companyRepository = companyRepository;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<Company> ResolveCompanyFromHostAsync(string host)
        {
            try
            {
                _logger.LogInformation("RESOLVER: Starting company resolution for host {Host}", host);
                
                // Extraer subdominio del host
                var subdomain = ExtractSubdomain(host);
                
                _logger.LogInformation("RESOLVER: Extracted subdomain {Subdomain} from host {Host}", subdomain, host);
                
                if (string.IsNullOrEmpty(subdomain))
                {
                    // Sin subdominio, retornar empresa principal
                    _logger.LogInformation("RESOLVER: No subdomain found in host {Host}, returning principal company", host);
                    return await GetPrincipalCompanyAsync();
                }

                // Buscar empresa por subdominio
                var company = await ResolveCompanyFromSubdomainAsync(subdomain);
                if (company != null)
                {
                    _logger.LogInformation("RESOLVER: Resolved company {CompanyId} ({CompanyName}) from subdomain {Subdomain}", 
                        company.Id, company.Nombre, subdomain);
                    return company;
                }

                // Si no se encuentra, retornar empresa principal
                _logger.LogWarning("RESOLVER: Company not found for subdomain {Subdomain}, falling back to principal company", subdomain);
                return await GetPrincipalCompanyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RESOLVER: Error resolving company from host {Host}, falling back to principal company", host);
                return await GetPrincipalCompanyAsync();
            }
        }

        private async Task<Company?> ResolveCompanyFromSubdomainAsync(string subdomain)
        {
            if (string.IsNullOrWhiteSpace(subdomain))
                return null;

            try
            {
                _logger.LogInformation("RESOLVER: Looking for company with subdomain {Subdomain}", subdomain);
                
                // Buscar por dominio personalizado exacto (ej: "distribuidora-norte")
                var company = await _companyRepository.GetByDomainAsync(subdomain);
                if (company != null)
                {
                    _logger.LogInformation("RESOLVER: Found company {CompanyId} ({CompanyName}) for subdomain {Subdomain}", 
                        company.Id, company.Nombre, subdomain);
                    return company;
                }

                // También buscar por dominio completo (ej: "distribuidora-norte.districatalogo.com")
                var fullDomain = $"{subdomain}.{_options.BaseDomain}";
                _logger.LogInformation("RESOLVER: Trying full domain {FullDomain}", fullDomain);
                company = await _companyRepository.GetByDomainAsync(fullDomain);
                if (company != null)
                {
                    _logger.LogInformation("RESOLVER: Found company {CompanyId} ({CompanyName}) for full domain {FullDomain}", 
                        company.Id, company.Nombre, fullDomain);
                    return company;
                }

                _logger.LogInformation("RESOLVER: No company found for subdomain {Subdomain} or full domain {FullDomain}", 
                    subdomain, fullDomain);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RESOLVER: Error resolving company from subdomain {Subdomain}", subdomain);
                return null;
            }
        }

        private async Task<Company> GetPrincipalCompanyAsync()
        {
            try
            {
                // Buscar la primera empresa principal (IsPrincipal = true)
                var companies = await _companyRepository.GetAllAsync();
                var principalCompany = companies.FirstOrDefault(c => c.IsPrincipal);
                
                if (principalCompany != null)
                {
                    _logger.LogDebug("Found principal company {CompanyId}", principalCompany.Id);
                    return principalCompany;
                }

                // Si no hay empresa principal definida, tomar la primera empresa disponible
                var firstCompany = companies.FirstOrDefault();
                if (firstCompany != null)
                {
                    _logger.LogWarning("No principal company found, using first available company {CompanyId}", firstCompany.Id);
                    return firstCompany;
                }

                throw new InvalidOperationException("No companies found in the system");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting principal company");
                throw;
            }
        }

        private string? ExtractSubdomain(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return null;

            try
            {
                // Remover puerto si existe (ej: localhost:5000 -> localhost)
                var hostWithoutPort = host.Split(':')[0].ToLowerInvariant();

                _logger.LogInformation("RESOLVER: Processing host {Host} -> {HostWithoutPort}", host, hostWithoutPort);

                // Si es localhost o una IP, no hay subdominio
                if (hostWithoutPort == "localhost" || IsIpAddress(hostWithoutPort))
                {
                    _logger.LogInformation("RESOLVER: Host {Host} is localhost or IP, no subdomain", hostWithoutPort);
                    return null;
                }

                // Dividir por puntos
                var parts = hostWithoutPort.Split('.');
                _logger.LogInformation("RESOLVER: Host parts: [{Parts}]", string.Join(", ", parts));

                // Si tiene menos de 3 partes, no hay subdominio
                // ej: "districatalogo.com" = 2 partes, no subdominio
                // ej: "distribuidora-norte.districatalogo.com" = 3 partes, subdominio = "distribuidora-norte"
                if (parts.Length < 3)
                {
                    _logger.LogInformation("RESOLVER: Host {Host} has less than 3 parts, no subdomain", hostWithoutPort);
                    return null;
                }

                // El subdominio es la primera parte
                var subdomain = parts[0];

                // Verificar que el resto coincida con el dominio base
                var remainingDomain = string.Join(".", parts.Skip(1));
                _logger.LogInformation("RESOLVER: Checking remaining domain {RemainingDomain} against base domain {BaseDomain}", 
                    remainingDomain, _options.BaseDomain);
                    
                if (!remainingDomain.Equals(_options.BaseDomain, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("RESOLVER: Host {Host} does not match base domain {BaseDomain}", 
                        host, _options.BaseDomain);
                    return null;
                }

                _logger.LogInformation("RESOLVER: Extracted subdomain {Subdomain} from host {Host}", subdomain, host);
                return subdomain;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RESOLVER: Error extracting subdomain from host {Host}", host);
                return null;
            }
        }

        private static bool IsIpAddress(string host)
        {
            return System.Net.IPAddress.TryParse(host, out _);
        }
    }

    public class CompanyResolverOptions
    {
        public const string SectionName = "CompanyResolver";

        /// <summary>
        /// Dominio base para la aplicación (ej: "districatalogo.com")
        /// </summary>
        public string BaseDomain { get; set; } = "districatalogo.com";

        /// <summary>
        /// ID de la empresa principal por defecto (fallback)
        /// </summary>
        public int? DefaultPrincipalCompanyId { get; set; }
    }
}