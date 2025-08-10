using FluentValidation;
using DistriCatalogoAPI.Application.Queries.Companies;

namespace DistriCatalogoAPI.Application.Validators.Companies
{
    public class GetCompaniesListQueryValidator : AbstractValidator<GetCompaniesListQuery>
    {
        public GetCompaniesListQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
        }
    }
}