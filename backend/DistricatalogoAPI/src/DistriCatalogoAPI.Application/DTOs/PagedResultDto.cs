using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class PagedResultDto<T>
    {
        [JsonPropertyName("usuarios")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<T> Usuarios { get; set; }
        
        [JsonPropertyName("empresas")]  
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<T> Empresas { get; set; }
        
        [JsonPropertyName("productos")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<T> Productos { get; set; }
        
        [JsonPropertyName("categorias")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<T> Categorias { get; set; }
        
        [JsonPropertyName("novedades")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<T> Novedades { get; set; }
        
        [JsonPropertyName("ofertas")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<T> Ofertas { get; set; }
        
        [JsonPropertyName("clientes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<T> Clientes { get; set; }
        
        public PaginationDto Pagination { get; set; }
        
        // Helper method to set the appropriate collection
        public void SetItems(IEnumerable<T> items, string resourceType)
        {
            switch (resourceType.ToLower())
            {
                case "usuarios":
                    Usuarios = items;
                    break;
                case "empresas":
                    Empresas = items;
                    break;
                case "productos":
                    Productos = items;
                    break;
                case "categorias":
                    Categorias = items;
                    break;
                case "novedades":
                    Novedades = items;
                    break;
                case "ofertas":
                    Ofertas = items;
                    break;
                case "clientes":
                    Clientes = items;
                    break;
                default:
                    Usuarios = items; // Default fallback
                    break;
            }
        }
    }

    public class PaginationDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int Pages => (int)Math.Ceiling(Total / (double)Limit);
    }
}