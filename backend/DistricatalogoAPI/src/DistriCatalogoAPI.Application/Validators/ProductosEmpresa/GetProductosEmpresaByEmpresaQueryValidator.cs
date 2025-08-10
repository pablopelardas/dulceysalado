using FluentValidation;
using DistriCatalogoAPI.Application.Queries.ProductosEmpresa;

namespace DistriCatalogoAPI.Application.Validators.ProductosEmpresa
{
    public class GetProductosEmpresaByEmpresaQueryValidator : AbstractValidator<GetProductosEmpresaByEmpresaQuery>
    {
        public GetProductosEmpresaByEmpresaQueryValidator()
        {
            RuleFor(x => x.EmpresaId)
                .GreaterThan(0)
                .WithMessage("El ID de empresa debe ser mayor a 0");

            RuleFor(x => x.CodigoRubro)
                .GreaterThan(0)
                .WithMessage("El código de rubro debe ser mayor a 0")
                .When(x => x.CodigoRubro.HasValue);

            RuleFor(x => x.Busqueda)
                .MaximumLength(100)
                .WithMessage("El término de búsqueda no puede exceder 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Busqueda));

            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("La página debe ser mayor a 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("El tamaño de página debe ser mayor a 0")
                .LessThanOrEqualTo(100)
                .WithMessage("El tamaño de página no puede exceder 100 elementos");
        }
    }
}