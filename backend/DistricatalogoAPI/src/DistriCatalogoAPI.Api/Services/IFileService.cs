using Microsoft.AspNetCore.Http;

namespace DistriCatalogoAPI.Api.Services;

public interface IFileService
{
    Task<string> SaveProductImageAsync(IFormFile file, string productCode, string productDescription, string productType);
    void DeleteProductImage(string imagePath);
    bool IsValidImageFile(IFormFile file);
    string GenerateProductImageName(string productCode, string productDescription, string extension);
    string SanitizeFileName(string input);
    Task<(byte[] content, string contentType)> GetImageAsync(string fileName);
    
    // Company image methods
    Task<string> SaveCompanyLogoAsync(IFormFile file, int companyId, string companyName);
    Task<string> SaveCompanyFaviconAsync(IFormFile file, int companyId, string companyName);
    void DeleteCompanyLogo(int companyId);
    void DeleteCompanyFavicon(int companyId);
    string GenerateCompanyImageName(int companyId, string companyName, string imageType, string extension);
}