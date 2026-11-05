using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore _rebateDataStore;
    private readonly IProductDataStore _productDataStore;
    private readonly IReadOnlyDictionary<IncentiveType, IIncentiveCalculator> _calculators;

    public RebateService(
        IRebateDataStore rebateDataStore,
        IProductDataStore productDataStore,
        IEnumerable<IIncentiveCalculator> calculators)
    {
        _rebateDataStore = rebateDataStore ?? throw new ArgumentNullException(nameof(rebateDataStore));
        _productDataStore = productDataStore ?? throw new ArgumentNullException(nameof(productDataStore));
        ArgumentNullException.ThrowIfNull(calculators);
        _calculators = calculators.ToDictionary(c => c.Type);
    }

    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);
        if (rebate == null) return Failed();

        var product = _productDataStore.GetProduct(request.ProductIdentifier);
        if (product == null) return Failed();

        if (!_calculators.TryGetValue(rebate.Incentive, out var calculator)
            || !calculator.CanCalculate(rebate, product, request))
        {
            return Failed();
        }

        var rebateAmount = calculator.Calculate(rebate, product, request);
        _rebateDataStore.StoreCalculationResult(rebate, rebateAmount);

        return new CalculateRebateResult { Success = true };
    }

    private static CalculateRebateResult Failed() => new() { Success = false };
}
