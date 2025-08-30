using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        // Operaciones básicas
        Task<Pedido?> GetByIdAsync(int id, bool includeItems = true);
        Task<Pedido?> GetByIdWithItemsAsync(int id); // Para correcciones
        Task<Pedido?> GetByNumeroAsync(string numero, bool includeItems = true);
        Task<IEnumerable<Pedido>> GetAllAsync(int empresaId, bool includeItems = false);
        Task<Pedido> AddAsync(Pedido pedido);
        Task UpdateAsync(Pedido pedido);
        Task DeleteAsync(Pedido pedido);
        
        // Consultas específicas para clientes
        Task<IEnumerable<Pedido>> GetByClienteIdAsync(int clienteId, int empresaId, bool includeItems = false);
        Task<(IEnumerable<Pedido> Items, int TotalCount)> GetPaginatedByClienteAsync(
            int clienteId,
            int empresaId,
            int page = 1,
            int pageSize = 20,
            PedidoEstado? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);
        
        // Consultas para backoffice
        Task<(IEnumerable<Pedido> Items, int TotalCount)> GetPaginatedAsync(
            int empresaId,
            int page = 1,
            int pageSize = 20,
            PedidoEstado? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? clienteId = null,
            string? numeroContiene = null);
        
        // Estadísticas
        Task<int> GetCountByEstadoAsync(int empresaId, PedidoEstado estado);
        Task<decimal> GetMontoTotalByEstadoAsync(int empresaId, PedidoEstado estado, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<IEnumerable<Pedido>> GetPendientesAsync(int empresaId);
        Task<IEnumerable<Pedido>> GetRecentesAsync(int empresaId, int dias = 7);
        
        // Validaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExisteNumeroAsync(string numero);
        Task<string> GenerateUniqueNumeroAsync();
        
        // Gestión de estado
        Task<bool> CambiarEstadoAsync(int pedidoId, PedidoEstado nuevoEstado, int usuarioId, string? motivo = null);
        
        // Items de pedido
        Task<PedidoItem?> GetItemByIdAsync(int itemId);
        Task<IEnumerable<PedidoItem>> GetItemsByPedidoIdAsync(int pedidoId);
        Task AddItemAsync(PedidoItem item);
        Task UpdateItemAsync(PedidoItem item);
        Task DeleteItemAsync(PedidoItem item);
    }
}