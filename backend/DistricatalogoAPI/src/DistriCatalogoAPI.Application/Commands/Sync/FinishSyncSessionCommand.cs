using System;
using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Sync
{
    public class FinishSyncSessionCommand : IRequest<FinishSyncSessionResult>
    {
        public Guid SessionId { get; set; }
        public string Estado { get; set; } = "completada";
        public int EmpresaPrincipalId { get; set; } // Se establece desde el JWT
    }

    public class FinishSyncSessionResult
    {
        public Guid SessionId { get; set; }
        public string EstadoFinal { get; set; } = string.Empty;
        public SessionSummary Resumen { get; set; } = new();
        public DateTime FechaFin { get; set; }
    }

    public class SessionSummary
    {
        public int ProductosTotales { get; set; }
        public int ProductosActualizados { get; set; }
        public int ProductosNuevos { get; set; }
        public int ProductosErrores { get; set; }
        public int LotesProcesados { get; set; }
        public int TiempoTotalMs { get; set; }
        public double ProductosPorSegundo { get; set; }
        public double TasaExito { get; set; }
    }
}