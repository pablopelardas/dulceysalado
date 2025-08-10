# Módulo Empresas - Plan de Implementación

## Análisis de Requerimientos

### Tipos de Empresa
- **Principal**: Gestiona catálogo base y empresas cliente
- **Cliente**: Consume catálogo base, gestiona su propio catálogo

### Funcionalidades del API

#### Endpoints principales
1. **GET /api/companies** - Listar empresas con paginación
   - Empresa Principal: Ve todas las empresas cliente
   - Empresa Cliente: Solo ve su propia empresa
   
2. **GET /api/companies/{id}** - Ver empresa específica
   - Empresa Principal: Puede ver cualquier empresa cliente
   - Empresa Cliente: Solo puede ver su propia empresa

3. **POST /api/companies** - Crear empresa cliente
   - Solo empresa principal puede crear empresas cliente
   - Se asigna automáticamente `empresa_principal_id`

4. **PUT /api/companies/{id}** - Actualizar empresa
   - Empresa Principal: Puede actualizar cualquier empresa cliente
   - Empresa Cliente: Solo puede actualizar su propia empresa (campos limitados)

5. **DELETE /api/companies/{id}** - Desactivar empresa cliente (soft delete)
   - Solo empresa principal puede desactivar empresas cliente

### Modelo de Datos

#### Campos principales
```json
{
  "id": 1,
  "codigo": "DIST-001",
  "nombre": "Distribuidora del Norte",
  "razon_social": "Distribuidora del Norte S.A.",
  "cuit": "20-12345678-9",
  "telefono": "+54 11 1234-5678",
  "email": "contacto@distribuidora.com",
  "direccion": "Av. Principal 123, Ciudad",
  "tipo_empresa": "cliente",
  "empresa_principal_id": 1,
  "logo_url": null,
  "colores_tema": {
    "acento": "#8BC34A",
    "primario": "#4A90E2",
    "secundario": "#FF6B35"
  },
  "favicon_url": null,
  "dominio_personalizado": "distribuidora-norte",
  "url_whatsapp": "https://wa.me/5491123456789",
  "url_facebook": "https://facebook.com/distribuidora",
  "url_instagram": "https://instagram.com/distribuidora",
  "mostrar_precios": true,
  "mostrar_stock": true,
  "permitir_pedidos": false,
  "productos_por_pagina": 20,
  "puede_agregar_productos": true,
  "puede_agregar_categorias": false,
  "activa": true,
  "fecha_vencimiento": "2027-06-23T00:00:00",
  "plan": "basico",
  "created_at": "2025-06-23T23:11:25Z",
  "updated_at": "2025-06-24T01:46:28Z"
}
```

### Reglas de Negocio

#### Autorización
- **Empresa Principal**:
  - CRUD completo sobre empresas cliente
  - No puede modificar su propia empresa (excepto configuraciones específicas)
  - Puede ver estadísticas de todas las empresas

- **Empresa Cliente**:
  - Solo READ de su propia empresa
  - UPDATE limitado: configuraciones de catálogo, datos de contacto, URLs redes sociales
  - No puede modificar: plan, fecha_vencimiento, permisos de productos/categorías

#### Validaciones
- Código único por empresa principal
- Dominio personalizado único global
- CUIT debe ser válido (formato argentino)
- Email debe ser válido
- Solo una empresa principal por sistema
- Empresas cliente deben tener empresa_principal_id

#### Soft Delete
- Las empresas se desactivan, no se eliminan
- Al desactivar empresa cliente, se desactivan todos sus usuarios
- Empresa principal nunca se puede desactivar

## Plan de Implementación

### Fase 1: Domain Layer
- [x] Entidad Company ya existe
- [ ] Crear value objects (si necesarios): CompanyCode, Domain
- [ ] Definir ICompanyRepository interface
- [ ] Agregar métodos de negocio a Company entity

### Fase 2: Infrastructure Layer  
- [ ] Implementar CompanyRepository
- [ ] Mapping entre EF Model (Empresa) y Domain Entity (Company)
- [ ] Métodos para búsqueda, paginación, filtros

### Fase 3: Application Layer
- [ ] Crear DTOs: CompanyDto, CreateCompanyDto, UpdateCompanyDto
- [ ] Crear Commands: CreateCompanyCommand, UpdateCompanyCommand, DeleteCompanyCommand  
- [ ] Crear Queries: GetCompaniesListQuery, GetCompanyByIdQuery
- [ ] Implementar Handlers para cada Command/Query
- [ ] Crear validadores con FluentValidation
- [ ] Configurar AutoMapper profiles

### Fase 4: API Layer
- [ ] Implementar CompaniesController
- [ ] Configurar rutas y autorización
- [ ] Documentación con Swagger
- [ ] Pruebas de endpoints

### Consideraciones Especiales

#### Nomenclatura
- Usar nombres en español consistentes con módulo usuarios
- Snake_case en responses JSON
- Endpoints en inglés: `/api/companies`

#### Performance
- Paginación en listados
- Eager loading de relaciones necesarias
- Caché para empresas principales (pocas y raramente cambian)

#### Seguridad
- Validar permisos en cada endpoint
- Rate limiting para creación de empresas
- Logs de auditoría para cambios importantes