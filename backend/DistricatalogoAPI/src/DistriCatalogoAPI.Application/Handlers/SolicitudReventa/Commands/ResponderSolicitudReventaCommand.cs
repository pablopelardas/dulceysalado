using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Commands
{
    public class ResponderSolicitudReventaCommand : IRequest<SolicitudReventaDto>
    {
        public int SolicitudId { get; set; }
        public string RespondidoPor { get; set; } = string.Empty;
        public ResponderSolicitudDto Respuesta { get; set; } = new();
    }
}