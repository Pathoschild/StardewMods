using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.15.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_15_Rewrites : BaseMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The names of tokens which dropped support for the {{token:search}} form in favor of the universal {{token |contains=search}} form.</summary>
        private readonly ISet<ConditionType> TokensWhichDroppedSearchForm = new HashSet<ConditionType>
        {
            // date and weather
            ConditionType.Day,
            ConditionType.DayEvent,
            ConditionType.DayOfWeek,
            ConditionType.DaysPlayed,
            ConditionType.Season,
            ConditionType.Year,
            ConditionType.Weather,

            // player
            ConditionType.HasFlag,
            ConditionType.HasReadLetter,
            ConditionType.HasSeenEvent,
            ConditionType.HasDialogueAnswer,
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
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_15_Rewrites()
            : base(new SemanticVersion(1, 15, 0)) { }

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public override bool TryMigrate(ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(lexToken, out error))
                return false;

            if (lexToken is LexTokenToken token && token.InputArgs != null)
            {
                ConditionType? conditionType = this.GetConditionType(token.Name);

                // 1.15 drops {{token:search}} form in favor of {{token |contains=search}}
                if (conditionType != null && this.TokensWhichDroppedSearchForm.Contains(conditionType.Value))
                {
                    var parts = new List<ILexToken>(token.InputArgs.Parts);

                    if (parts[0] is LexTokenLiteral literal)
                        literal.MigrateTo($"|contains={literal.Text}");
                    else
                        parts.Insert(0, new LexTokenLiteral("|contains="));

                    token.InputArgs.MigrateTo(parts.ToArray());
                }

                // 1.15 changes {{Random: choices | pinned-key}} to {{Random: choices |key=pinned-key}}
                if (conditionType == ConditionType.Random && token.InputArgs != null)
                {
                    LexTokenLiteral lexSeparator = token.InputArgs.Parts.OfType<LexTokenLiteral>().FirstOrDefault(p => p.ToString().Contains("|"));
                    if (lexSeparator != null && !Regex.IsMatch(lexSeparator.Text, @"\|\s*key\s*="))
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
        /// <summary>Get the condition type for a token name, if any.</summary>
        /// <param name="name">The token name.</param>
        private ConditionType? GetConditionType(string name)
        {
            if (Enum.TryParse(name, ignoreCase: true, out ConditionType type))
                return type;
            return null;
        }
    }
}
