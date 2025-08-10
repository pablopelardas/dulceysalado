using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Novedades;

namespace DistriCatalogoAPI.Application.Validators.Novedades
{
    public class DeleteEmpresaNovedadCommandValidator : AbstractValidator<DeleteEmpresaNovedadCommand>
    {
        public DeleteEmpresaNovedadCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID debe ser mayor a 0");
        }
    }
}