using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.API.HealthChecks;

public class StorageHealthCheck(string basePath) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            var testFile = Path.Combine(basePath, $".health-{Guid.NewGuid():N}");
            File.WriteAllText(testFile, "ok");
            File.Delete(testFile);

            return Task.FromResult(HealthCheckResult.Healthy("Storage is writable."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage is not writable.", ex));
        }
    }
}
