using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Sync;

namespace DistriCatalogoAPI.Application.Validators.Sync
{
    public class StartSyncSessionCommandValidator : AbstractValidator<StartSyncSessionCommand>
    {
        public StartSyncSessionCommandValidator()
        {
            RuleFor(x => x.TotalLotesEsperados)
                .GreaterThan(0)
                .WithMessage("El total de lotes esperados debe ser mayor a 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("El total de lotes esperados no puede exceder 1000");

            RuleFor(x => x.UsuarioProceso)
                .NotEmpty()
                .WithMessage("El usuario del proceso es requerido")
                .MaximumLength(100)
                .WithMessage("El usuario del proceso no puede exceder 100 caracteres");

            RuleFor(x => x.IpOrigen)
                .MaximumLength(45)
                .WithMessage("La IP de origen no puede exceder 45 caracteres");

            RuleFor(x => x.EmpresaPrincipalId)
                .GreaterThan(0)
                .WithMessage("El ID de la empresa principal debe ser mayor a 0");
        }
    }
}