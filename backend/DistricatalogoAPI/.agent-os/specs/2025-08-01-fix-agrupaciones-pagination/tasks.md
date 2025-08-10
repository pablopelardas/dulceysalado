# Spec Tasks

## Tasks

- [x] 1. Localizar y modificar el handler de consulta de agrupaciones
  - [x] 1.1 Buscar el handler GetAgrupacionesQuery en la capa Application
  - [x] 1.2 Identificar la línea donde se define el PageSize predeterminado
  - [x] 1.3 Cambiar el valor predeterminado de 20 a 100
  - [x] 1.4 Verificar que no haya otros límites hardcoded

- [x] 2. Validar la corrección del bug
  - [x] 2.1 Compilar el proyecto para verificar que no hay errores
  - [x] 2.2 Ejecutar el endpoint GET /api/agrupaciones sin parámetros
  - [x] 2.3 Verificar que la respuesta incluya las 28 agrupaciones
  - [x] 2.4 Confirmar que pagination muestre: page_size: 100, total_pages: 1