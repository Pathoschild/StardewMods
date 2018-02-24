using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux
{
    /// <summary>The API provided by the Custom Farming Redux mod.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by the Custom Farming Redux mod.")]
    public interface ICustomFarmingApi
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether a given item is a custom object or machine from Custom Farming Redux.</summary>
        /// <param name="item">The item instance.</param>
        bool isCustom(Item item);

        /// <summary>Get the spritesheet texture for a custom object or machine (if applicable).</summary>
        /// <param name="item">The item instance.</param>
        Texture2D getSpritesheet(Item item);

        /// <summary>Get the spritesheet source area for a custom object or machine (if applicable).</summary>
        /// <param name="item">The item instance.</param>
        Rectangle? getSpriteSourceArea(Item item);
    }
}
