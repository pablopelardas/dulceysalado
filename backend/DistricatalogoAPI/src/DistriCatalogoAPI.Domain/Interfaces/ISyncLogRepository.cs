using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ISyncLogRepository
    {
        Task<SyncLog> CreateAsync(SyncLog log);
        Task<List<SyncLog>> GetByCompanyAsync(int empresaId, DateTime desde, DateTime hasta);
        Task<List<SyncLog>> GetByCompanyAsync(int empresaId, int page, int limit);
        Task<int> GetCountByCompanyAsync(int empresaId);
        Task<SyncStats> GetStatsAsync(int empresaId, int dias);
        Task<List<SyncLog>> GetRecentLogsAsync(int empresaId, int cantidad = 10);
        Task<int> GetErrorCountAsync(int empresaId, DateTime desde);
    }

    public class SyncStats
    {
        public int TotalSyncs { get; set; }
        public int ProductosTotales { get; set; }
        public int ProductosActualizados { get; set; }
        public int ProductosNuevos { get; set; }
        public int ErroresTotales { get; set; }
        public double TiempoPromedioMs { get; set; }
        public int SyncsExitosos { get; set; }
        public int SyncsConErrores { get; set; }
        public int SyncsFallidos { get; set; }
        public DateTime? UltimaSincronizacion { get; set; }
        public double TasaExitoPromedio { get; set; }
    }
}