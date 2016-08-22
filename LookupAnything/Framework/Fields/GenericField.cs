using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A generic metadata field shown as an extended property in the encyclopedia.</summary>
    public class GenericField : ICustomField
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A short field label.</summary>
        public virtual string Label { get; }

        /// <summary>The field value.</summary>
        public virtual string Value { get; }

        /// <summary>Whether the field should be displayed.</summary>
        public virtual bool HasValue { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="value">The field value.</param>
        /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
        public GenericField(string label, string value, bool? hasValue = null)
        {
            this.Label = label;
            this.Value = value;
            this.HasValue = hasValue ?? !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="Value"/> using the default format).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped..</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="Value"/> using the default format.</returns>
        public virtual Vector2? DrawValue(SpriteBatch sprites, SpriteFont font, Vector2 position, float wrapWidth)
        {
            return null;
        }
    }
}