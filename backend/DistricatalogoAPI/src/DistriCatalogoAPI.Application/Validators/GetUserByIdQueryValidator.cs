using FluentValidation;
using DistriCatalogoAPI.Application.Queries.Users;

namespace DistriCatalogoAPI.Application.Validators
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");
        }
    }
}