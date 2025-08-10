using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Novedades;

namespace DistriCatalogoAPI.Application.Validators.Novedades
{
    public class CreateEmpresaNovedadCommandValidator : AbstractValidator<CreateEmpresaNovedadCommand>
    {
        public CreateEmpresaNovedadCommandValidator()
        {
            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("ID de empresa debe ser mayor a 0");

            RuleFor(x => x.AgrupacionId)
                .GreaterThan(0).WithMessage("ID de agrupación debe ser mayor a 0");
        }
    }
}