using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class CorreccionDto
    {
        public string Token { get; set; } = string.Empty;
        public int PedidoId { get; set; }
        public string PedidoNumero { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public bool EsValido { get; set; }
        
        // Datos del pedido original
        public PedidoOriginalDto PedidoOriginal { get; set; } = new();
        
        // Datos del pedido corregido
        public PedidoCorregidoDto PedidoCorregido { get; set; } = new();
        
        // Información del cliente
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        
        // Historial de correcciones del pedido
        public List<CorreccionTokenDto> HistorialCorrecciones { get; set; } = new();
        
        public class PedidoOriginalDto
        {
            public List<ItemPedidoDto> Items { get; set; } = new();
            public decimal MontoTotal { get; set; }
        }
        
        public class PedidoCorregidoDto
        {
            public List<ItemPedidoDto> Items { get; set; } = new();
            public decimal MontoTotal { get; set; }
            public string? MotivoCorreccion { get; set; }
        }
        
        public class ItemPedidoDto
        {
            public string CodigoProducto { get; set; } = string.Empty;
            public string NombreProducto { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal { get; set; }
            public string? Motivo { get; set; } // Solo para items corregidos
        }
    }
}