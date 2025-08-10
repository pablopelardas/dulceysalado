using FluentValidation;
using DistriCatalogoAPI.Application.Queries.Companies;

namespace DistriCatalogoAPI.Application.Validators.Companies
{
    public class GetCompanyByIdQueryValidator : AbstractValidator<GetCompanyByIdQuery>
    {
        public GetCompanyByIdQueryValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID must be greater than 0");
        }
    }
}