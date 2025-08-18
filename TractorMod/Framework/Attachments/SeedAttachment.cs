using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments;

/// <summary>An attachment for seeds.</summary>
internal class SeedAttachment : BaseAttachment
{
    /*********
    ** Fields
    *********/
    /// <summary>The attachment settings.</summary>
    private readonly SeedConfig Config;

    /// <summary>Simplifies access to private code.</summary>
    private readonly IReflectionHelper Reflection;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The attachment settings.</param>
    /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
    /// <param name="reflection">Simplifies access to private code.</param>
    public SeedAttachment(SeedConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
        : base(modRegistry)
    {
        this.Config = config;
        this.Reflection = reflection;
    }

    /// <inheritdoc />
    public override bool IsEnabled(Farmer player, Tool? tool, Item? item, GameLocation location)
    {
        return (this.Config.EnableSeeds || this.Config.EnableTreeSeeds) && this.IsSeed(item);
    }

    /// <inheritdoc />
    public override bool Apply(Vector2 tile, SObject? tileObj, TerrainFeature? tileFeature, Farmer player, Tool? tool, Item? item, GameLocation location)
    {
        if (item is not { Stack: > 0 })
            return false;

        if (this.Config.EnableTreeSeeds && this.IsTreeSeed(item) && item is SObject obj)
        {
            if (obj.canBePlacedHere(location, tile) && obj.placementAction(location, (int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), player))
            {
                this.ConsumeItem(player, item);
                if (this.Config.UseFertilizerWhenPlantingTrees && location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
                {
                    Item? fertilizer = player.Items.GetById("(O)805").FirstOrDefault();
                    if (fertilizer != null && feature is Tree tree && !tree.fertilized.Value && tree.growthStage.Value < Tree.treeStage && tree.fertilize())
                    {
                        this.ConsumeItem(player, fertilizer);
                    }
                }
                return true;
            }
        }
        else if (this.Config.EnableSeeds && this.IsSeed(item))
        {
            // get dirt
            if (!this.TryGetHoeDirt(tileFeature, tileObj, out HoeDirt? dirt, out bool dirtCoveredByObj, out IndoorPot? pot) || dirt.crop != null || pot?.bush.Value is not null)
                return false;

            // ignore if there's a giant crop, meteorite, etc covering the tile
            if (dirtCoveredByObj || this.HasResourceClumpCoveringTile(location, tile, this.Reflection))
                return false;

            // sow seeds
            bool sowed = dirt.plant(item.ItemId, player, false);
            if (sowed)
            {
                this.ConsumeItem(player, item);

                if (this.TryGetEnricher(location, tile, out Chest? enricher, out Item? fertilizer) && dirt.plant(fertilizer.ItemId, player, true))
                    this.ConsumeItem(enricher, fertilizer);
            }
            return sowed;
        }
        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the enricher and fertilizer in range of the given tile, if any.</summary>
    /// <param name="location">The location to check.</param>
    /// <param name="tile">The tile to which to apply fertilizer.</param>
    /// <param name="enricher">The enricher found.</param>
    /// <param name="fertilizer">The fertilizer item within the enricher.</param>
    /// <returns>Returns whether an enricher with fertilizer was found.</returns>
    private bool TryGetEnricher(GameLocation location, Vector2 tile, [NotNullWhen(true)] out Chest? enricher, [NotNullWhen(true)] out Item? fertilizer)
    {
        (Chest enricher, Item fertilizer)? entry = this.GetEnricher(location, tile);

        fertilizer = entry?.fertilizer;
        enricher = entry?.enricher;
        return entry != null;
    }

    /// <summary>Whether the given item is a seed</summary>
    /// <param name="item">The item to be checked</param>
    /// <returns>Returns whether the item is a seed</returns>
    private bool IsSeed(Item? item)
    {
        return item is { Category: SObject.SeedsCategory, Stack: > 0 };
    }

    /// <summary>Whether the given item is a tree seed</summary>
    /// <param name="item">The item to be checked</param>
    /// <returns>Returns whether the item is a tree seed</returns>
    private bool IsTreeSeed(Item? item)
    {
        return item != null && SObject.isWildTreeSeed(item.ItemId);
    }

    /// <summary>Get the enricher and fertilizer in range of the given tile, if any.</summary>
    /// <param name="location">The location to check.</param>
    /// <param name="tile">The tile to which to apply fertilizer.</param>
    /// <remarks>Derived from <see cref="SObject.placementAction"/>.</remarks>
    private (Chest enricher, Item fertilizer)? GetEnricher(GameLocation location, Vector2 tile)
    {
        foreach (SObject sprinkler in location.Objects.Values)
        {
            if (
                sprinkler.IsSprinkler()
                && sprinkler.heldObject.Value is { QualifiedItemId: "(O)913" } enricherObj
                && enricherObj.heldObject.Value is Chest enricher
                && sprinkler.IsInSprinklerRangeBroadphase(tile)
                && sprinkler.GetSprinklerTiles().Contains(tile)
                && enricher.Items.FirstOrDefault() is { Category: SObject.fertilizerCategory } fertilizer
            )
            {
                return (enricher, fertilizer);
            }
        }

        return null;
    }
}
