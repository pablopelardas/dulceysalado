using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Infrastructure.Services
{
    public class JwtService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly ILogger<JwtService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationHours;

        public JwtService(IConfiguration configuration, IMapper mapper, IUserRepository userRepository, ICompanyRepository companyRepository, IFeatureRepository featureRepository, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _mapper = mapper;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _featureRepository = featureRepository;
            _logger = logger;
            _secretKey = _configuration["Jwt:SecretKey"] ?? "default-secret-key-for-development-only-not-for-production";
            _issuer = _configuration["Jwt:Issuer"] ?? "DistriCatalogoAPI";
            _audience = _configuration["Jwt:Audience"] ?? "DistriCatalogoAPI";
            _expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");
        }

        public async Task<AuthResponseDto> GenerateTokenAsync(User user)
        {
            using var activity = _logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = user.Id,
                ["CompanyId"] = user.CompanyId,
                ["UserEmail"] = user.Email.Value,
                ["Operation"] = "GenerateToken"
            });

            _logger.LogInformation("Generating JWT token for user {UserId} ({UserEmail}) from company {CompanyId}", 
                user.Id, user.Email.Value, user.CompanyId);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                
                // Obtener información de la empresa para agregar claims adicionales
                var company = await _userRepository.GetCompanyAsync(user.CompanyId);
                
                if (company == null)
                {
                    _logger.LogWarning("Company {CompanyId} not found for user {UserId}", user.CompanyId, user.Id);
                }
                else
                {
                    _logger.LogDebug("Retrieved company info for token: {CompanyId} ({CompanyName}, Type: {CompanyType})", 
                        company.Id, company.Nombre, company.TipoEmpresa);
                }
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new("userId", user.Id.ToString()),
                new("email", user.Email.Value),
                new("nombre", user.FirstName),
                new("apellido", user.LastName),
                new("rol", GetRoleString(user.Role)),
                new("empresaId", user.CompanyId.ToString()),
                new("puede_gestionar_productos_base", user.CanManageBaseProducts.ToString().ToLower()),
                new("puede_gestionar_productos_empresa", user.CanManageCompanyProducts.ToString().ToLower()),
                new("puede_gestionar_categorias_base", user.CanManageBaseCategories.ToString().ToLower()),
                new("puede_gestionar_categorias_empresa", user.CanManageCompanyCategories.ToString().ToLower()),
                new("puede_gestionar_usuarios", user.CanManageUsers.ToString().ToLower()),
                new("puede_ver_estadisticas", user.CanViewStatistics.ToString().ToLower())
            };

            // Agregar claims de empresa si la empresa existe
            if (company != null)
            {
                claims.Add(new("tipo_empresa", company.TipoEmpresa));
                
                // Si es empresa cliente, agregar el ID de la empresa principal
                if (company.TipoEmpresa == "cliente" && company.EmpresaPrincipalId.HasValue)
                {
                    claims.Add(new("empresa_principal_id", company.EmpresaPrincipalId.Value.ToString()));
                }
                // Si es empresa principal, ella misma es la empresa principal
                else if (company.TipoEmpresa == "principal")
                {
                    claims.Add(new("empresa_principal_id", company.Id.ToString()));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_expirationHours),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                var userDto = _mapper.Map<UserDto>(user);
                
                // Map company data (already obtained above)
                var companyDto = company != null ? _mapper.Map<CompanyDto>(company) : null;
                
                // Load and map features for the company
                if (companyDto != null)
                {
                    var featuresWithDefaults = await GetFeaturesWithDefaultsAsync(company.Id);
                    companyDto.Features = _mapper.Map<List<FeatureConfigurationDto>>(featuresWithDefaults);
                }
                
                // Also add company data to user DTO
                if (company != null)
                {
                    userDto.Empresa = _mapper.Map<CompanyBasicDto>(company);
                }
                
                _logger.LogInformation("JWT token generated successfully for user {UserId}, expires at {ExpiresAt}", 
                    user.Id, DateTime.UtcNow.AddHours(_expirationHours));
                
                return new AuthResponseDto
                {
                    Message = "Login exitoso",
                    User = userDto,
                    Empresa = companyDto,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = $"{_expirationHours}h"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate JWT token for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            // Get user to obtain company ID
            var user = await _userRepository.GetByIdAsync(userId);
            var empresaId = user?.CompanyId.ToString() ?? "1";
            
            var claims = new List<Claim>
            {
                new("userId", userId.ToString()),
                new("empresaId", empresaId)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userId = principal.FindFirst("userId")?.Value;
                _logger.LogDebug("Token validated successfully for user {UserId}", userId);
                return true;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token validation failed: {Reason}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return false;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                
                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userId = principal.FindFirst("userId")?.Value;
                _logger.LogDebug("Refresh token validated successfully for user {UserId}", userId);
                return true;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Refresh token validation failed: {Reason}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during refresh token validation");
                return false;
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogDebug("Extracted userId {UserId} from token", userId);
                    return userId;
                }
                
                _logger.LogWarning("Unable to extract userId from token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting userId from token");
                return null;
            }
        }

        public int? GetUserIdFromRefreshToken(string refreshToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(refreshToken);
                
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogDebug("Extracted userId {UserId} from refresh token", userId);
                    return userId;
                }
                
                _logger.LogWarning("Unable to extract userId from refresh token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting userId from refresh token");
                return null;
            }
        }

        private string GetRoleString(Domain.Enums.UserRole role)
        {
            return role switch
            {
                Domain.Enums.UserRole.PrincipalAdmin or Domain.Enums.UserRole.ClientAdmin => "admin",
                Domain.Enums.UserRole.PrincipalEditor or Domain.Enums.UserRole.ClientEditor => "editor", 
                Domain.Enums.UserRole.PrincipalViewer or Domain.Enums.UserRole.ClientViewer => "viewer",
                _ => "viewer"
            };
        }

        public async Task<CompanyDto?> GetCompanyWithFeaturesAsync(int empresaId)
        {
            var company = await _companyRepository.GetByIdAsync(empresaId);
            if (company == null) return null;
            
            var companyDto = _mapper.Map<CompanyDto>(company);
            
            // Load and map features for the company
            var featuresWithDefaults = await GetFeaturesWithDefaultsAsync(empresaId);
            companyDto.Features = _mapper.Map<List<FeatureConfigurationDto>>(featuresWithDefaults);
            
            return companyDto;
        }
        
        private async Task<List<EmpresaFeature>> GetFeaturesWithDefaultsAsync(int empresaId)
        {
            // Obtener todas las definiciones de features activas
            var definitions = await _featureRepository.GetAllDefinitionsAsync();
            
            // Obtener configuraciones específicas de la empresa
            var empresaFeatures = await _featureRepository.GetFeaturesDictionaryAsync(empresaId);
            
            var result = new List<EmpresaFeature>();
            
            foreach (var definition in definitions)
            {
                if (empresaFeatures.TryGetValue(definition.Codigo, out var empresaFeature))
                {
                    // Ya existe configuración para esta empresa
                    result.Add(empresaFeature);
                }
                else
                {
                    // Crear feature con valores por defecto
                    var defaultFeature = new EmpresaFeature
                    {
                        EmpresaId = empresaId,
                        FeatureId = definition.Id,
                        Feature = definition,
                        Habilitado = false,
                        Valor = definition.ValorDefecto,
                        Metadata = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    result.Add(defaultFeature);
                }
            }
            
            return result;
        }
    }
}