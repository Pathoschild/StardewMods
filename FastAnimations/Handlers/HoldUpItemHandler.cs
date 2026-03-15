using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the hold-up-item animation.</summary>
/// <remarks>See game logic in <see cref="Farmer.holdUpItemThenMessage(Item,int,bool)"/>.</remarks>
internal sealed class HoldUpItemHandler : BaseAnimationHandler
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public HoldUpItemHandler(float multiplier)
        : base(multiplier) { }

    /// <inheritdoc />
    public override bool TryApply(int playerAnimationId)
    {
        Farmer player = Game1.player;

        return
            this.IsAnimating(player)
            && this.ApplySkipsWhile(() =>
            {
                // player animation
                player.Update(Game1.currentGameTime, player.currentLocation);

                // animation of item thrown in the air
                foreach (TemporaryAnimatedSprite sprite in this.GetTemporarySprites(player).ToArray())
                {
                    bool done = sprite.update(Game1.currentGameTime);
                    if (done)
                        player.currentLocation.TemporarySprites.Remove(sprite);
                }

                // reduce freeze time
                player.freezePause = Math.Max(0, player.freezePause - BaseAnimationHandler.MillisecondsPerFrame);

                return this.IsAnimating(player);
            });
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get whether the animation is playing now.</summary>
    /// <param name="player">The player performing the action.</param>
    private bool IsAnimating(Farmer player)
    {
        return
            player.mostRecentlyGrabbedItem is not null
            && player.FarmerSprite?.currentAnimation is [{ frame: 57, milliseconds: 0 }, { frame: 57, milliseconds: 2500 }, { milliseconds: 500 }];
    }

    /// <summary>Get the temporary animated sprites added as part of the item-hold-up animation.</summary>
    /// <param name="player">The player being animated.</param>
    /// <remarks>Derived from <see cref="Farmer.showHoldingItem"/>.</remarks>
    private IEnumerable<TemporaryAnimatedSprite> GetTemporarySprites(Farmer player)
    {
        // get hold up item
        Item? holdingItem = player.mostRecentlyGrabbedItem;

        foreach (TemporaryAnimatedSprite sprite in player.currentLocation.TemporarySprites)
        {
            switch (holdingItem)
            {
                case null:
                    if (sprite.textureName == "LooseSprites\\Cursors" && sprite.sourceRect == new Rectangle(420, 489, 25, 18))
                        yield return sprite;
                    break;

                case SpecialItem specialItem:
                    if (
                        sprite.textureName == "LooseSprites\\Cursors"
                        && (
                            sprite.sourceRect == new Rectangle(Game1.player.MaxItems == Farmer.maxInventorySpace ? 268 : 257, 1436, Game1.player.MaxItems == Farmer.maxInventorySpace ? 11 : 9, 13)
                            || sprite.sourceRect == new Rectangle(129 + 16 * specialItem.which.Value, 320, 16, 16)
                        )
                    )
                        yield return sprite;
                    break;

                default:
                    {
                        ParsedItemData data = ItemRegistry.GetDataOrErrorItem(holdingItem.QualifiedItemId);
                        if (sprite.textureName == data.TextureName && sprite.sourceRect == data.GetSourceRect())
                            yield return sprite;
                        break;
                    }
            }
        }
    }
}
