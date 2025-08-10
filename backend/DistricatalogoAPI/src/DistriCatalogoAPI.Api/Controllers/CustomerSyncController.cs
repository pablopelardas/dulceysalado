using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Text.Json.Serialization;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/customer-sync")]
    [Authorize]
    public class CustomerSyncController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerSyncController> _logger;

        public CustomerSyncController(IMediator mediator, ILogger<CustomerSyncController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/customer-sync/session/start
        /// Inicia una nueva sesión de sincronización de clientes
        /// </summary>
        [HttpPost("session/start")]
        public async Task<IActionResult> StartSession([FromBody] StartCustomerSyncRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                var ipOrigen = GetClientIpAddress();

                // TODO: Implementar StartCustomerSyncSessionCommand
                _logger.LogInformation("Iniciando sesión de sync de clientes para empresa {EmpresaId}", empresaId);

                return Ok(new
                {
                    success = true,
                    message = "Sesión de sincronización de clientes iniciada",
                    session_id = Guid.NewGuid().ToString(),
                    empresa_id = empresaId,
                    fecha_inicio = DateTime.UtcNow,
                    total_registros_esperados = request.TotalRegistrosEsperados,
                    usuario_proceso = request.UsuarioProceso
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión de sincronización de clientes");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// POST /api/customer-sync/customers/bulk
        /// Procesa un lote de clientes
        /// </summary>
        [HttpPost("customers/bulk")]
        public async Task<IActionResult> ProcessBulkCustomers([FromBody] ProcessBulkCustomersRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                // TODO: Implementar ProcessBulkCustomersCommand
                _logger.LogInformation("Procesando lote de {Count} clientes para empresa {EmpresaId}", 
                    request.Clientes.Count, empresaId);

                return Ok(new
                {
                    success = true,
                    message = "Lote de clientes procesado correctamente",
                    session_id = request.SessionId,
                    registros_procesados = request.Clientes.Count,
                    registros_insertados = 0, // TODO: obtener del comando
                    registros_actualizados = 0, // TODO: obtener del comando
                    errores = new string[0]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar lote de clientes");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// POST /api/customer-sync/session/end
        /// Finaliza la sesión de sincronización
        /// </summary>
        [HttpPost("session/end")]
        public async Task<IActionResult> EndSession([FromBody] EndCustomerSyncRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                // TODO: Implementar EndCustomerSyncSessionCommand
                _logger.LogInformation("Finalizando sesión de sync de clientes {SessionId} para empresa {EmpresaId}", 
                    request.SessionId, empresaId);

                return Ok(new
                {
                    success = true,
                    message = "Sesión de sincronización finalizada",
                    session_id = request.SessionId,
                    fecha_fin = DateTime.UtcNow,
                    estado = "COMPLETADA"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar sesión de sincronización de clientes");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/customer-sync/session/{sessionId}/status
        /// Obtiene el estado de una sesión de sincronización
        /// </summary>
        [HttpGet("session/{sessionId}/status")]
        public Task<IActionResult> GetSessionStatus(string sessionId)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                // TODO: Implementar GetCustomerSyncSessionQuery
                _logger.LogInformation("Consultando estado de sesión {SessionId} para empresa {EmpresaId}", 
                    sessionId, empresaId);

                return Task.FromResult<IActionResult>(Ok(new
                {
                    success = true,
                    session_id = sessionId,
                    estado = "EN_PROGRESO",
                    registros_procesados = 0,
                    total_esperado = 0,
                    fecha_inicio = DateTime.UtcNow.AddHours(-1),
                    fecha_fin = (DateTime?)null,
                    errores = new string[0]
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar estado de sesión de clientes");
                return Task.FromResult(HandleError(ex));
            }
        }

        private int GetEmpresaIdFromToken()
        {
            var empresaIdClaim = User.FindFirst("empresa_id")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
            {
                throw new UnauthorizedAccessException("Token inválido: empresa_id no encontrado");
            }
            return empresaId;
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            return ipAddress ?? "unknown";
        }

        private IActionResult HandleError(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => Unauthorized(new { message = ex.Message }),
                ArgumentException => BadRequest(new { message = ex.Message }),
                InvalidOperationException => BadRequest(new { message = ex.Message }),
                _ => StatusCode(500, new { message = "Error interno del servidor", error = ex.Message })
            };
        }
    }

    // DTOs para la sincronización de clientes
    public class StartCustomerSyncRequest
    {
        [JsonPropertyName("total_registros_esperados")]
        public int TotalRegistrosEsperados { get; set; }

        [JsonPropertyName("usuario_proceso")]
        public string UsuarioProceso { get; set; } = string.Empty;
    }

    public class ProcessBulkCustomersRequest
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("lote_numero")]
        public int LoteNumero { get; set; }

        [JsonPropertyName("clientes")]
        public List<CustomerSyncData> Clientes { get; set; } = new();
    }

    public class EndCustomerSyncRequest
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;
    }

    public class CustomerSyncData
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; }

        [JsonPropertyName("localidad")]
        public string? Localidad { get; set; }

        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; }

        [JsonPropertyName("cuit")]
        public string? Cuit { get; set; }

        [JsonPropertyName("altura")]
        public string? Altura { get; set; }

        [JsonPropertyName("provincia")]
        public string? Provincia { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("tipo_iva")]
        public string? TipoIva { get; set; }

        [JsonPropertyName("lista_precio_id")]
        public int? ListaPrecioId { get; set; }
    }
}