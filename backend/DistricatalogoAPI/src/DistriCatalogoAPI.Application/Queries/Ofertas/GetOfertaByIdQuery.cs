using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Ofertas
{
    public class GetOfertaByIdQuery : IRequest<EmpresaOfertaDto?>
    {
        public int Id { get; set; }

        public GetOfertaByIdQuery(int id)
        {
            Id = id;
        }
    }
}