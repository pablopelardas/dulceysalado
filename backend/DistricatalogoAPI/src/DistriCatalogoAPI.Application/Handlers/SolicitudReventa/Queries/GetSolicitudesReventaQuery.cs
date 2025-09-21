using System.Collections.Generic;
using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Queries
{
    public class GetSolicitudesReventaQuery : IRequest<IEnumerable<SolicitudReventaDto>>
    {
        public int EmpresaId { get; set; }
        public bool SoloPendientes { get; set; } = false;
    }
}