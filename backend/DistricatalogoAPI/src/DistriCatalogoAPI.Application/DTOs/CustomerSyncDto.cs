using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class StartCustomerSyncSessionDto
    {
        public string Source { get; set; } = "GECOM";
        public DateTime? Timestamp { get; set; }
    }
    
    public class CustomerSyncSessionResponseDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public CustomerSyncSummaryDto? Summary { get; set; }
    }
    
    public class ProcessBulkCustomersDto
    {
        public string SessionId { get; set; } = string.Empty;
        public List<CustomerSyncDto> Customers { get; set; } = new();
        public bool CreateCredentials { get; set; } = false;
    }
    
    public class CustomerSyncDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Telefono { get; set; }
        public string? Cuit { get; set; }
        public string? Altura { get; set; }
        public string? Provincia { get; set; }
        public string? Email { get; set; }
        public string? TipoIva { get; set; }
        public string? ListaPrecio { get; set; }
    }
    
    public class BulkProcessResultDto
    {
        public int Processed { get; set; }
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Unchanged { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    
    public class EndCustomerSyncSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
    }
    
    public class CustomerSyncSummaryDto
    {
        public int TotalProcessed { get; set; }
        public int TotalCreated { get; set; }
        public int TotalUpdated { get; set; }
        public int TotalUnchanged { get; set; }
        public int TotalErrors { get; set; }
        public string Duration { get; set; } = string.Empty;
    }
}