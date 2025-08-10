using System;
using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Sync
{
    public class StartSyncSessionCommand : IRequest<StartSyncSessionResult>
    {
        public int TotalLotesEsperados { get; set; }
        public string UsuarioProceso { get; set; } = string.Empty;
        public string IpOrigen { get; set; } = string.Empty;
        public int EmpresaPrincipalId { get; set; } // Se establece desde el JWT
        public string? ListaCodigo { get; set; } // Código de la lista de precios (opcional, default si no se especifica)
        public bool MultiLista { get; set; } = false; // NUEVO: Flag para modo multi-lista
    }

    public class StartSyncSessionResult
    {
        public Guid SessionId { get; set; }
        public string EmpresaPrincipal { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public int TotalLotesEsperados { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? ListaCodigo { get; set; }
        public string? ListaNombre { get; set; }
        public bool MultiLista { get; set; } = false; // NUEVO: Indica si la sesión maneja múltiples listas
    }
}