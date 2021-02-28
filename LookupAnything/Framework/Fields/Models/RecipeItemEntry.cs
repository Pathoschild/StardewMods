using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>An input or output item for a recipe model.</summary>
    internal class RecipeItemEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The sprite to display.</summary>
        public SpriteInfo Sprite { get; }

        /// <summary>The display text for the item name and count.</summary>
        public string DisplayText { get; }

        /// <summary>The pixel size of the display text.</summary>
        public Vector2 DisplayTextSize { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="sprite">The sprite to display.</param>
        /// <param name="displayText">The display text for the item name and count.</param>
        /// <param name="displayTextSize">The pixel size of the display text.</param>
        public RecipeItemEntry(SpriteInfo sprite, string displayText, Vector2 displayTextSize)
        {
            this.Sprite = sprite;
            this.DisplayText = displayText;
            this.DisplayTextSize = displayTextSize;
        }
    }
}
