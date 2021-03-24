using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Ingredients.Protos;
using Microsoft.Extensions.Logging;
using Pizza.Data;

namespace Ingredients
{
    internal class IngredientsService : Protos.IngredientsService.IngredientsServiceBase
    {
        private readonly IToppingData _toppingData;
        private readonly ICrustData _crustData;
        private readonly ILogger<IngredientsService> _logger;

        public IngredientsService(IToppingData toppingData, ICrustData crustData, ILogger<IngredientsService> logger)
        {
            _toppingData = toppingData;
            _crustData = crustData;
            _logger = logger;
        }

        public override async Task<GetToppingsResponse> GetToppings(GetToppingsRequest request, ServerCallContext context)
        {
            try
            {
                var toppings = await _toppingData.GetAsync(context.CancellationToken);
                var availableToppings = toppings.Select(t =>
                    new AvailableTopping
                    {
                        Quantity = t.StockCount,
                        Topping = new Topping
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Price = t.Price
                        }
                    });

                var response = new GetToppingsResponse
                {
                    Toppings = {availableToppings}
                };
                return response;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error: {ex.Message}");
                throw new RpcException(new Status(StatusCode.Unknown, ex.Message));
            }
        }

        public override async Task<GetCrustsResponse> GetCrusts(GetCrustsRequest request, ServerCallContext context)
        {
            try
            {
                var crusts = await _crustData.GetAsync(context.CancellationToken);
                var availableCrusts = crusts.Select(t =>
                    new AvailableCrust
                    {
                        Quantity = t.StockCount,
                        Crust = new Crust
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Size = t.Size,
                            Price = t.Price
                        }
                    });

                var response = new GetCrustsResponse
                {
                    Crusts = {availableCrusts}
                };

                return response;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error: {ex.Message}");
                throw;
            }
        }

        public override async Task<DecrementToppingsResponse> DecrementToppings(DecrementToppingsRequest request, ServerCallContext context)
        {
            var tasks = request.ToppingIds
                .Select(id => _toppingData.DecrementStockAsync(id));
            await Task.WhenAll(tasks);
            return new DecrementToppingsResponse();
        }

        public override async Task<DecrementCrustsResponse> DecrementCrusts(DecrementCrustsRequest request, ServerCallContext context)
        {
            var tasks = request.CrustIds
                .Select(id => _crustData.DecrementStockAsync(id));
            await Task.WhenAll(tasks);
            return new DecrementCrustsResponse();
        }
    }
}