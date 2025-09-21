using System;

namespace DistriCatalogoAPI.Application.DTOs.SolicitudReventa
{
    public class SolicitudReventaDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteEmail { get; set; }
        public string? Cuit { get; set; }
        public string? RazonSocial { get; set; }
        public string? DireccionComercial { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? TelefonoComercial { get; set; }
        public string? CategoriaIva { get; set; }
        public string? EmailComercial { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? ComentarioRespuesta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? RespondidoPor { get; set; }
        public DateTime FechaSolicitud { get; set; }
    }
}