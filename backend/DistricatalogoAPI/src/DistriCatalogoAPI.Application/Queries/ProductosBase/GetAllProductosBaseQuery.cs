using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ProductosBase
{
    public class GetAllProductosBaseQuery : IRequest<GetAllProductosBaseQueryResult>
    {
        public bool? Visible { get; set; }
        public bool? Destacado { get; set; }
        public int? CodigoRubro { get; set; }
        public string? Busqueda { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int? ListaPrecioId { get; set; }
        public bool? SoloSinConfiguracion { get; set; }
        public bool IncluirSinExistencia { get; set; } = false; // Por defecto false para filtrar solo productos con existencia > 0
        public int? EmpresaId { get; set; } // Nuevo: Para filtrar stock por empresa específica
    }

    public class GetAllProductosBaseQueryResult
    {
        public List<ProductoBaseDto> Productos { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public ListaPrecioInfo? ListaSeleccionada { get; set; }
        public List<ListaPrecioInfo> ListasDisponibles { get; set; } = new();
    }

    public class ProductoBaseDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int? CodigoRubro { get; set; }
        public decimal? Existencia { get; set; }
        public bool? Visible { get; set; }
        public bool? Destacado { get; set; }
        public int? OrdenCategoria { get; set; }
        public string? ImagenUrl { get; set; }
        public string? ImagenAlt { get; set; }
        public string? DescripcionCorta { get; set; }
        public string? DescripcionLarga { get; set; }
        public string? Tags { get; set; }
        public string? CodigoBarras { get; set; }
        public string? Marca { get; set; }
        public string? UnidadMedida { get; set; }
        public int AdministradoPorEmpresaId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal? PrecioSeleccionado { get; set; }
        public List<string> ConfiguracionesFaltantes { get; set; } = new();
    }

    public class ListaPrecioInfo
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public bool EsPredeterminada { get; set; }
    }
}