using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Novedades;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Novedades
{
    public class GetNovedadByIdQueryHandler : IRequestHandler<GetNovedadByIdQuery, EmpresaNovedadDto?>
    {
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly IMapper _mapper;

        public GetNovedadByIdQueryHandler(
            IEmpresaNovedadRepository empresaNovedadRepository,
            IMapper mapper)
        {
            _empresaNovedadRepository = empresaNovedadRepository;
            _mapper = mapper;
        }

        public async Task<EmpresaNovedadDto?> Handle(GetNovedadByIdQuery request, CancellationToken cancellationToken)
        {
            var novedad = await _empresaNovedadRepository.GetByIdAsync(request.Id);
            return novedad != null ? _mapper.Map<EmpresaNovedadDto>(novedad) : null;
        }
    }
}