using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Pedidos;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class GetPedidosEstadisticasQueryHandler : IRequestHandler<GetPedidosEstadisticasQuery, PedidoEstadisticasDto>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ILogger<GetPedidosEstadisticasQueryHandler> _logger;

        public GetPedidosEstadisticasQueryHandler(
            IPedidoRepository pedidoRepository,
            ILogger<GetPedidosEstadisticasQueryHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _logger = logger;
        }

        public async Task<PedidoEstadisticasDto> Handle(GetPedidosEstadisticasQuery request, CancellationToken cancellationToken)
        {
            var hoy = DateTime.UtcNow.Date;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var estadisticas = new PedidoEstadisticasDto
            {
                TotalPendientes = await _pedidoRepository.GetCountByEstadoAsync(request.EmpresaId, PedidoEstado.Pendiente),
                TotalAceptados = await _pedidoRepository.GetCountByEstadoAsync(request.EmpresaId, PedidoEstado.Aceptado),
                TotalRechazados = await _pedidoRepository.GetCountByEstadoAsync(request.EmpresaId, PedidoEstado.Rechazado),
                TotalCompletados = await _pedidoRepository.GetCountByEstadoAsync(request.EmpresaId, PedidoEstado.Completado),
                TotalCancelados = await _pedidoRepository.GetCountByEstadoAsync(request.EmpresaId, PedidoEstado.Cancelado),
                
                MontoTotalHoy = await _pedidoRepository.GetMontoTotalByEstadoAsync(request.EmpresaId, PedidoEstado.Completado, hoy, hoy.AddDays(1)),
                MontoTotalSemana = await _pedidoRepository.GetMontoTotalByEstadoAsync(request.EmpresaId, PedidoEstado.Completado, inicioSemana),
                MontoTotalMes = await _pedidoRepository.GetMontoTotalByEstadoAsync(request.EmpresaId, PedidoEstado.Completado, inicioMes)
            };

            return estadisticas;
        }
    }
}