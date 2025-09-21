using System.Threading;
using System.Threading.Tasks;
using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using DistriCatalogoAPI.Domain.Interfaces;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Queries
{
    public class GetSolicitudClienteQueryHandler : IRequestHandler<GetSolicitudClienteQuery, SolicitudReventaDto?>
    {
        private readonly ISolicitudReventaRepository _repository;

        public GetSolicitudClienteQueryHandler(ISolicitudReventaRepository repository)
        {
            _repository = repository;
        }

        public async Task<SolicitudReventaDto?> Handle(GetSolicitudClienteQuery request, CancellationToken cancellationToken)
        {
            var solicitud = await _repository.GetByClienteIdAsync(request.ClienteId);
            
            if (solicitud == null)
                return null;

            return new SolicitudReventaDto
            {
                Id = solicitud.Id,
                ClienteId = solicitud.ClienteId,
                ClienteNombre = solicitud.Cliente?.Nombre,
                ClienteEmail = solicitud.Cliente?.Email,
                Cuit = solicitud.Cuit,
                RazonSocial = solicitud.RazonSocial,
                DireccionComercial = solicitud.DireccionComercial,
                Localidad = solicitud.Localidad,
                Provincia = solicitud.Provincia,
                CodigoPostal = solicitud.CodigoPostal,
                TelefonoComercial = solicitud.TelefonoComercial,
                CategoriaIva = solicitud.CategoriaIva,
                EmailComercial = solicitud.EmailComercial,
                Estado = solicitud.Estado.ToString(),
                ComentarioRespuesta = solicitud.ComentarioRespuesta,
                FechaRespuesta = solicitud.FechaRespuesta,
                RespondidoPor = solicitud.RespondidoPor,
                FechaSolicitud = solicitud.FechaSolicitud
            };
        }
    }
}