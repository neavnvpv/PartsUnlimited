using PartsUnlimited.Models;

namespace PartsUnlimited;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Clear default configuration and add custom config
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddJsonFile("config.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddCommandLine(args);
        
        // Configure services using Startup class
        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);
        
        var app = builder.Build();
        
        // Seed database
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                await SampleData.InitializePartsUnlimitedDatabaseAsync(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
        }
        
        // Configure pipeline using Startup class
        startup.Configure(app, app.Environment);
        
        await app.RunAsync();
    }
}
