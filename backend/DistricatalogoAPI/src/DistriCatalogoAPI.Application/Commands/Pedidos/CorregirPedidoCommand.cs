using MediatR;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.Commands.Pedidos
{
    public class CorregirPedidoCommand : IRequest<CorregirPedidoResult>
    {
        public int PedidoId { get; set; }
        public int UsuarioId { get; set; }
        public List<ItemCorreccion> ItemsCorregidos { get; set; } = new();
        public string? MotivoCorreccion { get; set; }
        public bool EnviarAlCliente { get; set; } = true; // Si false, solo guarda en EnCorreccion
        
        public class ItemCorreccion
        {
            public string CodigoProducto { get; set; } = string.Empty;
            public int NuevaCantidad { get; set; }
            public string? Motivo { get; set; } // "Sin stock", "Stock limitado", etc.
        }
    }
    
    public class CorregirPedidoResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? CorrectionUrl { get; set; }
        public bool EnviadoAlCliente { get; set; }
    }
}