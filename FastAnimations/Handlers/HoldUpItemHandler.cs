using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the item-holdup animation.</summary>
/// <remarks>See game logic in <see cref="Farmer.holdUpItemThenMessage"/>.</remarks>
internal class HoldUpItemHandler : BaseAnimationHandler
{
    /*********
     ** Public methods
     *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="multiplier">The animation speed multiplier to apply.</param>
    public HoldUpItemHandler(float multiplier) : base(multiplier)
    {
    }

    /// <summary>Get whether the animation is currently active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override bool IsEnabled(int playerAnimationID)
    {
        return this.IsHoldingUpItem();
    }

    /// <summary>Perform any logic needed on update while the animation is active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override void Update(int playerAnimationID)
    {
        var player = Game1.player;
        var location = Game1.currentLocation;

        this.ApplySkips(
            () =>
            {
                player.Update(Game1.currentGameTime, location);

                foreach (var sprite in this.GetTemporarySprites(player).ToArray())
                {
                    bool done = sprite.update(Game1.currentGameTime);
                    if (done)
                        location.TemporarySprites.Remove(sprite);
                }
            },
            () => !this.IsHoldingUpItem()
        );
    }

    /*********
     ** Private methods
     *********/
    /// <summary>Get the temporary animated sprites added as part of the item-holdup animation.</summary>
    /// <param name="player">The player being animated.</param>
    /// <remarks>Derived from <see cref="Farmer.showHoldingItem"/>.</remarks>
    private IEnumerable<TemporaryAnimatedSprite> GetTemporarySprites(Farmer player)
    {
        // get hold up item
        Item? holdingItem = player.mostRecentlyGrabbedItem ?? player.ActiveItem;

        foreach (TemporaryAnimatedSprite sprite in player.currentLocation.TemporarySprites)
        {
            switch (holdingItem)
            {
                case null:
                {
                    if (sprite.textureName == "LooseSprites\\Cursors" && sprite.sourceRect == new Microsoft.Xna.Framework.Rectangle(420, 489, 25, 18))
                        yield return sprite;
                    break;
                }
                case SpecialItem specialItem:
                {
                    if (sprite.textureName == "LooseSprites\\Cursors" &&
                        (sprite.sourceRect == new Rectangle(Game1.player.MaxItems == 36 ? 268 : 257, 1436, Game1.player.MaxItems == 36 ? 11 : 9, 13) ||
                         sprite.sourceRect == new Rectangle(129 + 16 * specialItem.which.Value, 320, 16, 16)))
                        yield return sprite;
                    break;
                }
                default:
                {
                    var data = ItemRegistry.GetDataOrErrorItem(holdingItem.QualifiedItemId);
                    if (sprite.textureName == data.TextureName && sprite.sourceRect == data.GetSourceRect())
                        yield return sprite;
                    break;
                }
            }
        }
    }

    private bool IsHoldingUpItem()
    {
        List<FarmerSprite.AnimationFrame>? currentAnimation = Game1.player.FarmerSprite.CurrentAnimation;

        return currentAnimation.Count >= 3 &&
               currentAnimation[0].frame == 57 && currentAnimation[0].milliseconds == 0 &&
               currentAnimation[1].frame == 57 && currentAnimation[1].milliseconds == 2500 &&
               currentAnimation[2].frame == 0 && currentAnimation[2].milliseconds == 500;
    }
}
