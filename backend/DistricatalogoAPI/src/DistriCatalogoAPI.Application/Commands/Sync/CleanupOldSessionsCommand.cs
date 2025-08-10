using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Sync
{
    public class CleanupOldSessionsCommand : IRequest<CleanupOldSessionsResult>
    {
        public int DiasAntiguedad { get; set; } = 7;
        public int EmpresaPrincipalId { get; set; } // Se establece desde el JWT
    }

    public class CleanupOldSessionsResult
    {
        public int SesionesEliminadas { get; set; }
        public int DiasAntiguedad { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}