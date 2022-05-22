using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.25.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_25 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_25()
            : base(new SemanticVersion(1, 25, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.AbsoluteFilePath),
                nameof(ConditionType.FormatAssetName),
                nameof(ConditionType.InternalAssetKey)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            // 1.25 adds 'AnyPlayer' player type
            if (lexToken is LexTokenToken token && Enum.TryParse(token.Name, ignoreCase: true, out ConditionType type) && this.IsPlayerToken(type) && token.HasInputArgs() && this.IsPlayerToken(type))
            {
                var input = new InputArguments(new LiteralString(token.InputArgs.ToString(), new LogPathBuilder()));
                foreach (string value in input.PositionalArgs)
                {
                    if (Enum.TryParse(value, ignoreCase: true, out PlayerType playerType) && playerType is PlayerType.AnyPlayer)
                    {
                        error = this.GetNounPhraseError($"using the '{PlayerType.AnyPlayer}' player type");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.25 is more forgiving about Format version
            if (content.Format!.PatchVersion != 0 || content.Format.PrereleaseTag != null)
            {
                error = this.GetNounPhraseError($"using {nameof(content.Format)} with a patch version (like {content.Format} instead of {new SemanticVersion(content.Format.MajorVersion, content.Format.MinorVersion, 0)})");
                return false;
            }

            // 1.25 adds TargetField
            foreach (PatchConfig patch in content.Changes.WhereNotNull())
            {
                if (patch.TargetField.Any())
                {
                    error = this.GetNounPhraseError($"using {nameof(patch.TargetField)}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>Get whether a token type accepted a <see cref="PlayerType"/> input argument before Content Patcher 1.24.</summary>
        /// <param name="type">The condition type.</param>
        private bool IsPlayerToken(ConditionType type)
        {
            return type
                is ConditionType.ChildGenders
                or ConditionType.ChildNames
                or ConditionType.DailyLuck
                or ConditionType.FarmhouseUpgrade
                or ConditionType.HasActiveQuest
                or ConditionType.HasCaughtFish
                or ConditionType.HasConversationTopic
                or ConditionType.HasDialogueAnswer
                or ConditionType.HasFlag
                or ConditionType.HasProfession
                or ConditionType.HasReadLetter
                or ConditionType.HasSeenEvent
                or ConditionType.IsMainPlayer
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
