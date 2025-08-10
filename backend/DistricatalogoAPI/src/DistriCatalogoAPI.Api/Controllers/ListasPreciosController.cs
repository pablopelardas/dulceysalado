using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using DistriCatalogoAPI.Application.Queries.ListasPrecios;
using DistriCatalogoAPI.Application.Commands.ListasPrecios;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/listas-precios")]
    [Authorize]
    public class ListasPreciosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ListasPreciosController> _logger;

        public ListasPreciosController(IMediator mediator, ILogger<ListasPreciosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/listas-precios
        /// Obtiene todas las listas de precios disponibles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetListasPrecios()
        {
            try
            {
                var query = new GetAllListasPreciosQuery();
                var result = await _mediator.Send(query);
                
                return Ok(new
                {
                    success = true,
                    listas = result.Listas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listas de precios");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/listas-precios/{id}
        /// Obtiene una lista de precios específica
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetListaPrecio(int id)
        {
            try
            {
                var query = new GetListaPrecioByIdQuery { Id = id };
                var result = await _mediator.Send(query);
                
                if (result == null)
                    return NotFound(new { success = false, message = "Lista de precios no encontrada" });
                
                return Ok(new
                {
                    success = true,
                    lista = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de precios {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// POST /api/listas-precios
        /// Crea una nueva lista de precios (solo empresa principal)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateListaPrecio([FromBody] CreateListaPrecioRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                var command = new CreateListaPrecioCommand
                {
                    Codigo = request.Codigo,
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Orden = request.Orden,
                    EmpresaId = empresaId
                };

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    lista = new { id = result.ListaId, codigo = request.Codigo, nombre = request.Nombre }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lista de precios");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// PUT /api/listas-precios/{id}
        /// Actualiza una lista de precios (solo empresa principal)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateListaPrecio(int id, [FromBody] UpdateListaPrecioRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                var command = new UpdateListaPrecioCommand
                {
                    Id = id,
                    Codigo = request.Codigo,
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Activa = request.Activa,
                    Orden = request.Orden,
                    EmpresaId = empresaId
                };

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar lista de precios {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// DELETE /api/listas-precios/{id}
        /// Elimina una lista de precios (solo empresa principal, si no tiene precios asociados)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListaPrecio(int id)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                var command = new DeleteListaPrecioCommand
                {
                    Id = id,
                    EmpresaId = empresaId
                };

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lista de precios {Id}", id);
                return HandleError(ex);
            }
        }

        #region Private Methods

        private int GetEmpresaIdFromToken()
        {
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
            {
                throw new UnauthorizedAccessException("Token no contiene información válida de empresa");
            }
            return empresaId;
        }

        private void ValidateEmpresaPrincipal()
        {
            var tipoEmpresaClaim = User.FindFirst("tipo_empresa")?.Value;
            if (tipoEmpresaClaim != "principal")
            {
                throw new UnauthorizedAccessException("Solo las empresas principales pueden gestionar listas de precios");
            }
        }

        private IActionResult HandleError(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                }),
                InvalidOperationException => BadRequest(new
                {
                    success = false,
                    message = ex.Message
                }),
                ArgumentException => BadRequest(new
                {
                    success = false,
                    message = ex.Message
                }),
                _ => StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                        ? ex.Message 
                        : null
                })
            };
        }

        #endregion

        #region DTOs

        public class CreateListaPrecioRequest
        {
            public string Codigo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public int? Orden { get; set; }
        }

        public class UpdateListaPrecioRequest
        {
            public string? Codigo { get; set; }
            public string? Nombre { get; set; }
            public string? Descripcion { get; set; }
            public bool? Activa { get; set; }
            public int? Orden { get; set; }
        }

        #endregion
    }
}