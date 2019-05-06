using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using FluentAssertions;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher
{
    /// <summary>Unit tests for <see cref="TokenString"/>.</summary>
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class TokenStringTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Test that the <see cref="TokenString"/> constructor sets the expected property values when given a string not containing any tokens.</summary>
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("boop")]
        [TestCase("  assets/boop.png  ")]
        public void TokenStringBuilder_PlainString(string raw)
        {
            // act
            TokenString tokenStr = new TokenString(raw, new GenericTokenContext());

            // assert
            tokenStr.Raw.Should().Be(raw.Trim());
            tokenStr.GetTokenPlaceholders(recursive: false).Should().HaveCount(0);
            tokenStr.InvalidTokens.Should().HaveCount(0);
            tokenStr.HasAnyTokens.Should().BeFalse();
        }

        /// <summary>Test that the <see cref="TokenString"/> constructor sets the expected property values when given a single valid token.</summary>
        [TestCase("{{tokenKey}}", "{{tokenKey}}")]
        [TestCase("  {{tokenKey}}   ", "{{tokenKey}}")]
        [TestCase("  {{  tokenKey  }}   ", "{{tokenKey}}")]
        public void TokenStringBuilder_WithSingleToken(string raw, string parsed)
        {
            // arrange
            const string configKey = "tokenKey";
            var context = new GenericTokenContext();
            context.Save(new ImmutableToken(configKey, new InvariantHashSet { "value" }));

            // act
            TokenString tokenStr = new TokenString(raw, context);

            // assert
            tokenStr.Raw.Should().Be(parsed);
            tokenStr.GetTokenPlaceholders(recursive: false).Should().HaveCount(1);
            tokenStr.GetTokenPlaceholders(recursive: false).Select(p => p.Text).Should().BeEquivalentTo("{{" + configKey + "}}");
            tokenStr.InvalidTokens.Should().BeEmpty();
            tokenStr.HasAnyTokens.Should().BeTrue();
            tokenStr.IsSingleTokenOnly.Should().BeTrue();
        }

        /// <summary>Test that the <see cref="TokenString"/> constructor sets the expected property values when given a string containing config, condition, and invalid values.</summary>
        [TestCase]
        public void TokenStringBuilder_WithTokens()
        {
            // arrange
            const string configKey = "configKey";
            const string configValue = "A";
            const string tokenKey = "season";
            const string raw = "  assets/{{configKey}}_{{season}}_{{ invalid  }}.png  ";
            var context = new GenericTokenContext();
            context.Save(new ImmutableToken(configKey, new InvariantHashSet { configValue }));
            context.Save(new ImmutableToken(tokenKey, new InvariantHashSet { "A" }));

            // act
            TokenString tokenStr = new TokenString(raw, context);

            // assert
            tokenStr.Raw.Should().Be("assets/{{configKey}}_{{season}}_{{invalid}}.png");
            tokenStr.GetTokenPlaceholders(recursive: false).Should().HaveCount(3);
            tokenStr.GetTokenPlaceholders(recursive: false).Select(name => name.Text).Should().BeEquivalentTo(new[] { "{{configKey}}", "{{season}}", "{{invalid}}" });
            tokenStr.InvalidTokens.Should().HaveCount(1).And.BeEquivalentTo("invalid");
            tokenStr.HasAnyTokens.Should().BeTrue();
            tokenStr.IsSingleTokenOnly.Should().BeFalse();
        }
    }
}
