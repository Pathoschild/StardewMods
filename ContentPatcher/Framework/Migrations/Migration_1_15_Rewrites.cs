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
        };

        /// <summary>The dynamic and config token names defined by the content pack.</summary>
        private readonly Lazy<ISet<string>> LocalTokenNames;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="content">The content pack being validated.</param>
        public Migration_1_15_Rewrites(ContentConfig content)
            : base(new SemanticVersion(1, 15, 0))
        {
            this.LocalTokenNames = new Lazy<ISet<string>>(() => this.GetLocalTokenNames(content));
        }

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public override bool TryMigrate(ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(lexToken, out error))
                return false;

            // migrate token input arguments
            if (lexToken is LexTokenToken token && token.HasInputArgs())
            {
                ConditionType? conditionType = this.GetConditionType(token.Name);

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

        /// <summary>Get the dynamic and config token names defined by a content pack.</summary>
        /// <param name="content">The content pack to read.</param>
        private ISet<string> GetLocalTokenNames(ContentConfig content)
        {
            InvariantHashSet names = new InvariantHashSet();

            // dynamic tokens
            if (content.DynamicTokens?.Any() == true)
            {
                foreach (string name in content.DynamicTokens.Select(p => p.Name))
                {
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name);
                }
            }

            // config schema
            if (content.ConfigSchema != null)
            {
                foreach (string name in content.ConfigSchema.Select(p => p.Key))
                {
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name);
                }
            }

            // exclude tokens that conflict with a built-in condition, which will be ignored
            names.RemoveWhere(p => Enum.TryParse(p, ignoreCase: true, out ConditionType _));

            return names;
        }
    }
}
