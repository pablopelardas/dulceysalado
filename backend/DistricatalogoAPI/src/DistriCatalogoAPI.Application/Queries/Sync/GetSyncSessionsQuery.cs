using System;
using System.Collections.Generic;
using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Sync
{
    public class GetSyncSessionsQuery : IRequest<GetSyncSessionsResult>
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        public string Estado { get; set; }
        public int EmpresaPrincipalId { get; set; } // Se establece desde el JWT
    }

    public class GetSyncSessionsResult
    {
        public List<SyncSessionItem> Sessions { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class SyncSessionItem
    {
        public Guid Id { get; set; }
        public string Estado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string UsuarioProceso { get; set; }
        public SessionProgressList Progreso { get; set; } = new();
        public string Empresa { get; set; }
        public int ProductosTotales { get; set; }
        public int ProductosErrores { get; set; }
        public int TiempoTotalMs { get; set; }
    }

    public class PaginationInfo
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
    }

    public class SessionProgressList
    {
        public double Porcentaje { get; set; }
        public int LotesProcesados { get; set; }
        public int TotalLotesEsperados { get; set; }
        public string Estado { get; set; }
    }
}