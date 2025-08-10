using System;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class ProductoEmpresa : BaseEntity
    {
        public int Id { get; private set; }
        public int EmpresaId { get; private set; }
        public string Codigo { get; private set; }
        public string Descripcion { get; private set; }
        public int? CodigoRubro { get; private set; }
        public decimal? Precio { get; private set; }
        public decimal? Existencia { get; private set; }
        
        // Campos configurables desde web
        public bool? Visible { get; private set; }
        public bool? Destacado { get; private set; }
        public int? OrdenCategoria { get; private set; }
        public string? ImagenUrl { get; private set; }
        public string? ImagenAlt { get; private set; }
        public string? DescripcionCorta { get; private set; }
        public string? DescripcionLarga { get; private set; }
        public string? Tags { get; private set; }
        public string? CodigoBarras { get; private set; }
        public string? Marca { get; private set; }
        public string? UnidadMedida { get; private set; } = "UN";

        // Navigation
        public virtual Company Empresa { get; private set; }
        public virtual CategoryBase CodigoRubroNavigation { get; private set; }

        protected ProductoEmpresa() { }

        // Constructor para crear nuevo producto de empresa
        public static ProductoEmpresa Create(
            int empresaId,
            string codigo,
            string descripcion,
            int? codigoRubro = null,
            decimal? precio = null,
            decimal? existencia = null,
            bool? visible = true,
            bool? destacado = false,
            int? ordenCategoria = null,
            string? imagenUrl = null,
            string? imagenAlt = null,
            string? descripcionCorta = null,
            string? descripcionLarga = null,
            string? tags = null,
            string? codigoBarras = null,
            string? marca = null,
            string? unidadMedida = "UN")
        {
            if (empresaId <= 0)
                throw new ArgumentException("El ID de empresa debe ser mayor a 0");

            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código del producto es requerido");

            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción del producto es requerida");

            if (precio.HasValue && precio.Value < 0)
                throw new ArgumentException("El precio no puede ser negativo");

            if (existencia.HasValue && existencia.Value < 0)
                throw new ArgumentException("La existencia no puede ser negativa");

            return new ProductoEmpresa
            {
                EmpresaId = empresaId,
                Codigo = codigo,
                Descripcion = descripcion.Trim(),
                CodigoRubro = codigoRubro,
                Precio = precio ?? 0,
                Existencia = existencia ?? 0,
                Visible = visible ?? true,
                Destacado = destacado ?? false,
                OrdenCategoria = ordenCategoria ?? 0,
                ImagenUrl = imagenUrl,
                ImagenAlt = imagenAlt,
                DescripcionCorta = descripcionCorta,
                DescripcionLarga = descripcionLarga,
                Tags = tags,
                CodigoBarras = codigoBarras,
                Marca = marca,
                UnidadMedida = unidadMedida ?? "UN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Método para actualizar información básica del producto
        public void UpdateBasicInfo(
            string? descripcion = null,
            int? codigoRubro = null,
            decimal? precio = null,
            decimal? existencia = null)
        {
            if (descripcion != null)
            {
                if (string.IsNullOrWhiteSpace(descripcion))
                    throw new ArgumentException("La descripción del producto no puede estar vacía");
                Descripcion = descripcion.Trim();
            }

            if (codigoRubro.HasValue)
                CodigoRubro = codigoRubro;

            if (precio.HasValue)
            {
                if (precio.Value < 0)
                    throw new ArgumentException("El precio no puede ser negativo");
                Precio = precio.Value;
            }

            if (existencia.HasValue)
            {
                if (existencia.Value < 0)
                    throw new ArgumentException("La existencia no puede ser negativa");
                Existencia = existencia.Value;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        // Método para actualizar configuración web
        public void UpdateWebConfig(
            bool? visible = null,
            bool? destacado = null,
            int? ordenCategoria = null,
            string? imagenUrl = null,
            string? imagenAlt = null,
            string? descripcionCorta = null,
            string? descripcionLarga = null,
            string? tags = null,
            string? codigoBarras = null,
            string? marca = null,
            string? unidadMedida = null)
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

        // Método para actualizar todo el producto
        public void Update(
            string? descripcion = null,
            int? codigoRubro = null,
            decimal? precio = null,
            decimal? existencia = null,
            bool? visible = null,
            bool? destacado = null,
            int? ordenCategoria = null,
            string? imagenUrl = null,
            string? imagenAlt = null,
            string? descripcionCorta = null,
            string? descripcionLarga = null,
            string? tags = null,
            string? codigoBarras = null,
            string? marca = null,
            string? unidadMedida = null)
        {
            UpdateBasicInfo(descripcion, codigoRubro, precio, existencia);
            UpdateWebConfig(visible, destacado, ordenCategoria, imagenUrl, imagenAlt, 
                          descripcionCorta, descripcionLarga, tags, codigoBarras, marca, unidadMedida);
        }

        public bool IsVisible()
        {
            return Visible ?? false;
        }

        public bool IsHighlighted()
        {
            return Destacado ?? false;
        }

        public bool HasStock()
        {
            return Existencia > 0;
        }

        public bool BelongsToCompany(int companyId)
        {
            return EmpresaId == companyId;
        }
    }
}