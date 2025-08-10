using System.Threading.Tasks;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> GenerateTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(int userId);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
        int? GetUserIdFromToken(string token);
        int? GetUserIdFromRefreshToken(string refreshToken);
    }
}