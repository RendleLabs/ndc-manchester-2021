using System.Threading.Tasks;
using Ingredients.Protos;
using Xunit;

namespace Ingredients.Tests
{
    public class CrustsTests : IClassFixture<IngredientsApplicationFactory>
    {
        private IngredientsApplicationFactory _factory;

        public CrustsTests(IngredientsApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetsCrusts()
        {
            var client = _factory.CreateGrpcClient();

            var response = await client.GetCrustsAsync(new GetCrustsRequest());
            
            Assert.Collection(response.Crusts,
                c => Assert.Equal("thin9", c.Crust.Id),
                c => Assert.Equal("deep9", c.Crust.Id)
            );
        }
    }
}