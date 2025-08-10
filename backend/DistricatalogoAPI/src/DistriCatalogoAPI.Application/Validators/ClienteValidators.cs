using FluentValidation;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Constants;

namespace DistriCatalogoAPI.Application.Validators
{
    public class CreateClienteDtoValidator : AbstractValidator<CreateClienteDto>
    {
        public CreateClienteDtoValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("El código es requerido")
                .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("El código solo puede contener letras, números, guiones y guiones bajos");

            RuleFor(x => x.Nombre)
                .MaximumLength(255).WithMessage("El nombre no puede exceder 255 caracteres");

            RuleFor(x => x.Direccion)
                .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres");

            RuleFor(x => x.Localidad)
                .MaximumLength(255).WithMessage("La localidad no puede exceder 255 caracteres");

            RuleFor(x => x.Telefono)
                .MaximumLength(100).WithMessage("El teléfono no puede exceder 100 caracteres");

            RuleFor(x => x.Cuit)
                .MaximumLength(50).WithMessage("El CUIT no puede exceder 50 caracteres");
                // Nota: No validamos formato de CUIT porque es flexible según requerimientos

            RuleFor(x => x.Altura)
                .MaximumLength(50).WithMessage("La altura no puede exceder 50 caracteres");

            RuleFor(x => x.Provincia)
                .MaximumLength(100).WithMessage("La provincia no puede exceder 100 caracteres");

            RuleFor(x => x.Email)
                .MaximumLength(255).WithMessage("El email no puede exceder 255 caracteres")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("El formato del email no es válido");

            RuleFor(x => x.TipoIva)
                .Must(TipoIvaConstants.IsValid).When(x => !string.IsNullOrEmpty(x.TipoIva))
                .WithMessage($"El tipo de IVA debe ser uno de: {string.Join(", ", TipoIvaConstants.ValidTipos)}");

