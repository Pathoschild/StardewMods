namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for a shop that isn't available from the game data directly.</summary>
    /// <param name="DisplayKey">The translation key for the shop name.</param>
    /// <param name="BuysCategories">The categories of items that the player can sell to this shop.</param>
    internal record ShopData(string DisplayKey, int[] BuysCategories);
}
