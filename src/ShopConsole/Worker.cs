using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Protos;

namespace ShopConsole
{
    public class Worker : BackgroundService
    {
        private readonly OrdersService.OrdersServiceClient _client;
        private readonly ILogger<Worker> _logger;

        public Worker(OrdersService.OrdersServiceClient client, ILogger<Worker> logger)
        {
            _client = client;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting for orders...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new SubscribeRequest();
                    var call = _client.Subscribe(request);

                    await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                    {
                        var crust = response.CrustId;
                        var toppings = string.Join(", ", response.ToppingIds);
                        var time = response.Time.ToDateTimeOffset();
                        Console.WriteLine($"{time:T} {crust} {toppings}");
                    }
                }
                catch (OperationCanceledException)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Worker stopping");
                        break;
                    }
                }
            }
        }
    }
}
