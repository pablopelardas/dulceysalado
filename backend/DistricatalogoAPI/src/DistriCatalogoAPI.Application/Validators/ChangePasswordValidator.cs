using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Users;

namespace DistriCatalogoAPI.Application.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");

            // CurrentPassword validation is handled in the business logic layer (handler)
            // since we need database access to determine if it's required

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
                .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("New password must contain at least one digit");

            RuleFor(x => x.NewPassword)
                .Must((command, newPassword) => newPassword != command.CurrentPassword)
                .WithMessage("New password must be different from current password")
                .When(x => !string.IsNullOrEmpty(x.CurrentPassword) && !string.IsNullOrEmpty(x.NewPassword));
        }
    }
}