namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>A bundle entry parsed from the game's data files.</summary>
    /// <param name="ID">The unique bundle ID.</param>
    /// <param name="Name">The bundle name.</param>
    /// <param name="DisplayName">The translated bundle name.</param>
    /// <param name="Area">The community center area containing the bundle.</param>
    /// <param name="RewardData">The unparsed reward description, which can be parsed with <see cref="StardewValley.Utility.getItemFromStandardTextDescription"/>.</param>
    /// <param name="Ingredients">The required item ingredients.</param>
    internal record BundleModel(int ID, string Name, string DisplayName, string Area, string RewardData, BundleIngredientModel[] Ingredients);
}
