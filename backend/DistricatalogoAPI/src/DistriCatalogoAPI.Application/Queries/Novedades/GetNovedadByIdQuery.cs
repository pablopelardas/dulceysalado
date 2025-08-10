using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Novedades
{
    public class GetNovedadByIdQuery : IRequest<EmpresaNovedadDto?>
    {
        public int Id { get; set; }

        public GetNovedadByIdQuery(int id)
        {
            Id = id;
        }
    }
}