using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Users
{
    public class ChangePasswordCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string? CurrentPassword { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public int? RequestingUserId { get; set; }
    }
}