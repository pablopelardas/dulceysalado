using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Novedades
{
    public class SetNovedadesForEmpresaCommand : IRequest<bool>
    {
        public int EmpresaId { get; set; }
        public List<int> AgrupacionIds { get; set; } = new List<int>();

        public SetNovedadesForEmpresaCommand(int empresaId, List<int> agrupacionIds)
        {
            EmpresaId = empresaId;
            AgrupacionIds = agrupacionIds;
        }
    }
}