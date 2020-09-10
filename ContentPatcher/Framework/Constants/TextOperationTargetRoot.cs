using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Constants
{
    /// <summary>An allowed root value for a text operation target.</summary>
    internal enum TextOperationTargetRoot
    {
        /// <summary>An entry for an <see cref="PatchType.EditData"/> patch.</summary>
        Entries,

        /// <summary>A field for an <see cref="PatchType.EditData"/> patch.</summary>
        Fields,

        /// <summary>A map property for an <see cref="PatchType.EditMap"/> patch.</summary>
        MapProperties
    }
}
