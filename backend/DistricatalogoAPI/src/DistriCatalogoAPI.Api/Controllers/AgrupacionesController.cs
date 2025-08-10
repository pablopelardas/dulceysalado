using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/agrupaciones")]
    [Authorize]
    public class AgrupacionesController : ControllerBase
    {
        private readonly IAgrupacionRepository _agrupacionRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<AgrupacionesController> _logger;

        public AgrupacionesController(
            IAgrupacionRepository agrupacionRepository,
            ICompanyRepository companyRepository,
            ILogger<AgrupacionesController> logger)
        {
            _agrupacionRepository = agrupacionRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/agrupaciones
        /// Lista todas las agrupaciones de la empresa principal
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] bool? activa = null,
            [FromQuery] string? busqueda = null,
            [FromQuery] int type = 3)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                var (agrupaciones, total) = await _agrupacionRepository.GetPagedAsync(
                    empresaId, page, pageSize, activa, busqueda, type);

                return Ok(new
                {
                    success = true,
                    agrupaciones = agrupaciones.Select(a => new
                    {
                        id = a.Id,
                        codigo = a.Codigo,
                        nombre = a.Nombre,
                        descripcion = a.Descripcion,
                        activa = a.Activa,
                        empresa_principal_id = a.EmpresaPrincipalId,
                        created_at = a.CreatedAt,
                        updated_at = a.UpdatedAt,
                        tipo = a.Tipo
                    }),
                    pagination = new
                    {
                        total = total,
                        page = page,
                        page_size = pageSize,
                        total_pages = (int)Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/agrupaciones/{id}
        /// Obtiene una agrupación específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                var agrupacion = await _agrupacionRepository.GetByIdAsync(id);
                if (agrupacion == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Agrupación no encontrada"
                    });
                }

                // Verificar que pertenece a la empresa
                if (agrupacion.EmpresaPrincipalId != empresaId)
                {
                    return Forbid();
                }

                return Ok(new
                {
                    success = true,
                    agrupacion = new
                    {
                        id = agrupacion.Id,
                        codigo = agrupacion.Codigo,
                        nombre = agrupacion.Nombre,
                        descripcion = agrupacion.Descripcion,
                        activa = agrupacion.Activa,
                        empresa_principal_id = agrupacion.EmpresaPrincipalId,
                        created_at = agrupacion.CreatedAt,
                        updated_at = agrupacion.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupación {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// PUT /api/agrupaciones/{id}
        /// Actualiza una agrupación existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAgrupacionRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                var agrupacion = await _agrupacionRepository.GetByIdAsync(id);
                if (agrupacion == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Agrupación no encontrada"
                    });
                }

                // Verificar que pertenece a la empresa
                if (agrupacion.EmpresaPrincipalId != empresaId)
                {
                    return Forbid();
                }

                // Actualizar la agrupación
                agrupacion.Update(
                    nombre: request.Nombre,
                    descripcion: request.Descripcion,
                    activa: request.Activa);

                await _agrupacionRepository.UpdateAsync(agrupacion);

                return Ok(new
                {
                    success = true,
                    message = "Agrupación actualizada exitosamente",
                    agrupacion = new
                    {
                        id = agrupacion.Id,
                        codigo = agrupacion.Codigo,
                        nombre = agrupacion.Nombre,
                        descripcion = agrupacion.Descripcion,
                        activa = agrupacion.Activa
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar agrupación {Id}", id);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/agrupaciones/stats
        /// Obtiene estadísticas de agrupaciones de la empresa
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                ValidateEmpresaPrincipal();

                var totalAgrupaciones = await _agrupacionRepository.GetCountByEmpresaPrincipalAsync(empresaId);
                var todasLasAgrupaciones = await _agrupacionRepository.GetByEmpresaPrincipalAsync(empresaId, includeInactive: true);
                
                var activas = todasLasAgrupaciones.Count(a => a.IsActive());
                var inactivas = todasLasAgrupaciones.Count(a => !a.IsActive());

                return Ok(new
                {
                    success = true,
                    estadisticas = new
                    {
                        total_agrupaciones = totalAgrupaciones,
                        agrupaciones_activas = activas,
                        agrupaciones_inactivas = inactivas,
                        empresa_principal_id = empresaId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de agrupaciones");
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

        public class UpdateAgrupacionRequest
        {
            public string? Nombre { get; set; }
            public string? Descripcion { get; set; }
            public bool? Activa { get; set; }
        }

        #endregion
    }
}