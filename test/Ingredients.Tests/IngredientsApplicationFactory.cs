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
        
        public IToppingData ToppingDataSub { get; private set; }

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

                ToppingDataSub = Substitute.For<IToppingData>();
                
                ToppingDataSub.GetAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(toppingEntities));

                services.AddSingleton(ToppingDataSub);
                
                var crustEntities = new List<CrustEntity>
                {
                    new CrustEntity("thin9", "Thin", 9, 2.5m, 100),
                    new CrustEntity("deep9", "Deep", 9, 3m, 100),
                };

                var crustDataSub = Substitute.For<ICrustData>();
                
                crustDataSub.GetAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(crustEntities));

                services.AddSingleton(crustDataSub);
            });
            base.ConfigureWebHost(builder);
        }
    }
}