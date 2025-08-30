using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Pedidos
{
    public class GetCorreccionByTokenQuery : IRequest<CorreccionDto?>
    {
        public string Token { get; set; } = string.Empty;
    }
}