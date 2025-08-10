using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Ofertas;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Ofertas
{
    public class GetOfertaByIdQueryHandler : IRequestHandler<GetOfertaByIdQuery, EmpresaOfertaDto?>
    {
        private readonly IEmpresaOfertaRepository _empresaOfertaRepository;
        private readonly IMapper _mapper;

        public GetOfertaByIdQueryHandler(
            IEmpresaOfertaRepository empresaOfertaRepository,
            IMapper mapper)
        {
            _empresaOfertaRepository = empresaOfertaRepository;
            _mapper = mapper;
        }

        public async Task<EmpresaOfertaDto?> Handle(GetOfertaByIdQuery request, CancellationToken cancellationToken)
        {
            var oferta = await _empresaOfertaRepository.GetByIdAsync(request.Id);
            return oferta != null ? _mapper.Map<EmpresaOfertaDto>(oferta) : null;
        }
    }
}