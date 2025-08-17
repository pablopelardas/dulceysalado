using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Mappings
{
    public class PedidoMappingProfile : Profile
    {
        private static readonly TimeZoneInfo ArgentinaTimeZone = GetArgentinaTimeZone();

        public PedidoMappingProfile()
        {
            CreateMap<Pedido, PedidoDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombre : null))
                .ForMember(dest => dest.ClienteEmail, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Email : null))
                .ForMember(dest => dest.ClienteTelefono, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Telefono : null))
                .ForMember(dest => dest.FechaPedido, opt => opt.MapFrom(src => ConvertToArgentinaTime(src.FechaPedido)))
                .ForMember(dest => dest.FechaEntrega, opt => opt.MapFrom(src => src.FechaEntrega.HasValue ? ConvertToArgentinaTime(src.FechaEntrega.Value) : (DateTime?)null))
                .ForMember(dest => dest.FechaGestion, opt => opt.MapFrom(src => src.FechaGestion.HasValue ? ConvertToArgentinaTime(src.FechaGestion.Value) : (DateTime?)null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => ConvertToArgentinaTime(src.CreatedAt)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => ConvertToArgentinaTime(src.UpdatedAt)));

            CreateMap<PedidoItem, PedidoItemDto>();

            CreateMap<CrearPedidoDto, CrearPedidoCommand>();
            CreateMap<CrearPedidoItemDto, PedidoItem>();
        }

        private static TimeZoneInfo GetArgentinaTimeZone()
        {
            try
            {
                // Intentar Windows ID primero
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            }
            catch
            {
                try
                {
                    // Intentar IANA ID para Linux/Mac
                    return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
                }
                catch
                {
                    // Fallback: crear zona horaria UTC-3 manual
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "Argentina Custom", 
                        TimeSpan.FromHours(-3), 
                        "Argentina Standard Time", 
                        "Argentina Standard Time");
                }
            }
        }

        private static DateTime ConvertToArgentinaTime(DateTime utcDateTime)
        {
            try
            {
                // Asegurar que la fecha está en UTC
                var utcTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
                // Convertir a hora de Argentina
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, ArgentinaTimeZone);
            }
            catch
            {
                // Si hay error con la zona horaria, usar UTC-3 manualmente
                return utcDateTime.AddHours(-3);
            }
        }
    }
}