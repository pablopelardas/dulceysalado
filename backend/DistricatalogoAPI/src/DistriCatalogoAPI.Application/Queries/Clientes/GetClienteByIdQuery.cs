using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Clientes
{
    public class GetClienteByIdQuery : IRequest<ClienteDto?>
    {
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}