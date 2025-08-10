using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using DistriCatalogoAPI.Application.Queries.Users;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly IMapper _mapper;

        public GetCurrentUserQueryHandler(
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IFeatureRepository featureRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _featureRepository = featureRepository;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            // Get user by ID
            var user = await _userRepository.GetByIdAsync(request.UserId);
            
            if (user == null)
            {
                throw new UnauthorizedAccessException("Usuario no encontrado");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Usuario inactivo");
            }

            // Map user to DTO
            var userDto = _mapper.Map<UserDto>(user);
            
            // Get company
            var company = await _companyRepository.GetByIdAsync(user.CompanyId);
            CompanyDto? companyDto = null;
            
            if (company != null)
            {
                companyDto = _mapper.Map<CompanyDto>(company);
                
                // Load features with defaults
                var featuresWithDefaults = await _featureRepository.GetFeaturesWithDefaultsAsync(company.Id);
                companyDto.Features = _mapper.Map<List<FeatureConfigurationDto>>(featuresWithDefaults);
                
                // Also add company data to user DTO
                userDto.Empresa = new CompanyBasicDto
                {
                    Id = companyDto.Id,
                    Codigo = companyDto.Codigo,
                    Nombre = companyDto.Nombre,
                    TipoEmpresa = companyDto.TipoEmpresa
                };
            }
            
            // Return response without tokens
            return new AuthResponseDto
            {
                User = userDto,
                Empresa = companyDto
            };
        }
    }
}