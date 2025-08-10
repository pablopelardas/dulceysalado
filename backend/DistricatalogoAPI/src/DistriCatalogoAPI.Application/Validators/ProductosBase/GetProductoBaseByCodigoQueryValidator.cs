using FluentValidation;
using DistriCatalogoAPI.Application.Queries.ProductosBase;

namespace DistriCatalogoAPI.Application.Validators.ProductosBase
{
    public class GetProductoBaseByCodigoQueryValidator : AbstractValidator<GetProductoBaseByCodigoQuery>
    {
        public GetProductoBaseByCodigoQueryValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty()
                .WithMessage("El código del producto es requerido")
                .MaximumLength(50)
                .WithMessage("El código del producto no puede exceder 50 caracteres");
        }
    }
}