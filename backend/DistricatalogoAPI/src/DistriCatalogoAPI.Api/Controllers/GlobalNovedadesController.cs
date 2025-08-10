using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Novedades;
using DistriCatalogoAPI.Application.Queries.Novedades;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/empresas-novedades")]
    [Authorize]
    public class GlobalNovedadesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GlobalNovedadesController> _logger;

        public GlobalNovedadesController(
            IMediator mediator,
            ILogger<GlobalNovedadesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/empresas-novedades/{empresaId}
        /// Obtiene todas las novedades de una empresa con paginación
        /// </summary>
        [HttpGet("{empresaId}")]
        public async Task<IActionResult> GetNovedadesByEmpresa(
            int empresaId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? visible = null)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var query = new GetNovedadesByEmpresaQuery(empresaId, page, pageSize, visible);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener novedades por empresa {EmpresaId}", empresaId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/empresas-novedades/novedad/{id}
        /// Obtiene una novedad específica por ID
        /// </summary>
        [HttpGet("novedad/{id}")]
        public async Task<IActionResult> GetNovedadById(int id)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var query = new GetNovedadByIdQuery(id);
                var novedad = await _mediator.Send(query);

                if (novedad == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Novedad no encontrada"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = novedad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener novedad por ID {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// POST /api/empresas-novedades
        /// Crea una nueva novedad para una empresa
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNovedad([FromBody] CreateNovedadRequestDto request)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var command = new CreateEmpresaNovedadCommand(request.EmpresaId, request.AgrupacionId, request.Visible);
                var novedad = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetNovedadById),
                    new { id = novedad.Id },
                    new
                    {
                        success = true,
                        message = "Novedad creada exitosamente",
                        data = novedad
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear novedad para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", 
                    request.EmpresaId, request.AgrupacionId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// PUT /api/empresas-novedades/{id}
        /// Actualiza una novedad existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNovedad(int id, [FromBody] UpdateNovedadRequestDto request)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var command = new UpdateEmpresaNovedadCommand(id, request.Visible);
                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Novedad actualizada exitosamente"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Error al actualizar novedad"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar novedad con ID {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// DELETE /api/empresas-novedades/{id}
        /// Elimina una novedad
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNovedad(int id)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var command = new DeleteEmpresaNovedadCommand(id);
                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Novedad eliminada exitosamente"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Error al eliminar novedad"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar novedad con ID {Id}", id);
                return HandleError(ex);
            }
        }

        #region Private Methods

        private void ValidateEmpresaPrincipal()
        {
            var tipoEmpresaClaim = User.FindFirst("tipo_empresa")?.Value;
            if (tipoEmpresaClaim != "principal")
            {
                throw new UnauthorizedAccessException("Solo las empresas principales pueden gestionar novedades");
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
                    message = "Error interno del servidor"
                })
            };
        }

        #endregion

        #region DTOs

        public class CreateNovedadRequestDto
        {
            public int EmpresaId { get; set; }
            public int AgrupacionId { get; set; }
            public bool Visible { get; set; } = true;
        }

        public class UpdateNovedadRequestDto
        {
            public bool Visible { get; set; }
        }

        #endregion
    }
}