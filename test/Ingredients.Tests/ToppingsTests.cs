using System;
using System.Threading;
using System.Threading.Tasks;
using Ingredients.Protos;
using NSubstitute;
using TestHelpers;
using Xunit;

namespace Ingredients.Tests
{
    public class ToppingsTests : IClassFixture<IngredientsApplicationFactory>
    {
        private IngredientsApplicationFactory _factory;

        public ToppingsTests(IngredientsApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetsToppings()
        {
            var client = _factory.CreateGrpcClient();

            var response = await client.GetToppingsAsync(new GetToppingsRequest());
            
            Assert.Collection(response.Toppings,
                t => Assert.Equal("cheese", t.Topping.Id),
                t => Assert.Equal("tomato", t.Topping.Id)
                );
        }

        [Fact]
        public async Task DecrementsToppings()
        {
            var client = _factory.CreateGrpcClient();
            var request = new DecrementToppingsRequest
            {
                ToppingIds = {"42"}
            };
            await client.DecrementToppingsAsync(request);
            await _factory.ToppingDataSub.Received()
                .DecrementStockAsync("42", Arg.Any<CancellationToken>());
        }
    }
}
