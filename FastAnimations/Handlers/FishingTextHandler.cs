using System.Linq;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Tools;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the fishing text animations.</summary>
/// <remarks>See game logic in <see cref="BobberBar.update"/> and <see cref="FishingRod.DoFunction"/>.</remarks>
internal sealed class FishingTextHandler : BaseAnimationHandler
{
    /*********
    ** Fields
    *********/
    /// <summary>Simplifies access to private game code.</summary>
    private readonly IReflectionHelper Reflection;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="multiplier">The animation speed multiplier to apply.</param>
    /// <param name="reflection">Simplifies access to private game code.</param>
    public FishingTextHandler(float multiplier, IReflectionHelper reflection)
        : base(multiplier)
    {
        this.Reflection = reflection;
    }

    /// <inheritdoc />
    public override bool TryApply(int playerAnimationId)
    {
        Farmer player = Game1.player;

        // HIT! text
        if (player.CurrentTool is FishingRod && Game1.screenOverlayTempSprites.Any())
        {
            bool applied = this.ApplySkipsWhile(() =>
            {
                bool anyApplied = false;

                foreach (TemporaryAnimatedSprite sprite in Game1.screenOverlayTempSprites)
                {
                    if (sprite.id == 987654321)
                    {
                        sprite.update(Game1.currentGameTime);
                        anyApplied = true;
                    }
                }

                return anyApplied;
            });
            if (applied)
                return true;
        }

        // PERFECT text
        if (Game1.activeClickableMenu is BobberBar bobberMenu)
        {
            IReflectedField<SparklingText?> field = this.Reflection.GetField<SparklingText?>(bobberMenu, "sparkleText");
            return
                field.GetValue() is not null
                && this.ApplySkipsWhile(() =>
                {
                    if (field.GetValue() is null)
                        return false;

                    bobberMenu.update(Game1.currentGameTime);
                    return true;
                });
        }
        return false;
    }

}
