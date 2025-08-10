# Task: Agregar ordenamiento a ProductosBase API

## Objetivo
Agregar funcionalidad de ordenamiento al endpoint GET de ProductosBase para permitir ordenar por diferentes columnas de manera ascendente o descendente.

## Todo List

- [ ] Agregar parámetros de ordenamiento al controller (sortBy, sortOrder)
- [ ] Actualizar GetAllProductosBaseQuery para incluir los parámetros de ordenamiento
- [ ] Modificar GetAllProductosBaseQueryHandler para pasar los parámetros al repositorio
- [ ] Actualizar IProductBaseRepository para incluir parámetros de ordenamiento en GetPagedAsync
- [ ] Implementar la lógica de ordenamiento en el repositorio
- [ ] Probar el ordenamiento con diferentes columnas

## Columnas soportadas para ordenamiento
- descripcion (alfabético)
- codigo (numérico)
- codigoRubro (numérico)
- existencia/stock (numérico)
- precio (numérico)
- visible (booleano)
- destacado (booleano)

## Parámetros de la API
- `sortBy`: string - nombre de la columna para ordenar
- `sortOrder`: string - "asc" o "desc" (por defecto "asc")

## Ejemplo de uso
```
GET /api/productosbase?sortBy=descripcion&sortOrder=asc
GET /api/productosbase?sortBy=precio&sortOrder=desc
```

## Review

### Cambios implementados:

1. **Controller (ProductosBaseController.cs)**:
   - Agregados parámetros `sortBy` y `sortOrder` al método GetAll
   - Los parámetros son opcionales con valores null por defecto

2. **Query (GetAllProductosBaseQuery.cs)**:
   - Agregadas propiedades SortBy y SortOrder para transportar los parámetros

3. **Handler (GetAllProductosBaseQueryHandler.cs)**:
   - Actualizada la llamada al repositorio para incluir los nuevos parámetros

4. **Interface (IProductBaseRepository.cs)**:
   - Actualizada la firma del método GetPagedAsync con parámetros opcionales

5. **Repository (ProductBaseRepository.cs)**:
   - Implementada lógica de ordenamiento usando switch expression
   - Soporta ordenamiento ascendente (por defecto) y descendente
   - Maneja las columnas: descripcion, codigo, codigorubro, precio, existencia/stock, visible, destacado
   - Ordenamiento por defecto por Id si no se especifica

### Uso de la API:
```
GET /api/productosbase?sortBy=descripcion&sortOrder=asc
GET /api/productosbase?sortBy=precio&sortOrder=desc
GET /api/productosbase?sortBy=stock&sortOrder=desc
```

### Notas:
- El ordenamiento es case-insensitive para sortBy y sortOrder
- Si se proporciona un sortBy inválido, se ordena por Id
- "existencia" y "stock" son sinónimos para la misma columna