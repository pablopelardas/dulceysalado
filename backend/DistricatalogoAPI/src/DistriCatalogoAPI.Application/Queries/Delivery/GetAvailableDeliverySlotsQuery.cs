using System;
using System.Collections.Generic;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.Queries.Delivery
{
    public class GetAvailableDeliverySlotsQuery : IRequest<List<AvailableDeliverySlotDto>>
    {
        public int EmpresaId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public SlotType? SlotType { get; set; }
        public bool OnlyAvailable { get; set; } = true;
    }
}