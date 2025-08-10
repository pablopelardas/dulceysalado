using FluentValidation;
using DistriCatalogoAPI.Application.Queries.Sync;

namespace DistriCatalogoAPI.Application.Validators.Sync
{
    public class GetSyncStatsQueryValidator : AbstractValidator<GetSyncStatsQuery>
    {
        public GetSyncStatsQueryValidator()
        {
            RuleFor(x => x.Days)
                .GreaterThan(0)
                .WithMessage("Los días deben ser mayor a 0")
                .LessThanOrEqualTo(365)
                .WithMessage("Los días no pueden exceder 365");

            RuleFor(x => x.EmpresaPrincipalId)
                .GreaterThan(0)
                .WithMessage("El ID de la empresa principal debe ser mayor a 0");
        }
    }
}