using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ICatalogRepository
    {
        Task<(List<CatalogProduct> products, int totalCount)> GetCatalogProductsAsync(int empresaId, string? listaPrecioCodigo = null, bool? destacados = null, int? codigoRubro = null, string? busqueda = null, string? ordenarPor = null, int page = 1, int pageSize = 20);
        Task<List<CatalogProduct>> GetFeaturedProductsAsync(int empresaId, string? listaPrecioCodigo = null, int limit = 10);
        Task<CatalogProduct?> GetProductDetailsAsync(string productoCodigo, int empresaId, string? listaPrecioCodigo = null);
        Task<(List<CatalogProduct> products, int totalCount)> SearchCatalogAsync(int empresaId, string? texto = null, List<int>? codigosRubro = null, decimal? precioMinimo = null, decimal? precioMaximo = null, bool? soloDestacados = null, List<string>? tags = null, string? marca = null, string? orderBy = null, bool ascending = true, int page = 1, int pageSize = 20);
        Task<List<UnconfiguredProduct>> GetUnconfiguredProductsAsync(int empresaId, int page = 1, int pageSize = 20);
        Task<List<CategoryProductCount>> GetCategoriesWithProductCountAsync(int empresaId);
        Task<(List<CatalogProduct> products, int totalCount)> GetCatalogProductsByCategoryAsync(int empresaId, int codigoRubro, string? orderBy = null, bool ascending = true, int page = 1, int pageSize = 20);
        Task<List<CategoryProductCount>> GetCategoriesFromFilteredProductsAsync(int empresaId, string? listaPrecioCodigo = null, bool? destacados = null, int? codigoRubro = null, string? busqueda = null);
        Task<List<CatalogProduct>> GetProductsByAgrupacionIdsAsync(List<int> agrupacionIds, int empresaId, string? listaPrecioCodigo = null);
    }

    public class UnconfiguredProduct
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool TieneImagen { get; set; }
        public bool TieneDescripcionCorta { get; set; }
        public bool TieneDescripcionLarga { get; set; }
        public bool TieneTags { get; set; }
        public bool EsVisible { get; set; }
        public bool EsDestacado { get; set; }
        public List<string> ConfiguracionFaltante { get; set; } = new();
    }

    public class CategoryProductCount
    {
        public int CategoryId { get; set; }
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Icono { get; set; }
        public string? Color { get; set; }
        public int Orden { get; set; }
        public int ProductCount { get; set; }
    }
}