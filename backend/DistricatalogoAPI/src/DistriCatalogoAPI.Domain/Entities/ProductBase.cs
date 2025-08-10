using System;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class ProductBase : BaseEntity
    {
        public int Id { get; private set; }
        public string Codigo { get; private set; }
        public string Descripcion { get; private set; }
        public int? CodigoRubro { get; private set; }
        public decimal Precio { get; private set; }
        public decimal Existencia { get; private set; }
        public int? Grupo1 { get; private set; }
        public int? Grupo2 { get; private set; }
        public int? Grupo3 { get; private set; }
        public DateTime? FechaAlta { get; private set; }
        public DateTime? FechaModi { get; private set; }
        public string Imputable { get; private set; }
        public string Disponible { get; private set; }
        public string? CodigoUbicacion { get; private set; }
        
        // Campos configurables desde web (NO se actualizan en sync)
        public bool Visible { get; private set; }
        public bool Destacado { get; private set; }
        public int OrdenCategoria { get; private set; }
        public string? ImagenUrl { get; private set; }
        public string? ImagenAlt { get; private set; }
        public string? DescripcionCorta { get; private set; }
        public string? DescripcionLarga { get; private set; }
        public string? Tags { get; private set; }
        public string? CodigoBarras { get; private set; }
        public string? Marca { get; private set; }
        public string UnidadMedida { get; private set; } = "UN";
        
        // Control
        public int AdministradoPorEmpresaId { get; private set; }
        public DateTime? ActualizadoGecom { get; private set; }

        // Navigation
        public virtual Company AdministradoPorEmpresa { get; private set; }
        public virtual CategoryBase CodigoRubroNavigation { get; private set; }

        protected ProductBase() { }

        // Constructor para crear nuevo producto desde sincronización
        public static ProductBase CreateFromSync(
            string codigo,
            string descripcion,
            int? codigoRubro,
            decimal precio,
            decimal existencia,
            int? grupo1,
            int? grupo2,
            int? grupo3,
            DateTime? fechaAlta,
            DateTime? fechaModi,
            string? imputable,
            string? disponible,
            string? codigoUbicacion,
            int empresaId)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código del producto es requerido");

            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción del producto es requerida");

            if (precio < 0)
                throw new ArgumentException("El precio no puede ser negativo");

            if (existencia < 0)
                throw new ArgumentException("La existencia no puede ser negativa");

            return new ProductBase
            {
                Codigo = codigo,
                Descripcion = descripcion.Trim(),
                CodigoRubro = codigoRubro,
                Precio = precio,
                Existencia = existencia,
                Grupo1 = grupo1,
                Grupo2 = grupo2,
                Grupo3 = grupo3,
                FechaAlta = fechaAlta ?? DateTime.UtcNow,
                FechaModi = fechaModi ?? DateTime.UtcNow,
                Imputable = imputable ?? "S",
                Disponible = disponible ?? "S",
                CodigoUbicacion = codigoUbicacion,
                AdministradoPorEmpresaId = empresaId,
                ActualizadoGecom = DateTime.UtcNow,
                
                // Valores por defecto para campos web
                Visible = true,
                Destacado = false,
                OrdenCategoria = 0,
                UnidadMedida = "UN",
                
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Método para actualizar SOLO campos que vienen de Gecom
        public void UpdateFromSync(
            string descripcion,
            int? codigoRubro,
            decimal precio,
            decimal existencia,
            int? grupo1,
            int? grupo2,
            int? grupo3,
            DateTime? fechaAlta,
            DateTime? fechaModi,
            string? imputable,
            string? disponible,
            string? codigoUbicacion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción del producto es requerida");

            if (precio < 0)
                throw new ArgumentException("El precio no puede ser negativo");

            if (existencia < 0)
                throw new ArgumentException("La existencia no puede ser negativa");

            // SOLO actualizar campos de Gecom
            Descripcion = descripcion.Trim();
            CodigoRubro = codigoRubro;
            Precio = precio;
            Existencia = existencia;
            Grupo1 = grupo1;
            Grupo2 = grupo2;
            Grupo3 = grupo3;
            FechaAlta = fechaAlta ?? FechaAlta;
            FechaModi = fechaModi ?? DateTime.UtcNow;
            Imputable = imputable ?? "S";
            Disponible = disponible ?? "S";
            CodigoUbicacion = codigoUbicacion;
            ActualizadoGecom = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            // NO modificar: Visible, Destacado, OrdenCategoria, ImagenUrl, etc.
        }

        // Métodos para actualizar campos web (usados desde panel admin)
        public void UpdateWebFields(
            bool? visible = null,
            bool? destacado = null,
            int? ordenCategoria = null,
            string imagenUrl = null,
            string imagenAlt = null,
            string descripcionCorta = null,
            string descripcionLarga = null,
            string tags = null,
            string codigoBarras = null,
            string marca = null,
            string unidadMedida = null)
        {
            if (visible.HasValue) Visible = visible.Value;
            if (destacado.HasValue) Destacado = destacado.Value;
            if (ordenCategoria.HasValue) OrdenCategoria = ordenCategoria.Value;
            if (imagenUrl != null) ImagenUrl = imagenUrl;
            if (imagenAlt != null) ImagenAlt = imagenAlt;
            if (descripcionCorta != null) DescripcionCorta = descripcionCorta;
            if (descripcionLarga != null) DescripcionLarga = descripcionLarga;
            if (tags != null) Tags = tags;
            if (codigoBarras != null) CodigoBarras = codigoBarras;
            if (marca != null) Marca = marca;
            if (unidadMedida != null) UnidadMedida = unidadMedida;
            
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsActive()
        {
            return Disponible == "S";
        }

        public bool IsVisibleInCatalog()
        {
            return Visible && Imputable == "S";
        }

        public bool HasStock()
        {
            return Existencia > 0;
        }

        public bool NeedsGecomUpdate(DateTime umbralHoras = default)
        {
            if (umbralHoras == default)
                umbralHoras = DateTime.UtcNow.AddHours(-24);

            return !ActualizadoGecom.HasValue || ActualizadoGecom.Value < umbralHoras;
        }
    }
}