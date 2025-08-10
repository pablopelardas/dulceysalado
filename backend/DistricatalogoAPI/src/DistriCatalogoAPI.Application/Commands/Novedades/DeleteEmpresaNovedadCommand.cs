using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Novedades
{
    public class DeleteEmpresaNovedadCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteEmpresaNovedadCommand(int id)
        {
            Id = id;
        }
    }
}