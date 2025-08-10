using Serilog;
using Serilog.Events;

namespace DistriCatalogoAPI.Api.Extensions
{
    public static class SerilogExtensions
    {
        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            // Configure Serilog early
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "https://seq.districatalogo.com/",
                             apiKey: Environment.GetEnvironmentVariable("SEQ_API_KEY") ?? "API_KEY")
                .CreateLogger();

            // Replace built-in logging with Serilog and read configuration from appsettings.json
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId());
        }
    }
}