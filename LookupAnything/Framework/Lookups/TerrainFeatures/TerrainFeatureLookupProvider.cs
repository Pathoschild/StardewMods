using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
        /// <summary>Provides subject entries.</summary>
        private readonly ISubjectRegistry Codex;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="codex">Provides subject entries.</param>
        public TerrainFeatureLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, ISubjectRegistry codex)
            : base(reflection, gameHelper)
        {
            this.Codex = codex;
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
                        if (fruitTree.alpha >= 0.8f) // ignore when tree is faded out (so player can lookup things behind it)
                            yield return new FruitTreeTarget(this.GameHelper, fruitTree, entityTile, () => this.BuildSubject(fruitTree, entityTile));
                        break;

                    case Tree tree:
                        if (tree.alpha >= 0.8f) // ignore when tree is faded out (so player can lookup things behind it)
                            yield return new TreeTarget(this.GameHelper, tree, entityTile, () => this.BuildSubject(tree, entityTile));
                        break;

                    case Bush bush: // planted bush
                        yield return new BushTarget(this.GameHelper, bush, () => this.BuildSubject(bush));
                        break;
                }
            }

            // large terrain features
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                Vector2 entityTile = feature.Tile;
                if (!this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    continue;

                switch (feature)
                {
                    case Bush bush: // wild bush
                        yield return new BushTarget(this.GameHelper, bush, () => this.BuildSubject(bush));
                        break;
                }
            }
        }

        /// <inheritdoc />
        public override ISubject? GetSubjectFor(object entity, GameLocation? location)
        {
            return entity switch
            {
                Bush bush => this.BuildSubject(bush),
                _ => null
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Build a subject.</summary>
        /// <param name="bush">The entity to look up.</param>
        private ISubject BuildSubject(Bush bush)
        {
            return new BushSubject(this.GameHelper, bush);
        }

        /// <summary>Build a subject.</summary>
        /// <param name="tree">The entity to look up.</param>
        /// <param name="tile">The tree tile.</param>
        private ISubject BuildSubject(FruitTree tree, Vector2 tile)
        {
            return new FruitTreeSubject(this.GameHelper, tree, tile);
        }

        /// <summary>Build a subject.</summary>
        /// <param name="tree">The entity to look up.</param>
        /// <param name="tile">The tree tile.</param>
        private ISubject BuildSubject(Tree tree, Vector2 tile)
        {
            return new TreeSubject(this.Codex, this.GameHelper, tree, tile);
        }
    }
}
