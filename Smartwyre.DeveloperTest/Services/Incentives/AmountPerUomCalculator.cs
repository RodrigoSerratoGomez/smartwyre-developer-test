using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Incentives;

public class AmountPerUomCalculator : IIncentiveCalculator
{
    public IncentiveType Type => IncentiveType.AmountPerUom;

    public bool CanCalculate(Rebate rebate, Product product, CalculateRebateRequest request)
    {
        return product.SupportedIncentives.HasFlag(SupportedIncentiveType.AmountPerUom)
            && rebate.Amount != 0
            && request.Volume != 0;
    }

    public decimal Calculate(Rebate rebate, Product product, CalculateRebateRequest request)
    {
        return rebate.Amount * request.Volume;
    }
}
