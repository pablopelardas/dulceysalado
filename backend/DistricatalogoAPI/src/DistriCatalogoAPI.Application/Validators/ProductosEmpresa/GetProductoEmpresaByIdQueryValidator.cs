using FluentValidation;
using DistriCatalogoAPI.Application.Queries.ProductosEmpresa;

namespace DistriCatalogoAPI.Application.Validators.ProductosEmpresa
{
    public class GetProductoEmpresaByIdQueryValidator : AbstractValidator<GetProductoEmpresaByIdQuery>
    {
        public GetProductoEmpresaByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("El ID del producto debe ser mayor a 0");
        }
    }
}