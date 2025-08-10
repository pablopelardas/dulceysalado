# Sistema de Feature Flags por Empresa - Spec Lite

## Resumen
Sistema que permite configurar funcionalidades específicas por empresa mediante feature flags, facilitando la personalización del comportamiento según cada cliente.

## Motivación
- Diferentes empresas requieren funcionalidades personalizadas
- Necesidad de activar/desactivar features sin cambiar código
- Centralizar configuración de comportamientos por cliente

## Alcance
- ✅ Modelo de dominio y base de datos
- ✅ CRUD con patrón CQRS
- ✅ API de administración y consulta pública
- ✅ Sistema de caché para optimización
- ❌ UI de administración (futuro)
- ❌ Feature flags por usuario

## Diseño Principal

### Tablas
- `feature_definitions`: Catálogo de features disponibles
- `empresa_features`: Configuración por empresa

### Features Ejemplo
- `pedido_whatsapp`: Enviar pedidos por WhatsApp
- `pedido_campos_requeridos`: Campos adicionales en pedidos
- `cliente_autenticacion`: Login obligatorio en frontend

### API
```
# Admin
GET/POST /api/features/empresa/{empresaId}
PUT/DELETE /api/features/empresa/{empresaId}/{featureCode}

# Público
GET /api/public/features
GET /api/public/features/{featureCode}
```

## Estado
- 📝 Especificación creada
- ⏳ Pendiente de implementación