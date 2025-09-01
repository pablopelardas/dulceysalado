using System;
using MediatR;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.Commands.Delivery
{
    public class ReserveDeliverySlotCommand : IRequest<bool>
    {
        public int EmpresaId { get; set; }
        public DateOnly Date { get; set; }
        public SlotType SlotType { get; set; }
        public int PedidoId { get; set; }
    }
}