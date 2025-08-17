using System;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Queries.Pedidos
{
    public class GetPedidosByClienteQuery : IRequest<PedidosPagedResultDto>
    {
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public PedidoEstado? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }
}