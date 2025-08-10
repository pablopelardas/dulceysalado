using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("Nombre is required")
                .MaximumLength(50).WithMessage("Nombre cannot exceed 50 characters");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("Apellido is required")
                .MaximumLength(50).WithMessage("Apellido cannot exceed 50 characters");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("Valid empresa ID is required");

            RuleFor(x => x.Rol)
                .NotEmpty().WithMessage("Rol is required")
                .Must(BeAValidRole).WithMessage("Invalid rol");
        }

        private bool BeAValidRole(string role)
        {
            var validRoles = new[] { "admin", "editor", "viewer" };
            return validRoles.Contains(role?.ToLower());
        }
    }
}