using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Users
{
    public class GetCurrentUserQuery : IRequest<AuthResponseDto>
    {
        public int UserId { get; set; }
    }
}