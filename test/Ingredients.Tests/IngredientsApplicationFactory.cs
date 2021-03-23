using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ingredients.Protos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Pizza.Data;
using TestHelpers;

namespace Ingredients.Tests
{
    public class IngredientsApplicationFactory : WebApplicationFactory<Startup>
    {
        public IngredientsService.IngredientsServiceClient CreateGrpcClient()
        {
            var channel = this.CreateGrpcChannel();
            return new IngredientsService.IngredientsServiceClient(channel);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IToppingData>();

                var toppingEntities = new List<ToppingEntity>
                {
                    new ToppingEntity("cheese", "Cheese", 0.5m, 50),
                    new ToppingEntity("tomato", "Tomato", 0.75m, 100),
                };

                var toppingDataSub = Substitute.For<IToppingData>();
                toppingDataSub.GetAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(toppingEntities));

                services.AddSingleton(toppingDataSub);
            });
            base.ConfigureWebHost(builder);
        }
    }
}