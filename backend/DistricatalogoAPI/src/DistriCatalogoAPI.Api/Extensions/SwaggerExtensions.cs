using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace DistriCatalogoAPI.Api.Extensions
{
    public static class ApiDocumentationExtensions
    {
        public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            // Configuración moderna para OpenAPI y Scalar
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "DistriCatalogo API", 
                    Version = "v1.0.0",
                    Description = "API para gestión de catálogo de productos distribuidos con autenticación multi-tenant y sistema de clientes",
                    Contact = new OpenApiContact
                    {
                        Name = "DistriCatalogo Team",
                        Email = "soporte@districatalogo.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
                
                // JWT Authentication configuration para Scalar
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Configurar grupos por tags
                c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
                c.DocInclusionPredicate((name, api) => true);
            });

            return services;
        }

        public static IApplicationBuilder UseApiDocumentation(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // OpenAPI/Swagger JSON endpoint (necesario para Scalar)
                app.UseSwagger();
            }

            return app;
        }

        public static IEndpointRouteBuilder MapApiDocumentation(this IEndpointRouteBuilder endpoints, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Scalar API Documentation UI - configuración mejorada según documentación oficial
                endpoints.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("DistriCatalogo API")
                        .WithTheme(ScalarTheme.Default) // Cambiado a Default para mejor estabilidad
                        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
                        .WithModels(true)
                        .WithDownloadButton(true)
                        .WithSearchHotKey("k")
                        .WithOpenApiRoutePattern("/swagger/v1/swagger.json"); // Especificar la ruta del documento OpenAPI
                });
            }

            return endpoints;
        }
    }
}