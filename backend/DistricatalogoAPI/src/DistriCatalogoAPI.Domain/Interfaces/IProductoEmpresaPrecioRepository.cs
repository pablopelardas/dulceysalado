using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IProductoEmpresaPrecioRepository
    {
        Task UpsertPreciosAsync(List<ProductoEmpresaPrecioDto> precios);
        Task<List<ProductoEmpresaPrecioConListaDto>> GetPreciosPorProductoAsync(int productoEmpresaId);
        Task<List<ProductoEmpresaPrecioConListaDto>> GetPreciosPorProductoYListaAsync(int productoEmpresaId, int listaPrecioId);
        Task<Dictionary<int, decimal?>> GetPreciosPorProductosYListaAsync(List<int> productosEmpresaIds, int listaPrecioId);
        Task<bool> UpsertPrecioAsync(int productoEmpresaId, int listaPrecioId, decimal precio);
        Task<bool> DeletePrecioAsync(int productoEmpresaId, int listaPrecioId);
    }

    public class ProductoEmpresaPrecioDto
    {
        public int ProductoEmpresaId { get; set; }
        public int ListaPrecioId { get; set; }
        public decimal Precio { get; set; }
    }

    public class ProductoEmpresaPrecioConListaDto
    {
        public int ProductoEmpresaId { get; set; }
        public int ListaPrecioId { get; set; }
        public string ListaPrecioCodigo { get; set; } = "";
        public string ListaPrecioNombre { get; set; } = "";
        public decimal Precio { get; set; }
    }
}