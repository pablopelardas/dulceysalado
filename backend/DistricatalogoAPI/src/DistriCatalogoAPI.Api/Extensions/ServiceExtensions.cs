using DistriCatalogoAPI.Api.Services;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Application.Validators;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Mappings;
using DistriCatalogoAPI.Infrastructure.Models;
using DistriCatalogoAPI.Infrastructure.Repositories;
using DistriCatalogoAPI.Infrastructure.Services;
using DistriCatalogoAPI.Infrastructure.Configuration;
using DistriCatalogoAPI.Application.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DistriCatalogoAPI.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure ForwardedHeaders
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Controllers with JSON options
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
                    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                });

            // Database
            services.AddDbContext<DistricatalogoContext>(options =>
                options.UseMySQL(configuration.GetConnectionString("DefaultConnection") 
                    ?? "server=localhost;port=3306;database=districatalogo;user=root;password=;charset=utf8mb4;"));

            // MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

            // AutoMapper
            services.AddAutoMapper(typeof(CreateUserCommand).Assembly, typeof(SyncMappingProfile).Assembly);

            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(CreateUserValidator).Assembly);

            // Register Repositories
            services.AddRepositories();

            // Register Services
            services.AddDomainServices();

            // HTTP Context
            services.AddHttpContextAccessor();

            // Company Resolution Configuration
            services.Configure<CompanyResolverOptions>(configuration.GetSection(CompanyResolverOptions.SectionName));

            // Application Configuration
            services.Configure<ApplicationOptions>(configuration.GetSection(ApplicationOptions.SectionName));

            // Stock Cache Configuration
            services.AddStockCaching(configuration);

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ISyncSessionRepository, SyncSessionRepository>();
            services.AddScoped<ISyncLogRepository, SyncLogRepository>();
            services.AddScoped<IProductBaseRepository, ProductBaseRepository>();
            services.AddScoped<ICategoryBaseRepository, CategoryBaseRepository>();
            services.AddScoped<IProductBasePrecioRepository, ProductBasePrecioRepository>();
            services.AddScoped<IListaPrecioRepository, ListaPrecioRepository>();
            services.AddScoped<IAgrupacionRepository, AgrupacionRepository>();
            services.AddScoped<IEmpresaNovedadRepository, EmpresaNovedadRepository>();
            services.AddScoped<IEmpresaOfertaRepository, EmpresaOfertaRepository>();
            services.AddScoped<IProductoBaseStockRepository, ProductoBaseStockRepository>();
            services.AddScoped<IFeatureRepository, FeatureRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<ICorreccionTokenRepository, CorreccionTokenRepository>();
            services.AddScoped<IUserNotificationPreferencesRepository, UserNotificationPreferencesRepository>();

            // Repositories - Módulo 04 Catalog Management 
            services.AddScoped<ICategoryEmpresaRepository, CategoryEmpresaRepository>();
            services.AddScoped<ICatalogRepository, CatalogRepository>();
            services.AddScoped<IProductoEmpresaRepository, ProductoEmpresaRepository>();
            services.AddScoped<IProductoEmpresaPrecioRepository, ProductoEmpresaPrecioRepository>();

            return services;
        }

        private static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, JwtService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<CompanyResolverService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IStockCacheService, StockCacheService>();
            services.AddScoped<IClienteAuthService, ClienteAuthService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<INotificationService, NotificationService>();
            
            // HTTP Client para servicios de Google
            services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();

            return services;
        }

        /// <summary>
        /// Configura los servicios de caché de stock
        /// </summary>
        public static IServiceCollection AddStockCaching(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar opciones del caché de stock
            services.Configure<StockCacheOptions>(configuration.GetSection(StockCacheOptions.SectionName));
            
            // Configurar IMemoryCache con las opciones de la configuración
            services.AddMemoryCache(options =>
            {
                var stockCacheConfig = configuration.GetSection(StockCacheOptions.SectionName).Get<StockCacheOptions>() ?? new StockCacheOptions();
                options.SizeLimit = stockCacheConfig.MaxCacheSize;
                options.CompactionPercentage = stockCacheConfig.CompactionPercentage;
            });
            
            return services;
        }
    }
}