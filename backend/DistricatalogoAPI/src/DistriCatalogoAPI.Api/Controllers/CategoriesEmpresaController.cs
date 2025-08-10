using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Application.Queries.Categories;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/categories/empresa")]
    [Authorize]
    public class CategoriesEmpresaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoriesEmpresaController> _logger;

        public CategoriesEmpresaController(IMediator mediator, ILogger<CategoriesEmpresaController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener categorías de empresa
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<GetCategoriesEmpresaQueryResult>> GetByEmpresa(
            [FromQuery] int? empresaId = null,
            [FromQuery] bool? visibleOnly = null)
        {
            try
            {
                var query = new GetCategoriesEmpresaQuery
                {
                    EmpresaId = empresaId ?? 0,
                    VisibleOnly = visibleOnly
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener categorías empresa");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener categoría empresa por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCategoryEmpresaByIdQueryResult>> GetById(int id)
        {
            try
            {
                var query = new GetCategoryEmpresaByIdQuery { Id = id };
                var result = await _mediator.Send(query);
                
                if (result == null)
                    return NotFound();
                    
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener categoría empresa por ID");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría empresa por ID {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crear nueva categoría empresa
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateCategoryEmpresaCommandResult>> Create([FromBody] CreateCategoryEmpresaCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al crear categoría empresa");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear categoría empresa");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar categoría empresa
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateCategoryEmpresaCommandResult>> Update(int id, [FromBody] UpdateCategoryEmpresaCommand command)
        {
            try
            {
                command.Id = id;
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al actualizar categoría empresa");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar categoría empresa");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar categoría empresa
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var command = new DeleteCategoryEmpresaCommand { Id = id };
                var result = await _mediator.Send(command);
                
                if (result)
                    return NoContent();
                else
                    return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar categoría empresa");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al eliminar categoría empresa");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}