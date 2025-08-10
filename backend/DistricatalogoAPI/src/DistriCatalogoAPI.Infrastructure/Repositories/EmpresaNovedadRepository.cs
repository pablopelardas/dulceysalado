using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class EmpresaNovedadRepository : IEmpresaNovedadRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<EmpresaNovedadRepository> _logger;

        public EmpresaNovedadRepository(DistricatalogoContext context, ILogger<EmpresaNovedadRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EmpresaNovedad?> GetByIdAsync(int id)
        {
            try
            {
                var model = await _context.EmpresasNovedades
                    .Include(en => en.Empresa)
                    .Include(en => en.Agrupacion)
                    .FirstOrDefaultAsync(en => en.Id == id);

                return model != null ? MapToDomainEntity(model) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener EmpresaNovedad por ID {Id}", id);
                throw;
            }
        }

        public async Task<EmpresaNovedad?> GetByEmpresaAndAgrupacionAsync(int empresaId, int agrupacionId)
        {
            try
            {
                var model = await _context.EmpresasNovedades
                    .Include(en => en.Empresa)
                    .Include(en => en.Agrupacion)
                    .FirstOrDefaultAsync(en => en.EmpresaId == empresaId && en.AgrupacionId == agrupacionId);

                return model != null ? MapToDomainEntity(model) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener EmpresaNovedad por EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", empresaId, agrupacionId);
                throw;
            }
        }

        public async Task<List<EmpresaNovedad>> GetByEmpresaIdAsync(int empresaId, bool? visible = null)
        {
            try
            {
                var query = _context.EmpresasNovedades
                    .Include(en => en.Empresa)
                    .Include(en => en.Agrupacion)
                    .Where(en => en.EmpresaId == empresaId);

                if (visible.HasValue)
                {
                    query = query.Where(en => en.Visible == visible.Value);
                }

                var models = await query.ToListAsync();
                return models.Select(MapToDomainEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener EmpresasNovedades por EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<(List<EmpresaNovedad> novedades, int total)> GetPagedByEmpresaAsync(
            int empresaId, int page, int pageSize, bool? visible = null)
        {
            try
            {
                var query = _context.EmpresasNovedades
                    .Include(en => en.Empresa)
                    .Include(en => en.Agrupacion)
                    .Where(en => en.EmpresaId == empresaId);

                if (visible.HasValue)
                {
                    query = query.Where(en => en.Visible == visible.Value);
                }

                var total = await query.CountAsync();
                var models = await query
                    .OrderBy(en => en.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var novedades = models.Select(MapToDomainEntity).ToList();
                return (novedades, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener página de EmpresasNovedades para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<EmpresaNovedad> CreateAsync(EmpresaNovedad empresaNovedad)
        {
            try
            {
                var model = MapToInfrastructureEntity(empresaNovedad);
                _context.EmpresasNovedades.Add(model);
                await _context.SaveChangesAsync();

                // Recargar con includes
                var savedModel = await _context.EmpresasNovedades
                    .Include(en => en.Empresa)
                    .Include(en => en.Agrupacion)
                    .FirstAsync(en => en.Id == model.Id);

                return MapToDomainEntity(savedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear EmpresaNovedad para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", 
                    empresaNovedad.EmpresaId, empresaNovedad.AgrupacionId);
                throw;
            }
        }

        public async Task UpdateAsync(EmpresaNovedad empresaNovedad)
        {
            try
            {
                var model = await _context.EmpresasNovedades
                    .FirstOrDefaultAsync(en => en.Id == empresaNovedad.Id);

                if (model == null)
                    throw new InvalidOperationException($"EmpresaNovedad con ID {empresaNovedad.Id} no encontrada");

                model.Visible = empresaNovedad.Visible;
                model.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar EmpresaNovedad con ID {Id}", empresaNovedad.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var model = await _context.EmpresasNovedades.FindAsync(id);
                if (model != null)
                {
                    _context.EmpresasNovedades.Remove(model);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar EmpresaNovedad con ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int empresaId, int agrupacionId)
        {
            try
            {
                return await _context.EmpresasNovedades
                    .AnyAsync(en => en.EmpresaId == empresaId && en.AgrupacionId == agrupacionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de EmpresaNovedad para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", 
                    empresaId, agrupacionId);
                throw;
            }
        }

        public async Task<List<AgrupacionWithNovedadStatus>> GetAgrupacionesWithNovedadStatusAsync(int empresaId)
        {
            try
            {
                var result = await (from agrup in _context.Agrupaciones
                                  join novedad in _context.EmpresasNovedades
                                      on new { AgrupacionId = agrup.Id, EmpresaId = empresaId }
                                      equals new { AgrupacionId = novedad.AgrupacionId, EmpresaId = novedad.EmpresaId }
                                      into novedadGroup
                                  from novedad in novedadGroup.DefaultIfEmpty()
                                  where agrup.Tipo == 1 && agrup.Activa == true // Solo Grupo 1 activas
                                  select new AgrupacionWithNovedadStatus
                                  {
                                      Id = agrup.Id,
                                      Codigo = agrup.Codigo,
                                      Nombre = agrup.Nombre,
                                      Descripcion = agrup.Descripcion,
                                      Activa = agrup.Activa ?? false,
                                      IsNovedad = novedad != null && novedad.Visible == true,
                                      EmpresaPrincipalId = agrup.EmpresaPrincipalId
                                  })
                                  .OrderBy(a => a.Nombre)
                                  .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones con status de novedad para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task SetNovedadesForEmpresaAsync(int empresaId, List<int> agrupacionIds)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Primero, eliminar todas las novedades existentes de la empresa
                var existingNovedades = await _context.EmpresasNovedades
                    .Where(en => en.EmpresaId == empresaId)
                    .ToListAsync();

                _context.EmpresasNovedades.RemoveRange(existingNovedades);

                // Luego, agregar las nuevas novedades
                foreach (var agrupacionId in agrupacionIds)
                {
                    var novedad = new EmpresasNovedad
                    {
                        EmpresaId = empresaId,
                        AgrupacionId = agrupacionId,
                        Visible = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.EmpresasNovedades.Add(novedad);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Novedades actualizadas para EmpresaId {EmpresaId}. Total: {Count}", empresaId, agrupacionIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer novedades para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<Agrupacion>> GetNovedadesAgrupacionesByEmpresaAsync(int empresaId)
        {
            try
            {
                var agrupaciones = await (from novedad in _context.EmpresasNovedades
                                        join agrup in _context.Agrupaciones on novedad.AgrupacionId equals agrup.Id
                                        where novedad.EmpresaId == empresaId && novedad.Visible == true
                                        select agrup)
                                        .ToListAsync();

                return agrupaciones.Select(MapAgrupacionToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones de novedades para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> GetCountByEmpresaAsync(int empresaId)
        {
            try
            {
                return await _context.EmpresasNovedades
                    .Where(en => en.EmpresaId == empresaId && en.Visible == true)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar novedades para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task RemoveAllByEmpresaAsync(int empresaId)
        {
            try
            {
                var novedades = await _context.EmpresasNovedades
                    .Where(en => en.EmpresaId == empresaId)
                    .ToListAsync();

                _context.EmpresasNovedades.RemoveRange(novedades);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Eliminadas todas las novedades para EmpresaId {EmpresaId}", empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar todas las novedades para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        private static EmpresaNovedad MapToDomainEntity(EmpresasNovedad model)
        {
            return EmpresaNovedad.Create(
                model.EmpresaId,
                model.AgrupacionId,
                model.Visible ?? true);
        }

        private static EmpresasNovedad MapToInfrastructureEntity(EmpresaNovedad domain)
        {
            return new EmpresasNovedad
            {
                EmpresaId = domain.EmpresaId,
                AgrupacionId = domain.AgrupacionId,
                Visible = domain.Visible,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static Agrupacion MapAgrupacionToDomain(Agrupaciones model)
        {
            return Agrupacion.CreateFromSync(
                model.Codigo,
                model.EmpresaPrincipalId,
                model.Nombre,
                model.Descripcion,
                model.Tipo);
        }
    }
}