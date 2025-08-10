using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IListaPrecioRepository
    {
        Task<int?> GetIdByCodigoAsync(string codigo);
        Task<bool> ExistsAndActiveAsync(string codigo);
        Task<(string codigo, string nombre)?> GetCodigoAndNombreByIdAsync(int id);
        Task<List<ListaPrecioDto>> GetAllActiveAsync();
        Task<ListaPrecioDto?> GetByIdAsync(int id);
        Task<bool> TienePreciosAsociadosAsync(int id);
        Task DeleteAsync(int id);
        Task<int> CreateAsync(string codigo, string nombre, string? descripcion, int? orden);
        Task<bool> UpdateAsync(int id, string? codigo, string? nombre, string? descripcion, bool? activa, int? orden);
        Task<bool> CodigoExistsAsync(string codigo);
        Task<bool> IsDefaultListAsync(int id);
        Task<int?> GetDefaultListIdAsync();
        Task SetDefaultAsync(int id);
    }

    public class ListaPrecioDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public bool EsPredeterminada { get; set; }
        public bool Activa { get; set; }
    }
}