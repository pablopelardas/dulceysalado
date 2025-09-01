using System;
using System.Collections.Generic;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Pedidos
{
    public class CrearPedidoCommand : IRequest<PedidoDto>
    {
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public List<CrearPedidoItemDto> Items { get; set; } = new();
        public string? Observaciones { get; set; }
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? HorarioEntrega { get; set; }
        public string? DeliverySlot { get; set; }
        public string? CreatedBy { get; set; }
    }
}