using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Application.Queries.Categories;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/categories/base")]
    [Authorize]
    public class CategoriesBaseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoriesBaseController> _logger;

        public CategoriesBaseController(IMediator mediator, ILogger<CategoriesBaseController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener categoría base por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCategoryByIdQueryResult>> GetById(int id)
        {
            try
            {
                var query = new GetCategoryByIdQuery
                {
                    Id = id,
                    IsBaseCategory = true
                };

                var result = await _mediator.Send(query);
                
                if (result == null)
                    return NotFound(new { message = "Categoría base no encontrada" });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener categoría base por ID");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría base por ID {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener todas las categorías base
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<GetCategoriesBaseQueryResult>> GetAll(
            [FromQuery] bool? visibleOnly = null,
            [FromQuery] int? empresaId = null)
        {
            try
            {
                var query = new GetCategoriesBaseQuery
                {
                    VisibleOnly = visibleOnly,
                    EmpresaId = empresaId
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener categorías base");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías base");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crear nueva categoría base (Solo empresa principal)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateCategoryBaseCommandResult>> Create([FromBody] CreateCategoryBaseCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al crear categoría base");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear categoría base");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría base");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar categoría base (Solo empresa principal)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateCategoryBaseCommandResult>> Update(int id, [FromBody] UpdateCategoryBaseCommand command)
        {
            try
            {
                command.Id = id;
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al actualizar categoría base");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar categoría base");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría base");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar categoría base (Solo empresa principal)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var command = new DeleteCategoryBaseCommand { Id = id };
                var result = await _mediator.Send(command);
                
                if (result)
                    return NoContent();
                else
                    return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al eliminar categoría base");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al eliminar categoría base");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría base");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}