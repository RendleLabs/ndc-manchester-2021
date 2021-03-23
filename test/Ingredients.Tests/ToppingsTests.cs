using System;
using System.Threading.Tasks;
using Ingredients.Protos;
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
    }
}
