const { sequelize } = require('../config/database');
const { SyncSession, ProductoBase, CategoriaBase, Empresa, SyncLog } = require('../models');
const { Op } = require('sequelize');
const SyncLogger = require('../services/syncLogger');

class SyncController {
  /**
   * POST /api/admin/sync/session/start
   * Inicia una nueva sesión de sincronización
   */
  static async startSession(req, res) {
    console.log("startSession");
    const transaction = await sequelize.transaction();
    
    try {
      const { total_lotes_esperados, usuario_proceso } = req.body;
      const empresaPrincipal = req.empresa;
      
      // Crear nueva sesión
      const session = await SyncSession.create({
        empresa_principal_id: empresaPrincipal.id,
        total_lotes_esperados,
        usuario_proceso,
        ip_origen: req.ip || req.connection.remoteAddress,
        estado: 'iniciada'
      }, { transaction });

      await transaction.commit();

      // Log del inicio de sesión
      SyncLogger.logSessionStart(
        session.id, 
        empresaPrincipal.id, 
        total_lotes_esperados, 
        usuario_proceso
      );

      res.status(201).json({
        success: true,
        message: 'Sesión de sincronización iniciada',
        session_id: session.id,
        empresa_principal: empresaPrincipal.nombre,
        fecha_inicio: session.fecha_inicio,
        total_lotes_esperados: session.total_lotes_esperados
      });

    } catch (error) {
      await transaction.rollback();
      console.error('Error al iniciar sesión de sync:', error);
      
      res.status(500).json({
        success: false,
        message: 'Error interno al iniciar sesión de sincronización',
        error: process.env.NODE_ENV === 'development' ? error.message : undefined
      });
    }
  }

