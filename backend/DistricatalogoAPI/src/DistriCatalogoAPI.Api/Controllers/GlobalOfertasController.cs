using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Ofertas;
using DistriCatalogoAPI.Application.Queries.Ofertas;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/empresas-ofertas")]
    [Authorize]
    public class GlobalOfertasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GlobalOfertasController> _logger;

        public GlobalOfertasController(
            IMediator mediator,
            ILogger<GlobalOfertasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/empresas-ofertas/{empresaId}
        /// Obtiene todas las ofertas de una empresa con paginación
        /// </summary>
        [HttpGet("{empresaId}")]
        public async Task<IActionResult> GetOfertasByEmpresa(
            int empresaId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? visible = null)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var query = new GetOfertasByEmpresaQuery(empresaId, page, pageSize, visible);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ofertas por empresa {EmpresaId}", empresaId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/empresas-ofertas/oferta/{id}
        /// Obtiene una oferta específica por ID
        /// </summary>
        [HttpGet("oferta/{id}")]
        public async Task<IActionResult> GetOfertaById(int id)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var query = new GetOfertaByIdQuery(id);
                var oferta = await _mediator.Send(query);

                if (oferta == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Oferta no encontrada"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = oferta
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener oferta por ID {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// POST /api/empresas-ofertas
        /// Crea una nueva oferta para una empresa
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOferta([FromBody] CreateOfertaRequestDto request)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var command = new CreateEmpresaOfertaCommand(request.EmpresaId, request.AgrupacionId, request.Visible);
                var oferta = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetOfertaById),
                    new { id = oferta.Id },
                    new
                    {
                        success = true,
                        message = "Oferta creada exitosamente",
                        data = oferta
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear oferta para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", 
                    request.EmpresaId, request.AgrupacionId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// PUT /api/empresas-ofertas/{id}
        /// Actualiza una oferta existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOferta(int id, [FromBody] UpdateOfertaRequestDto request)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var command = new UpdateEmpresaOfertaCommand(id, request.Visible);
                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Oferta actualizada exitosamente"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Error al actualizar oferta"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar oferta con ID {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// DELETE /api/empresas-ofertas/{id}
        /// Elimina una oferta
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOferta(int id)
        {
            try
            {
                ValidateEmpresaPrincipal();

                var command = new DeleteEmpresaOfertaCommand(id);
                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Oferta eliminada exitosamente"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Error al eliminar oferta"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar oferta con ID {Id}", id);
                return HandleError(ex);
            }
        }

        #region Private Methods

        private void ValidateEmpresaPrincipal()
        {
            var tipoEmpresaClaim = User.FindFirst("tipo_empresa")?.Value;
            if (tipoEmpresaClaim != "principal")
            {
                throw new UnauthorizedAccessException("Solo las empresas principales pueden gestionar ofertas");
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

        public class CreateOfertaRequestDto
        {
            public int EmpresaId { get; set; }
            public int AgrupacionId { get; set; }
            public bool Visible { get; set; } = true;
        }

        public class UpdateOfertaRequestDto
        {
            public bool Visible { get; set; }
        }

        #endregion
    }
}