using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Health.V1;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Frontend
{
    public class IngredientsHealthCheck : IHealthCheck
    {
        private readonly Health.HealthClient _healthClient;

        public IngredientsHealthCheck(GrpcClientFactory clientFactory)
        {
            _healthClient = clientFactory.CreateClient<Health.HealthClient>("ingredients");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var response = await _healthClient.CheckAsync(new HealthCheckRequest(), cancellationToken: cancellationToken);
            if (response.Status == HealthCheckResponse.Types.ServingStatus.Serving)
            {
                return HealthCheckResult.Healthy();
            }
            return HealthCheckResult.Unhealthy();
        }
    }
}