  /**
   * POST /api/admin/sync/productos/bulk
   * Procesa un lote de productos
   */
  static async processBulkProducts(req, res) {
    const startTime = Date.now();
    let transaction;
    
    try {
      const { productos, session_id, lote_numero } = req.body;
      const empresaPrincipal = req.empresa;

      // Buscar la sesión
      const session = await SyncSession.findByPk(session_id);
      if (!session) {
        return res.status(404).json({
          success: false,
          message: 'Sesión de sincronización no encontrada'
        });
      }

      // Verificar que la sesión pertenece a la empresa
      if (session.empresa_principal_id !== empresaPrincipal.id) {
        return res.status(403).json({
          success: false,
          message: 'Sesión no pertenece a la empresa actual'
        });
      }

      // Verificar estado de la sesión
      if (!['iniciada', 'procesando'].includes(session.estado)) {
        return res.status(400).json({
          success: false,
          message: `No se puede procesar lotes en sesión con estado: ${session.estado}`
        });
      }

      // Cambiar estado a procesando si es el primer lote
      if (session.estado === 'iniciada') {
        await session.update({ estado: 'procesando' });
      }

      transaction = await sequelize.transaction();

      // Obtener y crear categorías automáticamente
      const categoriaIds = [...new Set(productos.map(p => p.categoria_id).filter(id => id !== null && id !== undefined))];
      let categoriasExistentesIds = [];
      let categoriasCreadas = [];
      
      console.log(`🔍 Procesando categorías en lote de ${productos.length} productos`);
      console.log(`📊 Categorías únicas en el lote: [${categoriaIds.join(', ')}]`);
      
      if (categoriaIds.length > 0) {
        // Buscar categorías existentes por codigo_rubro
        const categoriasExistentes = await CategoriaBase.findAll({
          where: { codigo_rubro: { [Op.in]: categoriaIds } },
          attributes: ['codigo_rubro'],
          transaction
        });

        categoriasExistentesIds = categoriasExistentes.map(c => c.codigo_rubro);
        const categoriasInexistentes = categoriaIds.filter(id => !categoriasExistentesIds.includes(id));
        
        console.log(`✅ Categorías existentes: [${categoriasExistentesIds.join(', ')}]`);
        
        // Crear automáticamente las categorías faltantes
        if (categoriasInexistentes.length > 0) {
          console.log(`🔧 Creando automáticamente ${categoriasInexistentes.length} categorías faltantes: [${categoriasInexistentes.join(', ')}]`);
          
          for (const codigoRubro of categoriasInexistentes) {
            try {
              const nuevaCategoria = await CategoriaBase.create({
                codigo_rubro: codigoRubro,
                nombre: `Categoría ${codigoRubro}`, // Nombre temporal - se puede editar después
                icono: '📦', // Icono por defecto
                visible: true,
                orden: 999 + codigoRubro, // Orden alto para que aparezcan al final
                color: '#6B7280', // Color gris por defecto
                descripcion: `Categoría creada automáticamente desde sincronización Gecom para rubro ${codigoRubro}`,
                created_by_empresa_id: empresaPrincipal.id
              }, { transaction });
              
              categoriasCreadas.push(codigoRubro);
              console.log(`✅ Categoría ${codigoRubro} creada exitosamente`);
              
            } catch (createError) {
              console.error(`❌ Error creando categoría ${codigoRubro}:`, createError.message);
              // Si falla la creación, continúa con las demás
            }
          }
          
          // Actualizar lista de categorías existentes
          categoriasExistentesIds = [...categoriasExistentesIds, ...categoriasCreadas];
          console.log(`📈 Total categorías disponibles después de creación: [${categoriasExistentesIds.join(', ')}]`);
        }
      }

      // Obtener productos existentes por código
      const codigosProductos = productos.map(p => p.codigo);
      const productosExistentes = await ProductoBase.findAll({
        where: { 
          codigo: { [Op.in]: codigosProductos },
          administrado_por_empresa_id: empresaPrincipal.id
        },
        transaction
      });

      const productosExistentesMap = new Map(
        productosExistentes.map(p => [p.codigo, p])
      );

      // Estadísticas del lote
      const stats = {
        productos_procesados: 0,
        productos_actualizados: 0,
        productos_nuevos: 0,
        errores: 0,
        errores_detalle: []
      };

      // Procesar cada producto
      for (let i = 0; i < productos.length; i++) {
        const productoData = productos[i];
        
        try {
          // Asignar categoria (ahora todas deberían existir después de la creación automática)
          let codigoRubro = null; // Por defecto null
          
          if (productoData.categoria_id !== null && productoData.categoria_id !== undefined) {
            if (categoriasExistentesIds.includes(productoData.categoria_id)) {
              codigoRubro = productoData.categoria_id; // Categoría válida (existente o recién creada)
              console.log(`✅ Producto ${productoData.codigo}: Categoría ${productoData.categoria_id} asignada`);
            } else {
              // Esto no debería pasar ahora, pero mantenemos el fallback
              codigoRubro = null;
              console.warn(`⚠️ Producto ${productoData.codigo}: Categoría ${productoData.categoria_id} no pudo procesarse, asignando NULL`);
            }
          } else {
            console.log(`📝 Producto ${productoData.codigo}: Sin categoría especificada, asignando NULL`);
          }

          const productoExistente = productosExistentesMap.get(productoData.codigo);
          
          if (productoExistente) {
            // SOLO actualizar campos que vienen de Gecom - según esquema productos_base
            const updateData = {
              // Campos base de Gecom (SIEMPRE se actualizan)
              descripcion: productoData.descripcion,
              codigo_rubro: codigoRubro, // Usar la categoría validada (null si no existe)
              precio: productoData.precio,
              existencia: productoData.existencia || productoData.stock, // Mapear stock a existencia
              grupo1: productoData.grupo1,
              grupo2: productoData.grupo2,
              grupo3: productoData.grupo3,
              fecha_alta: productoData.fecha_alta,
              fecha_modi: productoData.fecha_modi || new Date(),
              imputable: productoData.imputable || 'S',
              disponible: productoData.disponible || 'S',
              codigo_ubicacion: productoData.codigo_ubicacion,
              
              // Control de sincronización
              actualizado_gecom: new Date()
            };
            
            console.log(`🔄 Actualizando producto ${productoData.codigo} con codigo_rubro: ${codigoRubro}`);
            console.log(`📋 Datos de actualización:`, JSON.stringify(updateData, null, 2));

            // Actualizar producto existente - SOLO campos de Gecom
            await productoExistente.update(updateData, { transaction });
            
            stats.productos_actualizados++;
          } else {
            // Crear nuevo producto con estructura correcta del esquema
            const nuevoProductoData = {
              // Campos base de Gecom
              codigo: productoData.codigo,
              descripcion: productoData.descripcion,
              codigo_rubro: codigoRubro, // Usar la categoría validada (null si no existe)
              precio: productoData.precio,
              existencia: productoData.existencia || productoData.stock || 0, // Mapear stock a existencia
              grupo1: productoData.grupo1,
              grupo2: productoData.grupo2,
              grupo3: productoData.grupo3,
              fecha_alta: productoData.fecha_alta || new Date(),
              fecha_modi: productoData.fecha_modi || new Date(),
              imputable: productoData.imputable || 'S',
              disponible: productoData.disponible || 'S',
              codigo_ubicacion: productoData.codigo_ubicacion,
              
              // Control de administración
              administrado_por_empresa_id: empresaPrincipal.id,
              actualizado_gecom: new Date(),
              
              // Campos web con valores por defecto (se manejan desde panel)
              visible: true,
              destacado: false,
              orden_categoria: 0,
              unidad_medida: 'UN'
            };
            
            console.log(`➕ Creando producto ${productoData.codigo} con codigo_rubro: ${codigoRubro}`);
            console.log(`📋 Datos de creación:`, JSON.stringify(nuevoProductoData, null, 2));
            
            await ProductoBase.create(nuevoProductoData, { transaction });
            
            stats.productos_nuevos++;
          }
          
          stats.productos_procesados++;
          
        } catch (productError) {
          stats.errores++;
          
          // Análisis específico del error
          let errorTipo = 'unknown';
          let errorMensaje = productError.message;
          
          if (productError.code === 'ER_NO_REFERENCED_ROW_2') {
            errorTipo = 'foreign_key_constraint';
            errorMensaje = `Foreign key constraint: codigo_rubro ${codigoRubro} no existe`;
            console.error(`❌ CONSTRAINT ERROR - Producto ${productoData.codigo}: codigo_rubro ${codigoRubro} debería ser NULL pero se está enviando valor inválido`);
          } else if (productError.name === 'SequelizeValidationError') {
            errorTipo = 'validation_error';
            console.error(`❌ VALIDATION ERROR - Producto ${productoData.codigo}:`, productError.errors);
          } else {
            console.error(`❌ UNKNOWN ERROR - Producto ${productoData.codigo}:`, productError);
          }
          
          stats.errores_detalle.push({
            producto_codigo: productoData.codigo,
            producto_descripcion: productoData.descripcion,
            categoria_id_original: productoData.categoria_id,
            codigo_rubro_asignado: codigoRubro,
            error_tipo: errorTipo,
            error: errorMensaje,
            indice: i
          });
          
          // Log individual del error pero continúa procesando
          console.error(`💥 Error procesando producto ${productoData.codigo} (índice ${i}): ${errorMensaje}`);
        }
      }

      await transaction.commit();

      // Actualizar estadísticas de la sesión
      await session.actualizarEstadisticas(stats);

      const tiempoProcesamiento = Date.now() - startTime;

      // Logging detallado
      const metricsWithTime = { ...stats, tiempo_procesamiento_ms: tiempoProcesamiento };
      SyncLogger.logBatchMetrics(session.id, lote_numero || session.lotes_procesados, metricsWithTime);
      
      if (stats.errores > 0) {
        SyncLogger.logProductErrors(session.id, lote_numero || session.lotes_procesados, stats.errores_detalle);
      }

      // Advertencias de performance
      if (tiempoProcesamiento > 10000) { // Más de 10 segundos
        SyncLogger.logPerformanceWarning(session.id, {
          type: 'slow_batch',
          message: `Lote procesado lentamente: ${tiempoProcesamiento}ms`,
          metrics: metricsWithTime
        });
      }

      res.json({
        success: true,
        session_id: session.id,
        lote_numero: lote_numero || session.lotes_procesados,
        total_lotes: session.total_lotes_esperados,
        estadisticas: stats,
        tiempo_procesamiento_ms: tiempoProcesamiento,
        progreso: session.getProgreso(),
        categorias_info: {
          categorias_existentes_inicialmente: categoriasExistentesIds.length - categoriasCreadas.length,
          categorias_creadas_automaticamente: categoriasCreadas.length,
          categorias_creadas_lista: categoriasCreadas,
          total_categorias_procesadas: categoriasExistentesIds.length
        }
      });

    } catch (error) {
      if (transaction) {
        await transaction.rollback();
      }
      
      console.error('Error en procesamiento bulk:', error);
      
      res.status(500).json({
        success: false,
        message: 'Error interno en procesamiento de lote',
        error: process.env.NODE_ENV === 'development' ? error.message : undefined,
        tiempo_procesamiento_ms: Date.now() - startTime
      });
    }
  }

