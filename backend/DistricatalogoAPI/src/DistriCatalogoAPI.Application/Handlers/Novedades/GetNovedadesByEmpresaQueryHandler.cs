using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Novedades;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Novedades
{
    public class GetNovedadesByEmpresaQueryHandler : IRequestHandler<GetNovedadesByEmpresaQuery, PagedResultDto<EmpresaNovedadDto>>
    {
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly IMapper _mapper;

        public GetNovedadesByEmpresaQueryHandler(
            IEmpresaNovedadRepository empresaNovedadRepository,
            IMapper mapper)
        {
            _empresaNovedadRepository = empresaNovedadRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<EmpresaNovedadDto>> Handle(GetNovedadesByEmpresaQuery request, CancellationToken cancellationToken)
        {
            var (novedades, total) = await _empresaNovedadRepository.GetPagedByEmpresaAsync(
                request.EmpresaId,
                request.Page,
                request.PageSize,
                request.Visible);

            var novedadesDto = _mapper.Map<List<EmpresaNovedadDto>>(novedades);

            var result = new PagedResultDto<EmpresaNovedadDto>();
            result.SetItems(novedadesDto, "novedades");
            result.Pagination = new PaginationDto
            {
                Page = request.Page,
                Limit = request.PageSize,
                Total = total
            };

            return result;
        }
    }
}