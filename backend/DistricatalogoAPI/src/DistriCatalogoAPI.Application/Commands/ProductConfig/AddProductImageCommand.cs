using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductConfig
{
    public class AddProductImageCommand : IRequest<AddProductImageCommandResult>
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class AddProductImageCommandResult
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> AllImageUrls { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }
}