  /**
   * POST /api/admin/sync/session/:session_id/finish
   * Finaliza una sesión de sincronización
   */
  static async finishSession(req, res) {
    try {
      const { session_id } = req.params;
      const { estado } = req.body;
      const empresaPrincipal = req.empresa;

      const session = await SyncSession.findByPk(session_id);
      if (!session) {
        return res.status(404).json({
          success: false,
          message: 'Sesión de sincronización no encontrada'
        });
      }

      // Verificar que la sesión pertenece a la empresa
      if (session.empresa_principal_id !== empresaPrincipal.id) {
        return res.status(403).json({
          success: false,
          message: 'Sesión no pertenece a la empresa actual'
        });
      }

      // Finalizar la sesión
      await session.finalizar(estado);

      // Crear log detallado en sync_logs para auditoría
      await SyncLogger.logSyncSession(session.id);

      // Log de finalización
      SyncLogger.logSessionEnd(session.id, {
        estado: session.estado,
        lotes_procesados: session.lotes_procesados,
        productos_totales: session.productos_totales,
        productos_actualizados: session.productos_actualizados,
        productos_nuevos: session.productos_nuevos,
        productos_errores: session.productos_errores,
        tiempo_total_ms: session.tiempo_total_ms
      });

      res.json({
        success: true,
        message: 'Sesión de sincronización finalizada',
        session_id: session.id,
        estado_final: session.estado,
        resumen: {
          productos_totales: session.productos_totales,
          productos_actualizados: session.productos_actualizados,
          productos_nuevos: session.productos_nuevos,
          productos_errores: session.productos_errores,
          lotes_procesados: session.lotes_procesados,
          tiempo_total_ms: session.tiempo_total_ms
        }
      });

    } catch (error) {
      console.error('Error al finalizar sesión de sync:', error);
      
      res.status(500).json({
        success: false,
        message: 'Error interno al finalizar sesión',
        error: process.env.NODE_ENV === 'development' ? error.message : undefined
      });
    }
  }

