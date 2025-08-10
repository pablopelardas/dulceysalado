using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.ValueObjects;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(IUserRepository userRepository, IAuthService authService, ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            using var activity = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Operation"] = "UserLogin",
                ["UserIdentifier"] = request.Email
            });

            _logger.LogInformation("Login attempt for user {UserIdentifier}", request.Email);

            try
            {
                // Find user by email or username (only active users)
                var user = await _userRepository.GetByEmailOrUsernameAsync(request.Email, false);
                
                if (user == null)
                {
                    _logger.LogWarning("Login failed for {UserIdentifier}: User not found or inactive", request.Email);
                    throw new UnauthorizedAccessException("Email/Usuario o contraseña incorrectos");
                }

                // Verify password
                if (!Password.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed for user {UserId} ({UserIdentifier}): Invalid password", user.Id, request.Email);
                    throw new UnauthorizedAccessException("Email/Usuario o contraseña incorrectos");
                }

                _logger.LogInformation("User {UserId} ({UserIdentifier}) authenticated successfully", user.Id, request.Email);

                // Update last login
                user.UpdateLastLogin();
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogDebug("Updated last login time for user {UserId}", user.Id);

                // Generate token and response
                var authResponse = await _authService.GenerateTokenAsync(user);
                
                _logger.LogInformation("Login completed successfully for user {UserId} ({UserIdentifier}) from company {CompanyId}", 
                    user.Id, request.Email, user.CompanyId);
                
                return authResponse;
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw without logging again (already logged above)
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for user {UserIdentifier}", request.Email);
                throw;
            }
        }
    }
}