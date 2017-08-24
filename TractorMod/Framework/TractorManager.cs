using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Manages a spawned tractor.</summary>
    internal class TractorManager
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A static tractor NPC for display in the world.</summary>
        /// <remarks>This is deliberately separate to avoid conflicting with logic for summoning or managing the player horse.</remarks>
        public TractorStatic Static { get; }

        /// <summary>A tractor horse for riding.</summary>
        public TractorMount Mount { get; }

        /// <summary>The currently active tractor instance.</summary>
        public NPC Current => this.IsRiding ? (NPC)this.Mount : this.Static;

        /// <summary>Whether the player is currently riding the tractor.</summary>
        public bool IsRiding => this.Mount?.rider == Game1.player;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The internal NPC name.</param>
        /// <param name="tileX">The initial tile X position.</param>
        /// <param name="tileY">The initial tile Y position.</param>
        /// <param name="content">The content helper with which to load the tractor sprite.</param>
        public TractorManager(string name, int tileX, int tileY, IContentHelper content)
        {
            AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(@"assets\tractor.png"), 0, 32, 32)
            {
                textureUsesFlippedRightForLeft = true,
                loop = true
            };

            this.Static = new TractorStatic(name + "_static", tileX, tileY, sprite, () => this.SetMounted(true));
            this.Mount = new TractorMount(name, tileX, tileY, sprite, () => this.SetMounted(false));
        }

        /// <summary>Move the tractor to the given location.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="tile">The tile coordinate in the given location.</param>
        public void SetLocation(GameLocation location, Vector2 tile)
        {
            Game1.warpCharacter(this.Current, location.name, tile, false, true);
        }

        /// <summary>Move the tractor to a specific pixel position within its current location.</summary>
        /// <param name="position">The pixel coordinate in the current location.</param>
        public void SetPixelPosition(Vector2 position)
        {
            this.Current.Position = position;
        }

        /// <summary>Remove all tractors from the game.</summary>
        public void RemoveTractors()
        {
            // find all locations
            IEnumerable<GameLocation> locations = Game1.locations
                .Union(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors != null
                    select building.indoors
                );

            // remove tractors
            foreach (GameLocation location in locations)
                location.characters.RemoveAll(p => p is TractorStatic || p is TractorMount);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Set whether the player should be riding the tractor.</summary>
        /// <param name="mount">Whether the player should be riding the tractor.</param>
        private void SetMounted(bool mount)
        {
            // swap tractors
            NPC newTractor = mount ? (NPC)this.Mount : this.Static;
            NPC oldTractor = mount ? (NPC)this.Static : this.Mount;

            Game1.removeCharacterFromItsLocation(oldTractor.name);
            Game1.removeCharacterFromItsLocation(newTractor.name);
            Game1.currentLocation.addCharacter(newTractor);

            newTractor.position = oldTractor.position;
            newTractor.facingDirection = oldTractor.facingDirection;

            // mount
            if (mount)
                this.Mount.checkAction(Game1.player, Game1.currentLocation);
        }
    }
}
