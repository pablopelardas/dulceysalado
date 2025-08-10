using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Companies;

namespace DistriCatalogoAPI.Application.Validators.Companies
{
    public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
    {
        public DeleteCompanyCommandValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID must be greater than 0");
        }
    }
}