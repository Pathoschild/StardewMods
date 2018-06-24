using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.TheLongNight
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod, IAssetEditor
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The time at which the game clock will stop and all fish will become unlocked.</summary>
        private readonly int GameTimeLimit = 2550;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/Fish.xnb");
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            // unlock any in-season fish after 2am
            asset
                .AsDictionary<int, string>()
                .Set((key, value) =>
                {
                    string[] fields = value.Split('/');
                    if (fields[1] == "trap")
                        return value; // ignore non-fish entries

                    fields[5] = $"{fields[5]} {this.GameTimeLimit} {int.MaxValue}".Trim();
                    return string.Join("/", fields);
                });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked after the game updates (roughly 60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// All times are shown in the game's internal format, which is essentially military time with support for
        /// times past midnight (e.g. 2400 is midnight, 2600 is 2am).
        /// 
        /// The game's logic for collapsing the player mostly happens in <see cref="Game1.performTenMinuteClockUpdate"/>.
        /// It has three cases which affect staying up:
        ///   - time = 2600: dismounts the player.
        ///   - time ≥ 2600: sets the <see cref="Game1.farmerShouldPassOut"/> flag, which causes <see cref="Game1.UpdateOther"/>
        ///     to initiate a player collapse (animation #293).
        ///   - time = 2800: initiates a player collapse.
        /// </remarks>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || Game1.timeOfDay < this.GameTimeLimit)
                return;

            Game1.gameTimeInterval = 0;
        }
    }
}
