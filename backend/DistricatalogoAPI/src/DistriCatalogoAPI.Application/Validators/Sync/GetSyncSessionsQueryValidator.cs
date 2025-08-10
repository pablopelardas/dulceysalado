using FluentValidation;
using DistriCatalogoAPI.Application.Queries.Sync;

namespace DistriCatalogoAPI.Application.Validators.Sync
{
    public class GetSyncSessionsQueryValidator : AbstractValidator<GetSyncSessionsQuery>
    {
        private static readonly string[] ValidStates = { "iniciada", "procesando", "completada", "error", "cancelada" };

        public GetSyncSessionsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("La página debe ser mayor a 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("La página no puede exceder 1000");

            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .WithMessage("El límite debe ser mayor a 0")
                .LessThanOrEqualTo(100)
                .WithMessage("El límite no puede exceder 100 elementos por página");

            RuleFor(x => x.Estado)
                .Must(estado => string.IsNullOrEmpty(estado) || ValidStates.Contains(estado.ToLower()))
                .WithMessage("El estado debe ser 'iniciada', 'procesando', 'completada', 'error' o 'cancelada'")
                .When(x => !string.IsNullOrEmpty(x.Estado));

            RuleFor(x => x.EmpresaPrincipalId)
                .GreaterThan(0)
                .WithMessage("El ID de la empresa principal debe ser mayor a 0");
        }
    }
}