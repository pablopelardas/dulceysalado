using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Companies;

namespace DistriCatalogoAPI.Application.Validators.Companies
{
    public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
    {
        public CreateCompanyCommandValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("El código de empresa es requerido")
                .Length(1, 20).WithMessage("El código de empresa debe tener entre 1 y 20 caracteres");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre de empresa es requerido")
                .Length(1, 100).WithMessage("El nombre de empresa debe tener entre 1 y 100 caracteres");

            RuleFor(x => x.RazonSocial)
                .MaximumLength(150).WithMessage("La razón social no puede exceder 150 caracteres")
                .When(x => !string.IsNullOrEmpty(x.RazonSocial));

            RuleFor(x => x.Cuit)
                .Matches(@"^\d{2}-\d{8}-\d{1}$").WithMessage("El CUIT debe tener el formato XX-XXXXXXXX-X")
                .When(x => !string.IsNullOrEmpty(x.Cuit));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Formato de email inválido")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Telefono)
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Telefono));

            RuleFor(x => x.Direccion)
                .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Direccion));

            RuleFor(x => x.DominioPersonalizado)
                .Matches(@"^[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?$")
                .WithMessage("El dominio solo puede contener letras, números y guiones, y no puede empezar o terminar con guión")
                .When(x => !string.IsNullOrEmpty(x.DominioPersonalizado));

            RuleFor(x => x.Plan)
                .Must(plan => new[] { "basico", "premium", "enterprise" }.Contains(plan))
                .WithMessage("El plan debe ser 'basico', 'premium' o 'enterprise'")
                .When(x => !string.IsNullOrEmpty(x.Plan));

            RuleFor(x => x.FechaVencimiento)
                .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de vencimiento debe ser futura")
                .When(x => x.FechaVencimiento.HasValue);

            RuleFor(x => x.ProductosPorPagina)
                .GreaterThan(0).WithMessage("Los productos por página debe ser mayor a 0")
                .LessThanOrEqualTo(100).WithMessage("Los productos por página no puede exceder 100")
                .When(x => x.ProductosPorPagina.HasValue);
        }
    }
}