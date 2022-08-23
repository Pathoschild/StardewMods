using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>A record of the values under which a patch was previously indexed.</summary>
    internal class IndexedPatchValues
    {
        /// <summary>The patch target, as returned by <see cref="IPatch.TargetAsset"/>.</summary>
        public IAssetName? Target { get; set; }

        /// <summary>The tokens used directly by the patch, as returned by <see cref="IContextualInfo.GetTokensUsed"/>. Any token aliases are not resolved.</summary>
        public IInvariantSet RawTokens { get; set; } = InvariantSet.Empty;

        /// <summary>The tokens used by the patch both directly and indirectly, with token aliases resolved.</summary>
        public IInvariantSet ResolvedTokens { get; set; } = InvariantSet.Empty;
    }
}
