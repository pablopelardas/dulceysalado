using FluentValidation;
using DistriCatalogoAPI.Application.Queries.Users;

namespace DistriCatalogoAPI.Application.Validators
{
    public class GetUsersListQueryValidator : AbstractValidator<GetUsersListQuery>
    {
        public GetUsersListQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("Empresa ID must be greater than 0")
                .When(x => x.EmpresaId.HasValue);
        }
    }
}