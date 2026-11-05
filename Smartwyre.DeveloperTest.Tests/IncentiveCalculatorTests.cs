using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class IncentiveCalculatorTests
{
    private static CalculateRebateRequest Request(decimal volume) => new() { Volume = volume };

    [Theory]
    [InlineData(10, true)]
    [InlineData(0, false)]
    public void FixedCashAmount_CanCalculate_RequiresNonZeroAmount(decimal amount, bool expected)
    {
        var calculator = new FixedCashAmountCalculator();
        var rebate = new Rebate { Amount = amount };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount };

        Assert.Equal(expected, calculator.CanCalculate(rebate, product, Request(3)));
    }

    [Fact]
    public void FixedCashAmount_Calculate_ReturnsAmountRegardlessOfVolume()
    {
        var result = new FixedCashAmountCalculator().Calculate(new Rebate { Amount = 10 }, new Product(), Request(3));
        Assert.Equal(10m, result);
    }

    [Theory]
    [InlineData(0.05, 100, 3, true)]
    [InlineData(0, 100, 3, false)]
    [InlineData(0.05, 0, 3, false)]
    [InlineData(0.05, 100, 0, false)]
    public void FixedRateRebate_CanCalculate_RequiresAllFactorsNonZero(double percentage, double price, double volume, bool expected)
    {
        var calculator = new FixedRateRebateCalculator();
        var rebate = new Rebate { Percentage = (decimal)percentage };
        var product = new Product { Price = (decimal)price, SupportedIncentives = SupportedIncentiveType.FixedRateRebate };

        Assert.Equal(expected, calculator.CanCalculate(rebate, product, Request((decimal)volume)));
    }

    [Fact]
    public void FixedRateRebate_Calculate_IsPriceTimesPercentageTimesVolume()
    {
        var result = new FixedRateRebateCalculator().Calculate(
            new Rebate { Percentage = 0.05m }, new Product { Price = 100 }, Request(3));
        Assert.Equal(15m, result);
    }

    [Theory]
    [InlineData(2, 3, true)]
    [InlineData(0, 3, false)]
    [InlineData(2, 0, false)]
    public void AmountPerUom_CanCalculate_RequiresAmountAndVolume(double amount, double volume, bool expected)
    {
        var calculator = new AmountPerUomCalculator();
        var rebate = new Rebate { Amount = (decimal)amount };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };

        Assert.Equal(expected, calculator.CanCalculate(rebate, product, Request((decimal)volume)));
    }

    [Fact]
    public void AmountPerUom_Calculate_IsAmountTimesVolume()
    {
        var result = new AmountPerUomCalculator().Calculate(new Rebate { Amount = 2 }, new Product(), Request(3));
        Assert.Equal(6m, result);
    }

    [Fact]
    public void CanCalculate_IsFalse_WhenProductDoesNotSupportTheIncentive()
    {
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };
        Assert.False(new FixedCashAmountCalculator().CanCalculate(new Rebate { Amount = 10 }, product, Request(3)));
    }
}