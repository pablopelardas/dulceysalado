using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Ofertas;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Ofertas
{
    public class CreateEmpresaOfertaCommandHandler : IRequestHandler<CreateEmpresaOfertaCommand, EmpresaOfertaDto>
    {
        private readonly IEmpresaOfertaRepository _empresaOfertaRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateEmpresaOfertaCommandHandler> _logger;

        public CreateEmpresaOfertaCommandHandler(
            IEmpresaOfertaRepository empresaOfertaRepository,
            IMapper mapper,
            ILogger<CreateEmpresaOfertaCommandHandler> logger)
        {
            _empresaOfertaRepository = empresaOfertaRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<EmpresaOfertaDto> Handle(CreateEmpresaOfertaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si ya existe
                var existingOferta = await _empresaOfertaRepository.GetByEmpresaAndAgrupacionAsync(
                    request.EmpresaId, request.AgrupacionId);

                if (existingOferta != null)
                {
                    throw new InvalidOperationException(
                        $"Ya existe una oferta para EmpresaId {request.EmpresaId} y AgrupacionId {request.AgrupacionId}");
                }

                // Crear nueva entidad
                var oferta = EmpresaOferta.Create(
                    request.EmpresaId,
                    request.AgrupacionId,
                    request.Visible);

                // Guardar en repositorio
                oferta = await _empresaOfertaRepository.CreateAsync(oferta);

                _logger.LogInformation("Oferta creada exitosamente: EmpresaId {EmpresaId}, AgrupacionId {AgrupacionId}",
                    request.EmpresaId, request.AgrupacionId);

                return _mapper.Map<EmpresaOfertaDto>(oferta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear oferta para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}",
                    request.EmpresaId, request.AgrupacionId);
                throw;
            }
        }
    }
}