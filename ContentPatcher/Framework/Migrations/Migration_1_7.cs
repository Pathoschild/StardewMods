using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.7.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_7 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_7()
            : base(new SemanticVersion(1, 7, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.HasReadLetter),
                nameof(ConditionType.HasValue),
                nameof(ConditionType.IsCommunityCenterComplete),
                nameof(ConditionType.IsMainPlayer)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            // 1.7 adds nested tokens
            if (lexToken is LexTokenToken token && !token.Name.EqualsIgnoreCase("HasFile") && token.InputArgs?.Parts.Any(p => p.Type == LexTokenType.Token) == true)
            {
                error = this.GetNounPhraseError($"using nested tokens like '{lexToken}'");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.7 adds tokens in dynamic token values
            if (content.DynamicTokens.Any(p => p?.Value?.Contains("{{") == true))
            {
                error = this.GetNounPhraseError("using tokens in dynamic token values");
                return false;
            }

            // 1.7 adds tokens in field keys and condition values
            foreach (PatchConfig patch in content.Changes.WhereNotNull())
            {
                if (patch.Fields.Keys.Any(key => key.Contains("{{")))
                {
                    error = this.GetNounPhraseError("using tokens in field keys");
                    return false;
                }

                if (patch.When.Any(condition => condition.Value?.Contains("{{") == true && !condition.Value.ContainsIgnoreCase("HasFile")))
                {
                    error = this.GetNounPhraseError("using tokens in condition values");
                    return false;
                }
            }

            return true;
        }
    }
}
