namespace DistriCatalogoAPI.Api.DTOs;

public class CreateProductoEmpresaDto
{
    public int EmpresaId { get; set; }
    public int Codigo { get; set; }
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
}

public class UpdateProductoEmpresaDto
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

public class ProductoEmpresaResponseDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string? EmpresaNombre { get; set; }
    public int Codigo { get; set; }
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
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductosEmpresaListResponseDto
{
    public List<ProductoEmpresaResponseDto> Productos { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}