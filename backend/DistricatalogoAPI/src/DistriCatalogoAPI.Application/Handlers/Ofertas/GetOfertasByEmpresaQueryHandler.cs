using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Ofertas;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Ofertas
{
    public class GetOfertasByEmpresaQueryHandler : IRequestHandler<GetOfertasByEmpresaQuery, PagedResultDto<EmpresaOfertaDto>>
    {
        private readonly IEmpresaOfertaRepository _empresaOfertaRepository;
        private readonly IMapper _mapper;

        public GetOfertasByEmpresaQueryHandler(
            IEmpresaOfertaRepository empresaOfertaRepository,
            IMapper mapper)
        {
            _empresaOfertaRepository = empresaOfertaRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<EmpresaOfertaDto>> Handle(GetOfertasByEmpresaQuery request, CancellationToken cancellationToken)
        {
            var (ofertas, total) = await _empresaOfertaRepository.GetPagedByEmpresaAsync(
                request.EmpresaId,
                request.Page,
                request.PageSize,
                request.Visible);

            var ofertasDto = _mapper.Map<List<EmpresaOfertaDto>>(ofertas);

            var result = new PagedResultDto<EmpresaOfertaDto>();
            result.SetItems(ofertasDto, "ofertas");
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