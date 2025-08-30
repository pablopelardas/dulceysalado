using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Enums;
using DistriCatalogoAPI.Domain.Exceptions;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.ValueObjects;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserNotificationPreferencesRepository _notificationPreferencesRepository;
        private readonly IMapper _mapper;

        public CreateUserCommandHandler(
            IUserRepository userRepository, 
            ICompanyRepository companyRepository,
            IUserNotificationPreferencesRepository notificationPreferencesRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _notificationPreferencesRepository = notificationPreferencesRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Si no se especifica empresa, usar la empresa principal (primera empresa activa)
            int empresaId = request.EmpresaId;
            if (empresaId == 0)
            {
                var companies = await _companyRepository.GetAllAsync();
                var mainCompany = companies.FirstOrDefault(c => c.Activa);
                if (mainCompany == null)
                {
                    throw new InvalidOperationException("No hay empresas activas en el sistema");
                }
                empresaId = mainCompany.Id;
            }

            // Authorization check
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario solicitante inválido");
                }

                // Check if requesting user can manage users from the target company
                if (!requestingUser.CanManageUsersFromCompany(empresaId))
                {
                    throw new UnauthorizedAccessException("Permisos insuficientes para crear usuarios en esta empresa");
                }
            }

            // Validate email is unique among active users
            var email = new Email(request.Email);
            if (await _userRepository.ExistsByEmailAsync(email))
            {
                throw new DuplicateEmailException(request.Email);
            }

            // Validate password and hash it
            Password.ValidatePasswordStrength(request.Password);
            var passwordHash = Password.HashPassword(request.Password);

            // Get company type and map role correctly
            var companyType = await _userRepository.GetCompanyTypeAsync(empresaId);
            var role = _userRepository.MapDatabaseRoleToEnum(request.Rol, companyType);

            // Create user entity
            var user = new User(
                empresaId,
                email,
                passwordHash,
                request.Nombre,
                request.Apellido,
                role);

            // Save to repository
            user = await _userRepository.CreateAsync(user);

            // Crear preferencias de notificación por defecto para el nuevo usuario
            await _notificationPreferencesRepository.CreateDefaultPreferencesForUserAsync(user.Id);

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