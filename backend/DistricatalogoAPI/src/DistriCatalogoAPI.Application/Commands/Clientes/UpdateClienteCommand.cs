using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Clientes
{
    public class UpdateClienteCommand : IRequest<ClienteDto>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Telefono { get; set; }
        public string? Cuit { get; set; }
        public string? Altura { get; set; }
        public string? Provincia { get; set; }
        public string? Email { get; set; }
        public string? TipoIva { get; set; }
        public int? ListaPrecioId { get; set; }
        public bool? Activo { get; set; }
        public string? UpdatedBy { get; set; }
    }
}