using System.Collections.Generic;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Companies
{
    public class GetCompaniesListQuery : IRequest<PagedResultDto<CompanyDto>>
    {
        public int? PrincipalCompanyId { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? RequestingUserId { get; set; }
    }
}