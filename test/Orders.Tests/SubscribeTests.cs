using System;
using System.Threading;
using System.Threading.Tasks;
using Orders.Protos;
using Xunit;

namespace Orders.Tests
{
    public class SubscribeTests : IClassFixture<OrdersApplicationFactory>
    {
        private readonly OrdersApplicationFactory _factory;

        public SubscribeTests(OrdersApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact(Skip = "Everything is horrible")]
        public async Task Subscribes()
        {
            var client = _factory.CreateGrpcClient();

            SubscribeResponse actual;
            using (var call = client.Subscribe(new SubscribeRequest()))
            {
                var movedNext = await call.ResponseStream.MoveNext(CancellationToken.None);
                Assert.True(movedNext);
                actual = call.ResponseStream.Current;
            }

            Assert.Equal("thin", actual.CrustId);
        }
    }
}
