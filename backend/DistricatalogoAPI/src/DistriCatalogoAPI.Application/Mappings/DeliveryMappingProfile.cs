using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.Mappings
{
    public class DeliveryMappingProfile : Profile
    {
        public DeliveryMappingProfile()
        {
            // DeliverySettings mappings
            CreateMap<DeliverySettings, DeliverySettingsDto>();

            // DeliverySchedule mappings
            CreateMap<DeliverySchedule, DeliveryScheduleDto>();

            // DeliverySlot mappings
            CreateMap<DeliverySlot, DeliverySlotDto>()
                .ForMember(dest => dest.SlotTypeName, opt => opt.MapFrom(src => GetSlotTypeName(src.SlotType)))
                .ForMember(dest => dest.MaxCapacity, opt => opt.Ignore()) // Se calcula en el handler
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore()); // Se calcula en el handler

            // Los mappings de commands no son necesarios ya que se hacen manualmente en los controladores
        }

        private string GetSlotTypeName(SlotType slotType)
        {
            return slotType switch
            {
                SlotType.Morning => "Mañana",
                SlotType.Afternoon => "Tarde",
                _ => "Desconocido"
            };
        }
    }
}