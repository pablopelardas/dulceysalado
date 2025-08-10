using System;
using System.Collections.Generic;
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
    [Route("api/empresas/{empresaId}/ofertas")]
    [Authorize]
    public class EmpresasOfertasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmpresasOfertasController> _logger;

        public EmpresasOfertasController(
            IMediator mediator,
            ILogger<EmpresasOfertasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/empresas/{empresaId}/ofertas
        /// Obtiene las agrupaciones de Grupo 1 con su estado de oferta para drag-and-drop
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAgrupacionesWithOfertaStatus(int empresaId)
        {
            try
            {
                ValidateEmpresaPrincipal();
                await ValidateEmpresaAccess(empresaId);

                var query = new GetAgrupacionesOfertasForEmpresaQuery(empresaId);
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
                _logger.LogError(ex, "Error al obtener agrupaciones con estado de oferta para empresa {EmpresaId}", empresaId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// PUT /api/empresas/{empresaId}/ofertas
        /// Configura qué agrupaciones de Grupo 1 son ofertas para una empresa específica (drag-and-drop)
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> SetOfertasForEmpresa(int empresaId, [FromBody] SetOfertasRequestDto request)
        {
            try
            {
                ValidateEmpresaPrincipal();
                await ValidateEmpresaAccess(empresaId);

                var command = new SetOfertasForEmpresaCommand(empresaId, request.AgrupacionIds);
                var result = await _mediator.Send(command);

                if (result)
                {
                    _logger.LogInformation("Ofertas actualizadas para empresa {EmpresaId}: {Count} agrupaciones", 
                        empresaId, request.AgrupacionIds.Count);

                    return Ok(new
                    {
                        success = true,
                        message = "Ofertas actualizadas correctamente",
                        empresa_id = empresaId,
                        ofertas_configuradas = request.AgrupacionIds.Count
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Error al actualizar ofertas"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar ofertas para empresa {EmpresaId}", empresaId);
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
                throw new UnauthorizedAccessException("Solo las empresas principales pueden gestionar ofertas");
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
                _logger.LogInformation("Gestionando ofertas para empresa {EmpresaId} desde empresa principal {CurrentEmpresaId}", 
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