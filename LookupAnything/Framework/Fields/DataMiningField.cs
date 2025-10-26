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
        const string indent = "*";

        string? sectionName = null;
        for (int i = 0, last = values.Length - 1; i <= last; i++)
        {
            IDataMinedValue entry = values[i];

            // section label
            if (entry.Section != sectionName)
            {
                sectionName = entry.Section;

                if (i > 0)
                    yield return new FormattedText(Environment.NewLine);

                yield return new FormattedText($"{entry.Section}:{Environment.NewLine}", bold: true);
            }

            // indent
            if (sectionName != null)
                yield return new FormattedText(indent, color: Color.Transparent);

            // label
            yield return new FormattedText("*", Color.Red, bold: true);
            yield return new FormattedText($"{entry.Label}: ");

            // value
            if (!string.IsNullOrWhiteSpace(entry.Value))
            {
                string value = entry.Value;

                if (sectionName != null && entry.Value.Contains(Environment.NewLine))
                {
                    string[] parts = value.Split(Environment.NewLine);
                    for (int partIndex = 0; partIndex < parts.Length; partIndex++)
                    {
                        if (partIndex > 0)
                            yield return new FormattedText(indent + indent, color: Color.Transparent);

                        yield return new FormattedText(
                            partIndex < parts.Length - 1
                                ? $"{parts[partIndex]}{Environment.NewLine}"
                                : parts[partIndex]
                        );
                    }
                }
                else
                    yield return new FormattedText(value);
            }

            // line break between values
            if (i != last)
                yield return new FormattedText(Environment.NewLine);
        }
    }
}
