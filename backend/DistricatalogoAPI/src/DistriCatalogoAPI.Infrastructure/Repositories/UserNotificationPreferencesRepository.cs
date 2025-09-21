using Microsoft.EntityFrameworkCore;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class UserNotificationPreferencesRepository : IUserNotificationPreferencesRepository
    {
        private readonly DistricatalogoContext _context;

        public UserNotificationPreferencesRepository(DistricatalogoContext context)
        {
            _context = context;
        }

        public async Task<UserNotificationPreferences?> GetByUserIdAsync(int userId)
        {
            return await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<UserNotificationPreferences>> GetByCompanyIdAsync(int companyId)
        {
            return await _context.UserNotificationPreferences
                .Join(_context.Usuarios,
                    p => p.UserId,
                    u => u.Id,
                    (p, u) => new { Preferences = p, User = u })
                .Where(joined => joined.User.EmpresaId == companyId && joined.User.Activo == true)
                .Select(joined => joined.Preferences)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserNotificationPreferences>> GetUsersForNotificationTypeAsync(int companyId, TipoNotificacion tipoNotificacion)
        {
            var query = _context.UserNotificationPreferences
                .Join(_context.Usuarios,
                    p => p.UserId,
                    u => u.Id,
                    (p, u) => new { Preferences = p, User = u })
                .Where(joined => joined.User.EmpresaId == companyId 
                             && joined.User.Activo == true 
                             && !string.IsNullOrEmpty(joined.User.Email));

            // Filtrar por tipo de notificación específica
            query = tipoNotificacion switch
            {
                TipoNotificacion.NuevoPedido => query.Where(joined => joined.Preferences.NotificacionNuevosPedidos),
                TipoNotificacion.CorreccionAprobada => query.Where(joined => joined.Preferences.NotificacionCorreccionesAprobadas),
                TipoNotificacion.CorreccionRechazada => query.Where(joined => joined.Preferences.NotificacionCorreccionesRechazadas),
                TipoNotificacion.PedidoCancelado => query.Where(joined => joined.Preferences.NotificacionPedidosCancelados),
                TipoNotificacion.NuevaSolicitudReventa => query.Where(joined => joined.Preferences.NotificacionSolicitudesReventa),
                _ => throw new ArgumentException($"Tipo de notificación no soportado: {tipoNotificacion}")
            };

            return await query.Select(joined => joined.Preferences).ToListAsync();
        }

        public async Task<UserNotificationPreferences> CreateAsync(UserNotificationPreferences preferences)
        {
            _context.UserNotificationPreferences.Add(preferences);
            await _context.SaveChangesAsync();
            return preferences;
        }

        public async Task UpdateAsync(UserNotificationPreferences preferences)
        {
            _context.UserNotificationPreferences.Update(preferences);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            var preferences = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
                
            if (preferences != null)
            {
                _context.UserNotificationPreferences.Remove(preferences);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.UserNotificationPreferences
                .AnyAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<int>> GetUserIdsWithNotificationEnabledAsync(int companyId, TipoNotificacion tipoNotificacion)
        {
            var preferences = await GetUsersForNotificationTypeAsync(companyId, tipoNotificacion);
            return preferences.Select(p => p.UserId);
        }

        public async Task CreateDefaultPreferencesForUserAsync(int userId)
        {
            // Verificar si ya existen preferencias
            if (await ExistsByUserIdAsync(userId))
                return;

            var defaultPreferences = new UserNotificationPreferences
            {
                UserId = userId,
                NotificacionNuevosPedidos = true,
                NotificacionCorreccionesAprobadas = true,
                NotificacionCorreccionesRechazadas = true,
                NotificacionPedidosCancelados = true,
                NotificacionSolicitudesReventa = true,
                CreatedBy = "SYSTEM_AUTO",
                UpdatedBy = "SYSTEM_AUTO"
            };

            await CreateAsync(defaultPreferences);
        }

        public async Task<IEnumerable<(UserNotificationPreferences Preferences, string UserEmail)>> GetUsersWithEmailForNotificationTypeAsync(int companyId, TipoNotificacion tipoNotificacion)
        {
            var query = _context.UserNotificationPreferences
                .Join(_context.Usuarios,
                    p => p.UserId,
                    u => u.Id,
                    (p, u) => new { Preferences = p, User = u })
                .Where(joined => joined.User.EmpresaId == companyId 
                             && joined.User.Activo == true 
                             && !string.IsNullOrEmpty(joined.User.Email));

            // Filtrar por tipo de notificación específica
            query = tipoNotificacion switch
            {
                TipoNotificacion.NuevoPedido => query.Where(joined => joined.Preferences.NotificacionNuevosPedidos),
                TipoNotificacion.CorreccionAprobada => query.Where(joined => joined.Preferences.NotificacionCorreccionesAprobadas),
                TipoNotificacion.CorreccionRechazada => query.Where(joined => joined.Preferences.NotificacionCorreccionesRechazadas),
                TipoNotificacion.PedidoCancelado => query.Where(joined => joined.Preferences.NotificacionPedidosCancelados),
                TipoNotificacion.NuevaSolicitudReventa => query.Where(joined => joined.Preferences.NotificacionSolicitudesReventa),
                _ => throw new ArgumentException($"Tipo de notificación no soportado: {tipoNotificacion}")
            };

            var results = await query.ToListAsync();
            return results.Select(r => (r.Preferences, r.User.Email));
        }
    }
}