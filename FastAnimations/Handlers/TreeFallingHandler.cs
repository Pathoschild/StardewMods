using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the falling-tree animation.</summary>
    /// <remarks>See game logic in <see cref="StardewValley.TerrainFeatures.Tree.tickUpdate"/>.</remarks>
    internal class TreeFallingHandler : BaseAnimationHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The trees in the current location.</summary>
        private IDictionary<Vector2, TerrainFeature> Trees = new Dictionary<Vector2, TerrainFeature>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public TreeFallingHandler(float multiplier, IReflectionHelper reflection)
            : base(multiplier)
        {
            this.Reflection = reflection;
        }

        /// <summary>Perform any updates needed when the player enters a new location.</summary>
        /// <param name="location">The new location.</param>
        public override void OnNewLocation(GameLocation location)
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

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Context.IsWorldReady
                && this.GetFallingTrees().Any();
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            GameTime gameTime = Game1.currentGameTime;

            int skips = this.GetSkipsThisTick();
            foreach (var pair in this.GetFallingTrees())
                this.ApplySkips(skips, () => pair.Value.tickUpdate(gameTime, pair.Key, Game1.currentLocation));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get all trees in the current location which are currently falling.</summary>
        private IEnumerable<KeyValuePair<Vector2, TerrainFeature>> GetFallingTrees()
        {
            Rectangle visibleTiles = TileHelper.GetVisibleArea();
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in this.Trees)
            {
                if (visibleTiles.Contains((int)pair.Key.X, (int)pair.Key.Y))
                {
                    bool isFalling = this.Reflection.GetField<NetBool>(pair.Value, "falling").GetValue().Value;
                    if (isFalling)
                        yield return pair;
                }
            }
        }
    }
}
