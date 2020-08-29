using System.Threading.Tasks;
using HealthApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthApi.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class KubernetesHealthController : ControllerBase
    {
        private readonly ILogger<KubernetesHealthController> _logger;
        private readonly IHealthHandler _healthHandler;

        public KubernetesHealthController(ILogger<KubernetesHealthController> logger, IHealthHandler healthHandler)
        {
            _logger = logger;
            _healthHandler = healthHandler;
        }

        /// <summary>
        /// This GET should be called by the kubernetes liveness probe.
        /// Example:
        /// spec:
        ///   containers:
        ///     livenessProbe:
        ///       httpGet:
        ///         path: /api/health/liveness
        ///         port: 80
        ///       initialDelaySeconds: 3
        ///       periodSeconds: 3
        /// </summary>
        /// <returns>'200 OK' if background worker is live, else '406 Not Acceptable'</returns>
        [HttpGet]
        [Route("liveness")]
        public async Task<ActionResult> Liveness()
        {
            _logger.LogDebug("Liveness api check fired");

            return _healthHandler.Liveness ? await Task.Run(() => Ok(_healthHandler.Liveness)) : await Task.Run(() => StatusCode(406, false));
        }

        /// <summary>
        /// This GET should be called by the kubernetes readiness probe.
        /// Example:
        /// spec:
        ///   containers:
        ///     readinessProbe:
        ///       httpGet:
        ///         path: /api/health/readiness
        ///         port: 80
        ///       initialDelaySeconds: 3
        ///       periodSeconds: 3
        /// </summary>
        /// <returns>'200 OK' if background worker is ready, else '406 Not Acceptable'</returns>
        [HttpGet]
        [Route("readiness")]
        public async Task<ActionResult> Readiness()
        {
            _logger.LogDebug("Readiness api check fired");

            return _healthHandler.Readiness ? await Task.Run(() => Ok(_healthHandler.Readiness)) : await Task.Run(() => StatusCode(406, false));
        }

        /// <summary>
        /// This POST should be used by the kubernetes prestop hook
        /// </summary>
        /// Example:
        /// spec:
        ///   containers:
        ///     lifecycle:
        ///       preStop:
        ///         httpGet:
        ///           path: /api/health/readiness
        ///           port: 80
        /// <returns>'204 No Content' once the application has background worker has gracefully shut down</returns>
        [HttpGet]
        [Route("prestop")]
        public async Task<ActionResult> PreStop()
        {
            _logger.LogDebug("PreStopHook api call fired");

            _healthHandler.SetPreStopCancelToken();
            return await Task.Run(() => StatusCode(204));
        }
    }
}
