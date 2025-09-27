using System.Collections.Generic;
using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Queries
{
    public class GetSolicitudesReventaQuery : IRequest<IEnumerable<SolicitudReventaDto>>
    {
        public int EmpresaId { get; set; }
        public string? Estado { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        public string? SortBy { get; set; } = "fechaSolicitud";
        public string? SortOrder { get; set; } = "desc";
    }
}