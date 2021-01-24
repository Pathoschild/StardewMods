using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The parsed schema and value for a field in the <c>config.json</c> file.</summary>
    internal class ConfigField
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The values to allow.</summary>
        public InvariantHashSet AllowValues { get; }

        /// <summary>The default values if the field is missing or (if <see cref="AllowBlank"/> is <c>false</c>) blank.</summary>
        public InvariantHashSet DefaultValues { get; }

        /// <summary>Whether to allow blank values.</summary>
        public bool AllowBlank { get; }

        /// <summary>Whether the player can specify multiple values for this field.</summary>
        public bool AllowMultiple { get; }

        /// <summary>The value read from the player settings.</summary>
        public InvariantHashSet Value { get; set; }

        /// <summary>An optional explanation of the config field for players.</summary>
        public string Description { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="allowValues">The values to allow.</param>
        /// <param name="defaultValues">The default values if the field is missing or (if <paramref name="allowBlank"/> is <c>false</c>) blank.</param>
        /// <param name="allowBlank">Whether to allow blank values.</param>
        /// <param name="allowMultiple">Whether the player can specify multiple values for this field.</param>
        /// <param name="description">An optional explanation of the config field for players.</param>
        public ConfigField(InvariantHashSet allowValues, InvariantHashSet defaultValues, bool allowBlank, bool allowMultiple, string description)
        {
            this.AllowValues = allowValues;
            this.DefaultValues = defaultValues;
            this.AllowBlank = allowBlank;
            this.AllowMultiple = allowMultiple;
            this.Description = description;
        }

        /// <summary>Get whether the field represents a boolean value.</summary>
        public bool IsBoolean()
        {
            return
                this.AllowValues.Count == 2
                && this.AllowValues.Contains(true.ToString())
                && this.AllowValues.Contains(false.ToString());
        }

        /// <summary>Get whether the field consists of a consecutive numeric range.</summary>
        /// <param name="min">The parsed minimum.</param>
        /// <param name="max">The parsed maximum.</param>
        public bool IsNumericRange(out int min, out int max)
        {
            // parse numeric values
            var parsedValues = new List<int>(this.AllowValues.Count);
            foreach (string value in this.AllowValues)
            {
                if (!int.TryParse(value, out int parsed))
                {
                    min = -1;
                    max = -1;
                    return false;
                }

                parsedValues.Add(parsed);
            }

            // check for consecutive range
            parsedValues.Sort(Comparer<int>.Default);
            min = parsedValues.First();
            max = parsedValues.Last();
            return max - min == parsedValues.Count - 1; // each value is unique and guaranteed integer, so can check the number of increments between min and max
        }
    }
}
