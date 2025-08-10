using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<CompanyRepository> _logger;

        public CompanyRepository(DistricatalogoContext context, ILogger<CompanyRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving company by ID: {CompanyId}", id);
            
            var empresaModel = await _context.Empresas
                .Include(e => e.ListaPrecioPredeterminada)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (empresaModel == null)
            {
                _logger.LogWarning("Company with ID {CompanyId} not found", id);
                return null;
            }
            
            _logger.LogDebug("Company {CompanyId} ({CompanyName}) retrieved successfully", 
                empresaModel.Id, empresaModel.Nombre);
                
            return MapToDomainEntity(empresaModel);
        }

        public async Task<Company> GetByCodeAsync(string codigo)
        {
            var empresaModel = await _context.Empresas.FirstOrDefaultAsync(e => e.Codigo == codigo);
            return empresaModel != null ? MapToDomainEntity(empresaModel) : null;
        }

        public async Task<Company> GetByDomainAsync(string dominio)
        {
            _logger.LogDebug("Retrieving company by domain: {Domain}", dominio);
            
            var empresaModel = await _context.Empresas.FirstOrDefaultAsync(e => e.DominioPersonalizado == dominio);
            
            if (empresaModel == null)
            {
                _logger.LogDebug("No company found for domain {Domain}", dominio);
                return null;
            }
            
            _logger.LogInformation("Company {CompanyId} ({CompanyName}) resolved from domain {Domain}", 
                empresaModel.Id, empresaModel.Nombre, dominio);
                
            return MapToDomainEntity(empresaModel);
        }

        public async Task<IEnumerable<Company>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Empresas.AsQueryable();

            if (!includeInactive)
                query = query.Where(e => e.Activa == true);

            var empresaModels = await query.OrderBy(e => e.TipoEmpresa).ThenBy(e => e.Nombre).ToListAsync();
            return empresaModels.Select(MapToDomainEntity).ToList();
        }

        public async Task<IEnumerable<Company>> GetClientCompaniesAsync(int principalCompanyId, bool includeInactive = false)
        {
            var query = _context.Empresas
                .Where(e => e.EmpresaPrincipalId == principalCompanyId);

            if (!includeInactive)
                query = query.Where(e => e.Activa == true);

            var empresaModels = await query.OrderBy(e => e.Nombre).ToListAsync();
            return empresaModels.Select(MapToDomainEntity).ToList();
        }

        public async Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(int page, int pageSize, int? principalCompanyId = null, bool includeInactive = false)
        {
            var query = _context.Empresas.AsQueryable();

            // Filter by principal company if specified
            if (principalCompanyId.HasValue)
            {
                query = query.Where(e => e.EmpresaPrincipalId == principalCompanyId.Value);
            }

            // Filter by active status
            if (!includeInactive)
            {
                query = query.Where(e => e.Activa == true);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var skip = (page - 1) * pageSize;
            var empresaModels = await query
                .OrderBy(e => e.TipoEmpresa)
                .ThenBy(e => e.Nombre)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Map to domain entities
            var companies = empresaModels.Select(MapToDomainEntity).ToList();

            return (companies, totalCount);
        }

        public async Task<bool> ExistsByCodeAsync(string codigo, int? excludeId = null)
        {
            var query = _context.Empresas.Where(e => e.Codigo == codigo && e.Activa == true);
            
            if (excludeId.HasValue)
                query = query.Where(e => e.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsByDomainAsync(string dominio, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(dominio))
                return false;

            var query = _context.Empresas.Where(e => e.DominioPersonalizado == dominio && e.Activa == true);
            
            if (excludeId.HasValue)
                query = query.Where(e => e.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<Company> CreateAsync(Company company)
        {
            var empresaModel = MapToEfModel(company);
            _context.Empresas.Add(empresaModel);
            await _context.SaveChangesAsync();

            // Update company ID using reflection
            var companyType = typeof(Company);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var idField = companyType.GetField("<Id>k__BackingField", flags);
            idField?.SetValue(company, empresaModel.Id);

            return company;
        }

        public async Task UpdateAsync(Company company)
        {
            var existingEmpresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == company.Id);
            if (existingEmpresa == null)
                throw new InvalidOperationException($"Company with ID {company.Id} not found");

            // Update properties
            existingEmpresa.Codigo = company.Codigo;
            existingEmpresa.Nombre = company.Nombre;
            existingEmpresa.RazonSocial = company.RazonSocial;
            existingEmpresa.Cuit = company.Cuit;
            existingEmpresa.Telefono = company.Telefono;
            existingEmpresa.Email = company.Email;
            existingEmpresa.Direccion = company.Direccion;
            existingEmpresa.TipoEmpresa = company.TipoEmpresa;
            existingEmpresa.EmpresaPrincipalId = company.EmpresaPrincipalId;
            existingEmpresa.LogoUrl = company.LogoUrl;
            existingEmpresa.ColoresTema = company.ColoresTema;
            existingEmpresa.FaviconUrl = company.FaviconUrl;
            existingEmpresa.DominioPersonalizado = company.DominioPersonalizado;
            existingEmpresa.UrlWhatsapp = company.UrlWhatsapp;
            existingEmpresa.UrlFacebook = company.UrlFacebook;
            existingEmpresa.UrlInstagram = company.UrlInstagram;
            existingEmpresa.MostrarPrecios = company.MostrarPrecios;
            existingEmpresa.MostrarStock = company.MostrarStock;
            existingEmpresa.PermitirPedidos = company.PermitirPedidos;
            existingEmpresa.ProductosPorPagina = company.ProductosPorPagina;
            existingEmpresa.PuedeAgregarProductos = company.PuedeAgregarProductos;
            existingEmpresa.PuedeAgregarCategorias = company.PuedeAgregarCategorias;
            existingEmpresa.Activa = company.Activa;
            existingEmpresa.FechaVencimiento = company.FechaVencimiento;
            existingEmpresa.Plan = company.Plan;
            existingEmpresa.ListaPrecioPredeterminadaId = company.ListaPrecioPredeterminadaId;
            existingEmpresa.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        private Company MapToDomainEntity(Empresa model)
        {
            var company = new Company(model.Codigo, model.Nombre, model.TipoEmpresa, model.EmpresaPrincipalId);
            
            // Use reflection to set private properties for database values
            var companyType = typeof(Company);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            var properties = new Dictionary<string, object>
            {
                ["<Id>k__BackingField"] = model.Id,
                ["<RazonSocial>k__BackingField"] = model.RazonSocial,
                ["<Cuit>k__BackingField"] = model.Cuit,
                ["<Telefono>k__BackingField"] = model.Telefono,
                ["<Email>k__BackingField"] = model.Email,
                ["<Direccion>k__BackingField"] = model.Direccion,
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
                ["<ListaPrecioPredeterminadaId>k__BackingField"] = model.ListaPrecioPredeterminadaId,
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

        private Empresa MapToEfModel(Company company)
        {
            return new Empresa
            {
                Codigo = company.Codigo,
                Nombre = company.Nombre,
                RazonSocial = company.RazonSocial,
                Cuit = company.Cuit,
                Telefono = company.Telefono,
                Email = company.Email,
                Direccion = company.Direccion,
                TipoEmpresa = company.TipoEmpresa,
                EmpresaPrincipalId = company.EmpresaPrincipalId,
                LogoUrl = company.LogoUrl,
                ColoresTema = company.ColoresTema,
                FaviconUrl = company.FaviconUrl,
                DominioPersonalizado = company.DominioPersonalizado,
                UrlWhatsapp = company.UrlWhatsapp,
                UrlFacebook = company.UrlFacebook,
                UrlInstagram = company.UrlInstagram,
                MostrarPrecios = company.MostrarPrecios,
                MostrarStock = company.MostrarStock,
                PermitirPedidos = company.PermitirPedidos,
                ProductosPorPagina = company.ProductosPorPagina,
                PuedeAgregarProductos = company.PuedeAgregarProductos,
                PuedeAgregarCategorias = company.PuedeAgregarCategorias,
                Activa = company.Activa,
                FechaVencimiento = company.FechaVencimiento,
                Plan = company.Plan,
                ListaPrecioPredeterminadaId = company.ListaPrecioPredeterminadaId,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };
        }

        public async Task<List<Company>> GetByIdsAsync(List<int> ids)
        {
            try
            {
                if (!ids.Any())
                    return new List<Company>();

                var empresaModels = await _context.Empresas
                    .Where(e => ids.Contains(e.Id))
                    .ToListAsync();

                return empresaModels.Select(MapToDomainEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empresas por IDs");
                throw;
            }
        }
    }
}