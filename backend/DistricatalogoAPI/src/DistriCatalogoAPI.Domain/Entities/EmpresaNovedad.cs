using System;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class EmpresaNovedad : BaseEntity
    {
        public int Id { get; private set; }
        public int EmpresaId { get; private set; }
        public int AgrupacionId { get; private set; }
        public bool Visible { get; private set; }

        // Navigation properties
        public virtual Company Empresa { get; private set; }
        public virtual Agrupacion Agrupacion { get; private set; }

        protected EmpresaNovedad() { }

        public static EmpresaNovedad Create(
            int empresaId,
            int agrupacionId,
            bool visible = true)
        {
            if (empresaId <= 0)
                throw new ArgumentException("El ID de empresa debe ser mayor a 0");

            if (agrupacionId <= 0)
                throw new ArgumentException("El ID de agrupación debe ser mayor a 0");

            return new EmpresaNovedad
            {
                EmpresaId = empresaId,
                AgrupacionId = agrupacionId,
                Visible = visible,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public bool IsVisible()
        {
            return Visible;
        }

        public bool BelongsToCompany(int companyId)
        {
            return EmpresaId == companyId;
        }

        public bool IsForAgrupacion(int agrupacionId)
        {
            return AgrupacionId == agrupacionId;
        }
    }
}