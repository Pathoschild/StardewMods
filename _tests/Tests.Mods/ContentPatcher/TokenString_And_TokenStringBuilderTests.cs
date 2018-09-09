using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
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
            TokenStringBuilder builder = new TokenStringBuilder(raw, new InvariantDictionary<ConfigField>(), new InvariantDictionary<IToken>());
            TokenString tokenStr = builder.Build();

            // assert builder
            builder.RawValue.Should().Be(raw);
            builder.Tokens.Should().HaveCount(0);
            builder.ConfigTokens.Should().HaveCount(0);
            builder.InvalidTokens.Should().HaveCount(0);
            builder.HasAnyTokens.Should().BeFalse();

            // assert token string
            tokenStr.Raw.Should().Be(raw);
            tokenStr.Tokens.Should().HaveCount(0);
        }

        /// <summary>Test that the <see cref="TokenStringBuilder"/> constructor sets the expected property values when given a string containing config, condition, and invalid values.</summary>
        [TestCase]
        public void TokenStringBuilder_WithTokens()
        {
            // arrange
            const string configKey = "configKey";
            const string configValue = "A";
            const string tokenKey = "season";
            const string raw = "  assets/{{configKey}}_{{season}}_{{invalid}}.png  ";

            // act
            TokenStringBuilder builder = new TokenStringBuilder(
                rawValue: raw,
                config: new InvariantDictionary<ConfigField>
                {
                    [configKey] = new ConfigField(allowValues: new InvariantHashSet(configValue), allowBlank: false, allowMultiple: false, defaultValues: new InvariantHashSet { configValue }) { Value = new InvariantHashSet(configValue) }
                },
                tokens: new InvariantDictionary<IToken>
                {
                    [tokenKey] = new StaticToken(tokenKey, canHaveMultipleValues: false, values: new InvariantHashSet { "A" })
                }
            );
            TokenString tokenStr = builder.Build();

            // assert builder
            builder.RawValue.Should().Be(raw);
            builder.Tokens.Should().HaveCount(1).And.BeEquivalentTo(TokenKey.Season);
            builder.ConfigTokens.Should().HaveCount(1).And.BeEquivalentTo(configKey);
            builder.InvalidTokens.Should().HaveCount(1).And.BeEquivalentTo("invalid");
            builder.HasAnyTokens.Should().BeTrue();

            // assert token string
            tokenStr.Raw.Should().Be(raw.Replace("{{" + configKey + "}}", configValue), $"the config token should be substituted when the {nameof(TokenString)} is built");
            tokenStr.Tokens.Should().HaveCount(1).And.BeEquivalentTo(TokenKey.Season);
        }
    }
}
