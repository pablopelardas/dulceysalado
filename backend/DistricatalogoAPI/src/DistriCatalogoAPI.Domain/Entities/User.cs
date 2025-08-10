using System;
using DistriCatalogoAPI.Domain.ValueObjects;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public int CompanyId { get; private set; }
        public Email Email { get; private set; }
        public string? Username { get; private set; }
        public string PasswordHash { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public UserRole Role { get; private set; }
        
        public bool CanManageBaseProducts { get; private set; }
        public bool CanManageCompanyProducts { get; private set; }
        public bool CanManageBaseCategories { get; private set; }
        public bool CanManageCompanyCategories { get; private set; }
        public bool CanManageUsers { get; private set; }
        public bool CanViewStatistics { get; private set; }
        
        public bool IsActive { get; private set; }
        public DateTime? LastLogin { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected User() { }

        public User(
            int companyId,
            Email email,
            string passwordHash,
            string firstName,
            string lastName,
            UserRole role)
        {
            CompanyId = companyId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Role = role;
            
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            
            SetDefaultPermissions();
        }

        public string FullName => $"{FirstName} {LastName}";
        
        public bool IsFromPrincipalCompany => Role == UserRole.PrincipalAdmin || 
                                             Role == UserRole.PrincipalEditor || 
                                             Role == UserRole.PrincipalViewer;

        public bool CanManageUsersFromCompany(int targetCompanyId)
        {
            if (!CanManageUsers) return false;
            
            // Can always manage users from own company
            if (CompanyId == targetCompanyId) return true;
            
            // Principal company can manage users from any client company
            return IsFromPrincipalCompany;
        }

        public void UpdateProfile(string firstName, string lastName)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLastLogin()
        {
            LastLogin = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException("User is already inactive");
                
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePermissions(
            UserRole role,
            bool canManageBaseProducts,
            bool canManageCompanyProducts,
            bool canManageBaseCategories,
            bool canManageCompanyCategories,
            bool canManageUsers,
            bool canViewStatistics)
        {
            Role = role;
            CanManageBaseProducts = canManageBaseProducts;
            CanManageCompanyProducts = canManageCompanyProducts;
            CanManageBaseCategories = canManageBaseCategories;
            CanManageCompanyCategories = canManageCompanyCategories;
            CanManageUsers = canManageUsers;
            CanViewStatistics = canViewStatistics;
            UpdatedAt = DateTime.UtcNow;
        }

        private void SetDefaultPermissions()
        {
            switch (Role)
            {
                case UserRole.PrincipalAdmin:
                    CanManageBaseProducts = true;
                    CanManageCompanyProducts = true;
                    CanManageBaseCategories = true;
                    CanManageCompanyCategories = true;
                    CanManageUsers = true;
                    CanViewStatistics = true;
                    break;
                    
                case UserRole.PrincipalEditor:
                    CanManageBaseProducts = true;
                    CanManageBaseCategories = true;
                    CanViewStatistics = true;
                    break;
                    
                case UserRole.PrincipalViewer:
                    CanViewStatistics = true;
                    break;
                    
                case UserRole.ClientAdmin:
                    CanManageCompanyProducts = true;
                    CanManageCompanyCategories = true;
                    CanManageUsers = true;
                    break;
                    
                case UserRole.ClientEditor:
                    CanManageCompanyProducts = true;
                    CanManageCompanyCategories = true;
                    break;
                    
                case UserRole.ClientViewer:
                    break;
            }
        }
    }
}