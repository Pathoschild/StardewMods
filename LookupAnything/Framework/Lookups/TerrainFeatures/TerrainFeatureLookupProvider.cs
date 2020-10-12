using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Integrations.JsonAssets;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures
{
    /// <summary>Provides lookup data for in-game terrain features.</summary>
    internal class TerrainFeatureLookupProvider : BaseLookupProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Json Assets API.</summary>
        private readonly JsonAssetsIntegration JsonAssets;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="jsonAssets">The Json Assets API.</param>
        public TerrainFeatureLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, JsonAssetsIntegration jsonAssets)
            : base(reflection, gameHelper)
        {
            this.JsonAssets = jsonAssets;
        }

        /// <inheritdoc />
        public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            // terrain features
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                Vector2 entityTile = pair.Key;
                TerrainFeature feature = pair.Value;

                if (!this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    continue;

                switch (feature)
                {
                    case FruitTree fruitTree:
                        if (this.Reflection.GetField<float>(fruitTree, "alpha").GetValue() >= 0.8f) // ignore when tree is faded out (so player can lookup things behind it)
                            yield return new FruitTreeTarget(this.GameHelper, fruitTree, this.JsonAssets, entityTile);
                        break;

                    case Tree tree:
                        if (this.Reflection.GetField<float>(tree, "alpha").GetValue() >= 0.8f) // ignore when tree is faded out (so player can lookup things behind it)
                            yield return new TreeTarget(this.GameHelper, tree, entityTile, this.Reflection);
                        break;

                    case Bush bush: // planted bush
                        yield return new BushTarget(this.GameHelper, bush, this.Reflection);
                        break;
                }
            }

            // large terrain features
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                Vector2 entityTile = feature.tilePosition.Value;
                if (!this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    continue;

                switch (feature)
                {
                    case Bush bush: // wild bush
                        yield return new BushTarget(this.GameHelper, bush, this.Reflection);
                        break;
                }
            }
        }

        /// <inheritdoc />
        public override ISubject GetSubject(ITarget target)
        {
            return target switch
            {
                BushTarget bush => new BushSubject(this.GameHelper, bush.Value, this.Reflection),
                FruitTreeTarget tree => new FruitTreeSubject(this.GameHelper, tree.Value, target.Tile),
                TreeTarget tree => new TreeSubject(this.GameHelper, tree.Value, target.Tile),
                _ => null
            };
        }
    }
}
