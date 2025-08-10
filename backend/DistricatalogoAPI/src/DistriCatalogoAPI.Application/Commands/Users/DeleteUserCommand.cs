using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Users
{
    public class DeleteUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int? RequestingUserId { get; set; }
    }
}