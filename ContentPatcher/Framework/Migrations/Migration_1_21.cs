using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations;

/// <summary>Migrates patches to format version 1.21.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
internal class Migration_1_21 : BaseMigration
{
    /*********
    ** Fields
    *********/
    /// <summary>Handles parsing raw strings into tokens.</summary>
    private readonly Lexer Lexer = Lexer.Instance;

    /// <summary>Literal token strings to ignore when validating use of the <see cref="ConditionType.Render"/> token, since they were added by this migration.</summary>
    private readonly HashSet<string> IgnoreRenderStrings = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public Migration_1_21()
        : base(new SemanticVersion(1, 21, 0)) { }

    /// <inheritdoc />
    public override bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryMigrateMainContent(content, out error))
            return false;

        // 1.21 adds CustomLocations
        if (content.CustomLocations.Any())
        {
            error = this.GetNounPhraseError($"using the {nameof(content.CustomLocations)} field");
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryMigrate(ref patches, out error))
            return false;

        // validate patch changes
        foreach (PatchConfig patch in patches)
        {
            // 1.21 adds AddWarps
            if (patch.AddWarps.Any())
            {
                error = this.GetNounPhraseError($"using the {nameof(patch.AddWarps)} field");
                return false;
            }

            // 1.21 drops support for tokens in the 'Enabled' field
            // This converts them to 'When' conditions for backwards compatibility.
            if (!string.IsNullOrWhiteSpace(patch.Enabled))
            {
                ILexToken[] bits = this.Lexer.ParseBits(patch.Enabled, impliedBraces: false, trim: true).Take(2).ToArray();
                if (bits.Length == 1 && bits[0].Type == LexTokenType.Token)
                {
                    string renderStr = this.NormalizeLexicalString($"{ConditionType.Render}:{bits[0].ToString()}", impliedBraces: true);
                    this.IgnoreRenderStrings.Add(renderStr);

                    patch.When[renderStr] = "true";
                    patch.Enabled = null;
                }
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryMigrate(ref lexToken, out error))
            return false;

        // 1.21 adds the 'Render' token
        if (this.IsCustomRender(lexToken))
        {
            error = this.GetNounPhraseError($"using token {ConditionType.Render}");
            return false;
        }

        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get whether a lexical token represents a custom <see cref="ConditionType.Render"/> token usage, excluding <see cref="IgnoreRenderStrings"/>.</summary>
    /// <param name="lexToken">The token to check.</param>
    private bool IsCustomRender(ILexToken lexToken)
    {
        return
            lexToken is LexTokenToken token
            && token.Name.EqualsIgnoreCase(nameof(ConditionType.Render))
            && !this.IgnoreRenderStrings.Contains(lexToken.ToString());
    }

    /// <summary>Get the normalized representation of a lexical token string.</summary>
    /// <param name="str">The lexical token string to parse.</param>
    /// <param name="impliedBraces">Whether we're parsing a token context (so the outer '{{' and '}}' are implied); else parse as a tokenizable string which main contain a mix of literal and {{token}} values.</param>
    private string NormalizeLexicalString(string str, bool impliedBraces)
    {
        var bits = this.Lexer.ParseBits(str, impliedBraces);
        return string.Join("", bits.Select(p => p.ToString()));
    }
}
