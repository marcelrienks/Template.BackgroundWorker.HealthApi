using System;
using System.Threading;

namespace HealthApi.Interfaces
{
    public interface IHealthHandler
    {
        bool Liveness { get; set; }
        ManualResetEventSlim PrestopShutDown { get; set; }
        bool Readiness { get; set; }
        ManualResetEventSlim SigtermShutDown { get; set; }

        void Dispose();
        void Initialise(CancellationToken stoppingToken);
        bool IsApplicationCancelled();
        void ProcessExitEvent(object sender, EventArgs e);
        void SetApplicationShutDown();
        void SetPreStopCancelToken();
        void SetSigTermCancelToken();
        void ThrowExceptionIfApplicationCancelled();
    }
}