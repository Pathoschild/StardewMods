using System.Linq;
using System.Runtime.Serialization;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a <see cref="PatchConfig.TextOperations"/> field.</summary>
    internal class TextOperationConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The text operation to perform.</summary>
        public string Operation { get; set; }

        /// <summary>The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</summary>
        public string[] Target { get; set; }

        /// <summary>The value to append or prepend.</summary>
        public string Value { get; set; }

        /// <summary>If the target field already has a value, text to add between the previous and inserted values, if any.</summary>
        public string Delimiter { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public TextOperationConfig() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public TextOperationConfig(TextOperationConfig other)
        {
            this.Operation = other.Operation;
            this.Target = other.Target.ToArray();
            this.Value = other.Value;
            this.Delimiter = other.Delimiter;
        }

        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.Target ??= new string[0];
        }
    }
}
