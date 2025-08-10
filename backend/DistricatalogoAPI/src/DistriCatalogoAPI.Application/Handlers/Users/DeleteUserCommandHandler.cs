using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Exceptions;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public DeleteUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            // Get the user to delete
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
                    throw new UnauthorizedAccessException("Invalid requesting user");
                }

                // Check if requesting user can manage users from the target company
                if (!requestingUser.CanManageUsersFromCompany(user.CompanyId))
                {
                    throw new UnauthorizedAccessException("Insufficient permissions to delete this user");
                }

                // Prevent self-deletion
                if (requestingUser.Id == user.Id)
                {
                    throw new InvalidOperationException("Users cannot delete themselves");
                }
            }

            // Perform soft delete by deactivating the user
            user.Deactivate();

            // Save changes
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}