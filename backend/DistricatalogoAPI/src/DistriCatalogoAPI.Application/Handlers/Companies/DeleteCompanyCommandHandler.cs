using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Companies;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Companies
{
    public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, bool>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;

        public DeleteCompanyCommandHandler(ICompanyRepository companyRepository, IUserRepository userRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
        {
            // Get the company to delete
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null)
            {
                throw new InvalidOperationException($"Company with ID {request.CompanyId} not found");
            }

            // Authorization check - only principal companies can delete client companies
            if (request.RequestingUserId.HasValue)
            {
                var requestingUser = await _userRepository.GetByIdAsync(request.RequestingUserId.Value);
                if (requestingUser == null || !requestingUser.IsActive)
                {
                    throw new UnauthorizedAccessException("Invalid requesting user");
                }

                if (!requestingUser.IsFromPrincipalCompany)
                {
                    throw new UnauthorizedAccessException("Only principal companies can delete client companies");
                }

                // Cannot delete the requesting user's own company (principal company)
                if (requestingUser.CompanyId == company.Id)
                {
                    throw new InvalidOperationException("Cannot delete your own company");
                }
            }

            // Cannot delete principal companies
            if (company.IsPrincipal)
            {
                throw new InvalidOperationException("Principal companies cannot be deleted");
            }

            // Perform soft delete by deactivating the company
            company.Deactivate();

            // Save changes
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            return true;
        }
    }
}