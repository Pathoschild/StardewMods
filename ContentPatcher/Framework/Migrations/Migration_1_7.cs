using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.7.</summary>
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
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.HasReadLetter.ToString(),
                ConditionType.HasValue.ToString(),
                ConditionType.IsCommunityCenterComplete.ToString(),
                ConditionType.IsMainPlayer.ToString()
            };
        }

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public override bool TryMigrate(ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(lexToken, out error))
                return false;

            // 1.7 adds nested tokens
            if (lexToken is LexTokenToken token && !token.Name.EqualsIgnoreCase("HasFile") && token.InputArgs?.Parts.Any(p => p.Type == LexTokenType.Token) == true)
            {
                error = this.GetNounPhraseError($"using nested tokens like '{lexToken}'");
                return false;
            }

            return true;
        }

        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the content pack was successfully migrated.</returns>
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.7 adds tokens in dynamic token values
            if (content.DynamicTokens != null)
            {
                if (content.DynamicTokens.Any(p => p.Value?.Contains("{{") == true))
                {
                    error = this.GetNounPhraseError("using tokens in dynamic token values");
                    return false;
                }
            }

            // 1.7 adds tokens in field keys and condition values
            if (content.Changes?.Any() == true)
            {
                foreach (PatchConfig patch in content.Changes)
                {
                    if (patch.Fields != null && patch.Fields.Keys.Any(key => key.Contains("{{")))
                    {
                        error = this.GetNounPhraseError("using tokens in field keys");
                        return false;
                    }

                    if (patch.When != null && patch.When.Any(condition => condition.Value?.Contains("{{") == true && !condition.Value.ContainsIgnoreCase("HasFile")))
                    {
                        error = this.GetNounPhraseError("using tokens in condition values");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
