using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.ValueObjects;
using DistriCatalogoAPI.Domain.Enums;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DistricatalogoContext _context;

        public UserRepository(DistricatalogoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var userModel = await _context.Usuarios
                .Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.Id == id);

            return userModel != null ? MapToDomainEntity(userModel) : null;
        }

        public async Task<User> GetByEmailAsync(Email email, bool includeInactive = false)
        {
            var query = _context.Usuarios.Include(u => u.Empresa).AsQueryable();

            if (!includeInactive)
                query = query.Where(u => u.Activo == true);

            var userModel = await query
                .FirstOrDefaultAsync(u => u.Email == email.Value);

            return userModel != null ? MapToDomainEntity(userModel) : null;
        }

        public async Task<User> GetByEmailOrUsernameAsync(string emailOrUsername, bool includeInactive = false)
        {
            var query = _context.Usuarios.Include(u => u.Empresa).AsQueryable();

            if (!includeInactive)
                query = query.Where(u => u.Activo == true);

            var userModel = await query
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            return userModel != null ? MapToDomainEntity(userModel) : null;
        }

        public async Task<IEnumerable<User>> GetAllByCompanyAsync(int companyId, bool includeInactive = false)
        {
            var query = _context.Usuarios
                .Include(u => u.Empresa)
                .Where(u => u.EmpresaId == companyId);

            if (!includeInactive)
                query = query.Where(u => u.Activo == true);

            var users = await query.ToListAsync();
            return users.Select(MapToDomainEntity);
        }

        public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Usuarios.Include(u => u.Empresa).AsQueryable();

            if (!includeInactive)
                query = query.Where(u => u.Activo == true);

            var users = await query.ToListAsync();
            return users.Select(MapToDomainEntity);
        }

        public async Task<bool> ExistsByEmailAsync(Email email, bool onlyActive = true)
        {
            var query = _context.Usuarios.AsQueryable();

            if (onlyActive)
                query = query.Where(u => u.Activo == true);

            return await query.AnyAsync(u => u.Email == email.Value);
        }

        public async Task<User> CreateAsync(User user)
        {
            var userModel = MapToInfrastructureModel(user);
            userModel.CreatedAt = DateTime.UtcNow;
            userModel.UpdatedAt = DateTime.UtcNow;

            await _context.Usuarios.AddAsync(userModel);
            await _context.SaveChangesAsync();

            user = MapToDomainEntity(userModel);
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            var existingUser = await _context.Usuarios.FindAsync(user.Id);
            if (existingUser == null)
                throw new InvalidOperationException($"User with ID {user.Id} not found");

            // Update fields
            existingUser.Email = user.Email.Value;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.Nombre = user.FirstName;
            existingUser.Apellido = user.LastName;
            existingUser.Rol = MapEnumRoleToDatabase(user.Role);
            existingUser.Activo = user.IsActive;
            existingUser.UltimoLogin = user.LastLogin;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Update permissions
            existingUser.PuedeGestionarProductosBase = user.CanManageBaseProducts;
            existingUser.PuedeGestionarProductosEmpresa = user.CanManageCompanyProducts;
            existingUser.PuedeGestionarCategoriasBase = user.CanManageBaseCategories;
            existingUser.PuedeGestionarCategoriasEmpresa = user.CanManageCompanyCategories;
            existingUser.PuedeGestionarUsuarios = user.CanManageUsers;
            existingUser.PuedeVerEstadisticas = user.CanViewStatistics;

            _context.Usuarios.Update(existingUser);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, int? companyId = null, bool includeInactive = false)
        {
            var query = _context.Usuarios.Include(u => u.Empresa).AsQueryable();

            // Filter by company if specified
            if (companyId.HasValue)
            {
                query = query.Where(u => u.EmpresaId == companyId.Value);
            }

            // Filter by active status
            if (!includeInactive)
            {
                query = query.Where(u => u.Activo == true);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var skip = (page - 1) * pageSize;
            var userModels = await query
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Map to domain entities
            var users = userModels.Select(MapToDomainEntity).ToList();

            return (users, totalCount);
        }

        public async Task<string> GetCompanyTypeAsync(int companyId)
        {
            var company = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == companyId);
            return company?.TipoEmpresa ?? "cliente";
        }

        public async Task<Company> GetCompanyAsync(int companyId)
        {
            var empresaModel = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == companyId);
            return empresaModel != null ? MapCompanyToDomainEntity(empresaModel) : null;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        private User MapToDomainEntity(Usuario model)
        {
            var email = new Email(model.Email);
            var role = MapDatabaseRoleToEnum(model.Rol ?? "viewer", model.Empresa?.TipoEmpresa ?? "cliente");

            // Create user entity via constructor
            var user = new User(
                model.EmpresaId,
                email,
                model.PasswordHash,
                model.Nombre,
                model.Apellido,
                role);

            // Use reflection to set private properties for database values
            var userType = typeof(User);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            // Set Id using backing field
            var idField = userType.GetField("<Id>k__BackingField", flags);
            idField?.SetValue(user, model.Id);
            
            // Set Username using backing field
            var usernameField = userType.GetField("<Username>k__BackingField", flags);
            usernameField?.SetValue(user, model.Username);
            
            // Set IsActive using backing field
            var isActiveField = userType.GetField("<IsActive>k__BackingField", flags);
            isActiveField?.SetValue(user, model.Activo ?? true);
            
            // Set LastLogin using backing field
            var lastLoginField = userType.GetField("<LastLogin>k__BackingField", flags);
            lastLoginField?.SetValue(user, model.UltimoLogin);
            
            // Set CreatedAt using backing field
            var createdAtField = userType.GetField("<CreatedAt>k__BackingField", flags);
            createdAtField?.SetValue(user, model.CreatedAt ?? DateTime.UtcNow);
            
            // Set UpdatedAt using backing field
            var updatedAtField = userType.GetField("<UpdatedAt>k__BackingField", flags);
            updatedAtField?.SetValue(user, model.UpdatedAt ?? DateTime.UtcNow);

            // Set permissions using the public method
            user.UpdatePermissions(
                role,
                model.PuedeGestionarProductosBase ?? false,
                model.PuedeGestionarProductosEmpresa ?? false,
                model.PuedeGestionarCategoriasBase ?? false,
                model.PuedeGestionarCategoriasEmpresa ?? false,
                model.PuedeGestionarUsuarios ?? false,
                model.PuedeVerEstadisticas ?? false);

            return user;
        }

        private Usuario MapToInfrastructureModel(User entity)
        {
            return new Usuario
            {
                Id = entity.Id,
                EmpresaId = entity.CompanyId,
                Email = entity.Email.Value,
                PasswordHash = entity.PasswordHash,
                Nombre = entity.FirstName,
                Apellido = entity.LastName,
                Rol = MapEnumRoleToDatabase(entity.Role),
                Activo = entity.IsActive,
                UltimoLogin = entity.LastLogin,
                PuedeGestionarProductosBase = entity.CanManageBaseProducts,
                PuedeGestionarProductosEmpresa = entity.CanManageCompanyProducts,
                PuedeGestionarCategoriasBase = entity.CanManageBaseCategories,
                PuedeGestionarCategoriasEmpresa = entity.CanManageCompanyCategories,
                PuedeGestionarUsuarios = entity.CanManageUsers,
                PuedeVerEstadisticas = entity.CanViewStatistics
            };
        }

        public UserRole MapDatabaseRoleToEnum(string databaseRole, string companyType)
        {
            var isPrincipal = companyType?.ToLower() == "principal";
            
            return databaseRole?.ToLower() switch
            {
                "admin" => isPrincipal ? UserRole.PrincipalAdmin : UserRole.ClientAdmin,
                "editor" => isPrincipal ? UserRole.PrincipalEditor : UserRole.ClientEditor,
                "viewer" => isPrincipal ? UserRole.PrincipalViewer : UserRole.ClientViewer,
                _ => UserRole.ClientViewer
            };
        }

        private string MapEnumRoleToDatabase(UserRole enumRole)
        {
            return enumRole switch
            {
                UserRole.PrincipalAdmin or UserRole.ClientAdmin => "admin",
                UserRole.PrincipalEditor or UserRole.ClientEditor => "editor",
                UserRole.PrincipalViewer or UserRole.ClientViewer => "viewer",
                _ => "viewer"
            };
        }

        private Company MapCompanyToDomainEntity(Empresa model)
        {
            var company = new Company(model.Codigo, model.Nombre, model.TipoEmpresa);
            
            // Use reflection to set private properties for database values
            var companyType = typeof(Company);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            // Set all private properties
            var properties = new Dictionary<string, object>
            {
                ["<Id>k__BackingField"] = model.Id,
                ["<RazonSocial>k__BackingField"] = model.RazonSocial,
                ["<Cuit>k__BackingField"] = model.Cuit,
                ["<Telefono>k__BackingField"] = model.Telefono,
                ["<Email>k__BackingField"] = model.Email,
                ["<Direccion>k__BackingField"] = model.Direccion,
                ["<EmpresaPrincipalId>k__BackingField"] = model.EmpresaPrincipalId,
                ["<LogoUrl>k__BackingField"] = model.LogoUrl,
                ["<ColoresTema>k__BackingField"] = model.ColoresTema,
                ["<FaviconUrl>k__BackingField"] = model.FaviconUrl,
                ["<DominioPersonalizado>k__BackingField"] = model.DominioPersonalizado,
                ["<UrlWhatsapp>k__BackingField"] = model.UrlWhatsapp,
                ["<UrlFacebook>k__BackingField"] = model.UrlFacebook,
                ["<UrlInstagram>k__BackingField"] = model.UrlInstagram,
                ["<MostrarPrecios>k__BackingField"] = model.MostrarPrecios ?? false,
                ["<MostrarStock>k__BackingField"] = model.MostrarStock ?? false,
                ["<PermitirPedidos>k__BackingField"] = model.PermitirPedidos ?? false,
                ["<ProductosPorPagina>k__BackingField"] = model.ProductosPorPagina ?? 20,
                ["<PuedeAgregarProductos>k__BackingField"] = model.PuedeAgregarProductos ?? false,
                ["<PuedeAgregarCategorias>k__BackingField"] = model.PuedeAgregarCategorias ?? false,
                ["<Activa>k__BackingField"] = model.Activa ?? true,
                ["<FechaVencimiento>k__BackingField"] = model.FechaVencimiento,
                ["<Plan>k__BackingField"] = model.Plan,
                ["<CreatedAt>k__BackingField"] = model.CreatedAt ?? DateTime.UtcNow,
                ["<UpdatedAt>k__BackingField"] = model.UpdatedAt ?? DateTime.UtcNow
            };

            foreach (var property in properties)
            {
                var field = companyType.GetField(property.Key, flags);
                field?.SetValue(company, property.Value);
            }

            return company;
        }
    }
}