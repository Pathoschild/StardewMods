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
    /// <summary>Validates patches for format version 1.20.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_20 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_20()
            : base(new SemanticVersion(1, 20, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.LocationContext.ToString(),
                ConditionType.LocationUniqueName.ToString()
            };
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            string weatherName = ConditionType.Weather.ToString();

            // 1.20 adds input arguments for Weather token, and changes default from valley to current context
            if (lexToken is LexTokenToken token && token.Name.EqualsIgnoreCase(weatherName))
            {
                // can't add positional input args before 1.20
                if (token.HasInputArgs())
                {
                    var input = new InputArguments(new LiteralString(token.InputArgs.ToString(), new LogPathBuilder()));
                    if (input.HasPositionalArgs)
                    {
                        error = this.GetNounPhraseError($"using {weatherName} token with an input argument");
                        return false;
                    }
                }

                // default to valley
                var valleyArg = new[] { new LexTokenLiteral(LocationContext.Valley.ToString()) };
                lexToken = new LexTokenToken(
                    name: token.Name,
                    inputArgs: new LexTokenInput(
                        valleyArg.Concat(token.InputArgs?.Parts ?? new ILexToken[0]).ToArray()
                    ),
                    impliedBraces: true
                );
            }

            return true;
        }
    }
}
