using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ProductosBase
{
    public class GetProductoBaseByCodigoQuery : IRequest<GetProductoBaseByCodigoQueryResult?>
    {
        public string Codigo { get; set; } = "";
    }

    public class GetProductoBaseByCodigoQueryResult
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int? CodigoRubro { get; set; }
        public decimal? Precio { get; set; }
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
    }
}