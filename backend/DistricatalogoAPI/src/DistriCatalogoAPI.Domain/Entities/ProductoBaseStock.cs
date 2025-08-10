using System;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    /// <summary>
    /// Entidad que representa el stock de un producto base para una empresa específica
    /// Permite gestión independiente de inventario por sucursal
    /// </summary>
    public class ProductoBaseStock : BaseEntity
    {
        /// <summary>
        /// ID único del registro de stock
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la empresa/sucursal que posee este stock
        /// </summary>
        public int EmpresaId { get; set; }

        /// <summary>
        /// ID del producto base al que pertenece este stock
        /// </summary>
        public int ProductoBaseId { get; set; }

        /// <summary>
        /// Cantidad disponible en stock para esta empresa
        /// </summary>
        public decimal Existencia { get; set; }


        // Navigation properties
        /// <summary>
        /// Empresa propietaria del stock
        /// </summary>
        public virtual Company Empresa { get; set; } = null!;

        /// <summary>
        /// Producto base al que pertenece el stock
        /// </summary>
        public virtual ProductBase ProductoBase { get; set; } = null!;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public ProductoBaseStock()
        {
        }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoBaseId">ID del producto base</param>
        /// <param name="existencia">Cantidad en stock</param>
        public ProductoBaseStock(int empresaId, int productoBaseId, decimal existencia)
        {
            EmpresaId = empresaId;
            ProductoBaseId = productoBaseId;
            Existencia = existencia;
        }

        /// <summary>
        /// Actualiza la cantidad de stock
        /// </summary>
        /// <param name="nuevaExistencia">Nueva cantidad</param>
        public void ActualizarStock(decimal nuevaExistencia)
        {
            Existencia = nuevaExistencia;
        }

        /// <summary>
        /// Indica si el producto tiene stock disponible
        /// </summary>
        public bool TieneStock => Existencia > 0;

        /// <summary>
        /// Clave única para identificar el stock (empresa + producto)
        /// </summary>
        public string ClaveUnica => $"{EmpresaId}-{ProductoBaseId}";
    }
}