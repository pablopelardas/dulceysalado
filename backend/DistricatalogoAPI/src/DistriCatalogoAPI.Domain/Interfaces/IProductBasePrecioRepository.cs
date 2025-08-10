using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IProductBasePrecioRepository
    {
        Task UpsertPreciosAsync(List<ProductoPrecioDto> precios);
        Task<List<ProductoPrecioConListaDto>> GetPreciosPorProductoAsync(int productoBaseId);
        Task<List<ProductoPrecioConListaDto>> GetPreciosPorProductoYListaAsync(int productoBaseId, int listaPrecioId);
        Task<Dictionary<int, decimal>> GetPreciosPorProductosYListaAsync(List<int> productosBaseIds, int listaPrecioId);
        Task<bool> UpsertPrecioAsync(int productoBaseId, int listaPrecioId, decimal precio);
        Task<bool> DeletePrecioAsync(int productoBaseId, int listaPrecioId);
    }

    public class ProductoPrecioDto
    {
        public int ProductoBaseId { get; set; }
        public int ListaPrecioId { get; set; }
        public decimal Precio { get; set; }
    }

    public class ProductoPrecioConListaDto
    {
        public int ProductoBaseId { get; set; }
        public int ListaPrecioId { get; set; }
        public string ListaPrecioCodigo { get; set; } = "";
        public string ListaPrecioNombre { get; set; } = "";
        public decimal Precio { get; set; }
    }
}