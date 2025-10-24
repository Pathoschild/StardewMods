using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Pathoschild.Stardew.LookupAnything.Framework.DataMinedValues;

/// <summary>A raw data mined value.</summary>
internal class GenericDataMinedValue : IDataMinedValue
{
    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Label { get; protected set; }

    /// <inheritdoc />
    public string? Value { get; protected set; }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(GenericDataMinedValue.Value))]
    public bool HasValue { get; protected set; }

    /// <inheritdoc />
    public bool IsPinned { get; protected set; }

    /// <inheritdoc />
    public string? OverrideCategory { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label"><inheritdoc cref="Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    /// <param name="pinned"><inheritdoc cref="pinned" path="/summary"/></param>
    public GenericDataMinedValue(string label, string? value, bool? hasValue = null, bool pinned = false)
    {
        this.Label = label;
        this.Value = value;
        this.HasValue = hasValue ?? !string.IsNullOrWhiteSpace(this.Value);
        this.IsPinned = pinned;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="label"><inheritdoc cref="Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    /// <param name="pinned"><inheritdoc cref="pinned" path="/summary"/></param>
    public GenericDataMinedValue(string label, int value, bool? hasValue = null, bool pinned = false)
        : this(label, value.ToString(CultureInfo.InvariantCulture), hasValue, pinned) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="label"><inheritdoc cref="Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    /// <param name="pinned"><inheritdoc cref="pinned" path="/summary"/></param>
    public GenericDataMinedValue(string label, float value, bool? hasValue = null, bool pinned = false)
        : this(label, value.ToString(CultureInfo.InvariantCulture), hasValue, pinned) { }
}
