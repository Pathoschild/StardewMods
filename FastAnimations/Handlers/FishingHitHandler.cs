using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Tools;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the fishing "Hit!" animation.</summary>
/// <remarks>See game logic in <see cref="FishingRod.DoFunction"/>.</remarks>
internal sealed class FishingHitHandler : BaseAnimationHandler
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public FishingHitHandler(float multiplier)
        : base(multiplier) { }

    /// <inheritdoc />
    public override bool TryApply(int playerAnimationId)
    {
        Farmer player = Game1.player;

        return
            player.CurrentTool is FishingRod rod
            && Game1.screenOverlayTempSprites.Any()
            && this.ApplySkipsWhile(() =>
            {
                bool applied = false;

                foreach (TemporaryAnimatedSprite sprite in Game1.screenOverlayTempSprites)
                {
                    if (sprite.id == 987654321)
                    {
                        applied = true;
                        sprite.update(Game1.currentGameTime);
                    }
                }

                return applied;
            });
    }

}
