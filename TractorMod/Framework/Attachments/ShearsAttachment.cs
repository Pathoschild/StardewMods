using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the shears.</summary>
    internal class ShearsAttachment : BaseAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly GenericAttachmentConfig Config;

        /// <summary>The minimum delay before attempting to recheck the same tile.</summary>
        private readonly TimeSpan AnimalCheckDelay = TimeSpan.FromSeconds(1);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public ShearsAttachment(GenericAttachmentConfig config, IReflectionHelper reflection)
            : base(reflection)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool tool, Item item, GameLocation location)
        {
            return this.Config.Enable && tool is Shears;
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Farmer player, Tool tool, Item item, GameLocation location)
        {
            if (this.TryStartCooldown(tile.ToString(), this.AnimalCheckDelay))
            {
                // get best animal target
                Vector2 useAt = this.GetToolPixelPosition(tile);
                FarmAnimal animal = Utility.GetBestHarvestableFarmAnimal(
                    animals: this.GetFarmAnimals(location),
                    tool: tool,
                    toolRect: new Rectangle((int)useAt.X, (int)useAt.Y, Game1.tileSize, Game1.tileSize)
                );

                // shear if applicable
                if (animal != null && animal.toolUsedForHarvest.Value == tool.BaseName && animal.currentProduce.Value > 0 && animal.age.Value >= animal.ageWhenMature.Value)
                {
                    this.Reflection.GetField<FarmAnimal>(tool, "animal").SetValue(animal);
                    tool.DoFunction(location, (int)useAt.X, (int)useAt.Y, 0, player);
                    return true;
                }
            }

            return false;
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the farm animals in a location.</summary>
        /// <param name="location">The location to check.</param>
        /// <remarks>Derived from <see cref="Shears.beginUsing"/>.</remarks>
        private IEnumerable<FarmAnimal> GetFarmAnimals(GameLocation location)
        {
            switch (location)
            {
                case Farm farm:
                    return farm.animals.Values;

                case AnimalHouse house:
                    return house.animals.Values;

                default:
                    return location.characters.OfType<FarmAnimal>();
            }
        }
    }
}
