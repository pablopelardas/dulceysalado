using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Companies;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Exceptions;

namespace DistriCatalogoAPI.Application.Handlers.Companies
{
    public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly IMapper _mapper;

        public GetCompanyByIdQueryHandler(ICompanyRepository companyRepository, IUserRepository userRepository, IListaPrecioRepository listaPrecioRepository, IFeatureRepository featureRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _featureRepository = featureRepository;
            _mapper = mapper;
        }

        public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            
            if (company == null || (!request.IncludeInactive && !company.Activa))
            {
                throw new InvalidOperationException($"Company with ID {request.CompanyId} not found");
            }

            // Authorization check
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Invalid requesting user");
                }

                // Check if requesting user can view this company
                bool canViewCompany = requestingUser.IsFromPrincipalCompany || 
                                     requestingUser.CompanyId == company.Id;

                if (!canViewCompany)
                {
                    throw new UnauthorizedAccessException("Insufficient permissions to view this company");
                }
            }

            // Map to DTO
            var companyDto = _mapper.Map<CompanyDto>(company);
            
            // Map lista precio predeterminada name if exists
            if (company.ListaPrecioPredeterminadaId.HasValue)
            {
                var listaPrecio = await _listaPrecioRepository.GetByIdAsync(company.ListaPrecioPredeterminadaId.Value);
                companyDto.ListaPrecioPredeterminadaNombre = listaPrecio?.Nombre;
            }

            // Map features for the company
            var featuresWithDefaults = await GetFeaturesWithDefaultsAsync(company.Id);
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