using System;
using System.Collections.Generic;
using System.Linq;
using DistriCatalogoAPI.Domain.Common;
using DistriCatalogoAPI.Domain.ValueObjects;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class SyncSession : BaseEntity
    {
        public Guid Id { get; private set; }
        public int EmpresaPrincipalId { get; private set; }
        public int? ListaPrecioId { get; private set; }
        public SessionState Estado { get; private set; }
        public int TotalLotesEsperados { get; private set; }
        public int LotesProcesados { get; private set; }
        public int ProductosTotales { get; private set; }
        public int ProductosActualizados { get; private set; }
        public int ProductosNuevos { get; private set; }
        public int ProductosErrores { get; private set; }
        public List<ProductError> ErroresDetalle { get; private set; }
        public SyncMetrics Metricas { get; private set; }
        public DateTime FechaInicio { get; private set; }
        public DateTime? FechaFin { get; private set; }
        public int? TiempoTotalMs { get; private set; }
        public string UsuarioProceso { get; private set; }
        public string IpOrigen { get; private set; }

        // Navigation
        public virtual Company EmpresaPrincipal { get; private set; }

        protected SyncSession() 
        {
            ErroresDetalle = new List<ProductError>();
            Metricas = new SyncMetrics();
        }

        public SyncSession(
            int empresaPrincipalId, 
            int totalLotesEsperados, 
            string usuarioProceso, 
            string ipOrigen,
            int? listaPrecioId = null) : this()
        {
            Id = Guid.NewGuid();
            EmpresaPrincipalId = empresaPrincipalId;
            ListaPrecioId = listaPrecioId;
            Estado = SessionState.Iniciada;
            TotalLotesEsperados = totalLotesEsperados > 0 ? totalLotesEsperados : 1;
            LotesProcesados = 0;
            ProductosTotales = 0;
            ProductosActualizados = 0;
            ProductosNuevos = 0;
            ProductosErrores = 0;
            FechaInicio = DateTime.UtcNow;
            UsuarioProceso = usuarioProceso ?? "SISTEMA";
            IpOrigen = ipOrigen ?? "Unknown";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IniciarProcesamiento()
        {
            if (Estado != SessionState.Iniciada)
                throw new InvalidOperationException($"No se puede iniciar procesamiento en estado {Estado}");

            Estado = SessionState.Procesando;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ActualizarEstadisticas(BatchResult resultado)
        {
            if (!CanProcessBatch())
                throw new InvalidOperationException($"No se puede procesar lotes en estado {Estado}");

            LotesProcesados++;
            ProductosTotales += resultado.ProductosProcesados;
            ProductosActualizados += resultado.ProductosActualizados;
            ProductosNuevos += resultado.ProductosNuevos;
            ProductosErrores += resultado.Errores;

            if (resultado.ErroresDetalle?.Any() == true)
            {
                ErroresDetalle.AddRange(resultado.ErroresDetalle);
            }

            // Actualizar métricas
            Metricas.AgregarTiempoProcesamiento(resultado.TiempoProcesamientoMs);
            Metricas.TotalProductosProcesados = ProductosTotales;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Finalizar(string estadoFinal = "completada")
        {
            if (!CanFinish())
                throw new InvalidOperationException($"No se puede finalizar sesión en estado {Estado}");

            Estado = estadoFinal.ToLower() switch
            {
                "completada" => SessionState.Completada,
                "error" => SessionState.Error,
                "cancelada" => SessionState.Cancelada,
                _ => SessionState.Completada
            };

            FechaFin = DateTime.UtcNow;
            TiempoTotalMs = (int)(FechaFin.Value - FechaInicio).TotalMilliseconds;
            UpdatedAt = DateTime.UtcNow;
        }

        public ProgressInfo GetProgreso()
        {
            var porcentaje = TotalLotesEsperados > 0 
                ? (LotesProcesados * 100.0 / TotalLotesEsperados) 
                : 0;

            return new ProgressInfo
            {
                Porcentaje = Math.Min(100, porcentaje),
                LotesProcesados = LotesProcesados,
                TotalLotesEsperados = TotalLotesEsperados,
                ProductosProcesados = ProductosTotales,
                Estado = Estado.ToString()
            };
        }

        public bool CanProcessBatch()
        {
            return Estado == SessionState.Iniciada || Estado == SessionState.Procesando;
        }

        public bool CanFinish()
        {
            return Estado == SessionState.Iniciada || Estado == SessionState.Procesando;
        }

        public bool IsActive()
        {
            return Estado == SessionState.Iniciada || Estado == SessionState.Procesando;
        }

        public bool HasErrors()
        {
            return ProductosErrores > 0 || ErroresDetalle.Any();
        }

        // Value Objects y clases auxiliares
        public class BatchResult
        {
            public int ProductosProcesados { get; set; }
            public int ProductosActualizados { get; set; }
            public int ProductosNuevos { get; set; }
            public int Errores { get; set; }
            public int TiempoProcesamientoMs { get; set; }
            public List<ProductError> ErroresDetalle { get; set; } = new();
        }

        public class ProductError
        {
            public string ProductoCodigo { get; set; }
            public string ProductoDescripcion { get; set; }
            public int? CategoriaId { get; set; }
            public string ErrorTipo { get; set; }
            public string ErrorMensaje { get; set; }
            public int? IndiceEnLote { get; set; }
        }

        public class ProgressInfo
        {
            public double Porcentaje { get; set; }
            public int LotesProcesados { get; set; }
            public int TotalLotesEsperados { get; set; }
            public int ProductosProcesados { get; set; }
            public string Estado { get; set; }
        }
    }
}