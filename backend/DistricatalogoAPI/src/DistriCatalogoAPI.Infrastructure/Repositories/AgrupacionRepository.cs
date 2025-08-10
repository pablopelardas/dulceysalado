using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class AgrupacionRepository : IAgrupacionRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<AgrupacionRepository> _logger;

        public AgrupacionRepository(DistricatalogoContext context, ILogger<AgrupacionRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Agrupacion?> GetByIdAsync(int id)
        {
            try
            {
                var agrupacionModel = await _context.Agrupaciones
                    .Include(a => a.EmpresaPrincipal)
                    .FirstOrDefaultAsync(a => a.Id == id);

                return agrupacionModel != null ? MapToDomainEntity(agrupacionModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupación por ID {Id}", id);
                throw;
            }
        }

        public async Task<Agrupacion?> GetByCodigoAsync(int codigo, int empresaPrincipalId)
        {
            try
            {
                var agrupacionModel = await _context.Agrupaciones
                    .Include(a => a.EmpresaPrincipal)
                    .FirstOrDefaultAsync(a => a.Codigo == codigo && a.EmpresaPrincipalId == empresaPrincipalId);

                return agrupacionModel != null ? MapToDomainEntity(agrupacionModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupación por código {Codigo} y empresa {EmpresaId}", 
                    codigo, empresaPrincipalId);
                throw;
            }
        }

        public async Task<List<Agrupacion>> GetByEmpresaPrincipalAsync(int empresaPrincipalId, bool includeInactive = false)
        {
            try
            {
                var query = _context.Agrupaciones
                    .Include(a => a.EmpresaPrincipal)
                    .Where(a => a.EmpresaPrincipalId == empresaPrincipalId);

                if (!includeInactive)
                    query = query.Where(a => a.Activa == true);

                var agrupacionModels = await query
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();

                return agrupacionModels.Select(MapToDomainEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones por empresa principal {EmpresaId}", empresaPrincipalId);
                throw;
            }
        }

        public async Task<(List<Agrupacion> agrupaciones, int total)> GetPagedAsync(
            int empresaPrincipalId,
            int page,
            int pageSize,
            bool? activa = null,
            string? busqueda = null,
            int? tipo = 3)
        {
            try
            {
                var query = _context.Agrupaciones
                    .Include(a => a.EmpresaPrincipal)
                    .Where(a => a.EmpresaPrincipalId == empresaPrincipalId && a.Tipo == tipo);

                if (activa.HasValue)
                    query = query.Where(a => a.Activa == activa.Value);

                if (!string.IsNullOrEmpty(busqueda))
                    query = query.Where(a => a.Nombre.Contains(busqueda) || 
                                           (a.Descripcion != null && a.Descripcion.Contains(busqueda)));

                var total = await query.CountAsync();

                var agrupacionModels = await query
                    .OrderBy(a => a.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (agrupacionModels.Select(MapToDomainEntity).ToList(), total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones paginadas para empresa {EmpresaId}", empresaPrincipalId);
                throw;
            }
        }

        public async Task<Agrupacion> CreateAsync(Agrupacion agrupacion)
        {
            try
            {
                var agrupacionModel = MapToEfModel(agrupacion);
                _context.Agrupaciones.Add(agrupacionModel);
                await _context.SaveChangesAsync();

                // Update domain entity ID
                var agrupacionType = typeof(Agrupacion);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var idField = agrupacionType.GetField("<Id>k__BackingField", flags);
                idField?.SetValue(agrupacion, agrupacionModel.Id);

                _logger.LogInformation("Agrupación creada: {AgrupacionId} - {Codigo} - {Nombre}", 
                    agrupacionModel.Id, agrupacion.Codigo, agrupacion.Nombre);

                return agrupacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear agrupación {Codigo}", agrupacion.Codigo);
                throw;
            }
        }

        public async Task UpdateAsync(Agrupacion agrupacion)
        {
            try
            {
                var existingModel = await _context.Agrupaciones
                    .FirstOrDefaultAsync(a => a.Id == agrupacion.Id);

                if (existingModel == null)
                    throw new InvalidOperationException($"Agrupación {agrupacion.Id} no encontrada");

                existingModel.Nombre = agrupacion.Nombre;
                existingModel.Descripcion = agrupacion.Descripcion;
                existingModel.Activa = agrupacion.Activa;
                existingModel.UpdatedAt = agrupacion.UpdatedAt;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Agrupación actualizada: {AgrupacionId}", agrupacion.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar agrupación {AgrupacionId}", agrupacion.Id);
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoAsync(int codigo, int empresaPrincipalId, int? excludeId = null)
        {
            try
            {
                var query = _context.Agrupaciones
                    .Where(a => a.Codigo == codigo && a.EmpresaPrincipalId == empresaPrincipalId);

                if (excludeId.HasValue)
                    query = query.Where(a => a.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de agrupación {Codigo}", codigo);
                throw;
            }
        }

        public async Task<List<int>> GetVisibleCodigosForEmpresaAsync(int empresaId)
        {
            try
            {
                var visibleCodigos = await _context.EmpresasAgrupacionesVisibles
                    .Where(eav => eav.EmpresaId == empresaId && eav.Visible == true)
                    .Join(_context.Agrupaciones,
                        eav => eav.AgrupacionId,
                        a => a.Id,
                        (eav, a) => a.Codigo)
                    .ToListAsync();

                return visibleCodigos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener códigos visibles para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<bool> IsVisibleForEmpresaAsync(int agrupacionId, int empresaId)
        {
            try
            {
                var visibility = await _context.EmpresasAgrupacionesVisibles
                    .FirstOrDefaultAsync(eav => eav.AgrupacionId == agrupacionId && eav.EmpresaId == empresaId);

                return visibility?.Visible ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar visibilidad de agrupación {AgrupacionId} para empresa {EmpresaId}", 
                    agrupacionId, empresaId);
                throw;
            }
        }

        public async Task SetVisibilityForEmpresaAsync(int empresaId, List<int> agrupacionIds)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Remover configuraciones existentes
                var existingConfigs = await _context.EmpresasAgrupacionesVisibles
                    .Where(eav => eav.EmpresaId == empresaId)
                    .ToListAsync();

                _context.EmpresasAgrupacionesVisibles.RemoveRange(existingConfigs);

                // Agregar nuevas configuraciones
                var newConfigs = agrupacionIds.Select(agrupacionId => new EmpresasAgrupacionesVisible
                {
                    EmpresaId = empresaId,
                    AgrupacionId = agrupacionId,
                    Visible = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                _context.EmpresasAgrupacionesVisibles.AddRange(newConfigs);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Configuración de visibilidad actualizada para empresa {EmpresaId}: {Count} agrupaciones", 
                    empresaId, agrupacionIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar visibilidad para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> GetCountByEmpresaPrincipalAsync(int empresaPrincipalId)
        {
            try
            {
                return await _context.Agrupaciones
                    .Where(a => a.EmpresaPrincipalId == empresaPrincipalId && a.Activa == true)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar agrupaciones para empresa {EmpresaId}", empresaPrincipalId);
                throw;
            }
        }

        public async Task<List<Agrupacion>> GetVisibleByEmpresaAsync(int empresaId)
        {
            try
            {
                var visibleAgrupaciones = await _context.EmpresasAgrupacionesVisibles
                    .Where(eav => eav.EmpresaId == empresaId && eav.Visible == true)
                    .Join(_context.Agrupaciones,
                        eav => eav.AgrupacionId,
                        a => a.Id,
                        (eav, a) => a)
                    .Include(a => a.EmpresaPrincipal)
                    .ToListAsync();

                return visibleAgrupaciones.Select(MapToDomainEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones visibles para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<Agrupacion>> GetByIdsAsync(List<int> ids)
        {
            try
            {
                if (!ids.Any())
                    return new List<Agrupacion>();

                var agrupacionModels = await _context.Agrupaciones
                    .Include(a => a.EmpresaPrincipal)
                    .Where(a => ids.Contains(a.Id))
                    .ToListAsync();

                return agrupacionModels.Select(MapToDomainEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agrupaciones por IDs");
                throw;
            }
        }

        public async Task<Dictionary<int, bool>> CheckExistenceByCodigosAsync(List<int> codigos, int empresaPrincipalId)
        {
            try
            {
                if (!codigos.Any())
                    return new Dictionary<int, bool>();

                var existentes = await _context.Agrupaciones
                    .Where(a => codigos.Contains(a.Codigo) && a.EmpresaPrincipalId == empresaPrincipalId)
                    .Select(a => a.Codigo)
                    .ToListAsync();

                var resultado = codigos.ToDictionary(codigo => codigo, codigo => existentes.Contains(codigo));

                _logger.LogDebug("Verificación bulk de agrupaciones: {TotalCodigos} códigos, {Existentes} existentes", 
                    codigos.Count, existentes.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia bulk de agrupaciones para empresa {EmpresaId}", empresaPrincipalId);
                throw;
            }
        }

        public async Task<List<Agrupacion>> CreateBulkAsync(List<Agrupacion> agrupaciones)
        {
            try
            {
                if (!agrupaciones.Any())
                    return new List<Agrupacion>();

                var agrupacionModels = agrupaciones.Select(MapToEfModel).ToList();
                
                _context.Agrupaciones.AddRange(agrupacionModels);
                await _context.SaveChangesAsync();

                // Update domain entity IDs using reflection
                var agrupacionType = typeof(Agrupacion);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var idField = agrupacionType.GetField("<Id>k__BackingField", flags);

                for (int i = 0; i < agrupaciones.Count; i++)
                {
                    idField?.SetValue(agrupaciones[i], agrupacionModels[i].Id);
                }

                _logger.LogInformation("Agrupaciones creadas en bulk: {Count} agrupaciones para empresa {EmpresaId}", 
                    agrupaciones.Count, agrupaciones.First().EmpresaPrincipalId);

                return agrupaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear agrupaciones en bulk");
                throw;
            }
        }

        private Agrupacion MapToDomainEntity(Agrupaciones model)
        {
            var agrupacion = Agrupacion.CreateFromSync(model.Codigo, model.EmpresaPrincipalId, model.Nombre, model.Descripcion);

            // Set private properties using reflection
            var agrupacionType = typeof(Agrupacion);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            var properties = new Dictionary<string, object?>
            {
                ["<Id>k__BackingField"] = model.Id,
                ["<Activa>k__BackingField"] = model.Activa ?? true,
                ["<CreatedAt>k__BackingField"] = model.CreatedAt ?? DateTime.UtcNow,
                ["<UpdatedAt>k__BackingField"] = model.UpdatedAt ?? DateTime.UtcNow,
                ["<Tipo>k__BackingField"] = model.Tipo,
            };

            foreach (var property in properties)
            {
                var field = agrupacionType.GetField(property.Key, flags);
                field?.SetValue(agrupacion, property.Value);
            }

            return agrupacion;
        }

        private Agrupaciones MapToEfModel(Agrupacion agrupacion)
        {
            return new Agrupaciones
            {
                Codigo = agrupacion.Codigo,
                Nombre = agrupacion.Nombre,
                Descripcion = agrupacion.Descripcion,
                Activa = agrupacion.Activa,
                EmpresaPrincipalId = agrupacion.EmpresaPrincipalId,
                CreatedAt = agrupacion.CreatedAt,
                UpdatedAt = agrupacion.UpdatedAt,
                Tipo = agrupacion.Tipo
            };
        }
    }
}