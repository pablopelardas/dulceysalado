using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Users
{
    public class RefreshTokenCommand : IRequest<AuthResponseDto>
    {
        public string RefreshToken { get; set; }
    }
}