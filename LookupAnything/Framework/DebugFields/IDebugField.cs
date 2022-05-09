using System.Diagnostics.CodeAnalysis;

namespace Pathoschild.Stardew.LookupAnything.Framework.DebugFields
{
    /// <summary>A debug field containing a raw datamining value.</summary>
    internal interface IDebugField
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A short field label.</summary>
        string Label { get; }

        /// <summary>The field value.</summary>
        string? Value { get; }

        /// <summary>Whether the field should be displayed.</summary>
        [MemberNotNullWhen(true, nameof(IDebugField.Value))]
        bool HasValue { get; }

        /// <summary>Whether the field should be highlighted for special attention.</summary>
        bool IsPinned { get; }

        /// <summary>The debug category text.</summary>
        public string? OverrideCategory { get; set; }
    }
}
