using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The parsed schema and value for a field in the <c>config.json</c> file.</summary>
    internal class ConfigField
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The values to allow.</summary>
        public IImmutableSet<string> AllowValues { get; }

        /// <summary>The default values if the field is missing or (if <see cref="AllowBlank"/> is <c>false</c>) blank.</summary>
        public IImmutableSet<string> DefaultValues { get; }

        /// <summary>Whether to allow blank values.</summary>
        public bool AllowBlank { get; }

        /// <summary>Whether the player can specify multiple values for this field.</summary>
        public bool AllowMultiple { get; }

        /// <summary>The value read from the player settings.</summary>
        public IImmutableSet<string> Value { get; private set; }

        /// <summary>An optional explanation of the config field for players.</summary>
        public string? Description { get; }

        /// <summary>An optional section key to group related fields.</summary>
        public string? Section { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="allowValues">The values to allow.</param>
        /// <param name="defaultValues">The default values if the field is missing or (if <paramref name="allowBlank"/> is <c>false</c>) blank.</param>
        /// <param name="value">The value read from the player settings.</param>
        /// <param name="allowBlank">Whether to allow blank values.</param>
        /// <param name="allowMultiple">Whether the player can specify multiple values for this field.</param>
        /// <param name="description">An optional explanation of the config field for players.</param>
        /// <param name="section">An optional section key to group related fields.</param>
        public ConfigField(IImmutableSet<string>? allowValues, IImmutableSet<string>? defaultValues, IImmutableSet<string>? value, bool allowBlank, bool allowMultiple, string? description, string? section)
        {
            this.AllowValues = allowValues ?? ImmutableSets.Empty;
            this.DefaultValues = defaultValues ?? ImmutableSets.Empty;
            this.Value = value ?? ImmutableSets.Empty;
            this.AllowBlank = allowBlank;
            this.AllowMultiple = allowMultiple;
            this.Description = description;
            this.Section = section;
        }

        /// <summary>Get whether the field represents a boolean value.</summary>
        public bool IsBoolean()
        {
            if (object.ReferenceEquals(this.AllowValues, ImmutableSets.Boolean))
                return true;

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

        /// <summary>Override the config value.</summary>
        /// <param name="value">The config value to set.</param>
        public void SetValue(IImmutableSet<string> value)
        {
            this.Value = value;
        }
    }
}
