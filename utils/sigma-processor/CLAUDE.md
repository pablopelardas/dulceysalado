# Procesador CSV Gecom - Sincronización con DistriCatalogo

## Contexto del Proyecto

Necesito desarrollar un **programa C# (.NET Core)** que procese archivos CSV exportados desde el sistema **Gecom** y los sincronice con la base de datos **DistriCatalogo** que ya está implementada.

### Propósito
- **Automatizar** la sincronización de productos desde Gecom
- **Procesar** archivos CSV con datos de productos
- **Actualizar** la tabla `productos_base` en MySQL
- **Registrar logs** de sincronización para auditoría
- **Mover archivos** procesados para evitar reprocesamiento

---

## 📋 Funcionalidades Requeridas

### 1. **Procesamiento de CSV**
- **Detectar automáticamente** archivos .csv en la carpeta del ejecutable
- **Limpiar formato** del CSV (eliminar comillas, corregir encoding UTF-8)
- **Corregir headers** con caracteres especiales (Ã³ → ó, etc.)
- **Convertir formato numérico** (comas decimales → puntos)
- **Validar estructura** del archivo antes de procesar

### 2. **Sincronización con Base de Datos**
- **Conectar a MySQL** usando el esquema DistriCatalogo
- **Verificar existencia** de productos por código
- **Actualizar productos existentes** con nuevos datos
- **Insertar productos nuevos** en `productos_base`
- **Mantener integridad** referencial con categorías

### 3. **Logging y Auditoría**
- **Registrar en `sync_logs`** cada procesamiento
- **Contar productos** actualizados vs nuevos
- **Registrar errores** y tiempo de procesamiento
- **Mostrar progreso** en consola con estadísticas

### 4. **Gestión de Archivos**
- **Mover archivos procesados** a carpeta `procesados/`
- **Agregar timestamp** al nombre del archivo
- **Evitar reprocesamiento** de archivos ya procesados

---

## 🗃️ Esquema de Base de Datos (Tablas Relevantes)

