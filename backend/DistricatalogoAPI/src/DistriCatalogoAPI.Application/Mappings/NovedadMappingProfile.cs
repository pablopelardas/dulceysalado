using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Mappings
{
    public class NovedadMappingProfile : Profile
    {
        public NovedadMappingProfile()
        {
            CreateMap<EmpresaNovedad, EmpresaNovedadDto>()
                .ForMember(dest => dest.Agrupacion, opt => opt.Ignore()); // Will be filled by repository with includes

            CreateMap<Agrupacion, AgrupacionBasicDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Codigo))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion))
                .ForMember(dest => dest.Activa, opt => opt.MapFrom(src => src.Activa))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo))
                .ForMember(dest => dest.EmpresaPrincipalId, opt => opt.MapFrom(src => src.EmpresaPrincipalId));

            CreateMap<AgrupacionWithNovedadStatus, AgrupacionWithNovedadStatusDto>();

            // Mappings para Ofertas
            CreateMap<EmpresaOferta, EmpresaOfertaDto>()
                .ForMember(dest => dest.Agrupacion, opt => opt.Ignore()); // Will be filled by repository with includes

            CreateMap<AgrupacionWithOfertaStatus, AgrupacionWithOfertaStatusDto>();

            // Mapping para productos de catálogo
            CreateMap<CatalogProduct, ProductoCatalogoDto>()
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Codigo))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre ?? src.Descripcion))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion))
                .ForMember(dest => dest.DescripcionCorta, opt => opt.MapFrom(src => src.DescripcionCorta))
                .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio))
                .ForMember(dest => dest.Destacado, opt => opt.MapFrom(src => src.Destacado))
                .ForMember(dest => dest.ImagenUrls, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ImagenUrl) ? new List<string> { src.ImagenUrl } : new List<string>()))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Existencia))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags ?? new List<string>()))
                .ForMember(dest => dest.Marca, opt => opt.MapFrom(src => src.Marca))
                .ForMember(dest => dest.Unidad, opt => opt.MapFrom(src => src.UnidadMedida))
                .ForMember(dest => dest.CodigoBarras, opt => opt.MapFrom(src => src.CodigoBarras))
                .ForMember(dest => dest.CodigoRubro, opt => opt.MapFrom(src => src.CodigoRubro))
                .ForMember(dest => dest.ImagenAlt, opt => opt.MapFrom(src => src.ImagenAlt))
                .ForMember(dest => dest.TipoProducto, opt => opt.MapFrom(src => src.TipoProducto))
                .ForMember(dest => dest.ListaPrecioId, opt => opt.MapFrom(src => src.ListaPrecioId))
                .ForMember(dest => dest.ListaPrecioNombre, opt => opt.MapFrom(src => src.ListaPrecioNombre))
                .ForMember(dest => dest.ListaPrecioCodigo, opt => opt.MapFrom(src => src.ListaPrecioCodigo));
        }
    }
}