using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Exceptions;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public RefreshTokenCommandHandler(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Validate refresh token
            if (!await _authService.ValidateRefreshTokenAsync(request.RefreshToken))
            {
                throw new UnauthorizedAccessException("Token de actualización inválido");
            }

            // Get user ID from refresh token
            var userId = _authService.GetUserIdFromRefreshToken(request.RefreshToken);
            if (userId == null)
            {
                throw new UnauthorizedAccessException("Token de actualización inválido");
            }

            // Get user from database
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null || !user.IsActive)
            {
                throw new UserNotFoundException(userId.Value);
            }

            // Generate new tokens
            var authResponse = await _authService.GenerateTokenAsync(user);
            
            return authResponse;
        }
    }
}