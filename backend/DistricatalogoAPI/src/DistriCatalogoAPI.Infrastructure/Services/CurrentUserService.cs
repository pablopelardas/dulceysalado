using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogError("Usuario no encontrado: ID {UserId}", userId);
                throw new UnauthorizedAccessException($"Usuario con ID {userId} no encontrado");
            }

            return user;
        }

        public int GetCurrentUserId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("ID de usuario inválido en claims");
            }

            return userId;
        }

        public int GetCurrentCompanyId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }

            var companyIdClaim = context.User.FindFirst("empresaId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                throw new UnauthorizedAccessException("ID de empresa inválido en claims");
            }

            return companyId;
        }
    }
}