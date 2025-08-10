using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductConfig
{
    public class UpdateProductWebConfigCommand : IRequest<UpdateProductWebConfigCommandResult>
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public bool? Visible { get; set; }
        public bool? Destacado { get; set; }
        public int? OrdenCategoria { get; set; }
        public string? DescripcionCorta { get; set; }
        public string? DescripcionLarga { get; set; }
        public List<string>? Tags { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
    }

    public class UpdateProductWebConfigCommandResult
    {
        public int Id { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public bool Visible { get; set; }
        public bool Destacado { get; set; }
        public int OrdenCategoria { get; set; }
        public string? DescripcionCorta { get; set; }
        public string? DescripcionLarga { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }
}