# My notes on this refactor — Rodrigo Serrato

## What I found first
Before changing anything, I read the original `RebateService` line by line. A few things stood out:

- The `switch` reads `rebate.Incentive` **before** checking if `rebate` is null. So if the rebate doesn't exist, it crashes, and the null checks inside the `switch` never run.
- The fixed-cash branch never checks if the product is null, so that's a second crash.
- The service creates its own data stores with `new`, so there was no way to test it without a database.
- The runner and the test projects didn't reference the main project. It only built because nothing was using it yet.
- The data stores always return empty objects, so the runner could never get a real result with them.

## What I changed, and why
- **The service receives what it needs.** `RebateService` gets the two data stores and the calculators through the constructor. That's what makes it testable: in the tests I pass in-memory fakes instead of a database.
- **One calculator per incentive type.** Each calculator answers two questions: can I calculate this request (`CanCalculate`), and how much is it (`Calculate`). The service picks the right one from a dictionary by incentive type.
- **So adding a new incentive is a new class plus registering it.** The service doesn't change. That was the main goal in the README, because the business wants to add many more types.
- **I kept the rules exactly as they were.** Same validations (non-zero values and the supported-incentive flag) and the same formulas. The only behaviour change is that the null cases now fail cleanly instead of crashing.

## Something I had to be careful with
In the `switch`, the conditions were the reasons to **fail** (`== 0` → `Success = false`). In `CanCalculate` they have to say when it's **OK** to calculate, so every condition gets flipped (`!= 0`). It's an easy mistake because it still compiles. When I reviewed the service before committing, I also found a missing `!` in front of `CanCalculate`, which would have stored exactly the requests it should reject.

To be sure the tests protect against this, I flipped one condition on purpose: four tests went red (two calculator tests and two service tests), and I put it back.

## How to run it
```
dotnet test
dotnet run --project Smartwyre.DeveloperTest.Runner -- CASH10 P1 3
```
If you leave out the arguments, the runner asks for them. Sample data: rebates `CASH10`, `RATE5`, `UOM2` and products `P1`, `P2`.

## Tests
21 tests. Each calculator is tested on its formula and on every reason to say no (zero values, unsupported incentive). The service is tested for a missing rebate, a missing product, no calculator for that type, a calculator that rejects the request, the happy path that stores the right amount, and two calculators claiming the same type.

## What I'd do with more time
- Return the amount and the reason it failed in `CalculateRebateResult`. Right now it only says `Success`.
- Register the calculators in a DI container and add logging.
- Use the `RebateCalculation` type, which exists but nothing uses.
- Add `[Flags]` to `SupportedIncentiveType`.

## How I used AI
I used Copilot for autocompletion, and an AI assistant to help me write the unit tests and the console runner and to review my code before each commit. The design, the service and the calculators are mine, and I read every line before committing it. I treat AI output the same way I'd review a pull request from a teammate.

---

# Smartwyre Developer Test Instructions

You have been selected to complete our candidate coding exercise. Please follow the directions in this readme.

Clone, **DO NOT FORK**, this repository to your account on the online Git resource of your choosing (GitHub, BitBucket, GitLab, etc.). Your solution should retain previous commit history and you should utilize best practices for committing your changes to the repository.

You are welcome to use whatever tools you normally would when coding — including documentation, libraries, frameworks, or AI tools (such as ChatGPT or Copilot).

However, it is important that you fully understand your solution. As part of the interview process, we will review your code with you in detail. You should be able to:

- Explain the design choices you made.
- Walk us through how your solution works.
- Make modifications or extensions to your code during the review.

Please note: if your submission appears to have been generated entirely by an AI agent or another third party, without your own understanding or contribution, it will not meet our evaluation criteria.

# The Exercise

In the 'RebateService.cs' file you will find a method for calculating a rebate. At a high level the steps for calculating a rebate are:

 1. Lookup the rebate that the request is being made against.
 2. Lookup the product that the request is being made against.
 2. Check that the rebate and request are valid to calculate the incentive type rebate.
 3. Store the rebate calculation.

What we'd like you to do is refactor the code with the following things in mind:

 - Adherence to SOLID principles
 - Testability
 - Readability
 - Currently there are 3 known incentive types. In the future the business will want to add many more incentive types. Your solution should make it easy for developers to add new incentive types in the future.

We’d also like you to 
 - Add some unit tests to the Smartwyre.DeveloperTest.Tests project to show how you would test the code that you’ve produced 
 - Run the RebateService from the Smartwyre.DeveloperTest.Runner console application accepting inputs (either via command line arguments or via prompts is fine)

The only specific "rules" are:

- The solution must build
- All tests must pass

You are free to use any frameworks/NuGet packages that you see fit. You should plan to spend around 1 hour completing the exercise.

Feel free to use code comments to describe your changes. You are also welcome to update this readme with any important details for us to consider.

Once you have completed the exercise either ensure your repository is available publicly or contact the hiring manager to set up a private share.