            RuleFor(x => x.ListaPrecioId)
                .GreaterThan(0).When(x => x.ListaPrecioId.HasValue)
                .WithMessage("El ID de lista de precio debe ser mayor a 0");
        }
    }

    public class UpdateClienteDtoValidator : AbstractValidator<UpdateClienteDto>
    {
        public UpdateClienteDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .MaximumLength(255).WithMessage("El nombre no puede exceder 255 caracteres")
                .When(x => x.Nombre != null);

            RuleFor(x => x.Direccion)
                .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres")
                .When(x => x.Direccion != null);

            RuleFor(x => x.Localidad)
                .MaximumLength(255).WithMessage("La localidad no puede exceder 255 caracteres")
                .When(x => x.Localidad != null);

            RuleFor(x => x.Telefono)
                .MaximumLength(100).WithMessage("El teléfono no puede exceder 100 caracteres")
                .When(x => x.Telefono != null);

            RuleFor(x => x.Cuit)
                .MaximumLength(50).WithMessage("El CUIT no puede exceder 50 caracteres")
                .When(x => x.Cuit != null);

            RuleFor(x => x.Altura)
                .MaximumLength(50).WithMessage("La altura no puede exceder 50 caracteres")
                .When(x => x.Altura != null);

            RuleFor(x => x.Provincia)
                .MaximumLength(100).WithMessage("La provincia no puede exceder 100 caracteres")
                .When(x => x.Provincia != null);

            RuleFor(x => x.Email)
                .MaximumLength(255).WithMessage("El email no puede exceder 255 caracteres")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("El formato del email no es válido")
                .When(x => x.Email != null);

            RuleFor(x => x.TipoIva)
                .Must(TipoIvaConstants.IsValid).When(x => !string.IsNullOrEmpty(x.TipoIva))
                .WithMessage($"El tipo de IVA debe ser uno de: {string.Join(", ", TipoIvaConstants.ValidTipos)}")
                .When(x => x.TipoIva != null);

            RuleFor(x => x.ListaPrecioId)
                .GreaterThan(0).When(x => x.ListaPrecioId.HasValue)
                .WithMessage("El ID de lista de precio debe ser mayor a 0");
        }
    }

    public class CreateClienteCredentialsDtoValidator : AbstractValidator<CreateClienteCredentialsDto>
    {
        public CreateClienteCredentialsDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .MaximumLength(100).WithMessage("El nombre de usuario no puede exceder 100 caracteres")
                .Matches("^[a-zA-Z0-9._-]+$").WithMessage("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");
        }
    }

    public class UpdateClientePasswordDtoValidator : AbstractValidator<UpdateClientePasswordDto>
    {
        public UpdateClientePasswordDtoValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");
        }
    }

    public class ClienteLoginDtoValidator : AbstractValidator<ClienteLoginDto>
    {
        public ClienteLoginDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .MaximumLength(100).WithMessage("El nombre de usuario no puede exceder 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).When(x => x.EmpresaId.HasValue)
                .WithMessage("El ID de empresa debe ser mayor a 0");
        }
    }

    public class ClienteChangePasswordDtoValidator : AbstractValidator<ClienteChangePasswordDto>
    {
        public ClienteChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("La contraseña actual es requerida")
                .MaximumLength(100).WithMessage("La contraseña actual no puede exceder 100 caracteres");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(6).WithMessage("La nueva contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La nueva contraseña no puede exceder 100 caracteres")
                .NotEqual(x => x.CurrentPassword).WithMessage("La nueva contraseña debe ser diferente a la actual");
        }
    }

    public class CustomerSyncDtoValidator : AbstractValidator<CustomerSyncDto>
    {
        public CustomerSyncDtoValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("El código es requerido")
                .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres");

            RuleFor(x => x.Nombre)
                .MaximumLength(255).WithMessage("El nombre no puede exceder 255 caracteres");

            RuleFor(x => x.Direccion)
                .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres");

            RuleFor(x => x.Localidad)
                .MaximumLength(255).WithMessage("La localidad no puede exceder 255 caracteres");

            RuleFor(x => x.Telefono)
                .MaximumLength(100).WithMessage("El teléfono no puede exceder 100 caracteres");

            RuleFor(x => x.Cuit)
                .MaximumLength(50).WithMessage("El CUIT no puede exceder 50 caracteres");

            RuleFor(x => x.Altura)
                .MaximumLength(50).WithMessage("La altura no puede exceder 50 caracteres");

            RuleFor(x => x.Provincia)
                .MaximumLength(100).WithMessage("La provincia no puede exceder 100 caracteres");

            RuleFor(x => x.Email)
                .MaximumLength(255).WithMessage("El email no puede exceder 255 caracteres")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("El formato del email no es válido");

            RuleFor(x => x.TipoIva)
                .Must(TipoIvaConstants.IsValid).When(x => !string.IsNullOrEmpty(x.TipoIva))
                .WithMessage($"El tipo de IVA debe ser uno de: {string.Join(", ", TipoIvaConstants.ValidTipos)}");

            RuleFor(x => x.ListaPrecio)
                .MaximumLength(10).WithMessage("La lista de precio no puede exceder 10 caracteres");
        }
    }

    public class ProcessBulkCustomersDtoValidator : AbstractValidator<ProcessBulkCustomersDto>
    {
        public ProcessBulkCustomersDtoValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("El ID de sesión es requerido")
                .Length(36).WithMessage("El ID de sesión debe ser un GUID válido");

            RuleFor(x => x.Customers)
                .NotNull().WithMessage("La lista de clientes es requerida")
                .NotEmpty().WithMessage("Debe proporcionar al menos un cliente")
                .Must(x => x.Count <= 1000).WithMessage("No se pueden procesar más de 1000 clientes por lote");

            RuleForEach(x => x.Customers)
                .SetValidator(new CustomerSyncDtoValidator());
        }
    }

    public class StartCustomerSyncSessionDtoValidator : AbstractValidator<StartCustomerSyncSessionDto>
    {
        public StartCustomerSyncSessionDtoValidator()
        {
            RuleFor(x => x.Source)
                .NotEmpty().WithMessage("La fuente es requerida")
                .MaximumLength(50).WithMessage("La fuente no puede exceder 50 caracteres");
        }
    }

    public class EndCustomerSyncSessionDtoValidator : AbstractValidator<EndCustomerSyncSessionDto>
    {
        public EndCustomerSyncSessionDtoValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("El ID de sesión es requerido")
                .Length(36).WithMessage("El ID de sesión debe ser un GUID válido");
        }
    }
}