using AutoMapper;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Mappings
{
    public class SyncMappingProfile : Profile
    {
        public SyncMappingProfile()
        {
            // Company mappings (ya existente, solo para referencia)
            CreateMap<Empresa, Company>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Codigo))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.RazonSocial, opt => opt.MapFrom(src => src.RazonSocial))
                .ForMember(dest => dest.Cuit, opt => opt.MapFrom(src => src.Cuit))
                .ForMember(dest => dest.TipoEmpresa, opt => opt.MapFrom(src => src.TipoEmpresa))
                .ForMember(dest => dest.EmpresaPrincipalId, opt => opt.MapFrom(src => src.EmpresaPrincipalId))
                .ForMember(dest => dest.Activa, opt => opt.MapFrom(src => src.Activa ?? true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.UtcNow));

            CreateMap<Company, Empresa>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Codigo))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.RazonSocial, opt => opt.MapFrom(src => src.RazonSocial))
                .ForMember(dest => dest.Cuit, opt => opt.MapFrom(src => src.Cuit))
                .ForMember(dest => dest.TipoEmpresa, opt => opt.MapFrom(src => src.TipoEmpresa))
                .ForMember(dest => dest.EmpresaPrincipalId, opt => opt.MapFrom(src => src.EmpresaPrincipalId))
                .ForMember(dest => dest.Activa, opt => opt.MapFrom(src => src.Activa))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // Note: SyncSession, SyncLog, ProductBase y CategoryBase mapping 
            // se manejan manualmente en los repositorios debido a la complejidad
            // de las propiedades privadas y value objects del dominio
        }
    }
}