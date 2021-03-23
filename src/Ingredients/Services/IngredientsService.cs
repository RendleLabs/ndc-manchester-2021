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
        private readonly ILogger<IngredientsService> _logger;

        public IngredientsService(IToppingData toppingData, ILogger<IngredientsService> logger)
        {
            _toppingData = toppingData;
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
                            Price = (double) t.Price
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
                throw;
            }
        }
    }
}