using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
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
            TokenString tokenStr = new TokenString(raw, new GenericTokenContext(modId => false), new LogPathBuilder("unit test"));
            IContextualState diagnosticState = tokenStr.GetDiagnosticState();

            // assert
            tokenStr.Raw.Should().Be(raw.Trim());
            tokenStr.GetTokenPlaceholders(recursive: false).Should().HaveCount(0);
            tokenStr.HasAnyTokens.Should().BeFalse();
            diagnosticState.IsReady.Should().BeTrue();
            diagnosticState.UnreadyTokens.Should().BeEmpty();
            diagnosticState.InvalidTokens.Should().BeEmpty();
            diagnosticState.Errors.Should().BeEmpty();
        }

        /// <summary>Test that the <see cref="TokenString"/> constructor sets the expected property values when given a single valid token.</summary>
        [TestCase("{{tokenKey}}", "{{tokenKey}}")]
        [TestCase("  {{tokenKey}}   ", "{{tokenKey}}")]
        [TestCase("  {{  tokenKey  }}   ", "{{tokenKey}}")]
        public void TokenStringBuilder_WithSingleToken(string raw, string parsed)
        {
            // arrange
            const string configKey = "tokenKey";
            var context = new GenericTokenContext(modId => false);
            context.Save(this.GetImmutableToken(configKey, "value"));

            // act
            TokenString tokenStr = new TokenString(raw, context, new LogPathBuilder("unit test"));
            IContextualState diagnosticState = tokenStr.GetDiagnosticState();

            // assert
            tokenStr.Raw.Should().Be(parsed);
            tokenStr.GetTokenPlaceholders(recursive: false).Should().HaveCount(1);
            tokenStr.GetTokenPlaceholders(recursive: false).Select(p => p.ToString()).Should().BeEquivalentTo("{{" + configKey + "}}");
            tokenStr.HasAnyTokens.Should().BeTrue();
            tokenStr.IsSingleTokenOnly.Should().BeTrue();
            diagnosticState.IsReady.Should().BeTrue();
            diagnosticState.UnreadyTokens.Should().BeEmpty();
            diagnosticState.InvalidTokens.Should().BeEmpty();
            diagnosticState.Errors.Should().BeEmpty();
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
            var context = new GenericTokenContext(modId => false);
            context.Save(this.GetImmutableToken(configKey, configValue));
            context.Save(this.GetImmutableToken(tokenKey, "A"));

            // act
            TokenString tokenStr = new TokenString(raw, context, new LogPathBuilder("unit test"));
            IContextualState diagnosticState = tokenStr.GetDiagnosticState();

            // assert
            tokenStr.Raw.Should().Be("assets/{{configKey}}_{{season}}_{{invalid}}.png");
            tokenStr.GetTokenPlaceholders(recursive: false).Should().HaveCount(3);
            tokenStr.GetTokenPlaceholders(recursive: false).Select(name => name.ToString()).Should().BeEquivalentTo(new[] { "{{configKey}}", "{{season}}", "{{invalid}}" });
            tokenStr.IsReady.Should().BeFalse();
            tokenStr.HasAnyTokens.Should().BeTrue();
            tokenStr.IsSingleTokenOnly.Should().BeFalse();
            diagnosticState.IsReady.Should().BeFalse();
            diagnosticState.UnreadyTokens.Should().BeEmpty();
            diagnosticState.InvalidTokens.Should().HaveCount(1).And.BeEquivalentTo("invalid");
            diagnosticState.Errors.Should().BeEmpty();
        }

        /// <summary>Get an immutable token whose value never changes.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="values">The token values.</param>
        private IToken GetImmutableToken(string name, params string[] values)
        {
            IValueProvider valueProvider = new ImmutableValueProvider(name, new InvariantHashSet(values));
            return new Token(valueProvider);
        }
    }
}
