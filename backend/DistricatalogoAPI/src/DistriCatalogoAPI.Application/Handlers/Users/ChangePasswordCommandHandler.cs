using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Exceptions;
using DistriCatalogoAPI.Domain.ValueObjects;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public ChangePasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Get the user whose password will be changed
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
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

                // Users can change their own password, or admins can change passwords for users they can manage
                bool canChangePassword = requestingUser.Id == user.Id || 
                                       requestingUser.CanManageUsersFromCompany(user.CompanyId);

                if (!canChangePassword)
                {
                    throw new UnauthorizedAccessException("Insufficient permissions to change this user's password");
                }

                // If user is changing their own password, verify current password
                if (requestingUser.Id == user.Id)
                {
                    if (string.IsNullOrEmpty(request.CurrentPassword))
                    {
                        throw new UnauthorizedAccessException("Current password is required when changing your own password");
                    }
                    
                    if (!Password.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    {
                        throw new UnauthorizedAccessException("Current password is incorrect");
                    }
                }
                // If admin is changing another user's password, current password is not required
            }

            // Validate and hash new password
            Password.ValidatePasswordStrength(request.NewPassword);
            var newPasswordHash = Password.HashPassword(request.NewPassword);

            // Change password
            user.ChangePassword(newPasswordHash);

            // Save changes
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}