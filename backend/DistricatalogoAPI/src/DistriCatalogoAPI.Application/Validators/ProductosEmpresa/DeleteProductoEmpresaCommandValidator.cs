using FluentValidation;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresa;

namespace DistriCatalogoAPI.Application.Validators.ProductosEmpresa
{
    public class DeleteProductoEmpresaCommandValidator : AbstractValidator<DeleteProductoEmpresaCommand>
    {
        public DeleteProductoEmpresaCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("El ID del producto debe ser mayor a 0");
        }
    }
}