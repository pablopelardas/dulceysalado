using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Users;

namespace DistriCatalogoAPI.Application.Validators
{
    public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required");
        }
    }
}