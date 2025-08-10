using FluentValidation;
using DistriCatalogoAPI.Application.Commands.ProductosBase;

namespace DistriCatalogoAPI.Application.Validators.ProductosBase
{
    public class DeleteProductoBaseCommandValidator : AbstractValidator<DeleteProductoBaseCommand>
    {
        public DeleteProductoBaseCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("El ID del producto debe ser mayor a 0");
        }
    }
}