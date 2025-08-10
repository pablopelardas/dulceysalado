using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Categories;

namespace DistriCatalogoAPI.Application.Validators.Categories
{
    public class UpdateCategoryBaseCommandValidator : AbstractValidator<UpdateCategoryBaseCommand>
    {
        public UpdateCategoryBaseCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("El ID debe ser mayor a 0");

            RuleFor(x => x.Nombre)
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Nombre));

            RuleFor(x => x.Icono)
                .MaximumLength(10)
                .WithMessage("El icono no puede exceder 10 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Icono));

            RuleFor(x => x.Orden)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El orden debe ser mayor o igual a 0")
                .When(x => x.Orden.HasValue);

            RuleFor(x => x.Color)
                .Matches(@"^#[0-9A-Fa-f]{6}$")
                .WithMessage("El color debe ser un código hexadecimal válido (ej: #FF0000)")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.Descripcion)
                .MaximumLength(500)
                .WithMessage("La descripción no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Descripcion));
        }
    }
}