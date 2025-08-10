using DistriCatalogoAPI.Api.Extensions;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure Serilog
    builder.ConfigureSerilog();

    // Add services to the container
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddSecurityServices();
    builder.Services.AddAuthenticationServices(builder.Configuration);
    builder.Services.AddCorsServices();
    builder.Services.AddApiDocumentation();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseApplicationMiddleware(app.Environment);
    
    // Map controllers (debe ir después del middleware pipeline)
    app.MapControllers();
    
    // Map API documentation (Scalar)
    app.MapApiDocumentation(app.Environment);

    Log.Information("DistriCatalogo API starting up...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}