using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Companies
{
    public class DeleteCompanyCommand : IRequest<bool>
    {
        public int CompanyId { get; set; }
        public int? RequestingUserId { get; set; }
    }
}