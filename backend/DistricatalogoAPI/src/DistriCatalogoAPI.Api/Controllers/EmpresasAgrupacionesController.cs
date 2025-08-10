using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/empresas/{empresaId}/agrupaciones")]
    [Authorize]
    public class EmpresasAgrupacionesController : ControllerBase
    {
        private readonly IAgrupacionRepository _agrupacionRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<EmpresasAgrupacionesController> _logger;

        public EmpresasAgrupacionesController(
            IAgrupacionRepository agrupacionRepository,
            ICompanyRepository companyRepository,
            ILogger<EmpresasAgrupacionesController> logger)
        {
            _agrupacionRepository = agrupacionRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/empresas/{empresaId}/agrupaciones
        /// Obtiene las agrupaciones visibles para una empresa específica
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVisibleAgrupaciones(int empresaId)
        {
            try
            {
                var currentEmpresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                // Verificar que la empresa existe y pertenece a la empresa principal
                var empresa = await _companyRepository.GetByIdAsync(empresaId);
                if (empresa == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Empresa no encontrada"
                    });
                }

                // Verificar que la empresa pertenece a la empresa principal actual
                if (empresa.EmpresaPrincipalId != currentEmpresaId && empresa.Id != currentEmpresaId)
                {
                    return Forbid();
                }

                // Obtener todas las agrupaciones de la empresa principal
                var todasLasAgrupaciones = await _agrupacionRepository.GetByEmpresaPrincipalAsync(currentEmpresaId);

                // Obtener las agrupaciones visibles para la empresa específica
                var agrupacionesVisibles = await _agrupacionRepository.GetVisibleByEmpresaAsync(empresaId);
                var agrupacionesVisiblesIds = agrupacionesVisibles.Select(a => a.Id).ToHashSet();

                var result = todasLasAgrupaciones.Select(a => new
                {
                    id = a.Id,
                    codigo = a.Codigo,
                    nombre = a.Nombre,
                    descripcion = a.Descripcion,
                    activa = a.Activa,
                    visible = agrupacionesVisiblesIds.Contains(a.Id)
                }).ToList();

                return Ok(new
                {
                    success = true,
                    empresa_id = empresaId,
                    agrupaciones = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones visibles para empresa {EmpresaId}", empresaId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// PUT /api/empresas/{empresaId}/agrupaciones
        /// Configura qué agrupaciones son visibles para una empresa específica
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> SetVisibleAgrupaciones(int empresaId, [FromBody] SetVisibleAgrupacionesRequest request)
        {
            try
            {
                var currentEmpresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                // Verificar que la empresa existe y pertenece a la empresa principal
                var empresa = await _companyRepository.GetByIdAsync(empresaId);
                if (empresa == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Empresa no encontrada"
                    });
                }

                // Verificar que la empresa pertenece a la empresa principal actual
                if (empresa.EmpresaPrincipalId != currentEmpresaId && empresa.Id != currentEmpresaId)
                {
                    return Forbid();
                }

                // Verificar que todas las agrupaciones existen y pertenecen a la empresa principal
                var agrupacionesIds = request.AgrupacionesIds ?? new List<int>();
                if (agrupacionesIds.Any())
                {
                    var agrupacionesExistentes = await _agrupacionRepository.GetByIdsAsync(agrupacionesIds);
                    var agrupacionesExistentesIds = agrupacionesExistentes.Select(a => a.Id).ToHashSet();
                    
                    var agrupacionesInvalidas = agrupacionesIds.Where(id => !agrupacionesExistentesIds.Contains(id)).ToList();
                    if (agrupacionesInvalidas.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Agrupaciones no encontradas: {string.Join(", ", agrupacionesInvalidas)}"
                        });
                    }

                    // Verificar que todas las agrupaciones pertenecen a la empresa principal
                    var agrupacionesDeOtraEmpresa = agrupacionesExistentes.Where(a => a.EmpresaPrincipalId != currentEmpresaId).ToList();
                    if (agrupacionesDeOtraEmpresa.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "No se pueden asignar agrupaciones de otra empresa principal"
                        });
                    }
                }

                // Actualizar la visibilidad de las agrupaciones
                await _agrupacionRepository.SetVisibilityForEmpresaAsync(empresaId, agrupacionesIds);

                _logger.LogInformation("Configuración de visibilidad actualizada para empresa {EmpresaId}: {Count} agrupaciones visibles", 
                    empresaId, agrupacionesIds.Count);

                return Ok(new
                {
                    success = true,
                    message = "Configuración de visibilidad actualizada correctamente",
                    empresa_id = empresaId,
                    agrupaciones_visibles = agrupacionesIds.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar agrupaciones visibles para empresa {EmpresaId}", empresaId);
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
                throw new UnauthorizedAccessException("Solo las empresas principales pueden gestionar agrupaciones");
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

        /// <summary>
        /// PUT /api/empresas/agrupaciones/bulk
        /// Configura visibilidad de agrupaciones para múltiples empresas de forma masiva
        /// </summary>
        [HttpPut("../agrupaciones/bulk")]
        public async Task<IActionResult> SetBulkVisibleAgrupaciones([FromBody] BulkSetVisibleAgrupacionesRequest request)
        {
            try
            {
                var currentEmpresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                // Validar que todas las empresas existen y pertenecen a la empresa principal
                var empresasIds = request.Configuraciones.Select(c => c.EmpresaId).Distinct().ToList();
                var empresasExistentes = await _companyRepository.GetByIdsAsync(empresasIds);
                var empresasValidadas = empresasExistentes
                    .Where(e => e.EmpresaPrincipalId == currentEmpresaId || e.Id == currentEmpresaId)
                    .Select(e => e.Id)
                    .ToHashSet();

                var empresasInvalidas = empresasIds.Where(id => !empresasValidadas.Contains(id)).ToList();
                if (empresasInvalidas.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Empresas no válidas: {string.Join(", ", empresasInvalidas)}"
                    });
                }

                // Validar que todas las agrupaciones existen
                var todasLasAgrupaciones = request.Configuraciones
                    .SelectMany(c => c.AgrupacionesIds)
                    .Distinct()
                    .ToList();

                if (todasLasAgrupaciones.Any())
                {
                    var agrupacionesExistentes = await _agrupacionRepository.GetByIdsAsync(todasLasAgrupaciones);
                    var agrupacionesValidadas = agrupacionesExistentes
                        .Where(a => a.EmpresaPrincipalId == currentEmpresaId)
                        .Select(a => a.Id)
                        .ToHashSet();

                    var agrupacionesInvalidas = todasLasAgrupaciones.Where(id => !agrupacionesValidadas.Contains(id)).ToList();
                    if (agrupacionesInvalidas.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Agrupaciones no válidas: {string.Join(", ", agrupacionesInvalidas)}"
                        });
                    }
                }

                // Procesar configuraciones en lotes para mejor performance
                var resultados = new List<object>();
                var errores = new List<string>();

                foreach (var config in request.Configuraciones)
                {
                    try
                    {
                        await _agrupacionRepository.SetVisibilityForEmpresaAsync(config.EmpresaId, config.AgrupacionesIds);
                        resultados.Add(new
                        {
                            empresa_id = config.EmpresaId,
                            agrupaciones_configuradas = config.AgrupacionesIds.Count,
                            success = true
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error configurando visibilidad para empresa {EmpresaId}", config.EmpresaId);
                        errores.Add($"Empresa {config.EmpresaId}: {ex.Message}");
                        resultados.Add(new
                        {
                            empresa_id = config.EmpresaId,
                            success = false,
                            error = ex.Message
                        });
                    }
                }

                var exitosos = resultados.Count(r => ((dynamic)r).success);
                
                _logger.LogInformation("Configuración bulk completada: {Exitosos}/{Total} empresas procesadas exitosamente", 
                    exitosos, request.Configuraciones.Count);

                return Ok(new
                {
                    success = errores.Count == 0,
                    message = errores.Count == 0 
                        ? "Configuración bulk completada exitosamente" 
                        : $"Configuración parcialmente exitosa: {errores.Count} errores",
                    empresas_procesadas = request.Configuraciones.Count,
                    empresas_exitosas = exitosos,
                    empresas_con_errores = errores.Count,
                    resultados = resultados,
                    errores = errores
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en configuración bulk de agrupaciones");
                return HandleError(ex);
            }
        }

        public class SetVisibleAgrupacionesRequest
        {
            public List<int> AgrupacionesIds { get; set; } = new();
        }

        public class BulkSetVisibleAgrupacionesRequest
        {
            public List<EmpresaAgrupacionConfig> Configuraciones { get; set; } = new();
        }

        public class EmpresaAgrupacionConfig
        {
            public int EmpresaId { get; set; }
            public List<int> AgrupacionesIds { get; set; } = new();
        }

        #endregion
    }
}