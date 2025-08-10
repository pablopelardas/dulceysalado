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
    public class EmpresaOfertaRepository : IEmpresaOfertaRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<EmpresaOfertaRepository> _logger;

        public EmpresaOfertaRepository(DistricatalogoContext context, ILogger<EmpresaOfertaRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EmpresaOferta?> GetByIdAsync(int id)
        {
            try
            {
                var model = await _context.EmpresasOfertas
                    .Include(eo => eo.Empresa)
                    .Include(eo => eo.Agrupacion)
                    .FirstOrDefaultAsync(eo => eo.Id == id);

                return model != null ? MapToDomainEntity(model) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener EmpresaOferta por ID {Id}", id);
                throw;
            }
        }

        public async Task<EmpresaOferta?> GetByEmpresaAndAgrupacionAsync(int empresaId, int agrupacionId)
        {
            try
            {
                var model = await _context.EmpresasOfertas
                    .Include(eo => eo.Empresa)
                    .Include(eo => eo.Agrupacion)
                    .FirstOrDefaultAsync(eo => eo.EmpresaId == empresaId && eo.AgrupacionId == agrupacionId);

                return model != null ? MapToDomainEntity(model) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener EmpresaOferta por EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", empresaId, agrupacionId);
                throw;
            }
        }

        public async Task<List<EmpresaOferta>> GetByEmpresaIdAsync(int empresaId, bool? visible = null)
        {
            try
            {
                var query = _context.EmpresasOfertas
                    .Include(eo => eo.Empresa)
                    .Include(eo => eo.Agrupacion)
                    .Where(eo => eo.EmpresaId == empresaId);

                if (visible.HasValue)
                {
                    query = query.Where(eo => eo.Visible == visible.Value);
                }

                var models = await query.ToListAsync();
                return models.Select(MapToDomainEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener EmpresasOfertas por EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<(List<EmpresaOferta> ofertas, int total)> GetPagedByEmpresaAsync(
            int empresaId, int page, int pageSize, bool? visible = null)
        {
            try
            {
                var query = _context.EmpresasOfertas
                    .Include(eo => eo.Empresa)
                    .Include(eo => eo.Agrupacion)
                    .Where(eo => eo.EmpresaId == empresaId);

                if (visible.HasValue)
                {
                    query = query.Where(eo => eo.Visible == visible.Value);
                }

                var total = await query.CountAsync();
                var models = await query
                    .OrderBy(eo => eo.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var ofertas = models.Select(MapToDomainEntity).ToList();
                return (ofertas, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener página de EmpresasOfertas para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<EmpresaOferta> CreateAsync(EmpresaOferta empresaOferta)
        {
            try
            {
                var model = MapToInfrastructureEntity(empresaOferta);
                _context.EmpresasOfertas.Add(model);
                await _context.SaveChangesAsync();

                // Recargar con includes
                var savedModel = await _context.EmpresasOfertas
                    .Include(eo => eo.Empresa)
                    .Include(eo => eo.Agrupacion)
                    .FirstAsync(eo => eo.Id == model.Id);

                return MapToDomainEntity(savedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear EmpresaOferta para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", 
                    empresaOferta.EmpresaId, empresaOferta.AgrupacionId);
                throw;
            }
        }

        public async Task UpdateAsync(EmpresaOferta empresaOferta)
        {
            try
            {
                var model = await _context.EmpresasOfertas
                    .FirstOrDefaultAsync(eo => eo.Id == empresaOferta.Id);

                if (model == null)
                    throw new InvalidOperationException($"EmpresaOferta con ID {empresaOferta.Id} no encontrada");

                model.Visible = empresaOferta.Visible;
                model.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar EmpresaOferta con ID {Id}", empresaOferta.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var model = await _context.EmpresasOfertas.FindAsync(id);
                if (model != null)
                {
                    _context.EmpresasOfertas.Remove(model);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar EmpresaOferta con ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int empresaId, int agrupacionId)
        {
            try
            {
                return await _context.EmpresasOfertas
                    .AnyAsync(eo => eo.EmpresaId == empresaId && eo.AgrupacionId == agrupacionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de EmpresaOferta para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}", 
                    empresaId, agrupacionId);
                throw;
            }
        }

        public async Task<List<AgrupacionWithOfertaStatus>> GetAgrupacionesWithOfertaStatusAsync(int empresaId)
        {
            try
            {
                var result = await (from agrup in _context.Agrupaciones
                                  join oferta in _context.EmpresasOfertas
                                      on new { AgrupacionId = agrup.Id, EmpresaId = empresaId }
                                      equals new { AgrupacionId = oferta.AgrupacionId, EmpresaId = oferta.EmpresaId }
                                      into ofertaGroup
                                  from oferta in ofertaGroup.DefaultIfEmpty()
                                  where agrup.Tipo == 1 && agrup.Activa == true // Solo Grupo 1 activas
                                  select new AgrupacionWithOfertaStatus
                                  {
                                      Id = agrup.Id,
                                      Codigo = agrup.Codigo,
                                      Nombre = agrup.Nombre,
                                      Descripcion = agrup.Descripcion,
                                      Activa = agrup.Activa ?? false,
                                      IsOferta = oferta != null && oferta.Visible == true,
                                      EmpresaPrincipalId = agrup.EmpresaPrincipalId
                                  })
                                  .OrderBy(a => a.Nombre)
                                  .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones con status de oferta para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task SetOfertasForEmpresaAsync(int empresaId, List<int> agrupacionIds)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Primero, eliminar todas las ofertas existentes de la empresa
                var existingOfertas = await _context.EmpresasOfertas
                    .Where(eo => eo.EmpresaId == empresaId)
                    .ToListAsync();

                _context.EmpresasOfertas.RemoveRange(existingOfertas);

                // Luego, agregar las nuevas ofertas
                foreach (var agrupacionId in agrupacionIds)
                {
                    var oferta = new EmpresasOferta
                    {
                        EmpresaId = empresaId,
                        AgrupacionId = agrupacionId,
                        Visible = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.EmpresasOfertas.Add(oferta);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Ofertas actualizadas para EmpresaId {EmpresaId}. Total: {Count}", empresaId, agrupacionIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer ofertas para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<Agrupacion>> GetOfertasAgrupacionesByEmpresaAsync(int empresaId)
        {
            try
            {
                var agrupaciones = await (from oferta in _context.EmpresasOfertas
                                        join agrup in _context.Agrupaciones on oferta.AgrupacionId equals agrup.Id
                                        where oferta.EmpresaId == empresaId && oferta.Visible == true
                                        select agrup)
                                        .ToListAsync();

                return agrupaciones.Select(MapAgrupacionToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones de ofertas para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> GetCountByEmpresaAsync(int empresaId)
        {
            try
            {
                return await _context.EmpresasOfertas
                    .Where(eo => eo.EmpresaId == empresaId && eo.Visible == true)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar ofertas para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task RemoveAllByEmpresaAsync(int empresaId)
        {
            try
            {
                var ofertas = await _context.EmpresasOfertas
                    .Where(eo => eo.EmpresaId == empresaId)
                    .ToListAsync();

                _context.EmpresasOfertas.RemoveRange(ofertas);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Eliminadas todas las ofertas para EmpresaId {EmpresaId}", empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar todas las ofertas para EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        private static EmpresaOferta MapToDomainEntity(EmpresasOferta model)
        {
            return EmpresaOferta.Create(
                model.EmpresaId,
                model.AgrupacionId,
                model.Visible ?? true);
        }

        private static EmpresasOferta MapToInfrastructureEntity(EmpresaOferta domain)
        {
            return new EmpresasOferta
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