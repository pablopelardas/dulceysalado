using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Commands
{
    public class CreateSolicitudReventaCommand : IRequest<SolicitudReventaDto>
    {
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public CreateSolicitudReventaDto Datos { get; set; } = new();
    }
}