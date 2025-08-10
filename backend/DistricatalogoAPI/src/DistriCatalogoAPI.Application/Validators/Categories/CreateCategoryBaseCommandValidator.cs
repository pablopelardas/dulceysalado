using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Categories;

namespace DistriCatalogoAPI.Application.Validators.Categories
{
    public class CreateCategoryBaseCommandValidator : AbstractValidator<CreateCategoryBaseCommand>
    {
        public CreateCategoryBaseCommandValidator()
        {
            RuleFor(x => x.CodigoRubro)
                .GreaterThan(0)
                .WithMessage("El código de rubro debe ser mayor a 0");

            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre de la categoría es requerido")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Icono)
                .MaximumLength(10)
                .WithMessage("El icono no puede exceder 10 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Icono));

            RuleFor(x => x.Orden)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El orden debe ser mayor o igual a 0");

            RuleFor(x => x.Color)
                .Matches(@"^#[0-9A-Fa-f]{6}$")
                .WithMessage("El color debe ser un código hexadecimal válido (ej: #FF0000)")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.Descripcion)
                .MaximumLength(500)
                .WithMessage("La descripción no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Descripcion));

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0)
                .WithMessage("El ID de empresa debe ser mayor a 0");
        }
    }
}