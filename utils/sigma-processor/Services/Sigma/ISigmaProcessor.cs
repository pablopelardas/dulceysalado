namespace SigmaProcessor.Services.Sigma
{
    public interface ISigmaProcessor<T>
    {
        Task<List<T>> ProcessFileAsync(string filePath, string tempFolder);
        bool ValidateFileStructure(string filePath);
        Task<string> CleanFileAsync(string inputPath, string outputPath);
    }
}