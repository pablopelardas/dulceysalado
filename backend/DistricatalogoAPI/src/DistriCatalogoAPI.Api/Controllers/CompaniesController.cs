using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Companies;
using DistriCatalogoAPI.Application.Queries.Companies;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Api.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFileService _fileService;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(IMediator mediator, IFileService fileService, ILogger<CompaniesController> logger)
        {
            _mediator = mediator;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<CompanyDto>>> GetCompanies(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? principalCompanyId = null,
            [FromQuery] bool includeInactive = false)
        {
            var requestingUserId = GetRequestingUserId();
            
            var query = new GetCompaniesListQuery
            {
                Page = page,
                PageSize = pageSize,
                PrincipalCompanyId = principalCompanyId,
                IncludeInactive = includeInactive,
                RequestingUserId = requestingUserId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id, [FromQuery] bool includeInactive = false)
        {
            var requestingUserId = GetRequestingUserId();
            
            var query = new GetCompanyByIdQuery
            {
                CompanyId = id,
                IncludeInactive = includeInactive,
                RequestingUserId = requestingUserId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyCommand command)
        {
            var requestingUserId = GetRequestingUserId();
            command.RequestingUserId = requestingUserId;

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCompany), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CompanyDto>> UpdateCompany(int id, [FromBody] UpdateCompanyCommand command)
        {
            var requestingUserId = GetRequestingUserId();
            command.CompanyId = id;
            command.RequestingUserId = requestingUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCompany(int id)
        {
            var requestingUserId = GetRequestingUserId();
            
            var command = new DeleteCompanyCommand
            {
                CompanyId = id,
                RequestingUserId = requestingUserId
            };

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/upload-logo")]
        public async Task<ActionResult> UploadLogo(int id, IFormFile logo)
        {
            try
            {
                using var activity = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["CompanyId"] = id,
                    ["Operation"] = "UploadLogo"
                });

                _logger.LogInformation("Uploading logo for company {CompanyId}", id);

                // Validar archivo
                if (logo == null || logo.Length == 0)
                    return BadRequest(new { message = "No se proporcionó imagen de logo" });

                if (!_fileService.IsValidImageFile(logo))
                    return BadRequest(new { message = "Archivo inválido. Solo se permiten imágenes JPG, PNG o WEBP de máximo 5MB" });

                // Obtener empresa para verificar permisos y obtener nombre
                var requestingUserId = GetRequestingUserId();
                var query = new GetCompanyByIdQuery 
                { 
                    CompanyId = id, 
                    RequestingUserId = requestingUserId 
                };
                var company = await _mediator.Send(query);
                
                if (company == null)
                    return NotFound(new { message = "Empresa no encontrada" });

                // Eliminar logo anterior si existe y es local
                if (!string.IsNullOrEmpty(company.LogoUrl) && 
                    (company.LogoUrl.Contains("/api/images/") || company.LogoUrl.StartsWith("/api/images/")))
                {
                    _fileService.DeleteCompanyLogo(id);
                }

                // Guardar nuevo logo
                var logoPath = await _fileService.SaveCompanyLogoAsync(logo, id, company.Nombre);

                // Actualizar empresa con nueva URL de logo (solo logo)
                var updateCommand = new UpdateCompanyCommand
                {
                    CompanyId = id,
                    RequestingUserId = requestingUserId,
                    LogoUrl = logoPath // Solo actualizar logo
                };

                await _mediator.Send(updateCommand);

                _logger.LogInformation("Logo uploaded successfully for company {CompanyId}: {LogoPath}", id, logoPath);

                return Ok(new { logoUrl = logoPath, message = "Logo subido exitosamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access uploading logo for company {CompanyId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo for company {CompanyId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}/logo")]
        public async Task<ActionResult> DeleteLogo(int id)
        {
            try
            {
                using var activity = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["CompanyId"] = id,
                    ["Operation"] = "DeleteLogo"
                });

                _logger.LogInformation("Deleting logo for company {CompanyId}", id);

                // Obtener empresa para verificar permisos
                var requestingUserId = GetRequestingUserId();
                var query = new GetCompanyByIdQuery 
                { 
                    CompanyId = id, 
                    RequestingUserId = requestingUserId 
                };
                var company = await _mediator.Send(query);
                
                if (company == null)
                    return NotFound(new { message = "Empresa no encontrada" });

                // Eliminar logo si es local
                if (!string.IsNullOrEmpty(company.LogoUrl) && 
                    (company.LogoUrl.Contains("/api/images/") || company.LogoUrl.StartsWith("/api/images/")))
                {
                    _fileService.DeleteCompanyLogo(id);

                    // Actualizar empresa sin logo (solo limpiar logo)
                    var updateCommand = new UpdateCompanyCommand
                    {
                        CompanyId = id,
                        RequestingUserId = requestingUserId,
                        LogoUrl = "" // Solo eliminar logo
                    };

                    await _mediator.Send(updateCommand);
                }

                _logger.LogInformation("Logo deleted successfully for company {CompanyId}", id);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access deleting logo for company {CompanyId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting logo for company {CompanyId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("{id}/upload-favicon")]
        public async Task<ActionResult> UploadFavicon(int id, IFormFile favicon)
        {
            try
            {
                using var activity = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["CompanyId"] = id,
                    ["Operation"] = "UploadFavicon"
                });

                _logger.LogInformation("Uploading favicon for company {CompanyId}", id);

                // Validar archivo
                if (favicon == null || favicon.Length == 0)
                    return BadRequest(new { message = "No se proporcionó imagen de favicon" });

                if (!_fileService.IsValidImageFile(favicon))
                    return BadRequest(new { message = "Archivo inválido. Solo se permiten imágenes JPG, PNG o WEBP de máximo 5MB" });

                // Obtener empresa para verificar permisos y obtener nombre
                var requestingUserId = GetRequestingUserId();
                var query = new GetCompanyByIdQuery 
                { 
                    CompanyId = id, 
                    RequestingUserId = requestingUserId 
                };
                var company = await _mediator.Send(query);
                
                if (company == null)
                    return NotFound(new { message = "Empresa no encontrada" });

                // Eliminar favicon anterior si existe y es local
                if (!string.IsNullOrEmpty(company.FaviconUrl) && 
                    (company.FaviconUrl.Contains("/api/images/") || company.FaviconUrl.StartsWith("/api/images/")))
                {
                    _fileService.DeleteCompanyFavicon(id);
                }

                // Guardar nuevo favicon
                var faviconPath = await _fileService.SaveCompanyFaviconAsync(favicon, id, company.Nombre);

                // Actualizar empresa con nueva URL de favicon (solo favicon)
                var updateCommand = new UpdateCompanyCommand
                {
                    CompanyId = id,
                    RequestingUserId = requestingUserId,
                    FaviconUrl = faviconPath // Solo actualizar favicon
                };

                await _mediator.Send(updateCommand);

                _logger.LogInformation("Favicon uploaded successfully for company {CompanyId}: {FaviconPath}", id, faviconPath);

                return Ok(new { faviconUrl = faviconPath, message = "Favicon subido exitosamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access uploading favicon for company {CompanyId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading favicon for company {CompanyId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}/favicon")]
        public async Task<ActionResult> DeleteFavicon(int id)
        {
            try
            {
                using var activity = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["CompanyId"] = id,
                    ["Operation"] = "DeleteFavicon"
                });

                _logger.LogInformation("Deleting favicon for company {CompanyId}", id);

                // Obtener empresa para verificar permisos
                var requestingUserId = GetRequestingUserId();
                var query = new GetCompanyByIdQuery 
                { 
                    CompanyId = id, 
                    RequestingUserId = requestingUserId 
                };
                var company = await _mediator.Send(query);
                
                if (company == null)
                    return NotFound(new { message = "Empresa no encontrada" });

                // Eliminar favicon si es local
                if (!string.IsNullOrEmpty(company.FaviconUrl) && 
                    (company.FaviconUrl.Contains("/api/images/") || company.FaviconUrl.StartsWith("/api/images/")))
                {
                    _fileService.DeleteCompanyFavicon(id);

                    // Actualizar empresa sin favicon (solo limpiar favicon)
                    var updateCommand = new UpdateCompanyCommand
                    {
                        CompanyId = id,
                        RequestingUserId = requestingUserId,
                        FaviconUrl = "" // Solo eliminar favicon
                    };

                    await _mediator.Send(updateCommand);
                }

                _logger.LogInformation("Favicon deleted successfully for company {CompanyId}", id);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access deleting favicon for company {CompanyId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting favicon for company {CompanyId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private int? GetRequestingUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}