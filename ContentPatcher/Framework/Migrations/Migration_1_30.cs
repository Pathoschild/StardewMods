using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.30.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_30 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_30()
            : base(new SemanticVersion(1, 30, 0)) { }
    }
}
