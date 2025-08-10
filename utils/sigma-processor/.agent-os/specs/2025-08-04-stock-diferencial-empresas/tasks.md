# Spec Tasks

## Tasks

- [x] 1. Crear modelos para stock diferencial
  - [x] 1.1 Crear StockRecord.cs para mapear CSV de stocks
  - [x] 1.2 Crear EmpresaStock.cs para el DTO de la API
  - [x] 1.3 Extender ProductWithPrices a ProductWithPricesAndStock.cs
  - [x] 1.4 Agregar constante de mapeo empresa-columna hardcodeado
  - [x] 1.5 Compilar y verificar que no hay errores

- [ ] 2. Extender procesamiento de archivos CSV
  - [ ] 2.1 Crear StockProcessor.cs reutilizando lógica de CsvCleaner
  - [ ] 2.2 Modificar DualFileProcessor a TripleFileProcessor
  - [ ] 2.3 Agregar procesamiento de stocks.csv con limpieza de decimales
  - [ ] 2.4 Implementar lógica de combinación de productos+precios+stocks
  - [ ] 2.5 Compilar y verificar que no hay errores

- [ ] 3. Actualizar configuración y servicios
  - [ ] 3.1 Agregar StocksFileName a appsettings.json
  - [ ] 3.2 Actualizar ProcessingConfig con nueva propiedad
  - [ ] 3.3 Modificar Program.cs para usar TripleFileProcessor
  - [ ] 3.4 Actualizar inyección de dependencias
  - [ ] 3.5 Compilar y verificar que no hay errores

- [ ] 4. Integrar con API y validación final
  - [ ] 4.1 Modificar ApiService para enviar stocksPorEmpresa en el payload
  - [ ] 4.2 Actualizar serialización JSON para incluir nuevo campo
  - [ ] 4.3 Agregar validaciones para stock vacío/nulo → 0.00
  - [ ] 4.4 Probar procesamiento completo con archivos de ejemplo
  - [ ] 4.5 Verificar logs y manejo de errores
  - [ ] 4.6 Compilar y verificar que funciona end-to-end