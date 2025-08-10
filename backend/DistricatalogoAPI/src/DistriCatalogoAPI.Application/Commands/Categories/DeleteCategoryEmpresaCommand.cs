using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Categories
{
    public class DeleteCategoryEmpresaCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}