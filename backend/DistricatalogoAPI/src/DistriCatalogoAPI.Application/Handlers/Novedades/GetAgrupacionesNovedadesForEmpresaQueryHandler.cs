using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Novedades;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Novedades
{
    public class GetAgrupacionesNovedadesForEmpresaQueryHandler : IRequestHandler<GetAgrupacionesNovedadesForEmpresaQuery, List<AgrupacionWithNovedadStatusDto>>
    {
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly IMapper _mapper;

        public GetAgrupacionesNovedadesForEmpresaQueryHandler(
            IEmpresaNovedadRepository empresaNovedadRepository,
            IMapper mapper)
        {
            _empresaNovedadRepository = empresaNovedadRepository;
            _mapper = mapper;
        }

        public async Task<List<AgrupacionWithNovedadStatusDto>> Handle(GetAgrupacionesNovedadesForEmpresaQuery request, CancellationToken cancellationToken)
        {
            var agrupaciones = await _empresaNovedadRepository.GetAgrupacionesWithNovedadStatusAsync(request.EmpresaId);
            return _mapper.Map<List<AgrupacionWithNovedadStatusDto>>(agrupaciones);
        }
    }
}