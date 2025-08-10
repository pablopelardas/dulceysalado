using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductConfig
{
    public class RemoveProductImageCommand : IRequest<RemoveProductImageCommandResult>
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class RemoveProductImageCommandResult
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> RemainingImageUrls { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }
}