using FluentValidation;
using DistriCatalogoAPI.Application.Queries.ProductosBase;

namespace DistriCatalogoAPI.Application.Validators.ProductosBase
{
    public class GetProductoBaseByIdQueryValidator : AbstractValidator<GetProductoBaseByIdQuery>
    {
        public GetProductoBaseByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("El ID del producto debe ser mayor a 0");
        }
    }
}