using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Hosting;
using Pizza.Data;

namespace Ingredients
{
    public class HealthCheckService : BackgroundService
    {
        private readonly IToppingData _toppingData;
        private readonly HealthServiceImpl _service;

        public HealthCheckService(IToppingData toppingData, HealthServiceImpl service)
        {
            service.SetStatus("IngredientsService", HealthCheckResponse.Types.ServingStatus.Serving);
            _toppingData = toppingData;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var x = await _toppingData.GetAsync(stoppingToken);
                    _service.SetStatus("IngredientsService", HealthCheckResponse.Types.ServingStatus.Serving);
                }
                catch
                {
                    _service.SetStatus("IngredientsService", HealthCheckResponse.Types.ServingStatus.NotServing);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}