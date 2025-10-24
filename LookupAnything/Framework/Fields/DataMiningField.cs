using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.DataMinedValues;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>Shows a collection of data mined values.</summary>
internal class DataMiningField : GenericField
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="values">The data mined values to display.</param>
    public DataMiningField(string label, IEnumerable<IDataMinedValue>? values)
        : base(label)
    {
        IDataMinedValue[] valuesArray = values?.ToArray() ?? [];
        this.HasValue = valuesArray.Any();
        if (this.HasValue)
            this.Value = this.GetFormattedText(valuesArray).ToArray();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a formatted representation for a set of data mined values.</summary>
    /// <param name="values">The data mined values to display.</param>
    private IEnumerable<IFormattedText> GetFormattedText(IDataMinedValue[] values)
    {
        for (int i = 0, last = values.Length - 1; i <= last; i++)
        {
            IDataMinedValue entry = values[i];
            yield return new FormattedText("*", Color.Red, bold: true);
            yield return new FormattedText($"{entry.Label}:");
            yield return i != last
                ? new FormattedText($"{entry.Value}{Environment.NewLine}")
                : new FormattedText(entry.Value);
        }
    }
}
