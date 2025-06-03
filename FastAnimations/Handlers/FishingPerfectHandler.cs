using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Tools;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the fishing "perfect" toast animation.</summary>
/// <remarks>See game logic in <see cref="BobberBar.update"/>.</remarks>
internal sealed class FishingPerfectHandler : BaseAnimationHandler
{
    private readonly IModHelper helper;

    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public FishingPerfectHandler(IModHelper helper, float multiplier)
        : base(multiplier)
    {
        this.helper = helper;
    }

    /// <inheritdoc />
    public override bool TryApply(int playerAnimationId)
    {
        if (Game1.activeClickableMenu is BobberBar bobberMenu)
        {
            var field = this.helper.Reflection.GetField<SparklingText?>(bobberMenu, "sparkleText");
            return
                field.GetValue() is not null
                && this.ApplySkipsWhile(() => {
                    if (field.GetValue() is not null) {
                        bobberMenu.update(Game1.currentGameTime);
                    }
                    return field.GetValue() is not null;
                });
        }
        return false;
    }

}
