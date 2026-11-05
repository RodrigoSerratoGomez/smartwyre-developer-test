using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Incentives;

public interface IIncentiveCalculator
{
    IncentiveType Type { get; }
    bool CanCalculate(Rebate rebate, Product product, CalculateRebateRequest request);
    decimal Calculate(Rebate rebate, Product product, CalculateRebateRequest request);
}
