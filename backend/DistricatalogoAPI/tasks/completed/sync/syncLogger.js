const { SyncLog, SyncSession } = require('../models');

class SyncLogger {
  /**
   * Crea un log detallado de una sesión de sincronización
   */
  static async logSyncSession(sessionId, additionalInfo = {}) {
    try {
      const session = await SyncSession.findByPk(sessionId, {
        include: ['empresa']
      });

      if (!session) {
        console.error(`❌ Sesión ${sessionId} no encontrada para logging`);
        return null;
      }

      const logData = {
        empresa_principal_id: session.empresa_principal_id,
        archivo_nombre: additionalInfo.archivo_nombre || `Sesión Bulk ${session.id}`,
        productos_actualizados: session.productos_actualizados,
        productos_nuevos: session.productos_nuevos,
        errores: session.productos_errores,
        tiempo_procesamiento_ms: session.tiempo_total_ms,
        estado: this.mapSyncSessionEstadoToLogEstado(session.estado, session.productos_errores),
        detalles_errores: session.errores_detalle.length > 0 
          ? JSON.stringify(session.errores_detalle) 
          : null,
        usuario_proceso: session.usuario_proceso || 'Sistema'
      };

      const syncLog = await SyncLog.create(logData);
      
      console.log(`📝 Log de sync creado: ${syncLog.id} para sesión ${sessionId}`);
      return syncLog;
      
    } catch (error) {
      console.error('❌ Error creando log de sync:', error);
      return null;
    }
  }

  /**
   * Mapea el estado de la sesión al estado del log
   */
  static mapSyncSessionEstadoToLogEstado(sessionEstado, errores) {
    switch (sessionEstado) {
      case 'completada':
        return errores > 0 ? 'con_errores' : 'exitoso';
      case 'error':
      case 'cancelada':
        return 'fallido';
      default:
        return errores > 0 ? 'con_errores' : 'exitoso';
    }
  }

  /**
   * Registra métricas de performance por lote
   */
  static logBatchMetrics(sessionId, batchNumber, metrics) {
    const logMessage = {
      timestamp: new Date().toISOString(),
      session_id: sessionId,
      batch_number: batchNumber,
      metrics: {
        productos_procesados: metrics.productos_procesados,
        productos_actualizados: metrics.productos_actualizados,
        productos_nuevos: metrics.productos_nuevos,
        errores: metrics.errores,
        tiempo_procesamiento_ms: metrics.tiempo_procesamiento_ms,
        productos_por_segundo: metrics.productos_procesados / (metrics.tiempo_procesamiento_ms / 1000)
      }
    };

    console.log(`📊 Métricas de lote ${batchNumber}:`, JSON.stringify(logMessage, null, 2));
  }

  /**
   * Registra errores detallados por producto
   */
  static logProductErrors(sessionId, batchNumber, errors) {
    if (errors.length === 0) return;

    const errorLog = {
      timestamp: new Date().toISOString(),
      session_id: sessionId,
      batch_number: batchNumber,
      total_errors: errors.length,
      errors: errors.map(error => ({
        producto_codigo: error.producto_codigo,
        producto_nombre: error.producto_nombre,
        error_message: error.error,
        error_type: this.classifyError(error.error)
      }))
    };

    console.error(`❌ Errores en lote ${batchNumber}:`, JSON.stringify(errorLog, null, 2));
  }

  /**
   * Clasifica el tipo de error para mejores métricas
   */
  static classifyError(errorMessage) {
    const errorMsg = errorMessage.toLowerCase();
    
    if (errorMsg.includes('validation') || errorMsg.includes('invalid')) {
      return 'validation_error';
    } else if (errorMsg.includes('duplicate') || errorMsg.includes('unique')) {
      return 'duplicate_error';
    } else if (errorMsg.includes('foreign') || errorMsg.includes('reference')) {
      return 'reference_error';
    } else if (errorMsg.includes('timeout') || errorMsg.includes('connection')) {
      return 'connection_error';
    } else {
      return 'unknown_error';
    }
  }

  /**
   * Registra el inicio de una sesión de sync
   */
  static logSessionStart(sessionId, empresaId, totalLotes, usuario) {
    const logMessage = {
      timestamp: new Date().toISOString(),
      event: 'session_started',
      session_id: sessionId,
      empresa_id: empresaId,
      total_lotes_esperados: totalLotes,
      usuario: usuario
    };

    console.log(`🚀 Sesión de sync iniciada:`, JSON.stringify(logMessage, null, 2));
  }

  /**
   * Registra la finalización de una sesión de sync
   */
  static logSessionEnd(sessionId, finalStats) {
    const logMessage = {
      timestamp: new Date().toISOString(),
      event: 'session_finished',
      session_id: sessionId,
      final_stats: {
        estado: finalStats.estado,
        lotes_procesados: finalStats.lotes_procesados,
        productos_totales: finalStats.productos_totales,
        productos_actualizados: finalStats.productos_actualizados,
        productos_nuevos: finalStats.productos_nuevos,
        productos_errores: finalStats.productos_errores,
        tiempo_total_ms: finalStats.tiempo_total_ms,
        productos_por_segundo: finalStats.productos_totales / (finalStats.tiempo_total_ms / 1000)
      }
    };

    console.log(`✅ Sesión de sync finalizada:`, JSON.stringify(logMessage, null, 2));
  }

  /**
   * Registra advertencias de performance
   */
  static logPerformanceWarning(sessionId, warning) {
    const logMessage = {
      timestamp: new Date().toISOString(),
      level: 'warning',
      session_id: sessionId,
      warning_type: warning.type,
      warning_message: warning.message,
      metrics: warning.metrics
    };

    console.warn(`⚠️ Advertencia de performance:`, JSON.stringify(logMessage, null, 2));
  }

  /**
   * Obtiene estadísticas de logs de sync para reportes
   */
  static async getSyncStats(empresaId, days = 30) {
    try {
      const fechaInicio = new Date();
      fechaInicio.setDate(fechaInicio.getDate() - days);

      const logs = await SyncLog.findAll({
        where: {
          empresa_principal_id: empresaId,
          fecha_procesamiento: {
            [require('sequelize').Op.gte]: fechaInicio
          }
        },
        order: [['fecha_procesamiento', 'DESC']]
      });

      const stats = {
        total_syncs: logs.length,
        productos_totales: logs.reduce((sum, log) => sum + (log.productos_actualizados + log.productos_nuevos), 0),
        productos_actualizados: logs.reduce((sum, log) => sum + log.productos_actualizados, 0),
        productos_nuevos: logs.reduce((sum, log) => sum + log.productos_nuevos, 0),
        errores_totales: logs.reduce((sum, log) => sum + log.errores, 0),
        tiempo_promedio_ms: logs.length > 0 
          ? logs.reduce((sum, log) => sum + (log.tiempo_procesamiento_ms || 0), 0) / logs.length 
          : 0,
        syncs_exitosos: logs.filter(log => log.estado === 'exitoso').length,
        syncs_con_errores: logs.filter(log => log.estado === 'con_errores').length,
        syncs_fallidos: logs.filter(log => log.estado === 'fallido').length
      };

      return stats;
    } catch (error) {
      console.error('❌ Error obteniendo estadísticas de sync:', error);
      return null;
    }
  }
}

module.exports = SyncLogger;