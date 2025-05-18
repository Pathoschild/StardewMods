using NUnit.Framework;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.Tests.Common.CommonTests;

/// <summary>Unit tests for <see cref="CommonHelper"/>.</summary>
[TestFixture]
class CommonHelperTests
{
    /*********
    ** Unit tests
    *********/
    /****
    ** GetFormattedPercentageNumber
    ****/
    [Test(Description = $"Assert that {nameof(CommonHelper.GetFormattedPercentageNumber)} snaps out-of-range values to 0 or 100.")]
    [TestCase(-1f, ExpectedResult = "0")]
    [TestCase(0f, ExpectedResult = "0")]
    [TestCase(1f, ExpectedResult = "100")]
    [TestCase(2f, ExpectedResult = "100")]
    public string GetFormattedPercentageNumber_OutOfRangeValues_SnapToValidValue(float chance)
    {
        return CommonHelper.GetFormattedPercentageNumber(chance);
    }

    [Test]
    [TestCase(0.021f, ExpectedResult = "2")]
    [TestCase(0.025f, ExpectedResult = "3")]
    [TestCase(0.1f, ExpectedResult = "10")]
    [TestCase(0.5f, ExpectedResult = "50")]
    [TestCase(0.99f, ExpectedResult = "99")]
    [TestCase(0.995f, ExpectedResult = "100")]
    public string GetFormattedPercentageNumber_2PercentOrMore_RoundsToNearestInteger(float chance)
    {
        return CommonHelper.GetFormattedPercentageNumber(chance);
    }

    [Test]
    [TestCase(0.01f, ExpectedResult = "1")]
    [TestCase(0.014f, ExpectedResult = "1.4")]
    [TestCase(0.015f, ExpectedResult = "1.5")]
    [TestCase(0.0194f, ExpectedResult = "1.9")]
    [TestCase(0.0195f, ExpectedResult = "2")]
    public string GetFormattedPercentageNumber_Between1And2Percent_RoundsToOneDecimal(float chance)
    {
        return CommonHelper.GetFormattedPercentageNumber(chance);
    }

    [Test]
    [TestCase(0.000_01f, ExpectedResult = "0.001")]
    [TestCase(0.000_000_014f, ExpectedResult = "0.000001")]
    [TestCase(0.000_000_015f, ExpectedResult = "0.000002")]
    [TestCase(0.000_000_000_000_000_000_000_000_000_15f, ExpectedResult = "0.00000000000000000000000002")]
    public string GetFormattedPercentageNumber_Below1Percent_RoundsToFirstSignificantDigit(float chance)
    {
        return CommonHelper.GetFormattedPercentageNumber(chance);
    }
}
