using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>An input or output item for a recipe model.</summary>
    /// <param name="Sprite">The sprite to display.</param>
    /// <param name="DisplayText">The display text for the item name and count.</param>
    /// <param name="Quality">The item quality that will be produced, if applicable.</param>
    /// <param name="IsGoldPrice">Whether this is a gold price, rather than an ingredient.</param>
    /// <param name="IsError">False if this entry is an error item or category</param>
    internal record RecipeItemEntry(SpriteInfo? Sprite, string DisplayText, int? Quality, bool IsGoldPrice, bool IsError = false)
    {
        internal bool IsValid => this.IsGoldPrice || !this.IsError;
    }
}
