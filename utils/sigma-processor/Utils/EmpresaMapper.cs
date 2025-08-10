using SigmaProcessor.Config;

namespace SigmaProcessor.Utils;

/// <summary>
/// Clase utilitaria para el mapeo de columnas CSV a IDs de empresa
/// Configuración cargada desde appsettings.json
/// </summary>
public static class EmpresaMapper
{
    /// <summary>
    /// Obtiene el mapeo de empresas desde la configuración
    /// </summary>
    /// <param name="processingConfig">Configuración de procesamiento con el mapeo de empresas</param>
    /// <returns>Diccionario con mapeo de columnas CSV a IDs de empresa</returns>
    public static Dictionary<string, List<int>> GetEmpresaMapping(ProcessingConfig processingConfig)
    {
        return processingConfig.EmpresaMapping ?? new Dictionary<string, List<int>>();
    }

    /// <summary>
    /// Obtiene los IDs de empresa asociados a una columna específica
    /// </summary>
    /// <param name="columna">Nombre de la columna del CSV</param>
    /// <param name="processingConfig">Configuración de procesamiento</param>
    /// <returns>Lista de IDs de empresa para esa columna</returns>
    public static List<int> ObtenerEmpresasParaColumna(string columna, ProcessingConfig processingConfig)
    {
        var mapping = GetEmpresaMapping(processingConfig);
        return mapping.TryGetValue(columna.ToUpper(), out var empresas) 
            ? empresas 
            : new List<int>();
    }

    /// <summary>
    /// Obtiene todas las columnas disponibles para mapeo
    /// </summary>
    /// <param name="processingConfig">Configuración de procesamiento</param>
    /// <returns>Lista de nombres de columnas</returns>
    public static List<string> ObtenerColumnasDisponibles(ProcessingConfig processingConfig)
    {
        var mapping = GetEmpresaMapping(processingConfig);
        return mapping.Keys.ToList();
    }
}