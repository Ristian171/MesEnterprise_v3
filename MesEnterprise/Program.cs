using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting up application...");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .UseSerilog() // Using Serilog for logging
        .ConfigureWebHostDefaults(webBuilder => 
        {
            webBuilder.UseStartup<Startup>();

            // Log the URLs to which the application is listening
            webBuilder.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(5000); // Adjust this port and any other configuration
            });
            webBuilder.UseUrls("http://localhost:5000"); // Specify listening URLs here

            webBuilder.UseSerilog((context, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext());
        });
}