using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the crop harvesting animation.</summary>
    /// <remarks>See game logic in <see cref="GameLocation.checkAction(xTile.Dimensions.Location, xTile.Dimensions.Rectangle, Farmer)"/> (look for <c>who.animateOnce(279</c>), <see cref="FarmerSprite"/>'s private <c>animateOnce(GameTime)</c> method (runs animation + some logic), and <see cref="Farmer.showItemIntake"/>.</remarks>
    internal class HarvestHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public HarvestHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Context.IsWorldReady
                && Game1.player.Sprite.CurrentAnimation != null
                && (
                    playerAnimationID == FarmerSprite.harvestItemDown
                    || playerAnimationID == FarmerSprite.harvestItemLeft
                    || playerAnimationID == FarmerSprite.harvestItemRight
                    || playerAnimationID == FarmerSprite.harvestItemUp
                )
                && !this.IsRidingTractor();
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            var player = Game1.player;
            var location = player.currentLocation;

            this.ApplySkips(
                run: () =>
                {
                    // player animation
                    player.Update(Game1.currentGameTime, location);

                    // animation of item thrown in the air
                    foreach (var sprite in this.GetTemporarySprites(player))
                    {
                        bool done = sprite.update(Game1.currentGameTime);
                        if (done)
                            location.TemporarySprites.Remove(sprite);
                    }
                }
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the temporary animated sprites added as part of the harvest animation.</summary>
        /// <param name="player">The player being animated.</param>
        /// <remarks>Derived from <see cref="Farmer.showItemIntake"/>.</remarks>
        private IEnumerable<TemporaryAnimatedSprite> GetTemporarySprites(Farmer player)
        {
            // get harvested item
            SObject harvestedObj = player.mostRecentlyGrabbedItem as SObject ?? player.ActiveObject;
            if (harvestedObj == null)
                return new TemporaryAnimatedSprite[0];

            // get source rectangles
            Rectangle mainSourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, harvestedObj.ParentSheetIndex, 16, 16);
            Rectangle? coloredSourceRect = null;
            if (harvestedObj is ColoredObject)
                coloredSourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, harvestedObj.ParentSheetIndex + 1, 16, 16);

            // get temporary sprites
            return player.currentLocation
                .TemporarySprites
                .Where(sprite =>
                    sprite.Texture == Game1.objectSpriteSheet
                    && (
                        sprite.sourceRect == mainSourceRect
                        || sprite.sourceRect == coloredSourceRect
                    )
                )
                .ToArray();
        }
    }
}
