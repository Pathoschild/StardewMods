using System.Diagnostics.CodeAnalysis;

namespace Pathoschild.Stardew.LookupAnything.Framework.DataMinedValues;

/// <summary>A raw data mined value.</summary>
internal interface IDataMinedValue
{
    /*********
    ** Accessors
    *********/
    /// <summary>A translated section header with which to group related fields, if any.</summary>
    string? Section { get; }

    /// <summary>A short name for the value.</summary>
    string Label { get; }

    /// <summary>The value to display.</summary>
    string? Value { get; }

    /// <summary>Whether the value should be displayed.</summary>
    [MemberNotNullWhen(true, nameof(IDataMinedValue.Value))]
    bool HasValue { get; }

    /// <summary>The name of the parent field which contains this data mined value, if it shouldn't be in the general 'debug' field.</summary>
    public string? ParentFieldName { get; set; }
}
