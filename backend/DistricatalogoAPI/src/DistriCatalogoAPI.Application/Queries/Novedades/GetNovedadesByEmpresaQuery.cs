using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Novedades
{
    public class GetNovedadesByEmpresaQuery : IRequest<PagedResultDto<EmpresaNovedadDto>>
    {
        public int EmpresaId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? Visible { get; set; }

        public GetNovedadesByEmpresaQuery(int empresaId, int page = 1, int pageSize = 20, bool? visible = null)
        {
            EmpresaId = empresaId;
            Page = page;
            PageSize = pageSize;
            Visible = visible;
        }
    }
}