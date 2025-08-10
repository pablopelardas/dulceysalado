using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Novedades;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Novedades
{
    public class CreateEmpresaNovedadCommandHandler : IRequestHandler<CreateEmpresaNovedadCommand, EmpresaNovedadDto>
    {
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateEmpresaNovedadCommandHandler> _logger;

        public CreateEmpresaNovedadCommandHandler(
            IEmpresaNovedadRepository empresaNovedadRepository,
            IMapper mapper,
            ILogger<CreateEmpresaNovedadCommandHandler> logger)
        {
            _empresaNovedadRepository = empresaNovedadRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<EmpresaNovedadDto> Handle(CreateEmpresaNovedadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si ya existe
                var existingNovedad = await _empresaNovedadRepository.GetByEmpresaAndAgrupacionAsync(
                    request.EmpresaId, request.AgrupacionId);

                if (existingNovedad != null)
                {
                    throw new InvalidOperationException(
                        $"Ya existe una novedad para EmpresaId {request.EmpresaId} y AgrupacionId {request.AgrupacionId}");
                }

                // Crear nueva entidad
                var novedad = EmpresaNovedad.Create(
                    request.EmpresaId,
                    request.AgrupacionId,
                    request.Visible);

                // Guardar en repositorio
                novedad = await _empresaNovedadRepository.CreateAsync(novedad);

                _logger.LogInformation("Novedad creada exitosamente: EmpresaId {EmpresaId}, AgrupacionId {AgrupacionId}",
                    request.EmpresaId, request.AgrupacionId);

                return _mapper.Map<EmpresaNovedadDto>(novedad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear novedad para EmpresaId {EmpresaId} y AgrupacionId {AgrupacionId}",
                    request.EmpresaId, request.AgrupacionId);
                throw;
            }
        }
    }
}