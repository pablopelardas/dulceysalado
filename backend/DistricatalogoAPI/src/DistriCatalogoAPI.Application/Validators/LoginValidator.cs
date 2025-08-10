using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Users;

namespace DistriCatalogoAPI.Application.Validators
{
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("Formato de email inválido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida");
        }
    }
}