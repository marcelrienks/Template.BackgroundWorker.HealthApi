# Template BackgroundWorker HealthApi
This project is a template for a combination of a BackgroundWorker service, and a HealthApi that can be used to check on the status of the worker.
## Background Worker
The Background worker currently implements `Microsoft.Extensions.Hosting.BackgroundService`, but can be updated to implement other versions of workers.
## Health Api
The health Api is a rest api with various methods that can be implemented by various container systems.  
### Kubernetes:
Currently this template implements a health api for Kubernetes, specifically for the following probes and hooks.  
- **Liveness:**  
The liveness probe allows Kubernetes to know when the container is live imediately after starting, of this does not respond, kubernetes will restart the container.  
Example:  
    ```
    spec:
      containers:
        livenessProbe:
          httpGet:
            path: /api/health/liveness
            port: 80
          initialDelaySeconds: 3
          periodSeconds: 3
    ```
- **Readiness:**  
The Readiness probe allows Kubernetes to know when traffic can start to be sent to the container.  
Example:  
    ```
    spec:
      containers:
        readinessProbe:
          httpGet:
            path: /api/health/readiness
            port: 80
          initialDelaySeconds: 3
          periodSeconds: 3
    ```
- **Prestop:**  
Implementing this allows for a variable shutdown grace period.  
Kubernetes implements it's grace period 'strictly', meaning that whatever the grace period is set to, kubernetes will wait for that period to elapse before sending the 'SigKill', even if the application was able to, and did terminate it self early.
Therefore in order to allow for the woker to shut itself down in a variable amount of time (either at a point where it can safely do so, or at the end of it's opperation) we need to implement the 'PreStop Hook' (which Kubernetes fires just before 'SigTerm' and 'SigKill') and is a blocking call which allows the worker time to shut down before responding to the PreStop hook request, which will then complete it's lifecycle and depricate the container.  
Example:  
    ```
    spec:
      containers:
        lifecycle:
          preStop:
            httpGet:
              path: /api/health/readiness
              port: 80
    ```