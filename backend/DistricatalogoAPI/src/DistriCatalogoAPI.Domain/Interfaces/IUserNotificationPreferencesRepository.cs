using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IUserNotificationPreferencesRepository
    {
        Task<UserNotificationPreferences?> GetByUserIdAsync(int userId);
        Task<IEnumerable<UserNotificationPreferences>> GetByCompanyIdAsync(int companyId);
        Task<IEnumerable<UserNotificationPreferences>> GetUsersForNotificationTypeAsync(int companyId, TipoNotificacion tipoNotificacion);
        Task<UserNotificationPreferences> CreateAsync(UserNotificationPreferences preferences);
        Task UpdateAsync(UserNotificationPreferences preferences);
        Task DeleteAsync(int userId);
        Task<bool> ExistsByUserIdAsync(int userId);
        
        // Métodos de utilidad
        Task<IEnumerable<int>> GetUserIdsWithNotificationEnabledAsync(int companyId, TipoNotificacion tipoNotificacion);
        Task CreateDefaultPreferencesForUserAsync(int userId);
        
        // Método específico para NotificationService que incluye información del usuario
        Task<IEnumerable<(UserNotificationPreferences Preferences, string UserEmail)>> GetUsersWithEmailForNotificationTypeAsync(int companyId, TipoNotificacion tipoNotificacion);
    }
}