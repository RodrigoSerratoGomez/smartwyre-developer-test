using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Tests;

public class FakeRebateDataStore : IRebateDataStore
{
    public Rebate Rebate { get; set; }
    public bool Stored { get; private set; }
    public decimal StoredAmount { get; private set; }

    public Rebate GetRebate(string rebateIdentifier) => Rebate;

    public void StoreCalculationResult(Rebate rebate, decimal rebateAmount)
    {
        Stored = true;
        StoredAmount = rebateAmount;
    }
}

public class FakeProductDataStore : IProductDataStore
{
    public Product Product { get; set; }
    public Product GetProduct(string productIdentifier) => Product;
}
