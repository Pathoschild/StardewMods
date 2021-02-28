using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>An input or output item for a recipe model.</summary>
    internal struct RecipeItemEntry
    {
        /// <summary>The sprite to display.</summary>
        public SpriteInfo Sprite;

        /// <summary>The display text for the item name and count.</summary>
        public string DisplayText;

        /// <summary>The pixel size of the display text.</summary>
        public Vector2 DisplayTextSize;
    }
}
