using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.Protos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Orders.PubSub;
using TestHelpers;

namespace Orders.Tests
{
    public class OrdersApplicationFactory : WebApplicationFactory<Startup>
    {
        public OrdersService.OrdersServiceClient CreateGrpcClient()
        {
            var channel = this.CreateGrpcChannel();
            return new OrdersService.OrdersServiceClient(channel);
        }
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IOrderMessages>();

                int count = 0;

                var message = new OrderMessage
                {
                    CrustId = "thin",
                    ToppingIds = new[] {"sauce"},
                    Time = DateTimeOffset.UtcNow
                };

                var completionSource = new TaskCompletionSource<OrderMessage>();

                var orderMessagesSub = Substitute.For<IOrderMessages>();
                orderMessagesSub.ReadAsync(Arg.Any<CancellationToken>())
                    .Returns(_ => ++count == 1
                        ? new ValueTask<OrderMessage>(message)
                        : new ValueTask<OrderMessage>(completionSource.Task));

                services.AddSingleton(orderMessagesSub);
            });
            base.ConfigureWebHost(builder);
        }
    }
}