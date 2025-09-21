namespace DistriCatalogoAPI.Application.DTOs.SolicitudReventa
{
    public class CreateSolicitudReventaDto
    {
        public string? Cuit { get; set; }
        public string? RazonSocial { get; set; }
        public string? DireccionComercial { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? TelefonoComercial { get; set; }
        public string? CategoriaIva { get; set; }
        public string? EmailComercial { get; set; }
    }
}