using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using FluentAssertions;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher
{
    /// <summary>Unit tests for <see cref="TokenStringBuilder"/>.</summary>
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class TokenString_And_TokenStringBuilderTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Test that the <see cref="TokenStringBuilder"/> constructor sets the expected property values when given a string not containing any tokens.</summary>
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("boop")]
        [TestCase("  assets/boop.png  ")]
        public void TokenStringBuilder_PlainString(string raw)
        {
            // act
            TokenStringBuilder builder = new TokenStringBuilder(raw, new InvariantDictionary<ConfigField>());
            TokenString tokenStr = builder.Build();

            // assert builder
            Assert.AreEqual(raw, builder.RawValue, $"{nameof(TokenStringBuilder)}.{nameof(TokenStringBuilder.RawValue)} should match the input string.");
            Assert.AreEqual(0, builder.ConditionTokens.Count, $"{nameof(TokenStringBuilder)}.{nameof(TokenStringBuilder.ConditionTokens)} should have 0 tokens.");
            Assert.AreEqual(0, builder.ConfigTokens.Count, $"{nameof(TokenStringBuilder)}.{nameof(TokenStringBuilder.ConfigTokens)} should have 0 tokens.");
            Assert.AreEqual(0, builder.InvalidTokens.Count, $"{nameof(TokenStringBuilder)}.{nameof(TokenStringBuilder.InvalidTokens)} should have 0 tokens.");
            Assert.AreEqual(false, builder.HasAnyTokens, $"{nameof(TokenStringBuilder)}.{nameof(TokenStringBuilder.HasAnyTokens)} should be false.");

            // assert token string
            Assert.AreEqual(raw, tokenStr.Raw, $"{nameof(TokenString)}.{nameof(TokenString.Value)} should match the input string.");
            Assert.AreEqual(0, tokenStr.ConditionTokens.Count, $"{nameof(TokenString)}.{nameof(TokenString.ConditionTokens)} should have 0 tokens.");
        }

        /// <summary>Test that the <see cref="TokenStringBuilder"/> constructor sets the expected property values when given a string containing config, condition, and invalid values.</summary>
        [TestCase]
        public void TokenStringBuilder_WithTokens()
        {
            // arrange
            const string configKey = "configKey";
            const string configValue = "A";
            const string raw = "  assets/{{configKey}}_{{season}}_{{invalid}}.png  ";
            
            // act
            TokenStringBuilder builder = new TokenStringBuilder(raw, new InvariantDictionary<ConfigField>
            {
                [configKey] = new ConfigField(allowValues: new InvariantHashSet(configValue), allowBlank: false, allowMultiple: false, defaultValues: new InvariantHashSet { configValue }) { Value = new InvariantHashSet(configValue) }
            });
            TokenString tokenStr = builder.Build();

            // assert builder
            builder.RawValue.Should().Be(raw);
            builder.ConditionTokens.Should().HaveCount(1).And.BeEquivalentTo(ConditionKey.Season);
            builder.ConfigTokens.Should().HaveCount(1).And.BeEquivalentTo(configKey);
            builder.InvalidTokens.Should().HaveCount(1).And.BeEquivalentTo("invalid");
            builder.HasAnyTokens.Should().BeTrue();

            // assert token string
            tokenStr.Raw.Should().Be(raw.Replace("{{" + configKey + "}}", configValue), $"the config token should be substituted when the {nameof(TokenString)} is built");
            tokenStr.ConditionTokens.Should().HaveCount(1).And.BeEquivalentTo(ConditionKey.Season);
        }
    }
}
