using System;
using System.Collections.Generic;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner;

// In-memory stores with sample data: the real stores return empty objects, so they can never succeed.
public class DemoRebateDataStore : IRebateDataStore
{
    private readonly Dictionary<string, Rebate> _rebates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CASH10"] = new Rebate { Identifier = "CASH10", Incentive = IncentiveType.FixedCashAmount, Amount = 10m },
        ["RATE5"] = new Rebate { Identifier = "RATE5", Incentive = IncentiveType.FixedRateRebate, Percentage = 0.05m },
        ["UOM2"] = new Rebate { Identifier = "UOM2", Incentive = IncentiveType.AmountPerUom, Amount = 2m },
    };

    public decimal? LastStoredAmount { get; private set; }

    public Rebate GetRebate(string rebateIdentifier) =>
        _rebates.TryGetValue(rebateIdentifier ?? string.Empty, out var rebate) ? rebate : null;

    public void StoreCalculationResult(Rebate rebate, decimal rebateAmount) => LastStoredAmount = rebateAmount;
}

public class DemoProductDataStore : IProductDataStore
{
    private readonly Dictionary<string, Product> _products = new(StringComparer.OrdinalIgnoreCase)
    {
        ["P1"] = new Product
        {
            Identifier = "P1",
            Price = 100m,
            Uom = "unit",
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount | SupportedIncentiveType.FixedRateRebate,
        },
        ["P2"] = new Product
        {
            Identifier = "P2",
            Price = 50m,
            Uom = "kg",
            SupportedIncentives = SupportedIncentiveType.AmountPerUom,
        },
    };

    public Product GetProduct(string productIdentifier) =>
        _products.TryGetValue(productIdentifier ?? string.Empty, out var product) ? product : null;
}