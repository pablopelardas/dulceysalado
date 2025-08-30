using System;
using System.Collections.Generic;
using System.Linq;

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
            if (Estado == PedidoEstado.Completado || Estado == PedidoEstado.Rechazado || Estado == PedidoEstado.Cancelado)
                throw new InvalidOperationException("No se pueden rechazar pedidos completados, ya rechazados o cancelados");
                
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
            if (Estado != PedidoEstado.Pendiente)
                throw new InvalidOperationException("Solo se pueden cancelar pedidos pendientes");
                
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
        
        public void IniciarCorreccion(int usuarioId)
        {
            if (Estado != PedidoEstado.Pendiente && Estado != PedidoEstado.CorreccionRechazada)
                throw new InvalidOperationException("Solo se pueden corregir pedidos pendientes o con corrección rechazada");
                
            Estado = PedidoEstado.EnCorreccion;
            UsuarioGestionId = usuarioId;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void EnviarCorreccionAlCliente(int usuarioId)
        {
            if (Estado != PedidoEstado.EnCorreccion)
                throw new InvalidOperationException("El pedido debe estar en corrección");
                
            Estado = PedidoEstado.CorreccionPendiente;
            UsuarioGestionId = usuarioId;
            FechaGestion = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void AprobarCorreccion()
        {
            if (Estado != PedidoEstado.CorreccionPendiente)
                throw new InvalidOperationException("No hay corrección pendiente de aprobación");
                
            Estado = PedidoEstado.Pendiente; // Vuelve a pendiente para que pueda ser aceptado
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void RechazarCorreccion()
        {
            if (Estado != PedidoEstado.CorreccionPendiente)
                throw new InvalidOperationException("No hay corrección pendiente de rechazo");
                
            Estado = PedidoEstado.CorreccionRechazada;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void ModificarItem(string codigoProducto, int nuevaCantidad)
        {
            if (Estado != PedidoEstado.EnCorreccion)
                throw new InvalidOperationException("Solo se pueden modificar items durante la corrección");
                
            var item = Items.FirstOrDefault(i => i.CodigoProducto == codigoProducto);
            if (item == null)
                throw new ArgumentException($"Item con código {codigoProducto} no encontrado");
                
            if (nuevaCantidad <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.ActualizarCantidad(nuevaCantidad);
            }
            
            CalcularMontoTotal();
        }

        public void ModificarItem(string codigoProducto, int nuevaCantidad, string? motivoCorreccion)
        {
            if (Estado != PedidoEstado.EnCorreccion)
                throw new InvalidOperationException("Solo se pueden modificar items durante la corrección");
                
            var item = Items.FirstOrDefault(i => i.CodigoProducto == codigoProducto);
            if (item == null)
                throw new ArgumentException($"Item con código {codigoProducto} no encontrado");
                
            if (nuevaCantidad <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.ActualizarCantidad(nuevaCantidad);
                if (!string.IsNullOrWhiteSpace(motivoCorreccion))
                {
                    item.Observaciones = motivoCorreccion;
                }
            }
            
            CalcularMontoTotal();
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
        Cancelado = 4,
        EnCorreccion = 5,
        CorreccionPendiente = 6,
        CorreccionRechazada = 7
    }
}