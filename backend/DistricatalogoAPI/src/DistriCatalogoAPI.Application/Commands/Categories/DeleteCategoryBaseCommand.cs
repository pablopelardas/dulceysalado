using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Categories
{
    public class DeleteCategoryBaseCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}