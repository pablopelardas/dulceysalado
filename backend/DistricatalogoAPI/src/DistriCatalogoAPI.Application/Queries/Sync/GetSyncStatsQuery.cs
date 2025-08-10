using System;
using System.Collections.Generic;
using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Sync
{
    public class GetSyncStatsQuery : IRequest<GetSyncStatsResult>
    {
        public int Days { get; set; } = 30;
        public int EmpresaPrincipalId { get; set; } // Se establece desde el JWT
    }

    public class GetSyncStatsResult
    {
        public string Empresa { get; set; }
        public int PeriodoDias { get; set; }
        public SyncStatistics Estadisticas { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class SyncStatistics
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
        public double ProductosPorSegundoPromedio { get; set; }
        public PerformanceMetrics Performance { get; set; } = new();
        public List<DailyStats> EstadisticasDiarias { get; set; } = new();
    }

    public class PerformanceMetrics
    {
        public int TiempoMinimoMs { get; set; }
        public int TiempoMaximoMs { get; set; }
        public int SessionesConAdvertencias { get; set; }
        public double ProductivividadPromedio { get; set; }
    }

    public class DailyStats
    {
        public DateTime Fecha { get; set; }
        public int Syncs { get; set; }
        public int Productos { get; set; }
        public int Errores { get; set; }
        public double TiempoPromedioMs { get; set; }
    }
}