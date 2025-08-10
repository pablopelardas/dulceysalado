# Spec Summary (Lite)

Actualizar el sistema de sincronización para procesar el campo `stocksPorEmpresa` enviado por el procesador, permitiendo stock diferenciado por empresa en lugar del stock global actual. El sistema actualizará registros específicos en `productos_base_stock` para cada empresaId especificada, manteniendo stock existente para empresas no incluidas.