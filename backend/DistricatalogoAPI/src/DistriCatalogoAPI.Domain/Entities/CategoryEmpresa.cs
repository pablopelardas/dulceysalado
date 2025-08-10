using System;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    /// <summary>
    /// Categoría específica de una empresa cliente
    /// Permite a las empresas cliente crear sus propias categorías además de usar las base
    /// </summary>
    public class CategoryEmpresa : BaseEntity
    {
        public int Id { get; private set; }
        public int EmpresaId { get; private set; }
        public int CodigoRubro { get; private set; }
        public string Nombre { get; private set; }
        public string Icono { get; private set; }
        public bool Visible { get; private set; }
        public int Orden { get; private set; }
        public string Color { get; private set; }
        public string Descripcion { get; private set; }

        // Navigation
        public virtual Company Empresa { get; private set; }

        protected CategoryEmpresa() { }

        /// <summary>
        /// Crear nueva categoría específica de empresa
        /// </summary>
        public static CategoryEmpresa Create(
            int empresaId,
            int codigoRubro,
            string nombre,
            string? icono = null,
            bool visible = true,
            int orden = 100,
            string? color = null,
            string? descripcion = null)
        {
            if (empresaId <= 0)
                throw new ArgumentException("El ID de empresa debe ser mayor a 0");

            // Validación removida - permite cualquier código de rubro

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la categoría es requerido");

            return new CategoryEmpresa
            {
                EmpresaId = empresaId,
                CodigoRubro = codigoRubro,
                Nombre = nombre.Trim(),
                Icono = icono,
                Visible = visible,
                Orden = orden,
                Color = color ?? "#6B7280",
                Descripcion = descripcion?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Actualizar información de la categoría
        /// </summary>
        public void Update(
            string? nombre = null,
            string? icono = null,
            bool? visible = null,
            int? orden = null,
            string? color = null,
            string? descripcion = null,
            int? codigoRubro = null)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
                Nombre = nombre.Trim();

            if (!string.IsNullOrWhiteSpace(icono))
                Icono = icono;

            if (visible.HasValue)
                Visible = visible.Value;

            if (orden.HasValue && orden.Value >= 0)
                Orden = orden.Value;

            if (!string.IsNullOrWhiteSpace(color))
                Color = color;

            if (descripcion != null)
                Descripcion = descripcion.Trim();

            if (codigoRubro.HasValue)
                CodigoRubro = codigoRubro.Value;

            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Alternar visibilidad de la categoría
        /// </summary>
        public void ToggleVisibility()
        {
            Visible = !Visible;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Verificar si la categoría pertenece a la empresa especificada
        /// </summary>
        public bool BelongsToCompany(int empresaId)
        {
            return EmpresaId == empresaId;
        }

        /// <summary>
        /// Verificar si un usuario puede gestionar esta categoría
        /// Debe tener permisos de company categories y ser de la empresa dueña o principal
        /// </summary>
        public bool CanBeManageBy(User user)
        {
            if (!user.CanManageCompanyCategories)
                return false;

            // Empresa dueña siempre puede gestionar
            if (user.CompanyId == EmpresaId)
                return true;

            // Empresa principal puede gestionar todas las categorías de clientes
            return user.IsFromPrincipalCompany;
        }

        /// <summary>
        /// Verificar si la categoría puede ser eliminada
        /// </summary>
        public bool CanBeDeleted()
        {
            // Por ahora permitir eliminar siempre
            // En el futuro se podría verificar si tiene productos asociados
            return true;
        }
    }
}