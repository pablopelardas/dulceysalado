using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ListasPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ListasPrecios
{
    public class CreateListaPrecioCommandHandler : IRequestHandler<CreateListaPrecioCommand, CreateListaPrecioCommandResult>
    {
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<CreateListaPrecioCommandHandler> _logger;

        public CreateListaPrecioCommandHandler(
            IListaPrecioRepository listaPrecioRepository,
            ILogger<CreateListaPrecioCommandHandler> logger)
        {
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<CreateListaPrecioCommandResult> Handle(CreateListaPrecioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar que el código no esté vacío
                if (string.IsNullOrWhiteSpace(request.Codigo))
                {
                    return new CreateListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "El código de la lista de precios es requerido"
                    };
                }

                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(request.Nombre))
                {
                    return new CreateListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "El nombre de la lista de precios es requerido"
                    };
                }

                // Verificar que el código no exista
                var codigoExists = await _listaPrecioRepository.CodigoExistsAsync(request.Codigo);
                if (codigoExists)
                {
                    return new CreateListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "Ya existe una lista de precios con este código"
                    };
                }

                // Crear la lista de precios
                var listaId = await _listaPrecioRepository.CreateAsync(
                    request.Codigo,
                    request.Nombre,
                    request.Descripcion,
                    request.Orden);

                _logger.LogInformation("Lista de precios {Codigo} creada exitosamente por empresa {EmpresaId}", 
                    request.Codigo, request.EmpresaId);

                return new CreateListaPrecioCommandResult
                {
                    Success = true,
                    Message = "Lista de precios creada exitosamente",
                    ListaId = listaId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lista de precios {Codigo}", request.Codigo);
                return new CreateListaPrecioCommandResult
                {
                    Success = false,
                    Message = "Error interno al crear la lista de precios"
                };
            }
        }
    }
}