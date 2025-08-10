using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Companies;

namespace DistriCatalogoAPI.Application.Validators.Companies
{
    public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
    {
        public UpdateCompanyCommandValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Company ID must be greater than 0");

            RuleFor(x => x.Nombre)
                .Length(1, 100).WithMessage("Company name must be between 1 and 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Nombre));

            RuleFor(x => x.RazonSocial)
                .MaximumLength(150).WithMessage("Legal name cannot exceed 150 characters")
                .When(x => !string.IsNullOrEmpty(x.RazonSocial));

            RuleFor(x => x.Cuit)
                .Matches(@"^\d{2}-\d{8}-\d{1}$").WithMessage("CUIT must have format XX-XXXXXXXX-X")
                .When(x => !string.IsNullOrEmpty(x.Cuit));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Telefono)
                .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Telefono));

            RuleFor(x => x.Direccion)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Direccion));

            RuleFor(x => x.DominioPersonalizado)
                .Matches(@"^[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?$")
                .WithMessage("Domain must contain only letters, numbers and hyphens, and cannot start or end with a hyphen")
                .When(x => !string.IsNullOrEmpty(x.DominioPersonalizado));

            RuleFor(x => x.Plan)
                .Must(plan => new[] { "basico", "premium", "enterprise" }.Contains(plan))
                .WithMessage("Plan must be 'basico', 'premium', or 'enterprise'")
                .When(x => !string.IsNullOrEmpty(x.Plan));

            RuleFor(x => x.FechaVencimiento)
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
                .When(x => x.FechaVencimiento.HasValue);

            RuleFor(x => x.ProductosPorPagina)
                .InclusiveBetween(1, 100).WithMessage("Products per page must be between 1 and 100")
                .When(x => x.ProductosPorPagina.HasValue);

            RuleFor(x => x.LogoUrl)
                .Must(BeValidUrl).WithMessage("Invalid logo URL format")
                .When(x => !string.IsNullOrEmpty(x.LogoUrl));

            RuleFor(x => x.FaviconUrl)
                .Must(BeValidUrl).WithMessage("Invalid favicon URL format")
                .When(x => !string.IsNullOrEmpty(x.FaviconUrl));

            RuleFor(x => x.UrlWhatsapp)
                .Must(BeValidUrl).WithMessage("Invalid WhatsApp URL format")
                .When(x => !string.IsNullOrEmpty(x.UrlWhatsapp));

            RuleFor(x => x.UrlFacebook)
                .Must(BeValidUrl).WithMessage("Invalid Facebook URL format")
                .When(x => !string.IsNullOrEmpty(x.UrlFacebook));

            RuleFor(x => x.UrlInstagram)
                .Must(BeValidUrl).WithMessage("Invalid Instagram URL format")
                .When(x => !string.IsNullOrEmpty(x.UrlInstagram));
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}