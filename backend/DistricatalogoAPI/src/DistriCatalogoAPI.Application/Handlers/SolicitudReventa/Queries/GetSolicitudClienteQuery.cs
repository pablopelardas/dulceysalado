using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Queries
{
    public class GetSolicitudClienteQuery : IRequest<SolicitudReventaDto?>
    {
        public int ClienteId { get; set; }
    }
}