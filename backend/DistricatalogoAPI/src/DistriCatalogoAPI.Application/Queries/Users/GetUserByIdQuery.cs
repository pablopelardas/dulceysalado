using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Users
{
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        public int UserId { get; set; }
        public int? RequestingUserId { get; set; }
    }
}