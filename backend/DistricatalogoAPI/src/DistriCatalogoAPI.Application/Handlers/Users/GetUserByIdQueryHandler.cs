using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Users;
using DistriCatalogoAPI.Domain.Exceptions;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            
            if (user == null)
            {
                throw new UserNotFoundException(request.UserId);
            }

            // Authorization check
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Invalid requesting user");
                }

                // Users can view their own data, or users they can manage
                bool canViewUser = requestingUser.Id == user.Id || 
                                 requestingUser.CanManageUsersFromCompany(user.CompanyId);

                if (!canViewUser)
                {
                    throw new UnauthorizedAccessException("Insufficient permissions to view this user");
                }
            }

            var userDto = _mapper.Map<UserDto>(user);
            
            // Get and map company data
            var company = await _userRepository.GetCompanyAsync(user.CompanyId);
            if (company != null)
            {
                userDto.Empresa = _mapper.Map<CompanyBasicDto>(company);
            }
            
            return userDto;
        }
    }
}