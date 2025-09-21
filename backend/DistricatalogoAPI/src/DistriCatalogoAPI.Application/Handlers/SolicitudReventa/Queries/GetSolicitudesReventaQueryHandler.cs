using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using DistriCatalogoAPI.Domain.Interfaces;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Queries
{
    public class GetSolicitudesReventaQueryHandler : IRequestHandler<GetSolicitudesReventaQuery, IEnumerable<SolicitudReventaDto>>
    {
        private readonly ISolicitudReventaRepository _repository;

        public GetSolicitudesReventaQueryHandler(ISolicitudReventaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SolicitudReventaDto>> Handle(GetSolicitudesReventaQuery request, CancellationToken cancellationToken)
        {
            var solicitudes = request.SoloPendientes
                ? await _repository.GetPendientesAsync(request.EmpresaId)
                : await _repository.GetAllByEmpresaAsync(request.EmpresaId);

            return solicitudes.Select(s => new SolicitudReventaDto
            {
                Id = s.Id,
                ClienteId = s.ClienteId,
                ClienteNombre = s.Cliente?.Nombre,
                ClienteEmail = s.Cliente?.Email,
                Cuit = s.Cuit,
                RazonSocial = s.RazonSocial,
                DireccionComercial = s.DireccionComercial,
                Localidad = s.Localidad,
                Provincia = s.Provincia,
                CodigoPostal = s.CodigoPostal,
                TelefonoComercial = s.TelefonoComercial,
                CategoriaIva = s.CategoriaIva,
                EmailComercial = s.EmailComercial,
                Estado = s.Estado.ToString(),
                ComentarioRespuesta = s.ComentarioRespuesta,
                FechaRespuesta = s.FechaRespuesta,
                RespondidoPor = s.RespondidoPor,
                FechaSolicitud = s.FechaSolicitud
            });
        }
    }
}