using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Companies;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Companies
{
    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IMapper _mapper;

        public UpdateCompanyCommandHandler(ICompanyRepository companyRepository, IUserRepository userRepository, IListaPrecioRepository listaPrecioRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _mapper = mapper;
        }

        public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            // Get the company to update
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null || !company.Activa)
            {
                throw new InvalidOperationException($"No se encontró la empresa con ID {request.CompanyId}");
            }

            // Authorization check
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario solicitante inválido");
                }

                // Check if requesting user can manage this company
                bool canManageCompany = requestingUser.IsFromPrincipalCompany || 
                                       requestingUser.CompanyId == company.Id;

                if (!canManageCompany)
                {
                    throw new UnauthorizedAccessException("Permisos insuficientes para actualizar esta empresa");
                }

                // Client companies have limited update permissions
                if (!requestingUser.IsFromPrincipalCompany && requestingUser.CompanyId == company.Id)
                {
                    // Client companies can only update certain fields
                    ValidateClientCompanyUpdatePermissions(request);
                }
            }

            // Validate domain uniqueness if updating
            if (!string.IsNullOrEmpty(request.DominioPersonalizado) && 
                request.DominioPersonalizado != company.DominioPersonalizado &&
                await _companyRepository.ExistsByDomainAsync(request.DominioPersonalizado, company.Id))
            {
                throw new InvalidOperationException($"El dominio '{request.DominioPersonalizado}' ya está en uso");
            }

            // Update basic info if provided
            if (!string.IsNullOrEmpty(request.Nombre) || !string.IsNullOrEmpty(request.RazonSocial) ||
                !string.IsNullOrEmpty(request.Cuit) || !string.IsNullOrEmpty(request.Telefono) ||
                !string.IsNullOrEmpty(request.Email) || !string.IsNullOrEmpty(request.Direccion))
            {
                company.UpdateBasicInfo(
                    request.Nombre ?? company.Nombre,
                    request.RazonSocial ?? company.RazonSocial,
                    request.Cuit ?? company.Cuit,
                    request.Telefono ?? company.Telefono,
                    request.Email ?? company.Email,
                    request.Direccion ?? company.Direccion
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

            // Update branding if provided
            if (request.LogoUrl != null || request.ColoresTema != null ||
                request.FaviconUrl != null || request.DominioPersonalizado != null)
            {
                string coloresJson = company.ColoresTema;
                if (request.ColoresTema != null)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    coloresJson = JsonSerializer.Serialize(request.ColoresTema, options);
                }

                company.UpdateBranding(
                    request.LogoUrl ?? company.LogoUrl,
                    coloresJson,
                    request.FaviconUrl ?? company.FaviconUrl,
                    request.DominioPersonalizado ?? company.DominioPersonalizado
                );
            }

            // Update social media if provided
            if (request.UrlWhatsapp != null || request.UrlFacebook != null || request.UrlInstagram != null)
            {
                company.UpdateSocialMedia(
                    request.UrlWhatsapp ?? company.UrlWhatsapp,
                    request.UrlFacebook ?? company.UrlFacebook,
                    request.UrlInstagram ?? company.UrlInstagram
                );
            }

            // Update permissions if provided (only principal companies can do this)
            var requestingUserForPermissions = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
            if (requestingUserForPermissions.IsFromPrincipalCompany &&
                (request.PuedeAgregarProductos.HasValue || request.PuedeAgregarCategorias.HasValue))
            {
                company.UpdatePermissions(
                    request.PuedeAgregarProductos ?? company.PuedeAgregarProductos,
                    request.PuedeAgregarCategorias ?? company.PuedeAgregarCategorias
                );
            }

            // Update plan and expiration if provided (only principal companies can do this)
            if (requestingUserForPermissions.IsFromPrincipalCompany &&
                (!string.IsNullOrEmpty(request.Plan) || request.FechaVencimiento.HasValue))
            {
                company.UpdatePlanAndExpiration(
                    request.Plan ?? company.Plan,
                    request.FechaVencimiento ?? company.FechaVencimiento
                );
            }

            // Update lista precio predeterminada if provided
            if (request.ListaPrecioPredeterminadaId.HasValue)
            {
                // Validate that the lista precio exists and is active
                var listaPrecio = await _listaPrecioRepository.GetByIdAsync(request.ListaPrecioPredeterminadaId.Value);
                if (listaPrecio == null)
                {
                    throw new InvalidOperationException($"La lista de precios con ID {request.ListaPrecioPredeterminadaId.Value} no existe");
                }
                if (!listaPrecio.Activa)
                {
                    throw new InvalidOperationException($"La lista de precios '{listaPrecio.Nombre}' no está activa");
                }
            }

            // Set the lista precio predeterminada (null is allowed to reset to global default)
            if (request.ListaPrecioPredeterminadaId != company.ListaPrecioPredeterminadaId)
            {
                company.SetListaPrecioPredeterminada(request.ListaPrecioPredeterminadaId);
            }

            // Save changes
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Map to DTO
            var companyDto = _mapper.Map<CompanyDto>(company);
            
            // Map lista precio predeterminada name if exists
            if (company.ListaPrecioPredeterminadaId.HasValue)
            {
                var listaPrecio = await _listaPrecioRepository.GetByIdAsync(company.ListaPrecioPredeterminadaId.Value);
                companyDto.ListaPrecioPredeterminadaNombre = listaPrecio?.Nombre;
            }
            
            return companyDto;
        }

        private void ValidateClientCompanyUpdatePermissions(UpdateCompanyCommand request)
        {
            // Client companies cannot update these fields
            if (!string.IsNullOrEmpty(request.Plan) || request.FechaVencimiento.HasValue ||
                request.PuedeAgregarProductos.HasValue || request.PuedeAgregarCategorias.HasValue)
            {
                throw new UnauthorizedAccessException("Las empresas cliente no pueden actualizar plan, vencimiento o permisos de productos/categorías");
            }
        }
    }
}