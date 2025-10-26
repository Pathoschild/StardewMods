using System.Globalization;

namespace Pathoschild.Stardew.LookupAnything.Framework.DataMinedValues;

/// <summary>A raw data mined value which is pinned in the menu.</summary>
internal class PinnedDataMinedValue : GenericDataMinedValue
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label"><inheritdoc cref="IDataMinedValue.Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="IDataMinedValue.Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public PinnedDataMinedValue(string label, string? value, bool? hasValue = null)
        : base(I18n.DataMining_SectionPinned(), label, value, hasValue) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="label"><inheritdoc cref="IDataMinedValue.Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="IDataMinedValue.Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public PinnedDataMinedValue(string label, int value, bool? hasValue = null)
        : base(I18n.DataMining_SectionPinned(), label, value.ToString(CultureInfo.InvariantCulture), hasValue) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="label"><inheritdoc cref="IDataMinedValue.Label" path="/summary"/></param>
    /// <param name="value"><inheritdoc cref="IDataMinedValue.Value" path="/summary"/></param>
    /// <param name="hasValue">Whether the value should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public PinnedDataMinedValue(string label, float value, bool? hasValue = null)
        : base(I18n.DataMining_SectionPinned(), label, value.ToString(CultureInfo.InvariantCulture), hasValue) { }
}
