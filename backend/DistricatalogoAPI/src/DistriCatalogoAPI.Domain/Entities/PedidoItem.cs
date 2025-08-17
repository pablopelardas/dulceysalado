using System;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class PedidoItem
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
        public string? Observaciones { get; set; }
        
        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public Pedido Pedido { get; set; } = null!;
        
        // Métodos de dominio
        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");
                
            Cantidad = nuevaCantidad;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void ActualizarPrecio(decimal nuevoPrecio)
        {
            if (nuevoPrecio < 0)
                throw new ArgumentException("El precio no puede ser negativo");
                
            PrecioUnitario = nuevoPrecio;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public bool EsValido()
        {
            return !string.IsNullOrWhiteSpace(CodigoProducto) &&
                   !string.IsNullOrWhiteSpace(NombreProducto) &&
                   Cantidad > 0 &&
                   PrecioUnitario >= 0;
        }
    }
}