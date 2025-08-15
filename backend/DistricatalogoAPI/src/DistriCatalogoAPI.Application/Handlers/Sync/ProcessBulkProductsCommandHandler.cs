using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Sync;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class ProcessBulkProductsCommandHandler : IRequestHandler<ProcessBulkProductsCommand, ProcessBulkProductsResult>
    {
        private readonly ISyncSessionRepository _syncSessionRepository;
        private readonly IProductBaseRepository _productRepository;
        private readonly ICategoryBaseRepository _categoryRepository;
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly IAgrupacionRepository _agrupacionRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IProductoBaseStockRepository _stockRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<ProcessBulkProductsCommandHandler> _logger;

        public ProcessBulkProductsCommandHandler(
            ISyncSessionRepository syncSessionRepository,
            IProductBaseRepository productRepository,
            ICategoryBaseRepository categoryRepository,
            IProductBasePrecioRepository precioRepository,
            IAgrupacionRepository agrupacionRepository,
            IListaPrecioRepository listaPrecioRepository,
            IProductoBaseStockRepository stockRepository,
            ICompanyRepository companyRepository,
            ILogger<ProcessBulkProductsCommandHandler> logger)
        {
            _syncSessionRepository = syncSessionRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _precioRepository = precioRepository;
            _agrupacionRepository = agrupacionRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _stockRepository = stockRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<ProcessBulkProductsResult> Handle(ProcessBulkProductsCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Buscar y validar la sesión
                var session = await _syncSessionRepository.GetByIdAsync(request.SessionId);
                if (session == null)
                {
                    throw new InvalidOperationException("Sesión de sincronización no encontrada");
                }

                // Verificar que la sesión pertenece a la empresa
                if (session.EmpresaPrincipalId != request.EmpresaPrincipalId)
                {
                    throw new InvalidOperationException("La sesión no pertenece a la empresa actual");
                }

                // Verificar estado de la sesión
                if (!session.CanProcessBatch())
                {
                    throw new InvalidOperationException($"No se puede procesar lotes en sesión con estado: {session.Estado}");
                }

                // Cambiar estado a procesando si es el primer lote
                if (session.Estado.Value == "iniciada")
                {
                    session.IniciarProcesamiento();
                }

                // Variables para estadísticas
                var categoriasInfo = new CategoriesProcessResult();
                var productStats = new ProductProcessResult();
                Dictionary<string, ProductBase> productosExistentesMapCompleto;
                Dictionary<int, ListaPrecioStats>? estadisticasPorLista = null;

                if (request.StockOnlyMode)
                {
                    // MODO SOLO STOCK: Solo buscar productos existentes y actualizar stock
                    _logger.LogInformation("Modo SOLO STOCK activado - No se procesarán categorías, productos ni precios");

                    var codigosProductos = request.Productos
                        .Where(p => !string.IsNullOrWhiteSpace(p.Codigo))
                        .Select(p => p.Codigo)
                        .Distinct()
                        .ToList();

                    // Solo obtener productos existentes, no crear ni actualizar nada
                    var productosExistentes = await _productRepository.GetByCodigosAsync(codigosProductos, request.EmpresaPrincipalId);
                    productosExistentesMapCompleto = productosExistentes
                        .GroupBy(p => p.Codigo)
                        .ToDictionary(g => g.Key, g => g.First());

                    // Estadísticas simplificadas para modo stock
                    productStats.ProductosProcesados = productosExistentesMapCompleto.Count;
                    productStats.ProductosActualizados = 0; // No se actualizan productos en modo stock
                    productStats.ProductosNuevos = 0; // No se crean productos en modo stock

                    // Procesar SOLO actualizaciones de stock
                    await ProcessStockUpdates(request.Productos, request.EmpresaPrincipalId, productosExistentesMapCompleto);

                    _logger.LogInformation("Modo SOLO STOCK completado - {ProductosEncontrados} productos encontrados para actualización de stock", 
                        productosExistentesMapCompleto.Count);
                }
                else
                {
                    // MODO NORMAL: Procesar todo (categorías, productos, precios y stock)
                    // Procesar categorías primero (no en paralelo para evitar problemas de DbContext)
                    categoriasInfo = await ProcessCategories(request.Productos, request.EmpresaPrincipalId);
                    
                    var codigosProductos = request.Productos
                        .Where(p => !string.IsNullOrWhiteSpace(p.Codigo))
                        .Select(p => p.Codigo)
                        .Distinct()
                        .ToList();

                    // Obtener productos existentes después de procesar categorías
                    var productosExistentes = await _productRepository.GetByCodigosAsync(codigosProductos, request.EmpresaPrincipalId);

                    // Procesar productos con la información ya obtenida
                    var (productStatsResult, productosExistentesMap) = await ProcessProducts(request.Productos, request.EmpresaPrincipalId, categoriasInfo.CategoriasDisponibles, productosExistentes);
                    productStats = productStatsResult;

                    // IMPORTANTE: Para procesamiento de precios y stock, necesitamos TODOS los productos (existentes + recién creados)
                    // Recargar mapa de productos después del procesamiento para incluir productos recién creados
                    var productosActualizados = await _productRepository.GetByCodigosAsync(codigosProductos, request.EmpresaPrincipalId);
                    productosExistentesMapCompleto = productosActualizados
                        .GroupBy(p => p.Codigo)
                        .ToDictionary(g => g.Key, g => g.First());

                    // Procesar stock por separado usando la nueva tabla productos_base_stock
                    await ProcessStockUpdates(request.Productos, request.EmpresaPrincipalId, productosExistentesMapCompleto);

                    // Asegurar que existe una lista predeterminada antes de procesar precios (solo en el primer lote)
                    if (session.LotesProcesados == 0)
                    {
                        await EnsureDefaultListaPrecioExistsAsync();
                    }

                    // Procesar precios según el modo de la sesión
                    if (session.ListaPrecioId.HasValue)
                    {
                        // Modo tradicional: una lista específica por sesión
                        await ProcessPrices(request.Productos, productStats, session.ListaPrecioId.Value, request.EmpresaPrincipalId);
                    }
                    else
                    {
                        // Modo multi-lista: cada producto puede tener múltiples precios
                        estadisticasPorLista = await ProcessMultiListPrices(request.Productos, productStats, request.EmpresaPrincipalId, productosExistentesMapCompleto);
                    }
                }

                stopwatch.Stop();
                var tiempoProcesamientoMs = (int)stopwatch.ElapsedMilliseconds;

                // Crear resultado del lote
                var batchResult = new SyncSession.BatchResult
                {
                    ProductosProcesados = productStats.ProductosProcesados,
                    ProductosActualizados = productStats.ProductosActualizados,
                    ProductosNuevos = productStats.ProductosNuevos,
                    Errores = productStats.Errores,
                    TiempoProcesamientoMs = tiempoProcesamientoMs,
                    ErroresDetalle = productStats.ErroresDetalle
                };

                // Actualizar estadísticas de la sesión
                session.ActualizarEstadisticas(batchResult);
                await _syncSessionRepository.UpdateAsync(session);

                // Log de métricas
                LogBatchMetrics(request.SessionId, request.LoteNumero, batchResult);

                // Advertencias de performance
                if (tiempoProcesamientoMs > 10000)
                {
                    _logger.LogWarning("Lote procesado lentamente: {TiempoMs}ms para {ProductCount} productos", 
                        tiempoProcesamientoMs, request.Productos.Count);
                }

                return new ProcessBulkProductsResult
                {
                    SessionId = session.Id,
                    LoteNumero = request.LoteNumero,
                    TotalLotes = session.TotalLotesEsperados,
                    Estadisticas = new SyncStatistics
                    {
                        ProductosProcesados = productStats.ProductosProcesados,
                        ProductosActualizados = productStats.ProductosActualizados,
                        ProductosNuevos = productStats.ProductosNuevos,
                        Errores = productStats.Errores,
                        ErroresDetalle = ConvertToErrorDetails(productStats.ErroresDetalle),
                        EstadisticasPorLista = estadisticasPorLista // NUEVO: Incluir estadísticas por lista
                    },
                    TiempoProcesamientoMs = tiempoProcesamientoMs,
                    Progreso = new ProgressInfo
                    {
                        Porcentaje = session.GetProgreso().Porcentaje,
                        LotesProcesados = session.LotesProcesados,
                        TotalLotesEsperados = session.TotalLotesEsperados,
                        Estado = session.Estado.ToString()
                    },
                    CategoriasInfo = new CategoriesInfo
                    {
                        CategoriasExistentesInicialmente = categoriasInfo.CategoriasExistentesInicialmente,
                        CategoriasCreatesAutomaticamente = categoriasInfo.CategoriasCreadas.Count,
                        CategoriasCreatesLista = categoriasInfo.CategoriasCreadas,
                        TotalCategoriasProcesadas = categoriasInfo.CategoriasDisponibles.Count
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error procesando lote {LoteNumero} para sesión {SessionId}", 
                    request.LoteNumero, request.SessionId);
                throw;
            }
        }

        private async Task<CategoriesProcessResult> ProcessCategories(List<BulkProductDto> productos, int empresaId)
        {
            // Diagnóstico: verificar si los productos tienen CategoriaId
            var productosConCategoria = productos.Where(p => p.CategoriaId.HasValue).Count();
            var productosSinCategoria = productos.Count - productosConCategoria;
            
            _logger.LogInformation("Diagnóstico categorías: {TotalProductos} productos, {ConCategoria} con categoría, {SinCategoria} sin categoría", 
                productos.Count, productosConCategoria, productosSinCategoria);

            // Crear un mapeo de CategoriaId -> CategoriaNombre para las categorías que vienen con nombre
            var categoriaNombresMap = productos
                .Where(p => p.CategoriaId.HasValue && !string.IsNullOrWhiteSpace(p.CategoriaNombre))
                .GroupBy(p => p.CategoriaId.Value)
                .ToDictionary(g => g.Key, g => g.First().CategoriaNombre);

            var categoriaIds = productos
                .Where(p => p.CategoriaId.HasValue)
                .Select(p => p.CategoriaId.Value)
                .Distinct()
                .ToList();

            if (categoriaIds.Count == 0)
            {
                _logger.LogWarning("No se encontraron productos con CategoriaId en este lote");
                return new CategoriesProcessResult
                {
                    CategoriasDisponibles = new Dictionary<int, bool>(),
                    CategoriasCreadas = new List<int>(),
                    CategoriasExistentesInicialmente = 0
                };
            }

            _logger.LogInformation("Procesando {Count} categorías únicas: [{Categorias}]", 
                categoriaIds.Count, string.Join(", ", categoriaIds));

            // Verificar cuáles categorías existen
            var existencia = await _categoryRepository.CheckExistenceAsync(categoriaIds);
            var categoriasExistentes = existencia.Where(x => x.Value).Select(x => x.Key).ToList();
            var categoriasInexistentes = categoriaIds.Except(categoriasExistentes).ToList();

            _logger.LogInformation("Categorías existentes: [{Existentes}]", string.Join(", ", categoriasExistentes));

            // Actualizar nombres de categorías existentes si se proporcionaron
            if (categoriasExistentes.Count > 0 && categoriaNombresMap.Count > 0)
            {
                var categoriasParaActualizar = await _categoryRepository.GetByCodigosRubroAsync(categoriasExistentes);
                var actualizadas = 0;
                
                foreach (var categoria in categoriasParaActualizar)
                {
                    if (categoriaNombresMap.TryGetValue(categoria.CodigoRubro, out var nuevoNombre))
                    {
                        categoria.UpdateNombreFromSync(nuevoNombre);
                        actualizadas++;
                    }
                }
                
                if (actualizadas > 0)
                {
                    await _categoryRepository.UpdateBulkAsync(categoriasParaActualizar);
                    _logger.LogInformation("Actualizados nombres de {Count} categorías existentes", actualizadas);
                }
            }

            var categoriasCreadas = new List<int>();

            // Crear categorías faltantes automáticamente con nombres si se proporcionaron
            if (categoriasInexistentes.Count > 0)
            {
                _logger.LogInformation("Creando automáticamente {Count} categorías faltantes: [{Categorias}]", 
                    categoriasInexistentes.Count, string.Join(", ", categoriasInexistentes));

                var categoriasParaCrear = categoriasInexistentes
                    .Select(codigoRubro => 
                    {
                        var nombre = categoriaNombresMap.TryGetValue(codigoRubro, out var n) ? n : null;
                        return CategoryBase.CreateFromSync(codigoRubro, empresaId, nombre);
                    })
                    .ToList();

                try
                {
                    await _categoryRepository.BulkCreateAsync(categoriasParaCrear);
                    categoriasCreadas = categoriasInexistentes;
                    _logger.LogInformation("Creadas exitosamente {Count} categorías", categoriasCreadas.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creando categorías automáticamente");
                    // Continuar sin las categorías que no se pudieron crear
                }
            }

            // Crear diccionario final de categorías disponibles
            var categoriasDisponibles = new Dictionary<int, bool>();
            foreach (var categoriaId in categoriaIds)
            {
                categoriasDisponibles[categoriaId] = categoriasExistentes.Contains(categoriaId) || categoriasCreadas.Contains(categoriaId);
            }

            return new CategoriesProcessResult
            {
                CategoriasDisponibles = categoriasDisponibles,
                CategoriasCreadas = categoriasCreadas,
                CategoriasExistentesInicialmente = categoriasExistentes.Count
            };
        }

        private async Task<(ProductProcessResult result, Dictionary<string, ProductBase> productosExistentesMap)> ProcessProducts(List<BulkProductDto> productos, int empresaId, Dictionary<int, bool> categoriasDisponibles, List<ProductBase> productosExistentes = null)
        {
            var result = new ProductProcessResult();
            
            // Validar y limpiar códigos, detectar duplicados
            var codigosValidos = new List<string>();
            var codigosDuplicados = new HashSet<string>();
            var codigosVistos = new HashSet<string>();
            
            for (int i = 0; i < productos.Count; i++)
            {
                var productoData = productos[i];
                
                if (string.IsNullOrWhiteSpace(productoData.Codigo))
                {
                    result.Errores++;
                    result.ErroresDetalle.Add(new SyncSession.ProductError
                    {
                        ProductoCodigo = productoData.Codigo,
                        ProductoDescripcion = productoData.Descripcion,
                        ErrorTipo = "invalid_code",
                        ErrorMensaje = $"Código de producto vacío o inválido",
                        IndiceEnLote = i
                    });
                    continue;
                }

                var codigo = productoData.Codigo.Trim();

                if (codigosVistos.Contains(codigo))
                {
                    codigosDuplicados.Add(codigo);
                    result.Errores++;
                    result.ErroresDetalle.Add(new SyncSession.ProductError
                    {
                        ProductoCodigo = productoData.Codigo,
                        ProductoDescripcion = productoData.Descripcion,
                        ErrorTipo = "duplicate_code",
                        ErrorMensaje = $"Código duplicado en el lote: {codigo}",
                        IndiceEnLote = i
                    });
                    continue;
                }

                codigosVistos.Add(codigo);
                codigosValidos.Add(codigo);
            }

            if (codigosDuplicados.Count > 0)
            {
                _logger.LogWarning("Encontrados {DuplicateCount} códigos duplicados en el lote: [{Duplicates}]", 
                    codigosDuplicados.Count, string.Join(", ", codigosDuplicados));
            }

            // Usar productos existentes proporcionados o obtenerlos si no se pasaron
            if (productosExistentes == null && codigosValidos.Count > 0)
            {
                productosExistentes = await _productRepository.GetByCodigosAsync(codigosValidos, empresaId);
            }
            else if (productosExistentes == null)
            {
                productosExistentes = new List<ProductBase>();
            }
                
            var productosExistentesMap = productosExistentes
                .Where(p => codigosValidos.Contains(p.Codigo)) // Filtrar solo códigos válidos del lote actual
                .GroupBy(p => p.Codigo)
                .ToDictionary(g => g.Key, g => g.First()); // Usar GroupBy para manejar duplicados de DB
                
            _logger.LogInformation("Usando {ExistingCount} productos existentes para validación de {TotalCodigos} códigos válidos", 
                productosExistentesMap.Count, codigosValidos.Count);
            
            if (productosExistentesMap.Count > 0)
            {
                _logger.LogDebug("Primeros códigos existentes: [{Codigos}]", 
                    string.Join(", ", productosExistentesMap.Keys.Take(5)));
            }
            else
            {
                _logger.LogWarning("No se encontraron productos existentes. Primeros códigos buscados: [{Codigos}]", 
                    string.Join(", ", codigosValidos.Take(5)));
            }

            // Auto-crear agrupaciones nuevas basadas en Grupo3
            await EnsureAgrupacionesExistAsync(productos, empresaId);

            var productosParaCrear = new List<ProductBase>();
            var productosParaActualizar = new List<ProductBase>();

            // Procesar solo productos con códigos válidos (sin duplicados, errores de parsing, etc.)
            var productosValidos = new List<(BulkProductDto producto, string codigo, int indice)>();
            
            for (int i = 0; i < productos.Count; i++)
            {
                var productoData = productos[i];
                
                // Saltar productos que ya fueron marcados como erróneos en la validación previa
                if (string.IsNullOrWhiteSpace(productoData.Codigo) ||
                    codigosDuplicados.Contains(productoData.Codigo.Trim()))
                {
                    continue; // Ya se agregó el error en la validación previa
                }
                
                var codigo = productoData.Codigo.Trim();
                productosValidos.Add((productoData, codigo, i));
            }

            // Procesar productos válidos secuencialmente para evitar problemas de DbContext
            foreach (var (productoData, codigo, indice) in productosValidos)
            {
                try
                {
                    // Validar y asignar categoría (operación rápida, no necesita paralelización)
                    int? codigoRubro = null;
                    if (productoData.CategoriaId.HasValue && 
                        categoriasDisponibles.TryGetValue(productoData.CategoriaId.Value, out bool disponible) && 
                        disponible)
                    {
                        codigoRubro = productoData.CategoriaId.Value;
                    }

                    if (productosExistentesMap.TryGetValue(codigo, out var productoExistente))
                    {
                        // Actualizar producto existente - SOLO campos de Gecom
                        
                        productoExistente.UpdateFromSync(
                            productoData.Descripcion,
                            codigoRubro,
                            productoData.Precio ?? 0,
                            0, // No actualizar existencia aquí, se hace en tabla separada
                            productoData.Grupo1,
                            productoData.Grupo2,
                            productoData.Grupo3,
                            productoData.FechaAlta,
                            productoData.FechaModi,
                            productoData.Imputable,
                            productoData.Disponible,
                            productoData.CodigoUbicacion);

                        productosParaActualizar.Add(productoExistente);
                        result.ProductosActualizados++;
                    }
                    else
                    {
                        // Crear nuevo producto
                        _logger.LogDebug("Creando producto nuevo: código {Codigo}", codigo);
                        
                        var nuevoProducto = ProductBase.CreateFromSync(
                            codigo,
                            productoData.Descripcion,
                            codigoRubro,
                            productoData.Precio ?? 0,
                            0, // No guardar existencia aquí, se hace en tabla separada
                            productoData.Grupo1,
                            productoData.Grupo2,
                            productoData.Grupo3,
                            productoData.FechaAlta,
                            productoData.FechaModi,
                            productoData.Imputable,
                            productoData.Disponible,
                            productoData.CodigoUbicacion,
                            empresaId);

                        productosParaCrear.Add(nuevoProducto);
                        result.ProductosNuevos++;
                    }

                    result.ProductosProcesados++;
                }
                catch (Exception ex)
                {
                    result.Errores++;
                    
                    var errorDetail = new SyncSession.ProductError
                    {
                        ProductoCodigo = productoData.Codigo,
                        ProductoDescripcion = productoData.Descripcion,
                        CategoriaId = productoData.CategoriaId,
                        ErrorTipo = ClassifyError(ex),
                        ErrorMensaje = ex.Message,
                        IndiceEnLote = indice
                    };

                    result.ErroresDetalle.Add(errorDetail);
                    
                    _logger.LogError(ex, "Error procesando producto {Codigo} (índice {Indice})", 
                        productoData.Codigo, indice);
                }
            }

            // Ejecutar operaciones bulk de forma eficiente
            try
            {
                // Combinar todas las operaciones en una sola llamada bulk
                var todosLosProductos = new List<ProductBase>();
                todosLosProductos.AddRange(productosParaCrear);
                todosLosProductos.AddRange(productosParaActualizar);

                if (todosLosProductos.Count > 0)
                {
                    var stopwatchBulk = System.Diagnostics.Stopwatch.StartNew();
                    
                    if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    {
                        _logger.LogDebug("Ejecutando operación bulk para {TotalProductos} productos ({Crear} nuevos, {Actualizar} actualizaciones)", 
                            todosLosProductos.Count, productosParaCrear.Count, productosParaActualizar.Count);
                    }

                    var bulkResult = await _productRepository.BulkCreateOrUpdateAsync(todosLosProductos);
                    
                    stopwatchBulk.Stop();
                    _logger.LogInformation("Operación bulk completada en {ElapsedMs}ms: {Created} creados, {Updated} actualizados, {Failed} fallidos", 
                        stopwatchBulk.ElapsedMilliseconds, bulkResult.Created, bulkResult.Updated, bulkResult.Failed);

                    // Ajustar estadísticas si hay fallos
                    if (bulkResult.Failed > 0)
                    {
                        result.Errores += bulkResult.Failed;
                        
                        // Agregar errores del bulk operation a los detalles
                        foreach (var error in bulkResult.Errors)
                        {
                            result.ErroresDetalle.Add(new SyncSession.ProductError
                            {
                                ProductoCodigo = error.ProductCode,
                                ProductoDescripcion = "Error en operación bulk",
                                ErrorTipo = error.ErrorType,
                                ErrorMensaje = error.ErrorMessage,
                                IndiceEnLote = null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en operaciones bulk de productos");
                throw;
            }

            return (result, productosExistentesMap);
        }

        private string ClassifyError(Exception ex)
        {
            var message = ex.Message.ToLower();
            
            return message switch
            {
                _ when message.Contains("validation") || message.Contains("invalid") => "validation_error",
                _ when message.Contains("duplicate") || message.Contains("unique") => "duplicate_error",
                _ when message.Contains("foreign") || message.Contains("reference") => "reference_error",
                _ when message.Contains("timeout") || message.Contains("connection") => "connection_error",
                _ => "unknown_error"
            };
        }

        private List<ProductErrorDetail> ConvertToErrorDetails(List<SyncSession.ProductError> errors)
        {
            return errors.Select(e => new ProductErrorDetail
            {
                ProductoCodigo = e.ProductoCodigo,
                ProductoDescripcion = e.ProductoDescripcion,
                CategoriaIdOriginal = e.CategoriaId,
                CodigoRubroAsignado = null, // Se podría mapear si es necesario
                ErrorTipo = e.ErrorTipo,
                Error = e.ErrorMensaje,
                Indice = e.IndiceEnLote
            }).ToList();
        }

        private async Task ProcessPrices(List<BulkProductDto> productos, ProductProcessResult productStats, int listaPrecioId, int empresaId)
        {
            try
            {
                _logger.LogInformation("Procesando precios para lista {ListaPrecioId} - {Count} productos", 
                    listaPrecioId, productos.Count);

                // Obtener códigos válidos de productos que fueron procesados exitosamente
                var codigosValidos = productos
                    .Where(p => !string.IsNullOrWhiteSpace(p.Codigo))
                    .Select(p => p.Codigo.Trim())
                    .Distinct()
                    .ToList();

                if (codigosValidos.Count == 0)
                {
                    _logger.LogWarning("No hay códigos válidos para procesar precios");
                    return;
                }

                // Obtener los productos base que fueron creados/actualizados para mapear códigos a IDs
                var productosBase = await _productRepository.GetByCodigosAsync(codigosValidos, empresaId);
                var codigoToProductoId = productosBase.ToDictionary(p => p.Codigo, p => p.Id);

                // Crear registros de precios para productos exitosos
                var preciosParaCrear = new List<ProductoPrecioDto>();

                foreach (var producto in productos)
                {
                    if (string.IsNullOrWhiteSpace(producto.Codigo))
                        continue;

                    var codigo = producto.Codigo.Trim();
                    if (!codigoToProductoId.TryGetValue(codigo, out int productoId))
                    {
                        _logger.LogWarning("No se encontró producto base para código {Codigo}", codigo);
                        continue;
                    }

                    if (producto.Precio <= 0)
                    {
                        _logger.LogDebug("Precio no válido para producto {Codigo}: {Precio}", codigo, producto.Precio);
                        continue;
                    }

                    preciosParaCrear.Add(new ProductoPrecioDto
                    {
                        ProductoBaseId = productoId,
                        ListaPrecioId = listaPrecioId,
                        Precio = producto.Precio ?? 0
                    });
                }

                if (preciosParaCrear.Count > 0)
                {
                    _logger.LogInformation("Creando/actualizando {Count} registros de precios", preciosParaCrear.Count);
                    await _precioRepository.UpsertPreciosAsync(preciosParaCrear);
                    _logger.LogInformation("Precios procesados exitosamente: {Count} registros", preciosParaCrear.Count);
                }
                else
                {
                    _logger.LogWarning("No se generaron registros de precios válidos");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando precios para lista {ListaPrecioId}", listaPrecioId);
                // No lanzar la excepción para que no falle todo el procesamiento de productos
                // Solo log del error, los productos se crearon correctamente
            }
        }

        private async Task<Dictionary<int, ListaPrecioStats>> ProcessMultiListPrices(List<BulkProductDto> productos, ProductProcessResult productStats, int empresaPrincipalId, Dictionary<string, ProductBase> productosExistentesMap)
        {
            var estadisticasPorLista = new Dictionary<int, ListaPrecioStats>();
            
            try
            {
                var productosConListasPrecios = productos.Where(p => p.ListasPrecios?.Any() == true).ToList();
                var totalPrecios = productosConListasPrecios.Sum(p => p.ListasPrecios?.Count ?? 0);
                
                _logger.LogInformation("Procesando precios multi-lista: {ProductosConPrecios} productos, {TotalPrecios} precios", 
                    productosConListasPrecios.Count, totalPrecios);

                // Obtener mapeo de códigos de lista a IDs de BD
                var codigosLista = productos
                    .Where(p => p.ListasPrecios?.Any() == true)
                    .SelectMany(p => p.ListasPrecios)
                    .Select(precio => precio.ListaId.ToString())
                    .Distinct()
                    .ToList();

                // Auto-crear listas de precios que no existan
                await EnsureListasPreciosExistAsync(codigosLista);
                
                // Crear mapeo eficiente usando las listas ya cargadas
                var listasActualizadas = await _listaPrecioRepository.GetAllActiveAsync();
                var codigoToIdMap = listasActualizadas
                    .Where(l => codigosLista.Contains(l.Codigo))
                    .ToDictionary(l => int.Parse(l.Codigo), l => l.Id);

                // Log de listas no encontradas
                var codigosNoEncontrados = codigosLista.Where(codigo => !codigoToIdMap.ContainsKey(int.Parse(codigo))).ToList();
                if (codigosNoEncontrados.Any())
                {
                    _logger.LogWarning("Listas de precios no encontradas después de auto-creación: [{Codigos}]", string.Join(", ", codigosNoEncontrados));
                }
                
                _logger.LogDebug("Mapeo de códigos de lista a IDs: {Count} listas", codigoToIdMap.Count);

                // Agrupar productos por lista de precios para optimizar el procesamiento
                var productosPorLista = new Dictionary<int, List<(BulkProductDto producto, ProductPriceDto precio)>>();
                
                foreach (var producto in productos)
                {
                    if (producto.ListasPrecios?.Any() == true)
                    {
                        foreach (var precio in producto.ListasPrecios)
                        {
                            // Convertir código de lista a ID de BD
                            if (codigoToIdMap.TryGetValue(precio.ListaId, out int listaBdId))
                            {
                                if (!productosPorLista.ContainsKey(listaBdId))
                                    productosPorLista[listaBdId] = new List<(BulkProductDto, ProductPriceDto)>();
                                
                                productosPorLista[listaBdId].Add((producto, precio));
                            }
                            else
                            {
                                _logger.LogWarning("Código de lista {Codigo} no mapeado a ID de BD", precio.ListaId);
                            }
                        }
                    }
                }

                // Procesar cada lista de precios
                foreach (var kvp in productosPorLista)
                {
                    var listaBdId = kvp.Key;
                    var productosConPrecios = kvp.Value;
                    
                    var stats = new ListaPrecioStats
                    {
                        ListaId = listaBdId,
                        ListaNombre = $"Lista {listaBdId}" // TODO: Obtener nombre real de la lista
                    };

                    try
                    {
                        _logger.LogDebug("Procesando {Count} precios para lista {ListaId}", productosConPrecios.Count, listaBdId);

                        var preciosParaInsertar = new List<ProductoPrecioDto>();
                        
                        foreach (var (producto, precio) in productosConPrecios)
                        {
                            // Usar los productos ya procesados anteriormente para obtener los IDs
                            // Buscar en la lista de productos existentes ya cargados en memoria
                            if (string.IsNullOrWhiteSpace(producto.Codigo)) 
                            {
                                _logger.LogWarning("Código de producto vacío para precios: {Codigo}", producto.Codigo);
                                continue;
                            }
                            
                            var codigo = producto.Codigo.Trim();
                            var productoExistente = productosExistentesMap.TryGetValue(codigo, out var prod) ? prod : null;
                            
                            if (productoExistente != null)
                            {
                                preciosParaInsertar.Add(new ProductoPrecioDto
                                {
                                    ProductoBaseId = productoExistente.Id,
                                    ListaPrecioId = listaBdId,
                                    Precio = precio.Precio
                                });
                                
                            }
                            else
                            {
                                _logger.LogWarning("Producto no encontrado en mapa existentes para código {Codigo} (necesario para precios)", codigo);
                            }
                        }

                        if (preciosParaInsertar.Any())
                        {
                            await _precioRepository.UpsertPreciosAsync(preciosParaInsertar);
                            
                            stats.PreciosNuevos = preciosParaInsertar.Count;
                            stats.PreciosActualizados = preciosParaInsertar.Count;
                            stats.Errores = 0;

                            _logger.LogDebug("Lista {ListaId}: {Count} precios procesados", 
                                listaBdId, preciosParaInsertar.Count);
                        }
                        else
                        {
                            _logger.LogWarning("Lista {ListaId}: No se generaron precios válidos para insertar", listaBdId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando precios para lista {ListaId}", listaBdId);
                        stats.Errores = productosConPrecios.Count;
                        stats.ErroresDetalle.Add($"Error general: {ex.Message}");
                    }

                    estadisticasPorLista[listaBdId] = stats;
                }

                return estadisticasPorLista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ProcessMultiListPrices");
                return estadisticasPorLista;
            }
        }

        private void LogBatchMetrics(Guid sessionId, int loteNumero, SyncSession.BatchResult result)
        {
            var productosPerSecond = result.TiempoProcesamientoMs > 0 
                ? result.ProductosProcesados / (result.TiempoProcesamientoMs / 1000.0) 
                : 0;

            _logger.LogInformation("Métricas lote {LoteNumero} (Sesión {SessionId}): {Procesados} procesados, {Nuevos} nuevos, {Actualizados} actualizados, {Errores} errores, {TiempoMs}ms, {ProductosPerSec:F2} prod/seg",
                loteNumero, sessionId, result.ProductosProcesados, result.ProductosNuevos, 
                result.ProductosActualizados, result.Errores, result.TiempoProcesamientoMs, productosPerSecond);
        }

        private class CategoriesProcessResult
        {
            public Dictionary<int, bool> CategoriasDisponibles { get; set; } = new();
            public List<int> CategoriasCreadas { get; set; } = new();
            public int CategoriasExistentesInicialmente { get; set; }
        }

        private class ProductProcessResult
        {
            public int ProductosProcesados { get; set; }
            public int ProductosActualizados { get; set; }
            public int ProductosNuevos { get; set; }
            public int Errores { get; set; }
            public List<SyncSession.ProductError> ErroresDetalle { get; set; } = new();
        }

        /// <summary>
        /// Asegura que todas las agrupaciones (Grupo3) existan en la base de datos,
        /// creando automáticamente las que falten usando operaciones bulk para optimizar performance.
        /// </summary>
        private async Task EnsureAgrupacionesExistAsync(List<BulkProductDto> productos, int empresaPrincipalId)
        {
            try
            {
                // Obtener todos los valores únicos de los 3 grupos
                var agrupacionesPorProcesar = new List<(int codigo, int tipo, string tipoNombre)>();

                // Grupo1 → tipo = 1 (Novedades/Ofertas)
                var grupo1Values = productos
                    .Where(p => p.Grupo1.HasValue && p.Grupo1.Value > 0)
                    .Select(p => p.Grupo1.Value)
                    .Distinct();
                
                foreach (var codigo in grupo1Values)
                {
                    agrupacionesPorProcesar.Add((codigo, 1, "Grupo1(Novedades/Ofertas)"));
                }

                // Grupo2 → tipo = 2 (Futuro)
                var grupo2Values = productos
                    .Where(p => p.Grupo2.HasValue && p.Grupo2.Value > 0)
                    .Select(p => p.Grupo2.Value)
                    .Distinct();
                
                foreach (var codigo in grupo2Values)
                {
                    agrupacionesPorProcesar.Add((codigo, 2, "Grupo2(Futuro)"));
                }

                // Grupo3 → tipo = 3 (Actual)
                var grupo3Values = productos
                    .Where(p => p.Grupo3.HasValue && p.Grupo3.Value > 0)
                    .Select(p => p.Grupo3.Value)
                    .Distinct();
                
                foreach (var codigo in grupo3Values)
                {
                    agrupacionesPorProcesar.Add((codigo, 3, "Grupo3(Actual)"));
                }

                if (!agrupacionesPorProcesar.Any())
                {
                    _logger.LogDebug("No hay valores de grupos para procesar en este lote");
                    return;
                }

                // NUEVO: Agrupar por (codigo, tipo) para permitir agrupaciones repetidas en distintos tipos
                var agrupacionesUnicas = agrupacionesPorProcesar
                    .GroupBy(a => (a.codigo, a.tipo))
                    .Select(g => g.First())
                    .ToList();

                var codigosYTiposUnicos = agrupacionesUnicas.Select(a => (a.codigo, a.tipo)).ToList();

                _logger.LogInformation("Verificando {Count} agrupaciones únicas (codigo+tipo) para empresa {EmpresaId}: [{Grupos}]", 
                    agrupacionesUnicas.Count, empresaPrincipalId, string.Join(", ", codigosYTiposUnicos.Take(10)));

                // NUEVO: Verificar existencia de agrupaciones por (codigo, tipo)
                // Como no existe un método bulk, se hace una consulta manual
                var existentes = await _agrupacionRepository.GetByEmpresaPrincipalAsync(empresaPrincipalId, true);
                var existentesSet = existentes.Select(a => (a.Codigo, a.Tipo)).ToHashSet();

                var agrupacionesFaltantes = agrupacionesUnicas
                    .Where(a => !existentesSet.Contains((a.codigo, a.tipo)))
                    .ToList();
                    
                if (agrupacionesFaltantes.Any())
                {
                    _logger.LogInformation("Creando {Count} nuevas agrupaciones en bulk para empresa {EmpresaId}", 
                        agrupacionesFaltantes.Count, empresaPrincipalId);

                    // Crear todas las agrupaciones faltantes con el tipo correcto
                    var nuevasAgrupaciones = new List<Agrupacion>();
                    foreach (var a in agrupacionesFaltantes)
                    {
                        _logger.LogDebug("Creando agrupación código {Codigo} con tipo {Tipo} ({TipoNombre})", 
                            a.codigo, a.tipo, a.tipoNombre);
                        
                        var agrupacion = Agrupacion.CreateFromSync(
                            a.codigo, 
                            empresaPrincipalId,
                            $"Agrupación {a.codigo}",
                            $"Agrupación auto-creada desde sincronización para {a.tipoNombre}: {a.codigo}",
                            a.tipo);
                        
                        nuevasAgrupaciones.Add(agrupacion);
                    }

                    await _agrupacionRepository.CreateBulkAsync(nuevasAgrupaciones);

                    // Log detallado por tipo
                    var tipoStats = agrupacionesFaltantes
                        .GroupBy(a => a.tipo)
                        .Select(g => $"Tipo {g.Key}: {g.Count()}")
                        .ToList();

                    _logger.LogInformation("Auto-creadas {Count} nuevas agrupaciones para empresa {EmpresaId} - {TipoStats}", 
                        agrupacionesFaltantes.Count, empresaPrincipalId, string.Join(", ", tipoStats));
                }
                else
                {
                    _logger.LogDebug("Todas las {Count} agrupaciones ya existían, no fue necesario crear ninguna", agrupacionesUnicas.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al auto-crear agrupaciones para empresa {EmpresaId}", empresaPrincipalId);
                // No re-throw para no interrumpir el procesamiento de productos
                // Las agrupaciones se pueden crear manualmente después si es necesario
            }
        }

        /// <summary>
        /// Asegura que todas las listas de precios especificadas existan en la base de datos,
        /// creando automáticamente las que falten.
        /// </summary>
        private async Task EnsureListasPreciosExistAsync(List<string> codigosLista)
        {
            try
            {
                if (!codigosLista.Any())
                {
                    _logger.LogDebug("No hay códigos de listas de precios para procesar");
                    return;
                }

                _logger.LogInformation("Verificando {Count} listas de precios: [{Codigos}]", 
                    codigosLista.Count, string.Join(", ", codigosLista));

                // Verificar cuáles listas existen en una sola consulta bulk
                var listasActivasExistentes = await _listaPrecioRepository.GetAllActiveAsync();
                var codigosExistentes = listasActivasExistentes.Select(l => l.Codigo).ToHashSet();
                
                var listasExistentes = codigosLista.Where(codigo => codigosExistentes.Contains(codigo)).ToList();
                var listasFaltantes = codigosLista.Where(codigo => !codigosExistentes.Contains(codigo)).ToList();

                _logger.LogInformation("Listas existentes: [{Existentes}], Listas faltantes: [{Faltantes}]", 
                    string.Join(", ", listasExistentes), string.Join(", ", listasFaltantes));

                // Crear listas faltantes
                if (listasFaltantes.Any())
                {
                    _logger.LogInformation("Creando {Count} listas de precios faltantes", listasFaltantes.Count);

                    foreach (var codigo in listasFaltantes)
                    {
                        try
                        {
                            // Crear lista de precios usando el repositorio directamente
                            var listaId = await _listaPrecioRepository.CreateAsync(
                                codigo,
                                $"Lista {codigo}",
                                $"Lista de precios auto-creada desde sincronización: {codigo}",
                                0 // orden
                            );

                            _logger.LogInformation("Lista de precios {Codigo} creada exitosamente con ID {Id}", codigo, listaId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creando lista de precios {Codigo}", codigo);
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("Todas las {Count} listas de precios ya existían", codigosLista.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al auto-crear listas de precios");
                // No re-throw para no interrumpir el procesamiento de productos
            }
        }

        /// <summary>
        /// Procesa las actualizaciones de stock usando la nueva tabla productos_base_stock
        /// </summary>
        private async Task ProcessStockUpdates(List<BulkProductDto> productos, int empresaId, Dictionary<string, ProductBase> productosExistentesMap)
        {
            try
            {
                _logger.LogInformation("Procesando stock diferenciado por empresa para {Count} productos", productos.Count);

                // Agrupar actualizaciones de stock por empresa
                var stockUpdatesByEmpresa = new Dictionary<int, Dictionary<int, decimal>>();
                var empresasAfectadas = new HashSet<int>();

                foreach (var producto in productos)
                {
                    if (string.IsNullOrWhiteSpace(producto.Codigo))
                        continue;

                    var codigo = producto.Codigo.Trim();
                    if (!productosExistentesMap.TryGetValue(codigo, out var productoBase))
                    {
                        _logger.LogWarning("No se encontró producto base para código {Codigo} al procesar stock", codigo);
                        continue;
                    }

                    // Procesar stock por empresa desde StocksPorEmpresa
                    foreach (var stockEmpresa in producto.StocksPorEmpresa)
                    {
                        // Ignorar empresaId <= 0 
                        if (stockEmpresa.EmpresaId <= 0)
                        {
                            _logger.LogWarning("EmpresaId inválido {EmpresaId}, omitiendo stock para producto {Codigo}", 
                                stockEmpresa.EmpresaId, codigo);
                            continue;
                        }

                        if (!stockUpdatesByEmpresa.ContainsKey(stockEmpresa.EmpresaId))
                        {
                            stockUpdatesByEmpresa[stockEmpresa.EmpresaId] = new Dictionary<int, decimal>();
                        }

                        stockUpdatesByEmpresa[stockEmpresa.EmpresaId][productoBase.Id] = stockEmpresa.Stock;
                        empresasAfectadas.Add(stockEmpresa.EmpresaId);
                    }
                }

                if (stockUpdatesByEmpresa.Any())
                {
                    _logger.LogInformation("Actualizando stock para {EmpresaCount} empresas con {ProductUpdates} actualizaciones", 
                        empresasAfectadas.Count, stockUpdatesByEmpresa.Sum(kvp => kvp.Value.Count));

                    // Validar en batch qué empresas existen para optimizar
                    var empresasIds = stockUpdatesByEmpresa.Keys.ToList();
                    var empresasExistentes = await _companyRepository.GetByIdsAsync(empresasIds);
                    var empresasExistentesIds = empresasExistentes.Select(e => e.Id).ToHashSet();

                    // Procesar actualizaciones solo para empresas existentes
                    foreach (var empresaStockUpdate in stockUpdatesByEmpresa)
                    {
                        try
                        {
                            var empresaIdActual = empresaStockUpdate.Key;
                            var stockUpdates = empresaStockUpdate.Value;

                            // Verificar si la empresa existe (ya validado en batch)
                            if (!empresasExistentesIds.Contains(empresaIdActual))
                            {
                                _logger.LogWarning("EmpresaId {EmpresaId} no existe, omitiendo actualización de stock para {ProductCount} productos", 
                                    empresaIdActual, stockUpdates.Count);
                                continue;
                            }

                            _logger.LogDebug("Actualizando stock para empresa {EmpresaId} con {ProductCount} productos", 
                                empresaIdActual, stockUpdates.Count);
                            
                            // Usar el método bulk del repositorio de stock por empresa específica
                            await _stockRepository.BulkUpdateStockAsync(stockUpdates, empresaIdActual);
                            
                            _logger.LogDebug("Stock actualizado exitosamente para empresa {EmpresaId} ({ProductCount} productos)", 
                                empresaIdActual, stockUpdates.Count);
                        }
                        catch (Exception exEmpresa)
                        {
                            _logger.LogError(exEmpresa, "Error actualizando stock para empresa {EmpresaId}", 
                                empresaStockUpdate.Key);
                            // Continuar con las demás empresas
                        }
                    }

                    _logger.LogInformation("Stock diferenciado actualizado para {EmpresaCount} empresas, {TotalUpdates} actualizaciones totales", 
                        empresasAfectadas.Count, stockUpdatesByEmpresa.Sum(kvp => kvp.Value.Count));
                }
                else
                {
                    _logger.LogWarning("No se generaron actualizaciones de stock válidas por empresa");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando actualizaciones de stock por empresa");
                // No re-throw para no interrumpir todo el procesamiento
            }
        }

        /// <summary>
        /// Asegura que existe una lista de precios predeterminada en el sistema.
        /// Si no existe ninguna, marca la lista con código "1" como predeterminada.
        /// </summary>
        private async Task EnsureDefaultListaPrecioExistsAsync()
        {
            try
            {
                // Verificar si ya existe una lista predeterminada
                var defaultListId = await _listaPrecioRepository.GetDefaultListIdAsync();
                
                if (defaultListId.HasValue)
                {
                    _logger.LogDebug("Lista predeterminada ya existe con ID {Id}", defaultListId.Value);
                    return;
                }

                // Buscar la lista con código "1"
                var listaId = await _listaPrecioRepository.GetIdByCodigoAsync("1");
                
                if (!listaId.HasValue)
                {
                    _logger.LogWarning("No se encontró lista con código '1' para marcar como predeterminada");
                    return;
                }

                _logger.LogInformation("No existe lista predeterminada, marcando lista código '1' (ID {Id}) como predeterminada", listaId.Value);

                // Marcar la lista con código "1" como predeterminada
                await _listaPrecioRepository.SetDefaultAsync(listaId.Value);

                _logger.LogInformation("Lista código '1' (ID {Id}) marcada como predeterminada exitosamente", listaId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar lista predeterminada");
                // No re-throw para no interrumpir el procesamiento
            }
        }
    }
}