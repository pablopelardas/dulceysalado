using System;
using System.Text.Json;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class CorreccionToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int PedidoId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; } = false;
        public DateTime? FechaUso { get; set; }
        public string? RespuestaCliente { get; set; } // "Aprobado" o "Rechazado"
        public string? ComentarioCliente { get; set; }
        public string? MotivoCorreccion { get; set; }
        public string PedidoOriginalJson { get; set; } = null!; // Snapshot del pedido antes de corrección
        
        // Navegación
        public Pedido Pedido { get; set; } = null!;
        
        // Métodos de dominio
        public static CorreccionToken Crear(int pedidoId, string pedidoOriginalJson, string? motivoCorreccion = null, int horasValidez = 48)
        {
            return new CorreccionToken
            {
                Token = GenerarToken(),
                PedidoId = pedidoId,
                PedidoOriginalJson = pedidoOriginalJson,
                MotivoCorreccion = motivoCorreccion,
                FechaExpiracion = DateTime.UtcNow.AddHours(horasValidez),
                FechaCreacion = DateTime.UtcNow
            };
        }
        
        public PedidoOriginalData? GetPedidoOriginal()
        {
            if (string.IsNullOrWhiteSpace(PedidoOriginalJson))
                return null;
                
            try
            {
                return JsonSerializer.Deserialize<PedidoOriginalData>(PedidoOriginalJson);
            }
            catch
            {
                return null;
            }
        }
        
        public bool EsValido()
        {
            return !Usado && DateTime.UtcNow <= FechaExpiracion;
        }
        
        public void MarcarComoUsado(string respuesta, string? comentario = null)
        {
            if (Usado)
                throw new InvalidOperationException("El token ya fue utilizado");
                
            if (DateTime.UtcNow > FechaExpiracion)
                throw new InvalidOperationException("El token ha expirado");
                
            Usado = true;
            FechaUso = DateTime.UtcNow;
            RespuestaCliente = respuesta;
            ComentarioCliente = comentario;
        }
        
        private static string GenerarToken()
        {
            return Guid.NewGuid().ToString("N")[..12].ToUpper();
        }
    }
    
    public class PedidoOriginalData
    {
        public decimal MontoTotal { get; set; }
        public List<ItemOriginalData> Items { get; set; } = new();
    }
    
    public class ItemOriginalData
    {
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? Observaciones { get; set; }
    }
}