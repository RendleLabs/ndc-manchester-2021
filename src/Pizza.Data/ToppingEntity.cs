using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;

namespace Pizza.Data
{
    public class ToppingEntity : TableEntity
    {
        public ToppingEntity()
        {
            PartitionKey = "toppings";
        }
        
        public ToppingEntity(string id, string name, decimal price, int stockCount) : this()
        {
            Id = id;
            Name = name;
            Price = price;
            StockCount = stockCount;
        }

        [IgnoreProperty]
        public string Id
        {
            get => RowKey;
            set => RowKey = value;
        }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockCount { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            if (properties.TryGetValue(nameof(Price), out var priceProperty))
            {
                Price = Convert.ToDecimal(priceProperty.DoubleValue);
            }
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var dict = base.WriteEntity(operationContext);
            dict[nameof(Price)] = EntityProperty.GeneratePropertyForDouble(Convert.ToDouble(Price));
            return dict;
        }
    }
}
