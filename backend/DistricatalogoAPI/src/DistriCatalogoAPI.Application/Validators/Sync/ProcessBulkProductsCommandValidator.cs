using FluentValidation;
using DistriCatalogoAPI.Application.Commands.Sync;

namespace DistriCatalogoAPI.Application.Validators.Sync
{
    public class ProcessBulkProductsCommandValidator : AbstractValidator<ProcessBulkProductsCommand>
    {
        public ProcessBulkProductsCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("El ID de sesión es requerido");

            RuleFor(x => x.LoteNumero)
                .GreaterThan(0)
                .WithMessage("El número de lote debe ser mayor a 0");

            RuleFor(x => x.Productos)
                .NotEmpty()
                .WithMessage("La lista de productos no puede estar vacía")
                .Must(productos => productos.Count <= 1000)
                .WithMessage("No se pueden procesar más de 1000 productos por lote");

            RuleFor(x => x.EmpresaPrincipalId)
                .GreaterThan(0)
                .WithMessage("El ID de la empresa principal debe ser mayor a 0");

            RuleForEach(x => x.Productos)
                .SetValidator(cmd => new BulkProductDtoValidator(cmd.StockOnlyMode));
        }
    }

    public class BulkProductDtoValidator : AbstractValidator<BulkProductDto>
    {
        public BulkProductDtoValidator(bool stockOnlyMode = false)
        {
            RuleFor(x => x.Codigo)
                .NotEmpty()
                .WithMessage("El código del producto es requerido")
                .Must(codigo => int.TryParse(codigo, out var result) && result > 0)
                .WithMessage("El código del producto debe ser un número entero positivo");

            // En modo stock solo, la descripción no es requerida
            if (!stockOnlyMode)
            {
                RuleFor(x => x.Descripcion)
                    .NotEmpty()
                    .WithMessage("La descripción del producto es requerida")
                    .MaximumLength(500)
                    .WithMessage("La descripción no puede exceder 500 caracteres");
            }
            else
            {
                // En modo stock solo, descripción puede estar vacía
                RuleFor(x => x.Descripcion)
                    .MaximumLength(500)
                    .WithMessage("La descripción no puede exceder 500 caracteres")
                    .When(x => !string.IsNullOrEmpty(x.Descripcion));
            }

            // En modo stock solo, el precio no es relevante
            if (!stockOnlyMode)
            {
                RuleFor(x => x.Precio)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("El precio no puede ser negativo")
                    .LessThan(1000000)
                    .WithMessage("El precio no puede exceder 1,000,000");
            }

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El stock no puede ser negativo")
                .LessThan(1000000)
                .WithMessage("El stock no puede exceder 1,000,000");

            // En modo stock solo, la categoría no es relevante
            if (!stockOnlyMode)
            {
                RuleFor(x => x.CategoriaId)
                    .GreaterThan(0)
                    .WithMessage("El ID de categoría debe ser mayor a 0")
                    .When(x => x.CategoriaId.HasValue);
            }

            RuleFor(x => x.Imputable)
                .Must(value => string.IsNullOrEmpty(value) || value == "S" || value == "N")
                .WithMessage("Imputable debe ser 'S', 'N' o vacío");

            RuleFor(x => x.Disponible)
                .Must(value => string.IsNullOrEmpty(value) || value == "S" || value == "N")
                .WithMessage("Disponible debe ser 'S', 'N' o vacío");

            RuleFor(x => x.CodigoUbicacion)
                .MaximumLength(50)
                .WithMessage("El código de ubicación no puede exceder 50 caracteres");

            RuleFor(x => x.Grupo1)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Grupo1 no puede ser negativo")
                .When(x => x.Grupo1.HasValue);

            RuleFor(x => x.Grupo2)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Grupo2 no puede ser negativo")
                .When(x => x.Grupo2.HasValue);

            RuleFor(x => x.Grupo3)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Grupo3 no puede ser negativo")
                .When(x => x.Grupo3.HasValue);

            RuleFor(x => x.StocksPorEmpresa)
                .NotNull()
                .NotEmpty()
                .WithMessage("StocksPorEmpresa es obligatorio");

            RuleForEach(x => x.StocksPorEmpresa)
                .SetValidator(new StockPorEmpresaDtoValidator());
        }
    }

    public class StockPorEmpresaDtoValidator : AbstractValidator<StockPorEmpresaDto>
    {
        public StockPorEmpresaDtoValidator()
        {
            RuleFor(x => x.EmpresaId)
                .GreaterThan(0)
                .WithMessage("EmpresaId debe ser mayor a 0");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El stock no puede ser negativo")
                .LessThan(1000000)
                .WithMessage("El stock no puede exceder 1,000,000");
        }
    }
}