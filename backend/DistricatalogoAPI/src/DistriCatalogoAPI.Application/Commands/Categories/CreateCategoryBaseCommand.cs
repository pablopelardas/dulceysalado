using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Categories
{
    public class CreateCategoryBaseCommand : IRequest<CreateCategoryBaseCommandResult>
    {
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Icono { get; set; }
        public bool Visible { get; set; } = true;
        public int Orden { get; set; } = 100;
        public string? Color { get; set; }
        public string? Descripcion { get; set; }
        public int EmpresaId { get; set; }
    }

    public class CreateCategoryBaseCommandResult
    {
        public int Id { get; set; }
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public int Orden { get; set; }
        public string Color { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}