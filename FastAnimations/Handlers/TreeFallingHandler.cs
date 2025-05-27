using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the falling-tree animation.</summary>
/// <remarks>See game logic in <see cref="Tree.tickUpdate"/>.</remarks>
internal sealed class TreeFallingHandler : BaseAnimationHandler
{
    /*********
    ** Fields
    *********/
    /// <summary>The trees in the current location.</summary>
    private Dictionary<Vector2, TerrainFeature> Trees = [];


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public TreeFallingHandler(float multiplier)
        : base(multiplier)
    {
        if (Context.IsWorldReady)
            this.UpdateTreeCache(Game1.currentLocation);
    }

    /// <inheritdoc />
    public override void OnNewLocation(GameLocation location)
    {
        this.UpdateTreeCache(location);
    }

    /// <inheritdoc />
    public override bool TryApply(int playerAnimationId)
    {
        bool applied = false;

        if (Context.IsWorldReady)
        {
            GameTime gameTime = Game1.currentGameTime;

            foreach (TerrainFeature tree in this.GetFallingTrees())
            {
                applied |= this.ApplySkipsWhile(() =>
                {
                    tree.tickUpdate(gameTime);

                    return tree.Location is not null;
                });
            }
        }

        return applied;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Update the cached list of trees in the current location.</summary>
    /// <param name="location">The location to check.</param>
    private void UpdateTreeCache(GameLocation location)
    {
        this.Trees =
            (
                from pair in location.terrainFeatures.FieldDict
                let tree = pair.Value.Value as Tree
                let fruitTree = pair.Value.Value as FruitTree
                where
                    (
                        tree != null
                        && !tree.stump.Value
                        && tree.growthStage.Value > Tree.bushStage
                    )
                    || (
                        fruitTree != null
                        && !fruitTree.stump.Value
                        && fruitTree.growthStage.Value > FruitTree.bushStage
                    )
                select pair
            )
            .ToDictionary(p => p.Key, p => p.Value.Value);
    }

    /// <summary>Get all trees in the current location which are currently falling.</summary>
    private IEnumerable<TerrainFeature> GetFallingTrees()
    {
        Rectangle visibleTiles = TileHelper.GetVisibleArea();
        foreach (KeyValuePair<Vector2, TerrainFeature> pair in this.Trees)
        {
            if (visibleTiles.Contains((int)pair.Key.X, (int)pair.Key.Y))
            {
                bool isFalling = pair.Value switch
                {
                    Tree tree => tree.falling.Value,
                    FruitTree tree => tree.falling.Value,
                    _ => false
                };

                if (isFalling && pair.Value.Location != null)
                    yield return pair.Value;
            }
        }
    }
}
