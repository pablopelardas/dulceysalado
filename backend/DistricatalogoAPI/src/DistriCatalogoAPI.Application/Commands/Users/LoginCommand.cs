using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Users
{
    public class LoginCommand : IRequest<AuthResponseDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}