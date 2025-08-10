using System.Collections.Generic;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Users
{
    public class GetUsersListQuery : IRequest<PagedResultDto<UserDto>>
    {
        public int? EmpresaId { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? RequestingUserId { get; set; }
    }
}