using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Ofertas
{
    public class SetOfertasForEmpresaCommand : IRequest<bool>
    {
        public int EmpresaId { get; set; }
        public List<int> AgrupacionIds { get; set; } = new List<int>();

        public SetOfertasForEmpresaCommand(int empresaId, List<int> agrupacionIds)
        {
            EmpresaId = empresaId;
            AgrupacionIds = agrupacionIds;
        }
    }
}