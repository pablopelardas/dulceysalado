using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Users;

namespace DistriCatalogoAPI.Application.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("Nombre is required")
                .MaximumLength(50).WithMessage("Nombre cannot exceed 50 characters");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("Apellido is required")
                .MaximumLength(50).WithMessage("Apellido cannot exceed 50 characters");

            RuleFor(x => x.Rol)
                .NotEmpty().WithMessage("Rol is required")
                .Must(BeAValidRole).WithMessage("Invalid rol. Valid roles are: admin, editor, viewer");
        }

        private bool BeAValidRole(string role)
        {
            var validRoles = new[] { "admin", "editor", "viewer" };
            return validRoles.Contains(role?.ToLower());
        }
    }
}