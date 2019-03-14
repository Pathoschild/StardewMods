using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SmallBeachFarm
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class ModEntry : Mod, IAssetLoader
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Player.Warped += this.OnWarped;
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Maps/Farm_Fishing");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Maps/Farm_Fishing"))
                return this.Helper.Content.Load<T>("assets/[CP] SmallBeachFarm/assets/SmallBeachFarm.tbin");

            throw new NotSupportedException($"Unexpected asset '{asset.AssetName}'.");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && e.IsLocalPlayer && e.NewLocation.Name == "Farm" && Game1.player.getTileLocation().Y > 29)
                Game1.player.Position = new Vector2(79, 21) * Game1.tileSize;
        }
    }
}
