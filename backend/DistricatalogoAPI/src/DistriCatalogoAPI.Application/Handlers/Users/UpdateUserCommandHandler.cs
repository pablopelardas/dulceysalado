using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Exceptions;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            // Get the user to update
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                throw new UserNotFoundException(request.UserId);
            }

            // Get requesting user for authorization
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario solicitante inválido");
                }

                // Check if requesting user can manage users from the target company
                if (!requestingUser.CanManageUsersFromCompany(user.CompanyId))
                {
                    throw new UnauthorizedAccessException("Permisos insuficientes para actualizar este usuario");
                }
            }

            // Get company type and map role correctly
            var companyType = await _userRepository.GetCompanyTypeAsync(user.CompanyId);
            var role = _userRepository.MapDatabaseRoleToEnum(request.Rol, companyType);

            // Update user properties
            user.UpdateProfile(request.Nombre, request.Apellido);
            
            // Validate and update permissions based on company type
            var isPrincipalCompany = companyType?.ToLower() == "principal";
            
            var permissions = new
            {
                // Base permissions only allowed for principal companies
                CanManageBaseProducts = isPrincipalCompany ? (request.PuedeGestionarProductosBase ?? user.CanManageBaseProducts) : false,
                CanManageBaseCategories = isPrincipalCompany ? (request.PuedeGestionarCategoriasBase ?? user.CanManageBaseCategories) : false,
                
                // Company permissions allowed for all
                CanManageCompanyProducts = request.PuedeGestionarProductosEmpresa ?? user.CanManageCompanyProducts,
                CanManageCompanyCategories = request.PuedeGestionarCategoriasEmpresa ?? user.CanManageCompanyCategories,
                CanManageUsers = request.PuedeGestionarUsuarios ?? user.CanManageUsers,
                CanViewStatistics = request.PuedeVerEstadisticas ?? user.CanViewStatistics
            };
            
            // Validate base permissions request for client companies
            if (!isPrincipalCompany)
            {
                if (request.PuedeGestionarProductosBase == true || request.PuedeGestionarCategoriasBase == true)
                {
                    throw new UnauthorizedAccessException("Los usuarios de empresas cliente no pueden tener permisos de gestión de productos o categorías base");
                }
            }

            user.UpdatePermissions(
                role,
                permissions.CanManageBaseProducts,
                permissions.CanManageCompanyProducts,
                permissions.CanManageBaseCategories,
                permissions.CanManageCompanyCategories,
                permissions.CanManageUsers,
                permissions.CanViewStatistics
            );

            // Save changes
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            // Map to DTO
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