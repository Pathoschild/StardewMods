using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;

namespace ContentPatcher.Framework
{
    /// <summary>Internal constant values.</summary>
    internal static class InternalConstants
    {
        /*********
        ** Fields
        *********/
        /// <summary>The character used as a separator between the token name and positional input arguments.</summary>
        public const string PositionalInputArgSeparator = ":";

        /// <summary>The character used as a separator between the token name (or positional input arguments) and named input arguments.</summary>
        public const string NamedInputArgSeparator = "|";

        /// <summary>The character used as a separator between the mod ID and token name for a mod-provided token.</summary>
        public const string ModTokenSeparator = "/";

        /// <summary>A prefix for player names when specified as an input argument.</summary>
        public const string PlayerNamePrefix = "@";

        /// <summary>A temporary value assigned to input arguments during early patch loading, before the patch is updated with the context values.</summary>
        public const string TokenPlaceholder = "$~placeholder";

        /// <summary>The tokens which depend on the <see cref="PatchConfig.FromFile"/> field.</summary>
        public static readonly ISet<ConditionType> FromFileTokens = new HashSet<ConditionType> { ConditionType.FromFile };

        /// <summary>The tokens which depend on the <see cref="PatchConfig.Target"/> field.</summary>
        public static readonly ISet<ConditionType> TargetTokens = new HashSet<ConditionType> { ConditionType.Target, ConditionType.TargetPathOnly, ConditionType.TargetWithoutPath };
    }
}
