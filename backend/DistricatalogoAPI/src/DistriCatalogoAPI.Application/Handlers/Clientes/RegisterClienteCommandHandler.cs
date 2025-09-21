using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Clientes
{
    public class RegisterClienteCommandHandler : IRequestHandler<RegisterClienteCommand, ClienteDto>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IClienteAuthService _clienteAuthService;
        private readonly IMapper _mapper;
        private readonly ILogger<RegisterClienteCommandHandler> _logger;
        private readonly ICompanyRepository _companyRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;

        public RegisterClienteCommandHandler(
            IClienteRepository clienteRepository,
            IClienteAuthService clienteAuthService,
            IMapper mapper,
            ILogger<RegisterClienteCommandHandler> logger,
            ICompanyRepository companyRepository,
            IListaPrecioRepository listaPrecioRepository)
        {
            _clienteRepository = clienteRepository;
            _clienteAuthService = clienteAuthService;
            _mapper = mapper;
            _logger = logger;
            _companyRepository = companyRepository;
            _listaPrecioRepository = listaPrecioRepository;
        }

        public async Task<ClienteDto> Handle(RegisterClienteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Registering new cliente {Email} for empresa {EmpresaId}", 
                request.Email, request.EmpresaId);

            try
            {
                // Verificar que el email no esté en uso por otro cliente de la misma empresa
                var existingCliente = await _clienteRepository.GetByUsernameAsync(request.EmpresaId, request.Email);
                if (existingCliente != null)
                {
                    _logger.LogWarning("Email {Email} already exists for empresa {EmpresaId}", 
                        request.Email, request.EmpresaId);
                    throw new InvalidOperationException($"El email '{request.Email}' ya está registrado.");
                }

                // Generar código único para el cliente
                var nuevoCodigo = await GenerateUniqueCodeAsync(request.EmpresaId);

                // Encriptar contraseña
                var passwordHash = await _clienteAuthService.HashPasswordAsync(request.Password);

                // Crear nueva entidad Cliente
                var cliente = new Cliente
                {
                    EmpresaId = request.EmpresaId,
                    Codigo = nuevoCodigo,
                    Nombre = request.Nombre.Trim(),
                    Email = request.Email.Trim().ToLowerInvariant(),
                    Username = request.Email.Trim().ToLowerInvariant(), // Usar email como username
                    Telefono = request.Telefono?.Trim(),
                    Direccion = request.Direccion?.Trim(),
                    PasswordHash = passwordHash,
                    ListaPrecioId = await GetDefaultPriceListAsync(request.EmpresaId),
                    IsActive = true,
                    Activo = true,
                    EmailVerificado = false, // Se puede implementar verificación más adelante
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "REGISTRO_WEB"
                };

                // Guardar en base de datos
                await _clienteRepository.AddAsync(cliente);

                _logger.LogInformation("Cliente registered successfully with ID {ClienteId} and code {Codigo}", 
                    cliente.Id, cliente.Codigo);

                // Mapear a DTO y retornar
                return _mapper.Map<ClienteDto>(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering cliente {Email} for empresa {EmpresaId}", 
                    request.Email, request.EmpresaId);
                throw;
            }
        }

        private async Task<string> GenerateUniqueCodeAsync(int empresaId)
        {
            var random = new Random();
            string code;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                code = $"CLI{DateTime.Now:yyMMdd}{random.Next(1000, 9999)}";
                var exists = await _clienteRepository.ExistsByCodeAsync(code, empresaId);
                
                if (!exists) return code;
                
                attempts++;
            } while (attempts < maxAttempts);

            // Si no encontramos un código único después de 10 intentos, usamos un timestamp más específico
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"CLI{timestamp}";
        }

        private async Task<int?> GetDefaultPriceListAsync(int empresaId)
        {
            try
            {
                // Primero buscar la lista de precios predeterminada de la empresa
                var empresa = await _companyRepository.GetByIdAsync(empresaId);
                
                if (empresa?.ListaPrecioPredeterminadaId != null)
                {
                    // Verificar que la lista esté activa
                    var listaPredeterminadaEmpresa = await _listaPrecioRepository.GetByIdAsync(empresa.ListaPrecioPredeterminadaId.Value);
                    if (listaPredeterminadaEmpresa?.Activa == true)
                    {
                        _logger.LogInformation("Asignando lista de precios predeterminada {ListaPrecioId} ({Codigo}) de empresa {EmpresaId} a nuevo cliente", 
                            empresa.ListaPrecioPredeterminadaId, listaPredeterminadaEmpresa.Codigo, empresaId);
                        return empresa.ListaPrecioPredeterminadaId;
                    }
                }
                
                // Si no hay lista predeterminada configurada en la empresa, buscar la lista predeterminada global
                var listaPredeterminadaId = await _listaPrecioRepository.GetDefaultListIdAsync();
                if (listaPredeterminadaId != null)
                {
                    var listaPredeterminada = await _listaPrecioRepository.GetByIdAsync(listaPredeterminadaId.Value);
                    if (listaPredeterminada != null)
                    {
                        _logger.LogInformation("Asignando lista de precios predeterminada global {ListaPrecioId} ({Codigo}) a nuevo cliente para empresa {EmpresaId}", 
                            listaPredeterminadaId, listaPredeterminada.Codigo, empresaId);
                        return listaPredeterminadaId;
                    }
                }
                
                // Como último recurso, buscar la primera lista activa (preferiblemente con código \"1\")
                var listasActivas = await _listaPrecioRepository.GetAllActiveAsync();
                var listaPorDefecto = listasActivas
                    .OrderBy(lp => lp.Codigo == "1" ? 0 : 1) // Priorizar la lista con código \"1\"
                    .ThenBy(lp => lp.Id)
                    .FirstOrDefault();
                
                if (listaPorDefecto != null)
                {
                    _logger.LogInformation("Asignando lista de precios por defecto {ListaPrecioId} ({Codigo}) a nuevo cliente para empresa {EmpresaId}", 
                        listaPorDefecto.Id, listaPorDefecto.Codigo, empresaId);
                    return listaPorDefecto.Id;
                }
                
                _logger.LogWarning("No se encontró ninguna lista de precios activa para empresa {EmpresaId}", empresaId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener lista de precios por defecto para empresa {EmpresaId}", empresaId);
                return null;
            }
        }
    }
}