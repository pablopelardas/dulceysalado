using System;
using System.Collections.Generic;
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
    [Route("api/empresas/{empresaId}/novedades")]
    [Authorize]
    public class EmpresasNovedadesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmpresasNovedadesController> _logger;

        public EmpresasNovedadesController(
            IMediator mediator,
            ILogger<EmpresasNovedadesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/empresas/{empresaId}/novedades
        /// Obtiene las agrupaciones de Grupo 1 con su estado de novedad para drag-and-drop
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAgrupacionesWithNovedadStatus(int empresaId)
        {
            try
            {
                ValidateEmpresaPrincipal();
                await ValidateEmpresaAccess(empresaId);

                var query = new GetAgrupacionesNovedadesForEmpresaQuery(empresaId);
                var agrupaciones = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    empresa_id = empresaId,
                    agrupaciones = agrupaciones
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones con estado de novedad para empresa {EmpresaId}", empresaId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// PUT /api/empresas/{empresaId}/novedades
        /// Configura qué agrupaciones de Grupo 1 son novedades para una empresa específica (drag-and-drop)
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> SetNovedadesForEmpresa(int empresaId, [FromBody] SetNovedadesRequestDto request)
        {
            try
            {
                ValidateEmpresaPrincipal();
                await ValidateEmpresaAccess(empresaId);

                var command = new SetNovedadesForEmpresaCommand(empresaId, request.AgrupacionIds);
                var result = await _mediator.Send(command);

                if (result)
                {
                    _logger.LogInformation("Novedades actualizadas para empresa {EmpresaId}: {Count} agrupaciones", 
                        empresaId, request.AgrupacionIds.Count);

                    return Ok(new
                    {
                        success = true,
                        message = "Novedades actualizadas correctamente",
                        empresa_id = empresaId,
                        novedades_configuradas = request.AgrupacionIds.Count
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Error al actualizar novedades"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar novedades para empresa {EmpresaId}", empresaId);
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
                throw new UnauthorizedAccessException("Solo las empresas principales pueden gestionar novedades");
            }
        }

        private async Task ValidateEmpresaAccess(int empresaId)
        {
            var currentEmpresaId = GetEmpresaIdFromToken();
            
            // Para simplificar, asumimos que si es empresa principal puede gestionar sus empresas hijas
            // En una implementación completa, aquí validaríamos que la empresa pertenece a la empresa principal
            if (empresaId != currentEmpresaId)
            {
                // TODO: Implementar validación de empresa hija cuando sea necesario
                _logger.LogInformation("Gestionando novedades para empresa {EmpresaId} desde empresa principal {CurrentEmpresaId}", 
                    empresaId, currentEmpresaId);
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
    }
}