using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Categories
{
    public class UpdateCategoryBaseCommand : IRequest<UpdateCategoryBaseCommandResult>
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Icono { get; set; }
        public bool? Visible { get; set; }
        public int? Orden { get; set; }
        public string? Color { get; set; }
        public string? Descripcion { get; set; }
    }

    public class UpdateCategoryBaseCommandResult
    {
        public int Id { get; set; }
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public int Orden { get; set; }
        public string Color { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}