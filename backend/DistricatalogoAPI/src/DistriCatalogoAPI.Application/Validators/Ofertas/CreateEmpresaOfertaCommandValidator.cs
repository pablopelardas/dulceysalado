using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Ofertas;

namespace DistriCatalogoAPI.Application.Validators.Ofertas
{
    public class CreateEmpresaOfertaCommandValidator : AbstractValidator<CreateEmpresaOfertaCommand>
    {
        public CreateEmpresaOfertaCommandValidator()
        {
            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("ID de empresa debe ser mayor a 0");

            RuleFor(x => x.AgrupacionId)
                .GreaterThan(0).WithMessage("ID de agrupación debe ser mayor a 0");
        }
    }
}