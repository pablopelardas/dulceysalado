using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly DistricatalogoContext _context;

        public ClienteRepository(DistricatalogoContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _context.Clientes.Where(c => c.Id == id);
            
            if (!includeDeleted)
                query = query.Where(c => c.Activo);
                
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Cliente?> GetByCodigoAsync(int empresaId, string codigo)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && 
                                         c.Codigo == codigo && 
                                         c.Activo);
        }

        public async Task<Cliente?> GetByUsernameAsync(int empresaId, string username)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && 
                                         c.Username == username && 
                                         c.Activo);
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync(int empresaId)
        {
            return await _context.Clientes
                                .Where(c => c.EmpresaId == empresaId && c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Cliente> AddAsync(Cliente cliente)
        {
            cliente.CreatedAt = DateTime.UtcNow;
            cliente.UpdatedAt = DateTime.UtcNow;
            
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task UpdateAsync(Cliente cliente)
        {
            cliente.UpdatedAt = DateTime.UtcNow;
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Cliente cliente)
        {
            cliente.SoftDelete();
            await UpdateAsync(cliente);
        }

        public async Task<bool> ExistsAsync(int empresaId, string codigo)
        {
            return await _context.Clientes
                .AnyAsync(c => c.EmpresaId == empresaId && 
                              c.Codigo == codigo && 
                              c.Activo);
        }

        public async Task<(IEnumerable<Cliente> Items, int TotalCount)> GetPaginatedAsync(
            int empresaId,
            string? codigo = null,
            string? nombre = null,
            string? cuit = null,
            string? localidad = null,
            bool? tieneAcceso = null,
            bool includeDeleted = false,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Clientes
                .Where(c => c.EmpresaId == empresaId);
                
            if (!includeDeleted)
                query = query.Where(c => c.Activo);

            // Filtros
            if (!string.IsNullOrWhiteSpace(codigo))
                query = query.Where(c => c.Codigo.Contains(codigo));

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(c => c.Nombre != null && c.Nombre.Contains(nombre));

            if (!string.IsNullOrWhiteSpace(cuit))
                query = query.Where(c => c.Cuit != null && c.Cuit.Contains(cuit));

            if (!string.IsNullOrWhiteSpace(localidad))
                query = query.Where(c => c.Localidad != null && c.Localidad.Contains(localidad));

            if (tieneAcceso.HasValue)
            {
                if (tieneAcceso.Value)
                    query = query.Where(c => !string.IsNullOrEmpty(c.Username) && 
                                           !string.IsNullOrEmpty(c.PasswordHash) && 
                                           c.IsActive);
                else
                    query = query.Where(c => string.IsNullOrEmpty(c.Username) || 
                                           string.IsNullOrEmpty(c.PasswordHash) || 
                                           !c.IsActive);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Nombre ?? c.Codigo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Cliente>> GetByCodigosAsync(int empresaId, IEnumerable<string> codigos)
        {
            return await _context.Clientes
                                .Where(c => c.EmpresaId == empresaId && 
                           codigos.Contains(c.Codigo) && 
                           c.Activo)
                .ToListAsync();
        }

        public async Task BulkUpdateAsync(IEnumerable<Cliente> clientes)
        {
            foreach (var cliente in clientes)
            {
                cliente.UpdatedAt = DateTime.UtcNow;
                _context.Clientes.Update(cliente);
            }
            await _context.SaveChangesAsync();
        }

        public async Task BulkInsertAsync(IEnumerable<Cliente> clientes)
        {
            var now = DateTime.UtcNow;
            foreach (var cliente in clientes)
            {
                cliente.CreatedAt = now;
                cliente.UpdatedAt = now;
            }
            
            _context.Clientes.AddRange(clientes);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, Cliente>> GetClientesDictionaryAsync(int empresaId)
        {
            var clientes = await _context.Clientes
                                .Where(c => c.EmpresaId == empresaId && c.Activo)
                .ToListAsync();

            return clientes.ToDictionary(c => c.Codigo, c => c);
        }

        public async Task<int> GetCountAsync(int empresaId)
        {
            return await _context.Clientes
                .CountAsync(c => c.EmpresaId == empresaId && c.Activo);
        }

        public async Task<Cliente?> GetActiveByUsernameAsync(int empresaId, string username)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && 
                                         c.Username == username && 
                                         c.IsActive && 
                                         c.Activo);
        }

        public async Task UpdateLastLoginAsync(int clienteId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente != null)
            {
                cliente.RecordLogin();
                await _context.SaveChangesAsync();
            }
        }

        // Refresh Tokens
        public async Task<ClienteRefreshToken> AddRefreshTokenAsync(ClienteRefreshToken token)
        {
            _context.ClienteRefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<ClienteRefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.ClienteRefreshTokens
                .Include(t => t.Cliente)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task DeleteRefreshTokenAsync(ClienteRefreshToken token)
        {
            _context.ClienteRefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredRefreshTokensAsync()
        {
            var expiredTokens = await _context.ClienteRefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.ClienteRefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        // Historial de logins
        public async Task AddLoginHistoryAsync(ClienteLoginHistory history)
        {
            _context.ClienteLoginHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ClienteLoginHistory>> GetLoginHistoryAsync(int clienteId, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            return await _context.ClienteLoginHistories
                .Where(h => h.ClienteId == clienteId && h.LoginAt >= startDate)
                .OrderByDescending(h => h.LoginAt)
                .Take(100) // Limit for performance
                .ToListAsync();
        }

        // Sesiones de sincronización
        public async Task<CustomerSyncSession> CreateSyncSessionAsync(CustomerSyncSession session)
        {
            _context.CustomerSyncSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<CustomerSyncSession?> GetSyncSessionAsync(string sessionId)
        {
            return await _context.CustomerSyncSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task UpdateSyncSessionAsync(CustomerSyncSession session)
        {
            _context.CustomerSyncSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerSyncSession>> GetRecentSyncSessionsAsync(int empresaId, int days = 7)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            return await _context.CustomerSyncSessions
                .Where(s => s.EmpresaId == empresaId && s.StartedAt >= startDate)
                .OrderByDescending(s => s.StartedAt)
                .Take(50) // Limit for performance
                .ToListAsync();
        }
    }
}