### Tabla `productos_base`
```sql
CREATE TABLE productos_base (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo INT UNIQUE NOT NULL,
    descripcion VARCHAR(500) NOT NULL,
    codigo_rubro INT,
    precio DECIMAL(10,3) DEFAULT 0.000,
    existencia DECIMAL(8,2) DEFAULT 0.00,
    grupo1 INT DEFAULT NULL,
    grupo2 INT DEFAULT NULL,
    grupo3 INT DEFAULT NULL,
    fecha_alta DATE,
    fecha_modi DATE,
    imputable CHAR(1) DEFAULT 'S',
    disponible CHAR(1) DEFAULT 'S',
    codigo_ubicacion VARCHAR(50) DEFAULT NULL,
    
    -- Campos web adicionales
    visible BOOLEAN DEFAULT TRUE,
    destacado BOOLEAN DEFAULT FALSE,
    administrado_por_empresa_id INT NOT NULL,
    actualizado_gecom TIMESTAMP NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

### Tabla `sync_logs`
```sql
CREATE TABLE sync_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_principal_id INT NOT NULL,
    archivo_nombre VARCHAR(255) NOT NULL,
    fecha_procesamiento TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    productos_actualizados INT DEFAULT 0,
    productos_nuevos INT DEFAULT 0,
    errores INT DEFAULT 0,
    tiempo_procesamiento_ms INT DEFAULT NULL,
    estado ENUM('exitoso', 'con_errores', 'fallido') DEFAULT 'exitoso',
    detalles_errores TEXT DEFAULT NULL,
    usuario_proceso VARCHAR(100) DEFAULT NULL
);
```

---

## 📄 Formato CSV de Gecom

### Headers Esperados (con problemas de encoding)
```
CÃ³digo	DescripciÃ³n	CodigoRubro	caPrecio	Existencia	Grupo1	Grupo2	Grupo3	FechaAlta	FechaModi	Imputable	Disponible	CodigoUbicacion
```

### Headers Corregidos
```
Codigo	Descripcion	CodigoRubro	Precio	Existencia	Grupo1	Grupo2	Grupo3	FechaAlta	FechaModi	Imputable	Disponible	CodigoUbicacion
```

### Datos de Ejemplo
```
123	"Queso Cremoso"	1	125,50	10,00	NULL	NULL	NULL	2024-01-15	2024-06-20	S	S	A1-B2
456	"Leche Entera 1L"	7	85,75	25,50	NULL	NULL	NULL	2024-02-10	2024-06-20	S	S	C3-D4
```

### Problemas a Resolver
1. **Encoding UTF-8** con caracteres mal codificados
2. **Comillas dobles** alrededor de valores
3. **Comas decimales** en lugar de puntos
4. **Delimitador TAB** en lugar de coma
5. **Valores NULL** como texto "NULL"

---

## 🛠️ Tecnologías y Librerías

### Tecnologías Base
- **.NET 8.0** (Console Application)
- **C# 12** con nullable reference types
- **MySQL** como base de datos

### Librerías Sugeridas
- **MySql.Data** para conexión MySQL
- **CsvHelper** para procesamiento CSV robusto
- **System.Text.Json** para configuración (opcional)
- **Serilog** para logging avanzado (opcional)

### Configuración
- **appsettings.json** para string de conexión
- **Variables de entorno** para configuración sensible
- **Archivo .env** para desarrollo local

---

## 📁 Estructura del Proyecto

```
GecomProcessor/
├── GecomProcessor.csproj
├── Program.cs                 # Punto de entrada principal
├── Models/
│   ├── GecomRecord.cs        # Modelo del registro CSV
│   ├── ProductoBase.cs       # Modelo de producto
│   └── SyncLog.cs           # Modelo de log de sync
├── Services/
│   ├── CsvProcessor.cs       # Procesamiento de CSV
│   ├── DatabaseService.cs    # Operaciones de BD
│   └── FileManager.cs        # Gestión de archivos
├── Utils/
│   ├── CsvCleaner.cs        # Limpieza de CSV
│   └── Logger.cs            # Logging personalizado
├── Config/
│   └── AppSettings.cs       # Configuración
├── appsettings.json         # Configuración
└── README.md               # Documentación
```

---

## 🎯 Flujo de Procesamiento

### 1. **Inicialización**
```
1. Cargar configuración (connection string, etc.)
2. Verificar conexión a base de datos
3. Buscar archivos CSV en directorio actual
4. Mostrar archivos encontrados al usuario
```

### 2. **Procesamiento por Archivo**
```
1. Leer archivo CSV original
2. Limpiar formato y encoding
3. Guardar archivo limpio temporal
4. Parsear con CsvHelper
5. Procesar registro por registro
6. Actualizar/insertar en productos_base
7. Contar resultados (nuevos/actualizados/errores)
```

### 3. **Finalización**
```
1. Registrar sync_log en base de datos
2. Mover archivo a carpeta procesados/
3. Limpiar archivos temporales
4. Mostrar resumen en consola
```

---

## 🔧 Configuración Requerida

### appsettings.json
```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=districatalogo;Uid=tu_usuario;Pwd=tu_password;CharSet=utf8mb4;"
  },
  "Processing": {
    "EmpresaPrincipalId": 1,
    "UsuarioProceso": "SISTEMA_GECOM",
    "ProcessedFolder": "procesados",
    "TempFolder": "temp"
  }
}
```

### Variables de Entorno (.env)
```
DB_HOST=localhost
DB_DATABASE=districatalogo
DB_USERNAME=tu_usuario
DB_PASSWORD=tu_password
EMPRESA_PRINCIPAL_ID=1
```

---

## 📊 Validaciones y Reglas de Negocio

### Validaciones de Datos
1. **Código de producto** debe ser entero positivo
2. **Descripción** no puede estar vacía
3. **Precio** debe ser decimal válido >= 0
4. **Existencia** debe ser decimal válido >= 0
5. **CodigoRubro** debe existir en `categorias_base` (opcional)

### Reglas de Sincronización
1. **Si producto existe:** actualizar todos los campos
2. **Si producto es nuevo:** insertar con `visible = TRUE`
3. **Siempre actualizar:** `actualizado_gecom = NOW()`
4. **Solo empresa principal** puede sincronizar productos base
5. **Registrar log** independientemente del resultado

### Manejo de Errores
1. **Error de fila:** continuar procesamiento, contar error
2. **Error de conexión:** abortar procesamiento
3. **Error de archivo:** mover a carpeta de errores
4. **Timeout:** configurar timeout de conexión apropiado

---

## 🚀 Features Adicionales Sugeridas

### Modo Interactivo
- **Confirmar procesamiento** antes de ejecutar
- **Mostrar preview** de cambios antes de aplicar
- **Seleccionar archivos** específicos para procesar

### Configuración Avanzada
- **Mapeo de columnas** configurable
- **Filtros de productos** por código o categoría
- **Validaciones personalizadas** por reglas de negocio

### Reporting
- **Reporte detallado** de cambios por producto
- **Export de errores** a archivo separado
- **Estadísticas históricas** de sincronizaciones

---

## 📝 Request Específico

**Por favor, desarrolla un programa C# completo que:**

1. **Analice el esquema SQL** y confirme entendimiento de las tablas
2. **Implemente el procesamiento CSV** robusto con las limpiezas necesarias
3. **Gestione la conexión MySQL** con el esquema DistriCatalogo
4. **Procese los datos** según las reglas de negocio especificadas
5. **Registre logs detallados** en la tabla sync_logs
6. **Maneje archivos** de forma organizada (procesados/errores)

**Ejecuta el desarrollo paso a paso:**
- Configura el proyecto inicial con las dependencias
- Implementa el procesamiento de CSV primero
- Agrega la conexión y operaciones de base de datos
- Integra el sistema de logging y gestión de archivos
- Prueba con archivos CSV de ejemplo
- Consulta antes de avanzar a cada fase mayor

**Aspectos críticos:**
- **Robustez** en el procesamiento de CSV con formato inconsistente
- **Transacciones** de base de datos para integridad
- **Logging detallado** para debugging y auditoría
- **Manejo de errores** sin interrumpir el procesamiento completo
- **Performance** para archivos CSV grandes (miles de productos)

¿Puedes comenzar creando el proyecto base y la estructura de clases?

El esquema se encuentra en esquema.sql, ademas las configs deberian ir en el appsettings, no en un .env, estamos en una solucion de c#. Por ultimo m egustaria que el filepath de los archivos a procesar sea configurable, no que lo tome del actual. Esto deberia ir en un archivo de configuracion con el mismo ejecutable que se pueda cambiar