using System.Diagnostics.CodeAnalysis;

namespace Pathoschild.Stardew.LookupAnything.Framework.DataMinedValues;

/// <summary>A raw data mined value.</summary>
internal interface IDataMinedValue
{
    /*********
    ** Accessors
    *********/
    /// <summary>A short name for the value.</summary>
    string Label { get; }

    /// <summary>The value to display.</summary>
    string? Value { get; }

    /// <summary>Whether the value should be displayed.</summary>
    [MemberNotNullWhen(true, nameof(IDataMinedValue.Value))]
    bool HasValue { get; }

    /// <summary>Whether the value should be highlighted for special attention.</summary>
    bool IsPinned { get; }

    /// <summary>The name of the parent field which contains this data mined value, if it shouldn't be in the general 'debug' field.</summary>
    public string? OverrideCategory { get; set; }
}