  /**
   * GET /api/admin/sync/session/:session_id/status
   * Obtiene el estado actual de una sesión de sincronización
   */
  static async getSessionStatus(req, res) {
    try {
      const { session_id } = req.params;
      const empresaPrincipal = req.empresa;

      const session = await SyncSession.findByPk(session_id, {
        include: [{
          model: Empresa,
          as: 'empresa',
          attributes: ['id', 'nombre', 'codigo']
        }]
      });

      if (!session) {
        return res.status(404).json({
          success: false,
          message: 'Sesión de sincronización no encontrada'
        });
      }

      // Verificar que la sesión pertenece a la empresa
      if (session.empresa_principal_id !== empresaPrincipal.id) {
        return res.status(403).json({
          success: false,
          message: 'Sesión no pertenece a la empresa actual'
        });
      }

      const progreso = session.getProgreso();

      res.json({
        success: true,
        session: {
          id: session.id,
          estado: session.estado,
          empresa: session.empresa.nombre,
          fecha_inicio: session.fecha_inicio,
          fecha_fin: session.fecha_fin,
          usuario_proceso: session.usuario_proceso,
          progreso,
          metricas: session.metricas,
          errores_detalle: session.errores_detalle
        }
      });

    } catch (error) {
      console.error('Error al obtener estado de sesión:', error);
      
      res.status(500).json({
        success: false,
        message: 'Error interno al obtener estado de sesión',
        error: process.env.NODE_ENV === 'development' ? error.message : undefined
      });
    }
  }

