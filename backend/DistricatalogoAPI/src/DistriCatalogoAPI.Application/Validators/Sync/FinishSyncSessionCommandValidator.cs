using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Sync;

namespace DistriCatalogoAPI.Application.Validators.Sync
{
    public class FinishSyncSessionCommandValidator : AbstractValidator<FinishSyncSessionCommand>
    {
        private static readonly string[] ValidStates = { "completada", "error", "cancelada" };

        public FinishSyncSessionCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("El ID de sesión es requerido");

            RuleFor(x => x.Estado)
                .NotEmpty()
                .WithMessage("El estado es requerido")
                .Must(estado => ValidStates.Contains(estado?.ToLower()))
                .WithMessage("El estado debe ser 'completada', 'error' o 'cancelada'");

            RuleFor(x => x.EmpresaPrincipalId)
                .GreaterThan(0)
                .WithMessage("El ID de la empresa principal debe ser mayor a 0");
        }
    }
}