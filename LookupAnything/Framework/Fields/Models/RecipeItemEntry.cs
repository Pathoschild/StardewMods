using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>An input or output item for a recipe model.</summary>
    /// <param name="Sprite">The sprite to display.</param>
    /// <param name="DisplayText">The display text for the item name and count.</param>
    internal record RecipeItemEntry(SpriteInfo? Sprite, string DisplayText);
}
