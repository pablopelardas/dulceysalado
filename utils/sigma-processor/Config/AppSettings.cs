using SigmaProcessor.Models.Sigma;

namespace SigmaProcessor.Config;

public class AppSettings
{
    public ApiConfig Api { get; set; } = new();
    public ProcessingConfig Processing { get; set; } = new();
}

public class ApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int TimeoutMinutes { get; set; } = 5;
}

public class ProcessingConfig
{
    public int BatchSize { get; set; } = 500;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public string InputPath { get; set; } = ".\\input";
    public string ProcessedPath { get; set; } = ".\\processed";
    public string ErrorPath { get; set; } = ".\\errors";
    public string TempPath { get; set; } = ".\\temp";
    
    // Configuración para procesamiento triple (productos + precios + existencias)
    public string ProductsFileName { get; set; } = "productos.csv";
    public string PricesFileName { get; set; } = "precios.csv";
    public string StocksFileName { get; set; } = "existencias.csv";
    public List<int> ActivePriceListIds { get; set; } = new() { 1, 5, 50 };
    
    // Mapeo de columnas de existencias a IDs de empresa
    public Dictionary<string, List<int>> EmpresaMapping { get; set; } = new();
    
    // Configuración para procesamiento de stock individual
    public SingleStockConfig SingleStockConfig { get; set; } = new();
    
    // Configuración para procesamiento SIGMA XML
    public SigmaProcessConfig SigmaConfig { get; set; } = new();
    
    // Flag para mover archivos procesados
    public bool MoveProcessedFiles { get; set; } = true;
}

public class SingleStockConfig
{
    public bool Enabled { get; set; } = false;
    public string FileName { get; set; } = "stock-individual.csv";
    public int EmpresaId { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
}