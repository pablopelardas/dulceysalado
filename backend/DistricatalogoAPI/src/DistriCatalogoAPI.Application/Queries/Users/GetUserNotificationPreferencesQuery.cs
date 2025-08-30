using DistriCatalogoAPI.Application.DTOs;
using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Users
{
    public class GetUserNotificationPreferencesQuery : IRequest<UserNotificationPreferencesDto?>
    {
        public int UserId { get; set; }
    }
}