using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Clientes
{
    public class GetClienteByUsernameQuery : IRequest<ClienteDto?>
    {
        public int EmpresaId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IncludeDeleted { get; set; } = false;
    }
}