# Análisis del Modelo Usuario

## Campos Identificados

### Datos Básicos
- `Id` (int): Clave primaria
- `EmpresaId` (int): FK a tabla Empresa (relación obligatoria)
- `Email` (string): Único entre usuarios activos, no nulo
- `PasswordHash` (string): Hash de contraseña, no nulo
- `Nombre` (string): No nulo
- `Apellido` (string): No nulo
- `Rol` (string?): Nullable, posibles valores a definir

### Permisos Granulares
- `PuedeGestionarProductosBase` (bool?): Solo empresa principal
- `PuedeGestionarProductosEmpresa` (bool?): Productos propios
- `PuedeGestionarCategoriasBase` (bool?): Solo empresa principal
- `PuedeGestionarCategoriasEmpresa` (bool?): Categorías propias
- `PuedeGestionarUsuarios` (bool?): Usuarios de su empresa
- `PuedeVerEstadisticas` (bool?): Acceso a estadísticas

### Estado y Auditoría
- `Activo` (bool?): Estado del usuario
- `UltimoLogin` (DateTime?): Último acceso
- `CreatedAt` (DateTime?): Fecha de creación
- `UpdatedAt` (DateTime?): Fecha de actualización

### Relaciones
- `Empresa`: Relación N:1 con tabla Empresa (navegación)

## Reglas de Negocio Identificadas

1. **Email único entre usuarios activos**: 
   - El email debe ser único solo entre usuarios con Activo = true
   - Un usuario desactivado libera el email para reutilización
2. **Soft delete irreversible**: 
   - Cuando se desactiva un usuario (Activo = false) es permanente
   - No se puede reactivar un usuario desactivado
   - Se puede crear un nuevo usuario con el mismo email en cualquier empresa
3. **Usuario siempre pertenece a una empresa**: 
   - EmpresaId es obligatorio
   - Los usuarios NO se pueden transferir entre empresas
4. **Permisos base solo para empresa principal**: 
   - PuedeGestionarProductosBase
   - PuedeGestionarCategoriasBase
   - Solo tienen sentido si TipoEmpresa = 'principal'
5. **Estado activo por defecto**: Los usuarios nuevos deberían estar activos
6. **Auditoría automática**: CreatedAt y UpdatedAt deberían actualizarse automáticamente

## Roles Sugeridos

Basándome en el proyecto.md y los permisos:

### Empresa Principal
- `admin_principal`: Todos los permisos
- `editor_principal`: Gestión de catálogo base
- `viewer_principal`: Solo lectura

### Empresa Cliente
- `admin_cliente`: Gestión completa de su empresa
- `editor_cliente`: Gestión de productos/categorías propias
- `viewer_cliente`: Solo visualización

## Consideraciones para Domain

1. **Value Objects**:
   - Email: Validación de formato
   - Password: Nunca exponer hash, solo verificación

2. **Validaciones**:
   - Email formato válido
   - Email único entre usuarios activos
   - Nombre y Apellido no vacíos
   - Password con requisitos mínimos (al crear/cambiar)

3. **Comportamientos de la entidad**:
   - VerifyPassword(string password)
   - ChangePassword(string newPassword)
   - UpdateLastLogin()
   - Deactivate() - Solo desactivar, sin método Activate()

4. **Invariantes**:
   - Un usuario siempre debe tener una empresa
   - Los permisos de gestión base solo aplican a usuarios de empresa principal
   - Un usuario desactivado no puede ser reactivado
   - El email es único solo entre usuarios activos