using HealthApi.Interfaces;
using HealthApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace HealthApi.Handlers
{
    public class HealthHandler : IHealthHandler, IDisposable
    {
        private readonly ILogger<HealthHandler> _logger;
        private readonly IConfiguration _configuration;

        private CancellationTokenSource _preStopCancelTokenSource;
        private CancellationTokenSource _sigTermCancelTokenSource;
        private CancellationToken _stoppingCancelToken;

        public ManualResetEventSlim PrestopShutDown { get; set; }
        public ManualResetEventSlim SigtermShutDown { get; set; }

        public bool Liveness { get; set; }
        public bool Readiness { get; set; }

        public HealthHandler(ILogger<HealthHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _preStopCancelTokenSource = new CancellationTokenSource();
            _sigTermCancelTokenSource = new CancellationTokenSource();

            PrestopShutDown = new ManualResetEventSlim();
            SigtermShutDown = new ManualResetEventSlim();

            Liveness = true;
            Readiness = false;
        }

        /// <summary>
        /// Initialise the health handler
        /// </summary>
        /// <param name="stoppingToken">the stopping token from the worker service</param>
        public void Initialise(CancellationToken stoppingToken)
        {
            _stoppingCancelToken = stoppingToken;
        }

        /// <summary>
        /// Sets the PreStop cancellation token
        /// </summary>
        public void SetPreStopCancelToken()
        {
            _logger.LogDebug("Prestop caught, setting application cancellation token!");
            _preStopCancelTokenSource?.Cancel();

            // Because the PreStop hook call made by Kubernetes is blocking
            // Wait for the Application to set the shut down event, before responding
            PrestopShutDown.Wait(int.Parse(_configuration["AppSettings:Timeouts:PrestopWaitTimeoutMilliseconds"]));
        }

        /// <summary>
        /// Sets the SigTerm cancellation token
        /// </summary>
        public void SetSigTermCancelToken()
        {
            _logger.LogDebug("SigTerm caught, setting application cancellation token!");
            _sigTermCancelTokenSource?.Cancel();

            // Wait for the Application to set the shut down event, before responding
            SigtermShutDown.Wait(int.Parse(_configuration["AppSettings:Health:SigtermWaitTimeoutMilliseconds"]));
        }

        /// <summary>
        /// Sets that the application has shut down, to allow any blocked threads waiting the ability to respond
        /// </summary>
        public void SetApplicationShutDown()
        {
            PrestopShutDown?.Set();
            SigtermShutDown?.Set();
        }

        /// <summary>
        /// Check if an application cancellation token has been set for StoppingCancelToken, or PreStopCancelTokenSource
        /// </summary>
        /// <returns></returns>
        public bool IsApplicationCancelled()
        {
            return _stoppingCancelToken.IsCancellationRequested || _preStopCancelTokenSource.IsCancellationRequested || _sigTermCancelTokenSource.IsCancellationRequested;
        }

        /// <summary>
        /// If an application cancellation token has been set for StoppingCancelToken, or PreStopCancelTokenSource, throw an ApplicationCancelledException
        /// </summary>
        public void ThrowExceptionIfApplicationCancelled()
        {
            if (IsApplicationCancelled())
                throw new ApplicationCancelledException("Application cancellation flag found");
        }

        /// <summary>
        /// Event that will be triggered when the application catches a SigTerm signal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProcessExitEvent(object sender, EventArgs e)
        {
            _sigTermCancelTokenSource.Cancel();

            // Block waiting for the Application to shut down
            SigtermShutDown.Wait(int.Parse(_configuration["AppSettings:Health:SigtermWaitTimeoutMilliseconds"]));
        }

        public void Dispose()
        {
            _preStopCancelTokenSource?.Dispose();
            _sigTermCancelTokenSource?.Dispose();
        }
    }
}
