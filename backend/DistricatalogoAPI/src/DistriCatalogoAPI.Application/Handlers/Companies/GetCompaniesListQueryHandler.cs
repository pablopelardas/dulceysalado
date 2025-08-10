using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Companies;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Companies
{
    public class GetCompaniesListQueryHandler : IRequestHandler<GetCompaniesListQuery, PagedResultDto<CompanyDto>>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetCompaniesListQueryHandler(ICompanyRepository companyRepository, IUserRepository userRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<CompanyDto>> Handle(GetCompaniesListQuery request, CancellationToken cancellationToken)
        {
            // Authorization check
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Invalid requesting user");
                }

                // Handle filtering logic based on user type
                if (!requestingUser.IsFromPrincipalCompany)
                {
                    // Client companies can only see their own company
                    request.PrincipalCompanyId = requestingUser.CompanyId;
                    // Override to show only their own company by setting page size to specific company
                }
                // Principal companies can see all client companies (request.PrincipalCompanyId can be null to see all)
            }

            // Validate pagination parameters
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100; // Max page size limit

            // Get paginated companies
            var (companies, totalCount) = await _companyRepository.GetPagedAsync(
                request.Page, 
                request.PageSize, 
                request.PrincipalCompanyId, 
                request.IncludeInactive);

            // Map to DTOs
            var companyDtos = companies.Select(company => _mapper.Map<CompanyDto>(company)).ToList();

            var result = new PagedResultDto<CompanyDto>
            {
                Pagination = new PaginationDto
                {
                    Page = request.Page,
                    Limit = request.PageSize,
                    Total = totalCount
                }
            };
            
            result.SetItems(companyDtos, "empresas");
            return result;
        }
    }
}