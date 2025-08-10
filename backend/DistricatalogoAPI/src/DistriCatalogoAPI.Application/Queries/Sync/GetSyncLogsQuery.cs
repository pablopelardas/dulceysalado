using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Sync
{
    public class GetSyncLogsQuery : IRequest<GetSyncLogsResult>
    {
        public int EmpresaPrincipalId { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetSyncLogsResult
    {
        public List<SyncLogDto> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
    }

    public class SyncLogDto
    {
        public int Id { get; set; }
        public string ArchivoNombre { get; set; }
        public DateTime FechaProcesamiento { get; set; }
        public int ProductosActualizados { get; set; }
        public int ProductosNuevos { get; set; }
        public int Errores { get; set; }
        public int TiempoProcesamientoMs { get; set; }
        public string Estado { get; set; }
        public string UsuarioProceso { get; set; }
    }
}