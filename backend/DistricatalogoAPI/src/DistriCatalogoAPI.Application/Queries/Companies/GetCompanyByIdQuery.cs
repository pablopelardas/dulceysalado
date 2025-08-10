using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Companies
{
    public class GetCompanyByIdQuery : IRequest<CompanyDto>
    {
        public int CompanyId { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int? RequestingUserId { get; set; }
    }
}