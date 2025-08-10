using System;
using System.Collections.Generic;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class SyncLog : BaseEntity
    {
        public int Id { get; private set; }
        public int EmpresaPrincipalId { get; private set; }
        public string ArchivoNombre { get; private set; }
        public DateTime FechaProcesamiento { get; private set; }
        public int ProductosActualizados { get; private set; }
        public int ProductosNuevos { get; private set; }
        public int Errores { get; private set; }
        public int TiempoProcesamientoMs { get; private set; }
        public string Estado { get; private set; }
        public List<ErrorDetail> DetallesErrores { get; private set; }
        public string UsuarioProceso { get; private set; } = "SISTEMA";

        // Navigation
        public virtual Company EmpresaPrincipal { get; private set; }

        protected SyncLog()
        {
            DetallesErrores = new List<ErrorDetail>();
        }

        public SyncLog(
            int empresaPrincipalId,
            string archivoNombre,
            int productosActualizados,
            int productosNuevos,
            int errores,
            int tiempoProcesamientoMs,
            string usuarioProceso,
            List<ErrorDetail> detallesErrores = null) : this()
        {
            if (string.IsNullOrWhiteSpace(archivoNombre))
                throw new ArgumentException("El nombre del archivo es requerido");

            if (tiempoProcesamientoMs < 0)
                throw new ArgumentException("El tiempo de procesamiento no puede ser negativo");

            EmpresaPrincipalId = empresaPrincipalId;
            ArchivoNombre = archivoNombre;
            FechaProcesamiento = DateTime.UtcNow;
            ProductosActualizados = Math.Max(0, productosActualizados);
            ProductosNuevos = Math.Max(0, productosNuevos);
            Errores = Math.Max(0, errores);
            TiempoProcesamientoMs = tiempoProcesamientoMs;
            UsuarioProceso = usuarioProceso ?? "SISTEMA";
            
            // Determinar estado basado en resultados
            Estado = DeterminarEstado(errores, productosActualizados + productosNuevos);
            
            if (detallesErrores?.Count > 0)
            {
                DetallesErrores = detallesErrores;
            }

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public static SyncLog CreateFromSession(SyncSession session)
        {
            var detallesErrores = new List<ErrorDetail>();
            
            if (session.HasErrors())
            {
                foreach (var error in session.ErroresDetalle)
                {
                    detallesErrores.Add(new ErrorDetail
                    {
                        ProductoCodigo = error.ProductoCodigo,
                        ProductoDescripcion = error.ProductoDescripcion,
                        TipoError = error.ErrorTipo,
                        Mensaje = error.ErrorMensaje
                    });
                }
            }

            return new SyncLog(
                session.EmpresaPrincipalId,
                $"Sesión Bulk {session.Id}",
                session.ProductosActualizados,
                session.ProductosNuevos,
                session.ProductosErrores,
                session.TiempoTotalMs ?? 0,
                session.UsuarioProceso,
                detallesErrores
            );
        }

        private string DeterminarEstado(int errores, int productosProcesados)
        {
            if (productosProcesados == 0)
                return "fallido";
            
            if (errores == 0)
                return "exitoso";
            
            // Si hay algunos errores pero también productos procesados
            return "con_errores";
        }

        public int GetTotalProductosProcesados()
        {
            return ProductosActualizados + ProductosNuevos;
        }

        public double GetTasaExito()
        {
            var total = GetTotalProductosProcesados() + Errores;
            return total > 0 ? (GetTotalProductosProcesados() * 100.0 / total) : 0;
        }

        public double GetProductosPorSegundo()
        {
            return TiempoProcesamientoMs > 0 
                ? GetTotalProductosProcesados() / (TiempoProcesamientoMs / 1000.0) 
                : 0;
        }

        public class ErrorDetail
        {
            public string ProductoCodigo { get; set; }
            public string ProductoDescripcion { get; set; }
            public string TipoError { get; set; }
            public string Mensaje { get; set; }
        }
    }

}