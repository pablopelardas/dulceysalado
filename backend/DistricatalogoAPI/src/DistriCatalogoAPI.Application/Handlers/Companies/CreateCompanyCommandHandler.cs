using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Companies;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Companies
{
    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CreateCompanyCommandHandler(ICompanyRepository companyRepository, IUserRepository userRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            // Authorization check - only principal companies can create client companies
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario solicitante inválido");
                }

                if (!requestingUser.IsFromPrincipalCompany)
                {
                    throw new UnauthorizedAccessException("Solo las empresas principales pueden crear empresas cliente");
                }
            }

            // Validate business rules
            if (await _companyRepository.ExistsByCodeAsync(request.Codigo))
            {
                throw new InvalidOperationException($"Ya existe una empresa con el código '{request.Codigo}'");
            }

            if (!string.IsNullOrEmpty(request.DominioPersonalizado) && 
                await _companyRepository.ExistsByDomainAsync(request.DominioPersonalizado))
            {
                throw new InvalidOperationException($"El dominio '{request.DominioPersonalizado}' ya está en uso");
            }

            // Get the requesting user's company (principal company)
            var requestingUserForCompany = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
            var principalCompanyId = requestingUserForCompany.CompanyId;

            // Create company entity (always cliente type when created by principal)
            var company = new Company(request.Codigo, request.Nombre, "cliente", principalCompanyId);

            // Update additional properties
            if (!string.IsNullOrEmpty(request.RazonSocial) || !string.IsNullOrEmpty(request.Cuit) ||
                !string.IsNullOrEmpty(request.Telefono) || !string.IsNullOrEmpty(request.Email) ||
                !string.IsNullOrEmpty(request.Direccion))
            {
                company.UpdateBasicInfo(
                    request.Nombre,
                    request.RazonSocial,
                    request.Cuit,
                    request.Telefono,
                    request.Email,
                    request.Direccion
                );
            }

            // Update branding if provided
            if (!string.IsNullOrEmpty(request.LogoUrl) || request.ColoresTema != null ||
                !string.IsNullOrEmpty(request.FaviconUrl) || !string.IsNullOrEmpty(request.DominioPersonalizado))
            {
                string coloresJson = null;
                if (request.ColoresTema != null)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    coloresJson = JsonSerializer.Serialize(request.ColoresTema, options);
                }

                company.UpdateBranding(
                    request.LogoUrl,
                    coloresJson,
                    request.FaviconUrl,
                    request.DominioPersonalizado
                );
            }

            // Update social media if provided
            if (!string.IsNullOrEmpty(request.UrlWhatsapp) || !string.IsNullOrEmpty(request.UrlFacebook) || 
                !string.IsNullOrEmpty(request.UrlInstagram))
            {
                company.UpdateSocialMedia(
                    request.UrlWhatsapp,
                    request.UrlFacebook,
                    request.UrlInstagram
                );
            }

            if (!string.IsNullOrEmpty(request.Plan) || request.FechaVencimiento.HasValue)
            {
                company.UpdatePlanAndExpiration(
                    request.Plan ?? "basico",
                    request.FechaVencimiento
                );
            }

            // Update permissions if provided (only principal companies can do this)
            if (request.PuedeAgregarProductos.HasValue || request.PuedeAgregarCategorias.HasValue)
            {
                company.UpdatePermissions(
                    request.PuedeAgregarProductos ?? company.PuedeAgregarProductos,
                    request.PuedeAgregarCategorias ?? company.PuedeAgregarCategorias
                );
            }

            // Update catalog settings if provided
            if (request.MostrarPrecios.HasValue || request.MostrarStock.HasValue || 
                request.PermitirPedidos.HasValue || request.ProductosPorPagina.HasValue)
            {
                company.UpdateCatalogSettings(
                    request.MostrarPrecios ?? company.MostrarPrecios,
                    request.MostrarStock ?? company.MostrarStock,
                    request.PermitirPedidos ?? company.PermitirPedidos,
                    request.ProductosPorPagina ?? company.ProductosPorPagina
                );
            }

            // Save to repository
            company = await _companyRepository.CreateAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Map to DTO
            return _mapper.Map<CompanyDto>(company);
        }
    }
}