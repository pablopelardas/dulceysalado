using System;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class CustomerSyncSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int EmpresaId { get; set; }
        public string? Source { get; set; }
        public string Status { get; set; } = "active";
        public int TotalProcessed { get; set; } = 0;
        public int TotalCreated { get; set; } = 0;
        public int TotalUpdated { get; set; } = 0;
        public int TotalUnchanged { get; set; } = 0;
        public int TotalErrors { get; set; } = 0;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? CreatedBy { get; set; }
        
        // Navegación - La propiedad se configurará en la capa de infraestructura
        
        // Métodos de dominio
        public void Complete()
        {
            Status = "completed";
            CompletedAt = DateTime.UtcNow;
        }
        
        public void Fail()
        {
            Status = "failed";
            CompletedAt = DateTime.UtcNow;
        }
        
        public bool IsActive()
        {
            return Status == "active" && (DateTime.UtcNow - StartedAt).TotalMinutes < 30;
        }
        
        public void AddProcessedResult(int created, int updated, int unchanged, int errors = 0)
        {
            TotalCreated += created;
            TotalUpdated += updated;
            TotalUnchanged += unchanged;
            TotalErrors += errors;
            TotalProcessed = TotalCreated + TotalUpdated + TotalUnchanged + TotalErrors;
        }
        
        public TimeSpan GetDuration()
        {
            var endTime = CompletedAt ?? DateTime.UtcNow;
            return endTime - StartedAt;
        }
    }
}