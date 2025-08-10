using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Users
{
    public class CreateUserCommand : IRequest<UserDto>
    {
        public int EmpresaId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? RequestingUserId { get; set; } // User making the request
    }
}