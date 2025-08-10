using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosBase
{
    public class UpdateProductoBaseCommand : IRequest<UpdateProductoBaseCommandResult>
    {
        public int Id { get; set; }
        public string? Descripcion { get; set; }
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
        /// <summary>
        /// ID de la empresa para actualizar stock específico. Si no se proporciona, afecta la empresa principal
        /// </summary>
        public int? EmpresaId { get; set; }
    }

    public class UpdateProductoBaseCommandResult
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