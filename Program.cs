using BackgroundWorker.Workers;
using HealthApi.Handlers;
using HealthApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Template
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .ConfigureServices(services =>
            {
                // General Dependancy Injection registrations
                services.AddSingleton<IHealthHandler, HealthHandler>();

                // Create backend worker service
                services.AddHostedService<ExampleWorker>();
            });
    }
}
