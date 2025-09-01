using System;
using System.Collections.Generic;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? HorarioEntrega { get; set; }
        public string? DireccionEntrega { get; set; }
        public string? Observaciones { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? MotivoRechazo { get; set; }
        public int? UsuarioGestionId { get; set; }
        public DateTime? FechaGestion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Cliente info
        public string? ClienteNombre { get; set; }
        public string? ClienteEmail { get; set; }
        public string? ClienteTelefono { get; set; }
        
        // Items
        public List<PedidoItemDto> Items { get; set; } = new();
        
        // Correcciones
        public List<CorreccionTokenDto> Correcciones { get; set; } = new();
    }

    public class PedidoItemDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CrearPedidoDto
    {
        public List<CrearPedidoItemDto> Items { get; set; } = new();
        public string? Observaciones { get; set; }
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? HorarioEntrega { get; set; }
        public string? DeliverySlot { get; set; }
    }

    public class CrearPedidoItemDto
    {
        public string CodigoProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? NombreProducto { get; set; }
        public string? Observaciones { get; set; }
    }

    public class GestionarPedidoDto
    {
        public PedidoEstado NuevoEstado { get; set; }
        public string? Motivo { get; set; }
    }

    public class PedidosPagedResultDto
    {
        public List<PedidoDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    public class PedidoEstadisticasDto
    {
        public int TotalPendientes { get; set; }
        public int TotalAceptados { get; set; }
        public int TotalRechazados { get; set; }
        public int TotalCompletados { get; set; }
        public int TotalCancelados { get; set; }
        public decimal MontoTotalHoy { get; set; }
        public decimal MontoTotalSemana { get; set; }
        public decimal MontoTotalMes { get; set; }
    }

    public class CorreccionTokenDto
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Utilizado { get; set; }
        public DateTime? FechaUso { get; set; }
        public string? MotivoCorreccion { get; set; }
        public string? PedidoOriginalJson { get; set; }
    }
}