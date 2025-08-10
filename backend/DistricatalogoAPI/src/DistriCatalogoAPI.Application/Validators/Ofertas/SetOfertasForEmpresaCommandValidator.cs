using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Ofertas;

namespace DistriCatalogoAPI.Application.Validators.Ofertas
{
    public class SetOfertasForEmpresaCommandValidator : AbstractValidator<SetOfertasForEmpresaCommand>
    {
        public SetOfertasForEmpresaCommandValidator()
        {
            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("ID de empresa debe ser mayor a 0");

            RuleFor(x => x.AgrupacionIds)
                .NotNull().WithMessage("Lista de agrupaciones es requerida");

            RuleForEach(x => x.AgrupacionIds)
                .GreaterThan(0).WithMessage("ID de agrupación debe ser mayor a 0");
        }
    }
}