using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.24.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_24 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_24()
            : base(new SemanticVersion(1, 24, 0))
        {
            this.AddedTokens.AddMany(
                ConditionType.HasCookingRecipe.ToString(),
                ConditionType.HasCraftingRecipe.ToString(),
                ConditionType.LocationOwnerId.ToString(),
                ConditionType.Merge.ToString(),
                ConditionType.Roommate.ToString(),
                ConditionType.PathPart.ToString()
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            // ignore non-tokens
            if (lexToken is not LexTokenToken token || !Enum.TryParse(token.Name, ignoreCase: true, out ConditionType type))
                return true;

            // 1.24 changes input arguments for player tokens
            if (token.HasInputArgs())
            {
                bool wasPlayerToken = this.IsOldPlayerToken(type);
                bool isNewPlayerToken = !wasPlayerToken && this.IsNewPlayerToken(type);

                if (wasPlayerToken || isNewPlayerToken)
                {
                    var input = new InputArguments(new LiteralString(token.InputArgs.ToString(), new LogPathBuilder()));

                    // can't use player ID before 1.24
                    if (wasPlayerToken && long.TryParse(input.PositionalArgs.FirstOrDefault(), out _))
                    {
                        error = this.GetNounPhraseError($"using {type} token with a player ID");
                        return false;
                    }

                    // didn't accept input before 1.24
                    if (isNewPlayerToken && input.PositionalArgs.Any())
                    {
                        error = this.GetNounPhraseError($"using {type} token with input arguments");
                        return false;
                    }
                }
            }

            // 1.24 splits {{Roommate}} token out of {{Spouse}}
            if (type == ConditionType.Spouse)
            {
                lexToken = new LexTokenToken(
                    name: ConditionType.Merge.ToString(),
                    inputArgs: new LexTokenInput(new ILexToken[]
                    {
                        new LexTokenToken(ConditionType.Roommate.ToString(), null, impliedBraces: false),
                        new LexTokenLiteral(","),
                        new LexTokenToken(ConditionType.Spouse.ToString(), token.InputArgs, impliedBraces: false)
                    }),
                    impliedBraces: token.ImpliedBraces
                );
            }

            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a token type accepted a <see cref="PlayerType"/> input argument before Content Patcher 1.24.</summary>
        /// <param name="type">The condition type.</param>
        private bool IsOldPlayerToken(ConditionType type)
        {
            return type
                is ConditionType.DailyLuck
                or ConditionType.FarmhouseUpgrade
                or ConditionType.HasCaughtFish
                or ConditionType.HasConversationTopic
                or ConditionType.HasDialogueAnswer
                or ConditionType.HasFlag
                or ConditionType.HasProfession
                or ConditionType.HasReadLetter
                or ConditionType.HasSeenEvent
                or ConditionType.HasActiveQuest
                or ConditionType.ChildNames
                or ConditionType.ChildGenders;
        }

        /// <summary>Get whether Content Patcher 1.24 added support for specifying a player type for the token.</summary>
        /// <param name="type">The condition type.</param>
        private bool IsNewPlayerToken(ConditionType type)
        {
            return type
                is ConditionType.IsMainPlayer
                or ConditionType.IsOutdoors
                or ConditionType.LocationContext
                or ConditionType.LocationName
                or ConditionType.LocationUniqueName
                or ConditionType.PlayerGender
                or ConditionType.PlayerName
                or ConditionType.Spouse;
        }
    }
}
