using System;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Queries.Pedidos
{
    public class GetPedidosQuery : IRequest<PedidosPagedResultDto>
    {
        public int EmpresaId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public PedidoEstado? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? ClienteId { get; set; }
        public string? NumeroContiene { get; set; }
    }
}