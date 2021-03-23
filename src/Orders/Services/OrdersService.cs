using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Orders.Protos;
using Orders.PubSub;

namespace Orders.Services
{
    public class OrdersService : Protos.OrdersService.OrdersServiceBase
    {
        private readonly IngredientsService.IngredientsServiceClient _ingredients;
        private readonly IOrderPublisher _orderPublisher;
        private readonly IOrderMessages _orderMessages;
        private readonly ILogger<OrdersService> _logger;

        public OrdersService(IngredientsService.IngredientsServiceClient ingredients,
            IOrderPublisher orderPublisher,
            IOrderMessages orderMessages,
            ILogger<OrdersService> logger)
        {
            _ingredients = ingredients;
            _orderPublisher = orderPublisher;
            _orderMessages = orderMessages;
            _logger = logger;
        }

        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            var now = DateTimeOffset.UtcNow;

            await _orderPublisher.PublishOrder(request.CrustId, request.ToppingIds, now);
            
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
            
            return new PlaceOrderResponse
            {
                Time = now.ToTimestamp(),
            };
        }

        public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<SubscribeResponse> responseStream, ServerCallContext context)
        {
            var cancellationToken = context.CancellationToken;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var orderMessage = await _orderMessages.ReadAsync(cancellationToken);
                    var response = new SubscribeResponse
                    {
                        CrustId = orderMessage.CrustId,
                        ToppingIds = {orderMessage.ToppingIds},
                        Time = orderMessage.Time.ToTimestamp(),
                    };
                    await responseStream.WriteAsync(response);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Subscriber disconnected.");
                    break;
                }
            }
        }
    }
}