using System;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class RebateServiceTests
{
    private readonly FakeRebateDataStore _rebates = new();
    private readonly FakeProductDataStore _products = new();

    private RebateService CreateService(params IIncentiveCalculator[] calculators) =>
        new(_rebates, _products, calculators.Length > 0 ? calculators : new IIncentiveCalculator[]
        {
            new FixedCashAmountCalculator(),
            new FixedRateRebateCalculator(),
            new AmountPerUomCalculator(),
        });

    private static CalculateRebateRequest Request() =>
        new() { RebateIdentifier = "R1", ProductIdentifier = "P1", Volume = 3 };

    [Fact]
    public void Fails_WhenRebateDoesNotExist()
    {
        _products.Product = new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount };

        var result = CreateService().Calculate(Request());

        Assert.False(result.Success);
        Assert.False(_rebates.Stored);
    }

    [Fact]
    public void Fails_WhenProductDoesNotExist()
    {
        _rebates.Rebate = new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 10 };

        var result = CreateService().Calculate(Request());

        Assert.False(result.Success);
        Assert.False(_rebates.Stored);
    }

    [Fact]
    public void Fails_WhenNoCalculatorIsRegisteredForTheIncentive()
    {
        _rebates.Rebate = new Rebate { Incentive = IncentiveType.AmountPerUom, Amount = 2 };
        _products.Product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };

        var result = CreateService(new FixedCashAmountCalculator()).Calculate(Request());

        Assert.False(result.Success);
        Assert.False(_rebates.Stored);
    }

    [Fact]
    public void Fails_WhenCalculatorRejectsTheRequest()
    {
        _rebates.Rebate = new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 0 };
        _products.Product = new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount };

        var result = CreateService().Calculate(Request());

        Assert.False(result.Success);
        Assert.False(_rebates.Stored);
    }

    [Theory]
    [InlineData(IncentiveType.FixedCashAmount, SupportedIncentiveType.FixedCashAmount, 10)]
    [InlineData(IncentiveType.FixedRateRebate, SupportedIncentiveType.FixedRateRebate, 15)]  // 100 * 0.05 * 3
    [InlineData(IncentiveType.AmountPerUom, SupportedIncentiveType.AmountPerUom, 30)]        // 10 * 3
    public void Succeeds_AndStoresTheAmount(IncentiveType incentive, SupportedIncentiveType supported, decimal expected)
    {
        _rebates.Rebate = new Rebate { Incentive = incentive, Amount = 10, Percentage = 0.05m };
        _products.Product = new Product { SupportedIncentives = supported, Price = 100 };

        var result = CreateService().Calculate(Request());

        Assert.True(result.Success);
        Assert.True(_rebates.Stored);
        Assert.Equal(expected, _rebates.StoredAmount);
    }

    [Fact]
    public void Constructor_Throws_WhenTwoCalculatorsHandleTheSameIncentive()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateService(new FixedCashAmountCalculator(), new FixedCashAmountCalculator()));
    }
}