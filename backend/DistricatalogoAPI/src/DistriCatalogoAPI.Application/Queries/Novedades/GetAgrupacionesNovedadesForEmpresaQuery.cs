using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Novedades
{
    public class GetAgrupacionesNovedadesForEmpresaQuery : IRequest<List<AgrupacionWithNovedadStatusDto>>
    {
        public int EmpresaId { get; set; }

        public GetAgrupacionesNovedadesForEmpresaQuery(int empresaId)
        {
            EmpresaId = empresaId;
        }
    }
}