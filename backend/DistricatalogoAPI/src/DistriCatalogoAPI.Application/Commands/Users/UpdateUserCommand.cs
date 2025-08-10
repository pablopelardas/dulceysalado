using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Users
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public int UserId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Rol { get; set; }
        public bool? PuedeGestionarProductosBase { get; set; }
        public bool? PuedeGestionarProductosEmpresa { get; set; }
        public bool? PuedeGestionarCategoriasBase { get; set; }
        public bool? PuedeGestionarCategoriasEmpresa { get; set; }
        public bool? PuedeGestionarUsuarios { get; set; }
        public bool? PuedeVerEstadisticas { get; set; }
        public int? RequestingUserId { get; set; }
    }
}