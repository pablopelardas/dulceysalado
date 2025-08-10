using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using System;
using System.Text.Json;

namespace DistriCatalogoAPI.Application.Mappings
{
    public class CompanyMappingProfile : Profile
    {
        public CompanyMappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(dest => dest.ColoresTema, opt => opt.MapFrom(src => MapColoresTema(src.ColoresTema)))
                .ForMember(dest => dest.ListaPrecioPredeterminadaNombre, opt => opt.Ignore()) // Se mapea manualmente en el handler
                .ForMember(dest => dest.Features, opt => opt.Ignore()); // Se mapea manualmente con el FeatureRepository

            CreateMap<Company, CompanyBasicDto>();
        }

        private ThemeColorsDto MapColoresTema(string coloresJson)
        {
            if (string.IsNullOrEmpty(coloresJson))
                return new ThemeColorsDto { Acento = null, Primario = null, Secundario = null };

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null // No usar CamelCase policy para la deserialización
                };
                
                return JsonSerializer.Deserialize<ThemeColorsDto>(coloresJson, options);
            }
            catch (Exception)
            {
                // Si falla el parsing, devolver objeto vacío en lugar de null
                return new ThemeColorsDto { Acento = null, Primario = null, Secundario = null };
            }
        }
    }
}