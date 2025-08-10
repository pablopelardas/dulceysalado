using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ISyncSessionRepository
    {
        Task<SyncSession?> GetByIdAsync(Guid id);
        Task<SyncSession?> GetByIdWithCompanyAsync(Guid id);
        Task<List<SyncSession>> GetByCompanyAsync(int empresaId, int page, int pageSize, string? estado = null);
        Task<int> GetCountByCompanyAsync(int empresaId, string? estado = null);
        Task<SyncSession> CreateAsync(SyncSession session);
        Task UpdateAsync(SyncSession session);
        Task<int> CleanupOldSessionsAsync(int diasAntiguedad);
        Task<List<SyncSession>> GetActiveSessionsByCompanyAsync(int empresaId);
        Task<bool> ExistsActiveSessionAsync(int empresaId);
    }
}