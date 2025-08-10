using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Ofertas
{
    public class GetAgrupacionesOfertasForEmpresaQuery : IRequest<List<AgrupacionWithOfertaStatusDto>>
    {
        public int EmpresaId { get; set; }

        public GetAgrupacionesOfertasForEmpresaQuery(int empresaId)
        {
            EmpresaId = empresaId;
        }
    }
}