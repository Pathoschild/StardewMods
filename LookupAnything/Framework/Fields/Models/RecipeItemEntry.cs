using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

/// <summary>An input or output item for a recipe model.</summary>
/// <param name="Sprite">The sprite to display.</param>
/// <param name="DisplayText">The display text for the item name and count.</param>
/// <param name="Quality">The item quality that will be produced, if applicable.</param>
/// <param name="IsGoldPrice">Whether this is a gold price, rather than an ingredient.</param>
/// <param name="IsValid">Whether this recipe input or output is valid.</param>
/// <param name="Entity">The output entity represented by this entity, if applicable.</param>
internal record RecipeItemEntry(SpriteInfo? Sprite, string DisplayText, int? Quality, bool IsGoldPrice, bool IsValid = true, object? Entity = null);
