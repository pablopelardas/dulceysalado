using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DistriCatalogoAPI.Application.Mappings
{
    public class FeatureMappingProfile : Profile
    {
        public FeatureMappingProfile()
        {
            CreateMap<FeatureDefinition, FeatureDefinitionDto>()
                .ForMember(dest => dest.TipoValor, opt => opt.MapFrom(src => src.TipoValor.ToString()));

            CreateMap<EmpresaFeature, EmpresaFeatureDto>()
                .ForMember(dest => dest.FeatureCodigo, opt => opt.MapFrom(src => src.Feature.Codigo))
                .ForMember(dest => dest.FeatureNombre, opt => opt.MapFrom(src => src.Feature.Nombre))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => ParseMetadata(src.Metadata)));

            CreateMap<EmpresaFeature, FeatureConfigurationDto>()
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Feature.Codigo))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Feature.Nombre))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Feature.Descripcion))
                .ForMember(dest => dest.TipoValor, opt => opt.MapFrom(src => src.Feature.TipoValor.ToString()))
                .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Feature.Categoria))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => ParseMetadata(src.Metadata)));

            CreateMap<EmpresaFeature, PublicFeatureDto>()
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Feature.Codigo))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => ParseMetadata(src.Metadata)));
        }

        private Dictionary<string, object>? ParseMetadata(string? metadataJson)
        {
            if (string.IsNullOrEmpty(metadataJson))
                return null;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                return JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson, options);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}