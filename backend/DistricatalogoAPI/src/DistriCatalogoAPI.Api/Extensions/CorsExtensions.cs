namespace DistriCatalogoAPI.Api.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .SetIsOriginAllowed(origin =>
                        {
                            if (origin == null) return false;
                            try
                            {
                                var uri = new Uri(origin);
                                return uri.Host == "districatalogo.com" 
                                    || uri.Host.EndsWith(".districatalogo.com")
                                    || uri.Host == "dulceysaladomax.com"
                                    || uri.Host.EndsWith(".dulceysaladomax.com")
                                    || uri.Host == "localhost"
                                    || uri.Host == "127.0.0.1";
                            }
                            catch
                            {
                                return false;
                            }
                        })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            });

            return services;
        }

        public static IApplicationBuilder UseCorsLogging(this IApplicationBuilder app, IConfiguration configuration)
        {
            if (app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() || 
                configuration.GetValue<bool>("EnableCorsLogging", false))
            {
                app.Use(async (context, next) =>
                {
                    var origin = context.Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrEmpty(origin) && context.Request.Path.StartsWithSegments("/api"))
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation($"CORS Request - Origin: {origin}, Method: {context.Request.Method}, Path: {context.Request.Path}");
                    }
                    await next();
                });
            }

            return app;
        }
    }
}