  /**
   * GET /api/admin/sync/sessions
   * Lista las sesiones de sincronización recientes
   */
  static async listSessions(req, res) {
    try {
      const { page = 1, limit = 20, estado } = req.query;
      const empresaPrincipal = req.empresa;
      const offset = (page - 1) * limit;

      const whereClause = {
        empresa_principal_id: empresaPrincipal.id
      };

      if (estado) {
        whereClause.estado = estado;
      }

      const { count, rows: sessions } = await SyncSession.findAndCountAll({
        where: whereClause,
        include: [{
          model: Empresa,
          as: 'empresa',
          attributes: ['id', 'nombre', 'codigo']
        }],
        order: [['created_at', 'DESC']],
        limit: parseInt(limit),
        offset: parseInt(offset)
      });

      const sessionsWithProgress = sessions.map(session => ({
        id: session.id,
        estado: session.estado,
        fecha_inicio: session.fecha_inicio,
        fecha_fin: session.fecha_fin,
        usuario_proceso: session.usuario_proceso,
        progreso: session.getProgreso(),
        empresa: session.empresa.nombre
      }));

      res.json({
        success: true,
        sessions: sessionsWithProgress,
        pagination: {
          total: count,
          page: parseInt(page),
          limit: parseInt(limit),
          totalPages: Math.ceil(count / limit)
        }
      });

    } catch (error) {
      console.error('Error al listar sesiones:', error);
      
      res.status(500).json({
        success: false,
        message: 'Error interno al listar sesiones',
        error: process.env.NODE_ENV === 'development' ? error.message : undefined
      });
    }
  }

  /**
   * DELETE /api/admin/sync/sessions/cleanup
   * Limpia sesiones antiguas completadas
   */
  static async cleanupOldSessions(req, res) {
    try {
      const { dias = 7 } = req.query;
      
      const deletedCount = await SyncSession.limpiarSesionesAntiguas(parseInt(dias));
      
      res.json({
        success: true,
        message: `Limpieza de sesiones completada`,
        sesiones_eliminadas: deletedCount,
        dias_antiguedad: parseInt(dias)
      });

    } catch (error) {
      console.error('Error en limpieza de sesiones:', error);
      
      res.status(500).json({
        success: false,
        message: 'Error interno en limpieza de sesiones',
        error: process.env.NODE_ENV === 'development' ? error.message : undefined
      });
    }
  }

  /**
   * GET /api/admin/sync/stats
   * Obtiene estadísticas de sincronización de la empresa
   */
  static async getSyncStats(req, res) {
    try {
      const { days = 30 } = req.query;
      const empresaPrincipal = req.empresa;

      const stats = await SyncLogger.getSyncStats(empresaPrincipal.id, parseInt(days));
      
      if (!stats) {
        return res.status(500).json({
          success: false,
          message: 'Error obteniendo estadísticas de sincronización'
        });
      }

      res.json({
        success: true,
        empresa: empresaPrincipal.nombre,
        periodo_dias: parseInt(days),
        estadisticas: stats,
        timestamp: new Date().toISOString()
      });

    } catch (error) {
      console.error('Error obteniendo estadísticas de sync:', error);
      
      res.status(500).json({
        success: false,
        message: 'Error interno obteniendo estadísticas',
        error: process.env.NODE_ENV === 'development' ? error.message : undefined
      });
    }
  }
}

module.exports = SyncController;