using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Novedades
{
    public class CreateEmpresaNovedadCommand : IRequest<EmpresaNovedadDto>
    {
        public int EmpresaId { get; set; }
        public int AgrupacionId { get; set; }
        public bool Visible { get; set; } = true;

        public CreateEmpresaNovedadCommand(int empresaId, int agrupacionId, bool visible = true)
        {
            EmpresaId = empresaId;
            AgrupacionId = agrupacionId;
            Visible = visible;
        }
    }
}