using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>An in-game tractor which only serves to display in the world.</summary>
    internal sealed class TractorStatic : NPC
    {
        /*********
        ** Properties
        *********/
        /// <summary>The callback to invoke when the player mounts the tractor.</summary>
        private readonly Action OnMount;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The internal NPC name.</param>
        /// <param name="tileX">The initial tile X position.</param>
        /// <param name="tileY">The initial tile Y position.</param>
        /// <param name="sprite">The animated tractor sprite.</param>
        /// <param name="onMount">The callback to invoke when the player mounts the tractor.</param>
        public TractorStatic(string name, int tileX, int tileY, AnimatedSprite sprite, Action onMount)
        {
            // set basic info
            this.name = name;
            this.setTilePosition(tileX, tileY);
            this.sprite = sprite;
            this.OnMount = onMount;

            // configure NPC to match horse
            this.breather = false;
            this.willDestroyObjectsUnderfoot = false;
            this.hideShadow = true;
            this.drawOffset = new Vector2(-Game1.tileSize / 4, 0.0f);
            this.faceDirection(3);
        }

        /// <summary>Get the bounding box for collision checks.</summary>
        public override Rectangle GetBoundingBox()
        {
            Rectangle boundingBox = base.GetBoundingBox();
            if ((this.facingDirection == 0 || this.facingDirection == 2))
                boundingBox.Inflate(-Game1.tileSize / 2 - Game1.pixelZoom, 0);
            return boundingBox;
        }

        /// <summary>Perform an action when the player activates the tractor.</summary>
        /// <param name="player">The player who activated the tractor.</param>
        /// <param name="location">The location where the tractor was activated.</param>
        public override bool checkAction(StardewValley.Farmer player, GameLocation location)
        {
            this.OnMount();
            return true;
        }

        /// <summary>Perform any update logic needed on update tick.</summary>
        /// <param name="time">The current game time.</param>
        /// <param name="location">The location containing the tractor.</param>
        public override void update(GameTime time, GameLocation location)
        {
            // don't do any NPC stuff
        }

        /// <summary>Perform any update logic needed on update tick.</summary>
        /// <param name="time">The current game time.</param>
        /// <param name="location">The location containing the tractor.</param>
        /// <param name="id">The NPC's unique ID for multiplayer broadcasting.</param>
        /// <param name="move">Whether the NPC can move if needed.</param>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "Not relevant since this is an empty method.")]
        public override void update(GameTime time, GameLocation location, long id, bool move)
        {
            // don't do any NPC stuff
        }
    }
}
