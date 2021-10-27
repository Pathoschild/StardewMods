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
                ConditionType.PathPart.ToString()
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            // 1.24 changes input arguments for player tokens
            if (lexToken is LexTokenToken token && Enum.TryParse(token.Name, ignoreCase: true, out ConditionType type) && this.IsPlayerToken(type) && token.HasInputArgs())
            {
                var input = new InputArguments(new LiteralString(token.InputArgs.ToString(), new LogPathBuilder()));

                // can't use player ID before 1.24
                if (long.TryParse(input.PositionalArgs.FirstOrDefault(), out _))
                {
                    error = this.GetNounPhraseError($"using {type} token with a player ID");
                    return false;
                }
            }

            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a token type takes a <see cref="PlayerType"/> input argument.</summary>
        /// <param name="type">The condition type.</param>
        private bool IsPlayerToken(ConditionType type)
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
    }
}
