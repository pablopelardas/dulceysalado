using System;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Rol { get; set; }
        public bool PuedeGestionarProductosBase { get; set; }
        public bool PuedeGestionarProductosEmpresa { get; set; }
        public bool PuedeGestionarCategoriasBase { get; set; }
        public bool PuedeGestionarCategoriasEmpresa { get; set; }
        public bool PuedeGestionarUsuarios { get; set; }
        public bool PuedeVerEstadisticas { get; set; }
        public bool Activo { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CompanyBasicDto Empresa { get; set; }
    }
}