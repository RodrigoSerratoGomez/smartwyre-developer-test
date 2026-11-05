using System;
using System.Globalization;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    // Usage: dotnet run --project Smartwyre.DeveloperTest.Runner -- <rebateId> <productId> <volume>
    // Missing arguments are asked for. Sample data: rebates CASH10, RATE5, UOM2; products P1, P2.
    static int Main(string[] args)
    {
        var rebateIdentifier = args.Length > 0 ? args[0] : Prompt("Rebate identifier (CASH10, RATE5, UOM2)");
        var productIdentifier = args.Length > 1 ? args[1] : Prompt("Product identifier (P1, P2)");
        var volumeText = args.Length > 2 ? args[2] : Prompt("Volume");

        if (!decimal.TryParse(volumeText, NumberStyles.Number, CultureInfo.InvariantCulture, out var volume) || volume < 0)
        {
            Console.Error.WriteLine($"Invalid volume: '{volumeText}'. Use a non-negative number, e.g. 3 or 2.5");
            return 1;
        }

        // Composition root: wired by hand. In a real app this would be a DI container registration.
        var rebateDataStore = new DemoRebateDataStore();
        var rebateService = new RebateService(
            rebateDataStore,
            new DemoProductDataStore(),
            new IIncentiveCalculator[]
            {
                new FixedCashAmountCalculator(),
                new FixedRateRebateCalculator(),
                new AmountPerUomCalculator(),
            });

        var result = rebateService.Calculate(new CalculateRebateRequest
        {
            RebateIdentifier = rebateIdentifier,
            ProductIdentifier = productIdentifier,
            Volume = volume,
        });

        if (!result.Success)
        {
            Console.WriteLine("Rebate could not be calculated for this request.");
            return 2;
        }

        Console.WriteLine($"Rebate calculated and stored: {rebateDataStore.LastStoredAmount}");
        return 0;
    }

    private static string Prompt(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }
}