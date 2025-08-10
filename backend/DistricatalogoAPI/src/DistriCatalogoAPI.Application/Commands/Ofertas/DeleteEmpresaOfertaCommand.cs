using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Ofertas
{
    public class DeleteEmpresaOfertaCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteEmpresaOfertaCommand(int id)
        {
            Id = id;
        }
    }
}