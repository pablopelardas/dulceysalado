using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Ofertas
{
    public class UpdateEmpresaOfertaCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public bool Visible { get; set; }

        public UpdateEmpresaOfertaCommand(int id, bool visible)
        {
            Id = id;
            Visible = visible;
        }
    }
}