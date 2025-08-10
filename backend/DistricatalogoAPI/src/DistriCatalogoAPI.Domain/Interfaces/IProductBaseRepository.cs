using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IProductBaseRepository
    {
        Task<ProductBase?> GetByIdAsync(int id);
        Task<ProductBase?> GetByCodigoAsync(string codigo);
        Task<ProductBase?> GetByCodigoAsync(string codigo, int empresaId);
        Task<List<ProductBase>> GetByCodigosAsync(List<string> codigos, int empresaId);
        Task<(List<ProductBase> products, int total)> GetPagedAsync(
            bool? visible,
            bool? destacado,
            int? codigoRubro,
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy = null,
            string? sortOrder = null,
            int? listaPrecioIdForSorting = null,
            bool incluirSinExistencia = false,
            bool? soloSinConfiguracion = null);
        Task<ProductBase> CreateAsync(ProductBase product);
        Task UpdateAsync(ProductBase product);
        Task UpdateFromSyncAsync(ProductBase product);
        Task DeleteAsync(ProductBase product);
        Task<BulkOperationResult> BulkCreateOrUpdateAsync(List<ProductBase> products);
        Task<bool> ExistsByCodigoAsync(string codigo, int empresaId);
        Task<bool> ExistsByCodigoInPrincipalCompanyAsync(string codigo, int empresaPrincipalId);
        Task<int> GetCountByEmpresaAsync(int empresaId);
    }

    public class BulkOperationResult
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Failed { get; set; }
        public List<BulkOperationError> Errors { get; set; } = new();
    }

    public class BulkOperationError
    {
        public string ProductCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
    }
}