using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Ofertas;

namespace DistriCatalogoAPI.Application.Validators.Ofertas
{
    public class DeleteEmpresaOfertaCommandValidator : AbstractValidator<DeleteEmpresaOfertaCommand>
    {
        public DeleteEmpresaOfertaCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID debe ser mayor a 0");
        }
    }
}