using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

/// <summary>Migrates patches to format version 1.15.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
internal class Migration_1_15_Rewrites : BaseMigration
{
    /*********
    ** Fields
    *********/
    /// <summary>A pattern which matches a <c>{{Random}}</c> argument which was valid before 1.15.</summary>
    private static readonly Regex IgnoreRandomArgumentPattern = new(@"\|\s*key\s*=", RegexOptions.Compiled);

    /// <summary>The names of tokens which dropped support for the {{token:search}} form in favor of the universal {{token |contains=search}} form.</summary>
    private readonly HashSet<ConditionType> TokensWhichDroppedSearchForm =
    [
        // date and weather
        ConditionType.Day,
        ConditionType.DayEvent,
        ConditionType.DayOfWeek,
        ConditionType.DaysPlayed,
        ConditionType.Season,
        ConditionType.Year,
        ConditionType.Weather,

        // player
        ConditionType.HasDialogueAnswer,
        ConditionType.HasFlag,
        ConditionType.HasProfession,
        ConditionType.HasReadLetter,
        ConditionType.HasSeenEvent,
        ConditionType.HasWalletItem,
        ConditionType.IsMainPlayer,
        ConditionType.IsOutdoors,
        ConditionType.LocationName,
        ConditionType.PlayerGender,
        ConditionType.PlayerName,
        ConditionType.PreferredPet,

        // relationships
        ConditionType.Spouse,

        // world
        ConditionType.FarmCave,
        ConditionType.FarmhouseUpgrade,
        ConditionType.FarmName,
        ConditionType.FarmType,
        ConditionType.IsCommunityCenterComplete,
        ConditionType.IsJojaMartComplete,
        ConditionType.Pregnant,
        ConditionType.HavingChild,

        // metadata
        ConditionType.HasMod,
        ConditionType.Language
    ];

    /// <summary>The dynamic and config token names defined by the content pack.</summary>
    private readonly Lazy<IInvariantSet> LocalTokenNames;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="content">The content pack being validated, if any.</param>
    public Migration_1_15_Rewrites(ContentConfig? content)
        : base(new SemanticVersion(1, 15, 0))
    {
        this.LocalTokenNames = new Lazy<IInvariantSet>(() => this.GetLocalTokenNames(content));
    }

    /// <inheritdoc />
    public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryMigrate(ref lexToken, out error))
            return false;

        // migrate token input arguments
        if (lexToken is LexTokenToken token && token.HasInputArgs())
        {
            ConditionType? conditionType = this.GetEnum<ConditionType>(token.Name);

            // 1.15 drops {{token:search}} form in favor of {{token |contains=search}}
            if (this.LocalTokenNames.Value.Contains(token.Name) || (conditionType != null && this.TokensWhichDroppedSearchForm.Contains(conditionType.Value)))
            {
                var parts = new List<ILexToken>(token.InputArgs.Parts);

                if (parts[0] is LexTokenLiteral literal)
                    literal.MigrateTo($"|contains={literal.Text}");
                else
                    parts.Insert(0, new LexTokenLiteral("|contains="));

                token.InputArgs.MigrateTo(parts.ToArray());
            }

            // 1.15 changes {{Random: choices | pinned-key}} to {{Random: choices |key=pinned-key}}
            if (conditionType == ConditionType.Random)
            {
                LexTokenLiteral? lexSeparator = token.InputArgs.Parts.OfType<LexTokenLiteral>().FirstOrDefault(p => p.ToString().Contains("|"));
                if (lexSeparator != null && !IgnoreRandomArgumentPattern.IsMatch(lexSeparator.Text))
                {
                    int sepIndex = lexSeparator.Text.IndexOf("|", StringComparison.Ordinal);
                    string newText = lexSeparator.Text.Substring(0, sepIndex + 1) + "key=" + lexSeparator.Text.Substring(sepIndex + 1).TrimStart();

                    lexSeparator.MigrateTo(newText);
                }
            }
        }

        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the dynamic and config token names defined by a content pack.</summary>
    /// <param name="content">The content pack to read.</param>
    private IInvariantSet GetLocalTokenNames(ContentConfig? content)
    {
        if (content is null)
            return InvariantSet.Empty;

        MutableInvariantSet? names = null;

        // dynamic tokens
        foreach (string? name in content.DynamicTokens.Select(p => p.Name))
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                names ??= new();
                names.Add(name);
            }
        }

        // config schema
        foreach (string? name in content.ConfigSchema.Select(p => p.Key))
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                names ??= new();
                names.Add(name);
            }
        }

        // exclude tokens that conflict with a built-in condition, which will be ignored
        if (names != null)
        {
            foreach (string name in names.Where(p => this.GetEnum<ConditionType>(p) != null).ToArray())
                names.Remove(name);
        }

        return names?.Lock() ?? InvariantSets.Empty;
    }
}
