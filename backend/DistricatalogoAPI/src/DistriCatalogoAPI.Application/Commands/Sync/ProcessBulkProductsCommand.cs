using System;
using System.Collections.Generic;
using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Sync
{
    public class ProcessBulkProductsCommand : IRequest<ProcessBulkProductsResult>
    {
        public Guid SessionId { get; set; }
        public int LoteNumero { get; set; }
        public List<BulkProductDto> Productos { get; set; } = new();
        public int EmpresaPrincipalId { get; set; } // Se establece desde el JWT
        public bool StockOnlyMode { get; set; } = false; // Flag para actualizar solo stock
    }

    public class BulkProductDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int? CategoriaId { get; set; }
        public decimal? Precio { get; set; } // Precio para la lista actual de la sesión (opcional para compatibilidad)
        public List<ProductPriceDto>? ListasPrecios { get; set; } // NUEVO: Array de precios para múltiples listas
        public decimal Stock { get; set; } // Mantenido para compatibilidad de estructura - no se usa para stock
        public List<StockPorEmpresaDto> StocksPorEmpresa { get; set; } = new(); // NUEVO: Stock diferenciado por empresa
        public int? Grupo1 { get; set; }
        public int? Grupo2 { get; set; }
        public int? Grupo3 { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaModi { get; set; }
        public string? Imputable { get; set; }
        public string? Disponible { get; set; }
        public string? CodigoUbicacion { get; set; }
    }

    public class ProductPriceDto
    {
        public int ListaId { get; set; }
        public decimal Precio { get; set; }
        public DateTime? Fecha { get; set; }
    }

    public class StockPorEmpresaDto
    {
        public int EmpresaId { get; set; }
        public decimal Stock { get; set; }
    }

    public class ProcessBulkProductsResult
    {
        public Guid SessionId { get; set; }
        public int LoteNumero { get; set; }
        public int TotalLotes { get; set; }
        public SyncStatistics Estadisticas { get; set; } = new();
        public int TiempoProcesamientoMs { get; set; }
        public ProgressInfo Progreso { get; set; } = new();
        public CategoriesInfo CategoriasInfo { get; set; } = new();
    }

    public class SyncStatistics
    {
        public int ProductosProcesados { get; set; }
        public int ProductosActualizados { get; set; }
        public int ProductosNuevos { get; set; }
        public int Errores { get; set; }
        public List<ProductErrorDetail> ErroresDetalle { get; set; } = new();
        public Dictionary<int, ListaPrecioStats>? EstadisticasPorLista { get; set; } // NUEVO: Estadísticas por lista
    }

    public class ListaPrecioStats
    {
        public int ListaId { get; set; }
        public string ListaNombre { get; set; } = string.Empty;
        public int PreciosActualizados { get; set; }
        public int PreciosNuevos { get; set; }
        public int Errores { get; set; }
        public List<string> ErroresDetalle { get; set; } = new();
    }

    public class ProductErrorDetail
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoDescripcion { get; set; } = string.Empty;
        public int? CategoriaIdOriginal { get; set; }
        public int? CodigoRubroAsignado { get; set; }
        public string ErrorTipo { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int? Indice { get; set; }
    }

    public class ProgressInfo
    {
        public double Porcentaje { get; set; }
        public int LotesProcesados { get; set; }
        public int TotalLotesEsperados { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class CategoriesInfo
    {
        public int CategoriasExistentesInicialmente { get; set; }
        public int CategoriasCreatesAutomaticamente { get; set; }
        public List<int> CategoriasCreatesLista { get; set; } = new();
        public int TotalCategoriasProcesadas { get; set; }
    }
}