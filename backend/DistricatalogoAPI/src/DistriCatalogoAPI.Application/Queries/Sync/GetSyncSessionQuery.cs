using System;
using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Sync
{
    public class GetSyncSessionQuery : IRequest<GetSyncSessionResult>
    {
        public Guid SessionId { get; set; }
        public int EmpresaPrincipalId { get; set; } // Se establece desde el JWT
    }

    public class GetSyncSessionResult
    {
        public Guid Id { get; set; }
        public string Estado { get; set; }
        public string Empresa { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string UsuarioProceso { get; set; }
        public SessionProgress Progreso { get; set; } = new();
        public SessionMetrics Metricas { get; set; } = new();
        public SessionErrors ErroresDetalle { get; set; } = new();
    }

    public class SessionProgress
    {
        public double Porcentaje { get; set; }
        public int LotesProcesados { get; set; }
        public int TotalLotesEsperados { get; set; }
        public int ProductosProcesados { get; set; }
        public string Estado { get; set; }
    }

    public class SessionMetrics
    {
        public int ProductosTotales { get; set; }
        public int ProductosActualizados { get; set; }
        public int ProductosNuevos { get; set; }
        public int ProductosErrores { get; set; }
        public int TiempoTotalMs { get; set; }
        public double ProductosPorSegundo { get; set; }
        public int TiempoPromedioMs { get; set; }
    }

    public class SessionErrors
    {
        public int TotalErrores { get; set; }
        public object DetallesErrores { get; set; } // JSON serializado
    }
}