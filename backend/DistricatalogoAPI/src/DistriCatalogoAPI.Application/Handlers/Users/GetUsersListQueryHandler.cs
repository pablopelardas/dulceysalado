using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Users;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, PagedResultDto<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUsersListQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<UserDto>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
        {
            // Authorization check
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Invalid requesting user");
                }

                // Handle empresaId logic based on user type
                if (!request.EmpresaId.HasValue)
                {
                    // If no empresaId specified:
                    // - Principal company users can see all users (empresaId stays null)
                    // - Client company users can only see their own company users
                    if (!requestingUser.IsFromPrincipalCompany)
                    {
                        request.EmpresaId = requestingUser.CompanyId;
                    }
                    // For principal company users, leave empresaId as null to get all users
                }
                else
                {
                    // If empresaId is specified, ensure requesting user can manage users from that company
                    if (!requestingUser.CanManageUsersFromCompany(request.EmpresaId.Value))
                    {
                        throw new UnauthorizedAccessException("Insufficient permissions to view users from this company");
                    }
                }
            }

            // Validate pagination parameters
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100; // Max page size limit

            // Get paginated users
            var (users, totalCount) = await _userRepository.GetPagedAsync(
                request.Page, 
                request.PageSize, 
                request.EmpresaId, 
                request.IncludeInactive);

            // Map to DTOs and populate company data efficiently
            var userDtos = new List<UserDto>();
            var companyIds = users.Select(u => u.CompanyId).Distinct().ToList();
            var companiesDict = new Dictionary<int, CompanyBasicDto>();
            
            // Get all companies in one query
            foreach (var companyId in companyIds)
            {
                var company = await _userRepository.GetCompanyAsync(companyId);
                if (company != null)
                {
                    companiesDict[companyId] = _mapper.Map<CompanyBasicDto>(company);
                }
            }
            
            // Map users and assign companies
            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDto>(user);
                
                if (companiesDict.TryGetValue(user.CompanyId, out var companyDto))
                {
                    userDto.Empresa = companyDto;
                }
                
                userDtos.Add(userDto);
            }

            var result = new PagedResultDto<UserDto>
            {
                Pagination = new PaginationDto
                {
                    Page = request.Page,
                    Limit = request.PageSize,
                    Total = totalCount
                }
            };
            
            result.SetItems(userDtos, "usuarios");
            return result;
        }
    }
}