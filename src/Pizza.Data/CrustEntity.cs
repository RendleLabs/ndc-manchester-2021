using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;

namespace Pizza.Data
{
    public class CrustEntity : TableEntity
    {
        public CrustEntity()
        {
            PartitionKey = "crust";
        }
        
        public CrustEntity(string id, string name, int size, decimal price, int stockCount) : this()
        {
            Id = id;
            Name = name;
            Size = size;
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
        public int Size { get; set; }
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