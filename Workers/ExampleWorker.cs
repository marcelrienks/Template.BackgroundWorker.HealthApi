using HealthApi.Interfaces;
using HealthApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundWorker.Workers
{
    public class ExampleWorker : BackgroundService, IDisposable
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<ExampleWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHealthHandler _healthHandler;


        public ExampleWorker(IHostApplicationLifetime hostApplicationLifetime, ILogger<ExampleWorker> logger, IConfiguration configuration, IHealthHandler healthHandler)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            _configuration = configuration;
            _healthHandler = healthHandler;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ExampleWorker Startup fired");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Pass the stopping token to the health handler
                _healthHandler.Initialise(stoppingToken);

                do
                {
                    await DoWorkAsync();

                } while (!_healthHandler.IsApplicationCancelled());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExampleWorker Exception: {message}", ex.Message);
                //TODO: handle Exceptions
            }
            finally
            {
                _healthHandler.SetApplicationShutDown();

                //Time to let the controller in the API to respond to prestop hook call
                Thread.Sleep(500);
                _hostApplicationLifetime.StopApplication();
            }
        }

        /// <summary>
        /// This is where the actual work for the backround worker should be. 
        /// </summary>
        /// <returns></returns>
        private async Task DoWorkAsync()
        {
            try
            {
                // OPTIONALLY: Set the readiness flag to true, to allow kubernetes to send traffic to this application
                _healthHandler.Readiness = true;

                //TODO: Rename the method, and replace code below with actual logic...
                _logger.LogInformation("ExampleWorker iterative DoWork running...");
                await Task.Delay(3000);

                // OPTIONALLY: Set the readiness flag to false while a new iteration is being prepped, to prevent kubernetes from sending traffic to this application
                _healthHandler.Readiness = true;

                // OPTIONALLY: This method can be called if application should be shut down before all of the logic in the worker is complete
                // Or just let the this method end, and the loop from ExecuteAsync will handle shutting down application
                _healthHandler.ThrowExceptionIfApplicationCancelled();
            }
            catch (ApplicationCancelledException applicationCancelledException)
            {
                // Catch an application cancel request, log it, and then let the while loop exit normally and the shutdown run
                _logger.LogError(applicationCancelledException, applicationCancelledException.Message);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ExampleWorker Stop fired");

            return base.StopAsync(cancellationToken);
        }
    }
}
