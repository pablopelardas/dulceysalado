using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Ofertas;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Ofertas
{
    public class GetAgrupacionesOfertasForEmpresaQueryHandler : IRequestHandler<GetAgrupacionesOfertasForEmpresaQuery, List<AgrupacionWithOfertaStatusDto>>
    {
        private readonly IEmpresaOfertaRepository _empresaOfertaRepository;
        private readonly IMapper _mapper;

        public GetAgrupacionesOfertasForEmpresaQueryHandler(
            IEmpresaOfertaRepository empresaOfertaRepository,
            IMapper mapper)
        {
            _empresaOfertaRepository = empresaOfertaRepository;
            _mapper = mapper;
        }

        public async Task<List<AgrupacionWithOfertaStatusDto>> Handle(GetAgrupacionesOfertasForEmpresaQuery request, CancellationToken cancellationToken)
        {
            var agrupaciones = await _empresaOfertaRepository.GetAgrupacionesWithOfertaStatusAsync(request.EmpresaId);
            return _mapper.Map<List<AgrupacionWithOfertaStatusDto>>(agrupaciones);
        }
    }
}