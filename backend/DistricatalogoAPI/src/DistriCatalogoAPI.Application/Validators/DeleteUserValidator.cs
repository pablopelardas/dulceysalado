using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Users;

namespace DistriCatalogoAPI.Application.Validators
{
    public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");
        }
    }
}