using System;
using System.Threading.Tasks;
using Grpc.Core;
using Orders.Protos;

namespace Orders.Services
{
    public class OrdersService : Protos.OrdersService.OrdersServiceBase
    {
        private readonly IngredientsService.IngredientsServiceClient _ingredients;

        public OrdersService(IngredientsService.IngredientsServiceClient ingredients)
        {
            _ingredients = ingredients;
        }

        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            var now = DateTimeOffset.UtcNow;
            var decrementCrustsRequest = new DecrementCrustsRequest
            {
                CrustIds = {request.CrustId}
            };
            await _ingredients.DecrementCrustsAsync(decrementCrustsRequest);

            var decrementToppingsRequest = new DecrementToppingsRequest
            {
                ToppingIds = {request.ToppingIds}
            };
            await _ingredients.DecrementToppingsAsync(decrementToppingsRequest);
            
            return new PlaceOrderResponse();
        }
    }
}