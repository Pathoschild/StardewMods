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
    public string? Section { get; }

    /// <inheritdoc />
    public string Label { get; protected set; }

    /// <inheritdoc />
    public string? Value { get; protected set; }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(GenericDataMinedValue.Value))]
    public bool HasValue { get; protected set; }

    /// <inheritdoc />
    public string? ParentFieldName { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="section"><inheritdoc cref="Section" path="/summary"/></param>
    /// <param name="label"><inheritdoc cref="Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericDataMinedValue(string? section, string label, string? value, bool? hasValue = null)
    {
        this.Section = section;
        this.Label = label;
        this.Value = value;
        this.HasValue = hasValue ?? !string.IsNullOrWhiteSpace(this.Value);
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="section"><inheritdoc cref="Section" path="/summary"/></param>
    /// <param name="label"><inheritdoc cref="Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericDataMinedValue(string? section, string label, int value, bool? hasValue = null)
        : this(section, label, value.ToString(CultureInfo.InvariantCulture), hasValue) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="section"><inheritdoc cref="Section" path="/summary"/></param>
    /// <param name="label"><inheritdoc cref="Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericDataMinedValue(string? section, string label, float value, bool? hasValue = null)
        : this(section, label, value.ToString(CultureInfo.InvariantCulture), hasValue) { }
}
