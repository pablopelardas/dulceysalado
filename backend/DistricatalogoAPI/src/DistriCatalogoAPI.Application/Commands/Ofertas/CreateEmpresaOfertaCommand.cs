using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Ofertas
{
    public class CreateEmpresaOfertaCommand : IRequest<EmpresaOfertaDto>
    {
        public int EmpresaId { get; set; }
        public int AgrupacionId { get; set; }
        public bool Visible { get; set; } = true;

        public CreateEmpresaOfertaCommand(int empresaId, int agrupacionId, bool visible = true)
        {
            EmpresaId = empresaId;
            AgrupacionId = agrupacionId;
            Visible = visible;
        }
    }
}