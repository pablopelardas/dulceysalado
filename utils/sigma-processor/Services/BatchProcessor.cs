
namespace SigmaProcessor.Services;

public class BatchProcessingSummary
{
    public int TotalBatches { get; set; }
    public int ProcessedBatches { get; set; }
    public int SuccessfulBatches { get; set; }
    public int FailedBatches { get; set; }
    
    public int TotalProductosNuevos { get; set; }
    public int TotalProductosActualizados { get; set; }
    public int TotalErrores { get; set; }
    
    // Categorías faltantes consolidadas
    public HashSet<int> CategoriasNoEncontradas { get; set; } = new();
    public int TotalCategoriasNoEncontradas { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalProcessingTime { get; set; }
    
    public List<string> ErrorDetails { get; set; } = new();

    public bool IsSuccessful => SuccessfulBatches == TotalBatches;
    public double SuccessRate => TotalBatches > 0 ? (double)SuccessfulBatches / TotalBatches * 100 : 0;
}