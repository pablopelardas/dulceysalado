using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Mappings
{
    public class ClienteMappingProfile : Profile
    {
        public ClienteMappingProfile()
        {
            // Cliente to ClienteDto
            CreateMap<Cliente, ClienteDto>()
                .ForMember(dest => dest.TieneAcceso, 
                    opt => opt.MapFrom(src => src.CanAuthenticate()))
                .ForMember(dest => dest.ListaPrecio, opt => opt.Ignore()); // Se mapea manualmente
            
            // CreateClienteDto to Cliente
            CreateMap<CreateClienteDto, Cliente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmpresaId, opt => opt.Ignore())
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
            
            // UpdateClienteDto to Cliente (partial update)
            CreateMap<UpdateClienteDto, Cliente>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            
            // CustomerSyncDto to Cliente
            CreateMap<CustomerSyncDto, Cliente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmpresaId, opt => opt.Ignore())
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.ListaPrecioId, opt => opt.Ignore()) // Se mapea manualmente
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
            
            // Cliente to ClienteAuthDto
            CreateMap<Cliente, ClienteAuthDto>();
            
            // CustomerSyncSession to CustomerSyncSessionResponseDto
            CreateMap<CustomerSyncSession, CustomerSyncSessionResponseDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.StartedAt))
                .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => 
                    src.Status == "completed" ? new CustomerSyncSummaryDto
                    {
                        TotalProcessed = src.TotalProcessed,
                        TotalCreated = src.TotalCreated,
                        TotalUpdated = src.TotalUpdated,
                        TotalUnchanged = src.TotalUnchanged,
                        TotalErrors = src.TotalErrors,
                        Duration = src.GetDuration().ToString(@"hh\:mm\:ss")
                    } : null));
            
            // ListaPrecio to ListaPrecioDto (si no existe en otro profile)
            CreateMap<ListaPrecio, ListaPrecioDto>();
            
            // Comentado - No necesario por ahora
            // CreateMap<Infrastructure.Models.ListasPrecio, ListaPrecioDto>()
            //     .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Activa));
        }
    }
}