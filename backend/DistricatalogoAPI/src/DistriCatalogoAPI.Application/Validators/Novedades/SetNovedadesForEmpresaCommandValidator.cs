using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Novedades;

namespace DistriCatalogoAPI.Application.Validators.Novedades
{
    public class SetNovedadesForEmpresaCommandValidator : AbstractValidator<SetNovedadesForEmpresaCommand>
    {
        public SetNovedadesForEmpresaCommandValidator()
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