using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IClienteRepository
    {
        // Operaciones básicas
        Task<Cliente?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<Cliente?> GetByCodigoAsync(int empresaId, string codigo);
        Task<Cliente?> GetByUsernameAsync(int empresaId, string username);
        Task<IEnumerable<Cliente>> GetAllAsync(int empresaId);
        Task<Cliente> AddAsync(Cliente cliente);
        Task UpdateAsync(Cliente cliente);
        Task DeleteAsync(Cliente cliente);
        Task<bool> ExistsAsync(int empresaId, string codigo);
        
        // Búsquedas y filtros
        Task<(IEnumerable<Cliente> Items, int TotalCount)> GetPaginatedAsync(
            int empresaId,
            string? codigo = null,
            string? nombre = null,
            string? cuit = null,
            string? localidad = null,
            bool? tieneAcceso = null,
            bool includeDeleted = false,
            int page = 1,
            int pageSize = 20);
        
        // Operaciones en lote
        Task<IEnumerable<Cliente>> GetByCodigosAsync(int empresaId, IEnumerable<string> codigos);
        Task BulkUpdateAsync(IEnumerable<Cliente> clientes);
        Task BulkInsertAsync(IEnumerable<Cliente> clientes);
        
        // Sincronización
        Task<Dictionary<string, Cliente>> GetClientesDictionaryAsync(int empresaId);
        Task<int> GetCountAsync(int empresaId);
        
        // Autenticación
        Task<Cliente?> GetActiveByUsernameAsync(int empresaId, string username);
        Task UpdateLastLoginAsync(int clienteId);
        
        // Refresh Tokens
        Task<ClienteRefreshToken> AddRefreshTokenAsync(ClienteRefreshToken token);
        Task<ClienteRefreshToken?> GetRefreshTokenAsync(string token);
        Task DeleteRefreshTokenAsync(ClienteRefreshToken token);
        Task DeleteExpiredRefreshTokensAsync();
        
        // Historial de logins
        Task AddLoginHistoryAsync(ClienteLoginHistory history);
        Task<IEnumerable<ClienteLoginHistory>> GetLoginHistoryAsync(int clienteId, int days = 30);
        
        // Sesiones de sincronización
        Task<CustomerSyncSession> CreateSyncSessionAsync(CustomerSyncSession session);
        Task<CustomerSyncSession?> GetSyncSessionAsync(string sessionId);
        Task UpdateSyncSessionAsync(CustomerSyncSession session);
        Task<IEnumerable<CustomerSyncSession>> GetRecentSyncSessionsAsync(int empresaId, int days = 7);
    }
}