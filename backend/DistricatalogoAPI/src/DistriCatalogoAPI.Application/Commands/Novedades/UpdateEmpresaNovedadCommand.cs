using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Novedades
{
    public class UpdateEmpresaNovedadCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public bool Visible { get; set; }

        public UpdateEmpresaNovedadCommand(int id, bool visible)
        {
            Id = id;
            Visible = visible;
        }
    }
}