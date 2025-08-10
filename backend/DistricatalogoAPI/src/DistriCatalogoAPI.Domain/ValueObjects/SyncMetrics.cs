using System;
using System.Collections.Generic;
using System.Linq;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.ValueObjects
{
    public class SyncMetrics : ValueObject
    {
        private readonly List<int> _tiemposProcesamiento;

        public int TotalProductosProcesados { get; set; }
        public int TiempoPromedioMs => _tiemposProcesamiento.Any() 
            ? (int)_tiemposProcesamiento.Average() 
            : 0;
        public int TiempoMinimoMs => _tiemposProcesamiento.Any() 
            ? _tiemposProcesamiento.Min() 
            : 0;
        public int TiempoMaximoMs => _tiemposProcesamiento.Any() 
            ? _tiemposProcesamiento.Max() 
            : 0;
        public double ProductosPorSegundo => TotalProductosProcesados > 0 && TiempoTotalMs > 0
            ? TotalProductosProcesados / (TiempoTotalMs / 1000.0)
            : 0;

        public int TiempoTotalMs => _tiemposProcesamiento.Sum();
        public int CantidadLotes => _tiemposProcesamiento.Count;

        public SyncMetrics()
        {
            _tiemposProcesamiento = new List<int>();
            TotalProductosProcesados = 0;
        }

        public void AgregarTiempoProcesamiento(int tiempoMs)
        {
            if (tiempoMs < 0)
                throw new ArgumentException("El tiempo de procesamiento no puede ser negativo");

            _tiemposProcesamiento.Add(tiempoMs);
        }

        public bool TieneAdvertenciaPerformance(int umbralMs = 10000)
        {
            return _tiemposProcesamiento.Any(t => t > umbralMs);
        }

        public List<int> ObtenerLotesLentos(int umbralMs = 10000)
        {
            return _tiemposProcesamiento
                .Select((tiempo, indice) => new { tiempo, indice })
                .Where(x => x.tiempo > umbralMs)
                .Select(x => x.indice + 1)
                .ToList();
        }

        public Dictionary<string, object> ToJson()
        {
            return new Dictionary<string, object>
            {
                ["total_productos_procesados"] = TotalProductosProcesados,
                ["tiempo_promedio_ms"] = TiempoPromedioMs,
                ["tiempo_minimo_ms"] = TiempoMinimoMs,
                ["tiempo_maximo_ms"] = TiempoMaximoMs,
                ["tiempo_total_ms"] = TiempoTotalMs,
                ["productos_por_segundo"] = Math.Round(ProductosPorSegundo, 2),
                ["cantidad_lotes"] = CantidadLotes,
                ["lotes_lentos"] = ObtenerLotesLentos()
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return TotalProductosProcesados;
            foreach (var tiempo in _tiemposProcesamiento)
            {
                yield return tiempo;
            }
        }
    }
}