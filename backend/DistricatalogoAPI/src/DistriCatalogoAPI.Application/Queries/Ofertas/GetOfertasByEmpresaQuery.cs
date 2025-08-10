using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Ofertas
{
    public class GetOfertasByEmpresaQuery : IRequest<PagedResultDto<EmpresaOfertaDto>>
    {
        public int EmpresaId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? Visible { get; set; }

        public GetOfertasByEmpresaQuery(int empresaId, int page = 1, int pageSize = 20, bool? visible = null)
        {
            EmpresaId = empresaId;
            Page = page;
            PageSize = pageSize;
            Visible = visible;
        }
    }
}