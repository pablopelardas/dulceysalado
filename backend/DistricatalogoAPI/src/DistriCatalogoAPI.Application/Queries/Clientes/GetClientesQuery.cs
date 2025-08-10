using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Clientes
{
    public class GetClientesQuery : IRequest<PagedResultDto<ClienteDto>>
    {
        public int EmpresaId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SearchTerm { get; set; }
        public int? ListaPrecioId { get; set; }
        public bool? IsActive { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}