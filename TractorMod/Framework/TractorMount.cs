using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The in-game tractor that can be ridden by the player.</summary>
    internal sealed class TractorMount : Horse
    {
        /*********
        ** Properties
        *********/
        /// <summary>The callback to invoke when the player mounts the tractor.</summary>
        private readonly Action OnDismount;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The internal NPC name.</param>
        /// <param name="tileX">The initial tile X position.</param>
        /// <param name="tileY">The initial tile Y position.</param>
        /// <param name="sprite">The animated tractor sprite.</param>
        /// <param name="onDismount">The callback to invoke when the player dismounts the tractor.</param>
        public TractorMount(string name, int tileX, int tileY, AnimatedSprite sprite, Action onDismount)
            : base(tileX, tileY)
        {
            this.name = name;
            this.sprite = sprite;
            this.OnDismount = onDismount;
        }

        /// <summary>Perform any update logic needed on update tick.</summary>
        /// <param name="time">The current game time.</param>
        /// <param name="location">The location containing the tractor.</param>
        public override void update(GameTime time, GameLocation location)
        {
            if (this.rider == null)
                this.OnDismount();
            else
                base.update(time, location);
        }
    }
}
