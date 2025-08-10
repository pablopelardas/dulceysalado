using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IProductoEmpresaRepository
    {
        Task<ProductoEmpresa?> GetByIdAsync(int id);
        Task<(List<ProductoEmpresa> products, int total)> GetPagedByEmpresaAsync(
            int? empresaId,
            bool? visible,
            bool? destacado,
            int? codigoRubro,
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy = null,
            string? sortOrder = null,
            int? listaPrecioIdForSorting = null,
            bool incluirSinExistencia = false);
        Task<ProductoEmpresa> CreateAsync(ProductoEmpresa product);
        Task UpdateAsync(ProductoEmpresa product);
        Task DeleteAsync(ProductoEmpresa product);
        Task<bool> ExistsByCodigoAsync(string codigo, int empresaId);
        Task<bool> ExistsByCodigoInPrincipalCompanyAsync(string codigo, int empresaPrincipalId);
    }
}