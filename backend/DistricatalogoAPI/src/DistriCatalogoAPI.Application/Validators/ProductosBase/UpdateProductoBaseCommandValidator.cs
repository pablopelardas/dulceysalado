using FluentValidation;
using DistriCatalogoAPI.Application.Commands.ProductosBase;

namespace DistriCatalogoAPI.Application.Validators.ProductosBase
{
    public class UpdateProductoBaseCommandValidator : AbstractValidator<UpdateProductoBaseCommand>
    {
        public UpdateProductoBaseCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("El ID del producto debe ser mayor a 0");

            RuleFor(x => x.Descripcion)
                .NotEmpty()
                .WithMessage("La descripción del producto no puede estar vacía")
                .MaximumLength(200)
                .WithMessage("La descripción no puede exceder 200 caracteres")
                .When(x => x.Descripcion != null);

            // RuleFor(x => x.CodigoRubro) - Sin validación, permite cualquier valor

            RuleFor(x => x.Precio)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El precio debe ser mayor o igual a 0")
                .When(x => x.Precio.HasValue);

            RuleFor(x => x.Existencia)
                .GreaterThanOrEqualTo(0)
                .WithMessage("La existencia debe ser mayor o igual a 0")
                .When(x => x.Existencia.HasValue);

            RuleFor(x => x.OrdenCategoria)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El orden de categoría debe ser mayor o igual a 0")
                .When(x => x.OrdenCategoria.HasValue);

            RuleFor(x => x.ImagenUrl)
                .MaximumLength(500)
                .WithMessage("La URL de imagen no puede exceder 500 caracteres")
                .Must(BeAValidUrl)
                .WithMessage("La URL de imagen debe ser válida")
                .When(x => !string.IsNullOrEmpty(x.ImagenUrl));

            RuleFor(x => x.ImagenAlt)
                .MaximumLength(200)
                .WithMessage("El texto alternativo de imagen no puede exceder 200 caracteres")
                .When(x => x.ImagenAlt != null);

            RuleFor(x => x.DescripcionCorta)
                .MaximumLength(500)
                .WithMessage("La descripción corta no puede exceder 500 caracteres")
                .When(x => x.DescripcionCorta != null);

            RuleFor(x => x.DescripcionLarga)
                .MaximumLength(2000)
                .WithMessage("La descripción larga no puede exceder 2000 caracteres")
                .When(x => x.DescripcionLarga != null);

            RuleFor(x => x.Tags)
                .MaximumLength(500)
                .WithMessage("Los tags no pueden exceder 500 caracteres")
                .When(x => x.Tags != null);

            RuleFor(x => x.CodigoBarras)
                .MaximumLength(50)
                .WithMessage("El código de barras no puede exceder 50 caracteres")
                .When(x => x.CodigoBarras != null);

            RuleFor(x => x.Marca)
                .MaximumLength(100)
                .WithMessage("La marca no puede exceder 100 caracteres")
                .When(x => x.Marca != null);

            RuleFor(x => x.UnidadMedida)
                .NotEmpty()
                .WithMessage("La unidad de medida no puede estar vacía")
                .MaximumLength(10)
                .WithMessage("La unidad de medida no puede exceder 10 caracteres")
                .When(x => x.UnidadMedida != null);
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}