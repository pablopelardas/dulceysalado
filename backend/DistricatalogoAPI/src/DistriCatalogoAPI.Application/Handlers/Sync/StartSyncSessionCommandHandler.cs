using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Sync;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class StartSyncSessionCommandHandler : IRequestHandler<StartSyncSessionCommand, StartSyncSessionResult>
    {
        private readonly ISyncSessionRepository _syncSessionRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<StartSyncSessionCommandHandler> _logger;

        public StartSyncSessionCommandHandler(
            ISyncSessionRepository syncSessionRepository,
            ICompanyRepository companyRepository,
            IListaPrecioRepository listaPrecioRepository,
            ILogger<StartSyncSessionCommandHandler> logger)
        {
            _syncSessionRepository = syncSessionRepository;
            _companyRepository = companyRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<StartSyncSessionResult> Handle(StartSyncSessionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando nueva sesión de sincronización para empresa {EmpresaId}", 
                request.EmpresaPrincipalId);

            try
            {
                // Verificar que la empresa existe y es empresa principal
                var empresa = await _companyRepository.GetByIdAsync(request.EmpresaPrincipalId);
                if (empresa == null)
                {
                    throw new InvalidOperationException("Empresa no encontrada");
                }

                if (empresa.TipoEmpresa != "principal")
                {
                    throw new InvalidOperationException("Solo las empresas principales pueden realizar sincronización");
                }

                // Verificar si ya hay sesiones activas para esta empresa
                var hasActiveSessions = await _syncSessionRepository.ExistsActiveSessionAsync(request.EmpresaPrincipalId);
                if (hasActiveSessions)
                {
                    _logger.LogWarning("La empresa {EmpresaId} ya tiene sesiones activas de sincronización", 
                        request.EmpresaPrincipalId);
                    // Permitir continuar pero registrar la advertencia
                }

                // Resolver lista de precios según el modo
                int? listaPrecioId = null;
                (string codigo, string nombre)? listaInfo = null;
                
                if (request.MultiLista)
                {
                    // Modo multi-lista: no necesita lista específica
                    _logger.LogInformation("Sesión en modo multi-lista - los precios se procesarán por producto");
                }
                else if (!string.IsNullOrEmpty(request.ListaCodigo))
                {
                    // Modo tradicional: verificar lista específica
                    var exists = await _listaPrecioRepository.ExistsAndActiveAsync(request.ListaCodigo);
                    if (!exists)
                    {
                        throw new InvalidOperationException($"Lista de precios con código '{request.ListaCodigo}' no encontrada o no está activa");
                    }
                    
                    // Obtener el ID
                    listaPrecioId = await _listaPrecioRepository.GetIdByCodigoAsync(request.ListaCodigo);
                    if (!listaPrecioId.HasValue)
                    {
                        throw new InvalidOperationException($"No se pudo obtener ID para lista de precios '{request.ListaCodigo}'");
                    }
                    
                    // Obtener información para el resultado
                    listaInfo = await _listaPrecioRepository.GetCodigoAndNombreByIdAsync(listaPrecioId.Value);
                    
                    _logger.LogInformation("Sesión asociada a lista de precios: {ListaCodigo} (ID: {ListaId})", 
                        request.ListaCodigo, listaPrecioId);
                }
                else
                {
                    _logger.LogInformation("Sesión sin lista de precios específica - no se crearán registros de precios");
                }

                // Crear nueva sesión
                _logger.LogDebug("Creando nueva sesión para empresa {EmpresaId} con {TotalLotes} lotes esperados", 
                    request.EmpresaPrincipalId, request.TotalLotesEsperados);

                var session = new SyncSession(
                    request.EmpresaPrincipalId,
                    request.TotalLotesEsperados,
                    request.UsuarioProceso ?? "SISTEMA",
                    request.IpOrigen ?? "Unknown",
                    listaPrecioId);

                _logger.LogDebug("Sesión creada en memoria: {SessionId}, Estado: {Estado}", 
                    session.Id, session.Estado?.ToString() ?? "NULL");

                var createdSession = await _syncSessionRepository.CreateAsync(session);

                if (createdSession == null)
                {
                    throw new InvalidOperationException("Error al crear la sesión - el repository devolvió null");
                }

                _logger.LogInformation("Sesión de sincronización creada exitosamente: {SessionId} para empresa {EmpresaId}", 
                    createdSession.Id, request.EmpresaPrincipalId);

                // Validar propiedades antes de crear el resultado
                if (createdSession.Estado == null)
                {
                    _logger.LogError("createdSession.Estado es null para sesión {SessionId}", createdSession.Id);
                    throw new InvalidOperationException("El estado de la sesión es null");
                }

                if (string.IsNullOrEmpty(empresa.Nombre))
                {
                    _logger.LogError("empresa.Nombre es null o vacío para empresa {EmpresaId}", request.EmpresaPrincipalId);
                    throw new InvalidOperationException("El nombre de la empresa es null o vacío");
                }

                return new StartSyncSessionResult
                {
                    SessionId = createdSession.Id,
                    EmpresaPrincipal = empresa.Nombre,
                    FechaInicio = createdSession.FechaInicio,
                    TotalLotesEsperados = createdSession.TotalLotesEsperados,
                    Estado = createdSession.Estado.ToString(),
                    ListaCodigo = listaInfo?.codigo,
                    ListaNombre = listaInfo?.nombre,
                    MultiLista = request.MultiLista
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión de sincronización para empresa {EmpresaId}", 
                    request.EmpresaPrincipalId);
                throw;
            }
        }
    }
}