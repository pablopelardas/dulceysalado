namespace DistriCatalogoAPI.Api.DTOs;

public class CreateProductoBaseDto
{
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int? CodigoRubro { get; set; }
    public decimal? Precio { get; set; }
    public decimal? Existencia { get; set; }
    public bool? Visible { get; set; } = true;
    public bool? Destacado { get; set; } = false;
    public int? OrdenCategoria { get; set; }
    public string? ImagenUrl { get; set; }
    public string? ImagenAlt { get; set; }
    public string? DescripcionCorta { get; set; }
    public string? DescripcionLarga { get; set; }
    public string? Tags { get; set; }
    public string? CodigoBarras { get; set; }
    public string? Marca { get; set; }
    public string? UnidadMedida { get; set; } = "UN";
    // Removido: AdministradoPorEmpresaId - se toma del usuario autenticado
}

public class UpdateProductoBaseDto
{
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
}

public class ProductoBaseResponseDto
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

public class ProductosBaseListResponseDto
{
    public List<ProductoBaseResponseDto> Productos { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}