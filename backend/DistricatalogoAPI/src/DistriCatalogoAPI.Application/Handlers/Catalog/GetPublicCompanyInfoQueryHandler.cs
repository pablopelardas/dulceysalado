using MediatR;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Queries.Catalog;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.Handlers.Catalog
{
    public class GetPublicCompanyInfoQueryHandler : IRequestHandler<GetPublicCompanyInfoQuery, GetPublicCompanyInfoQueryResult>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly ILogger<GetPublicCompanyInfoQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetPublicCompanyInfoQueryHandler(
            ICompanyRepository companyRepository,
            IFeatureRepository featureRepository,
            ILogger<GetPublicCompanyInfoQueryHandler> logger,
            IMapper mapper)
        {
            _companyRepository = companyRepository;
            _featureRepository = featureRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<GetPublicCompanyInfoQueryResult> Handle(GetPublicCompanyInfoQuery request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(request.EmpresaId);
            
            if (company == null)
            {
                _logger.LogWarning("Company {EmpresaId} not found", request.EmpresaId);
                throw new KeyNotFoundException($"Empresa {request.EmpresaId} no encontrada");
            }

            // Get features with defaults
            var featuresWithDefaults = await _featureRepository.GetFeaturesWithDefaultsAsync(request.EmpresaId);
            
            var result = new GetPublicCompanyInfoQueryResult
            {
                Id = company.Id,
                Codigo = company.Codigo,
                Nombre = company.Nombre,
                RazonSocial = company.RazonSocial ?? string.Empty,
                Telefono = company.Telefono ?? string.Empty,
                Email = company.Email ?? string.Empty,
                Direccion = company.Direccion ?? string.Empty,
                LogoUrl = company.LogoUrl ?? string.Empty,
                ColoresTema = company.ColoresTema ?? string.Empty,
                FaviconUrl = company.FaviconUrl ?? string.Empty,
                DominioPersonalizado = company.DominioPersonalizado ?? string.Empty,
                UrlWhatsapp = company.UrlWhatsapp ?? string.Empty,
                UrlFacebook = company.UrlFacebook ?? string.Empty,
                UrlInstagram = company.UrlInstagram ?? string.Empty,
                MostrarPrecios = company.MostrarPrecios,
                MostrarStock = company.MostrarStock,
                PermitirPedidos = company.PermitirPedidos,
                ProductosPorPagina = company.ProductosPorPagina,
                Plan = company.Plan ?? string.Empty,
                Activa = company.Activa,
                Features = _mapper.Map<List<FeatureConfigurationDto>>(featuresWithDefaults)
            };
            
            return result;
        }
    }
}