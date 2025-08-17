using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
        public DateTime? FechaEntrega { get; set; }
        public string? HorarioEntrega { get; set; }
        public string? DireccionEntrega { get; set; }
        public string? Observaciones { get; set; }
        public decimal MontoTotal { get; set; }
        public PedidoEstado Estado { get; set; } = PedidoEstado.Pendiente;
        public string? MotivoRechazo { get; set; }
        public int? UsuarioGestionId { get; set; } // Usuario que gestiona el pedido
        public DateTime? FechaGestion { get; set; }
        
        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Navegación
        public Cliente Cliente { get; set; } = null!;
        public ICollection<PedidoItem> Items { get; set; } = new List<PedidoItem>();
        
        // Métodos de dominio
        public void AceptarPedido(int usuarioId)
        {
            if (Estado != PedidoEstado.Pendiente)
                throw new InvalidOperationException("Solo se pueden aceptar pedidos pendientes");
                
            Estado = PedidoEstado.Aceptado;
            UsuarioGestionId = usuarioId;
            FechaGestion = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void RechazarPedido(int usuarioId, string motivo)
        {
            if (Estado != PedidoEstado.Pendiente)
                throw new InvalidOperationException("Solo se pueden rechazar pedidos pendientes");
                
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de rechazo es requerido");
                
            Estado = PedidoEstado.Rechazado;
            UsuarioGestionId = usuarioId;
            MotivoRechazo = motivo;
            FechaGestion = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void CompletarPedido(int usuarioId)
        {
            if (Estado != PedidoEstado.Aceptado)
                throw new InvalidOperationException("Solo se pueden completar pedidos aceptados");
                
            Estado = PedidoEstado.Completado;
            UsuarioGestionId = usuarioId;
            FechaGestion = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void CancelarPedido(int usuarioId, string motivo = "Cancelado por el cliente")
        {
            if (Estado == PedidoEstado.Completado)
                throw new InvalidOperationException("No se pueden cancelar pedidos completados");
                
            Estado = PedidoEstado.Cancelado;
            UsuarioGestionId = usuarioId;
            MotivoRechazo = motivo;
            FechaGestion = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void CalcularMontoTotal()
        {
            MontoTotal = Items.Sum(i => i.Cantidad * i.PrecioUnitario);
            UpdatedAt = DateTime.UtcNow;
        }
        
        public bool PuedeSerModificado()
        {
            return Estado == PedidoEstado.Pendiente;
        }
        
        public string GenerateNumero()
        {
            var fecha = FechaPedido.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"PED-{fecha}-{random}";
        }
    }
    
    public enum PedidoEstado
    {
        Pendiente = 0,
        Aceptado = 1,
        Rechazado = 2,
        Completado = 3,
        Cancelado = 4
    }
}