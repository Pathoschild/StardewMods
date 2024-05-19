using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the chest-open animation.</summary>
/// <remarks>See game logic in <see cref="Chest.checkForAction"/>.</remarks>
internal class OpenChestHandler : BaseAnimationHandler
{
    /*********
     ** Fields
     *********/
    /// <summary>The chests in the current location.</summary>
    private List<Chest> Chests = [];

    /*********
     ** Public methods
     *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="multiplier">The animation speed multiplier to apply.</param>
    public OpenChestHandler(float multiplier) : base(multiplier)
    {
    }

    /// <summary>Perform any updates needed when the player enters a new location.</summary>
    /// <param name="location">The new location.</param>
    public override void OnNewLocation(GameLocation location)
    {
        this.Chests = location.objects.Values.OfType<Chest>().ToList();
    }

    /// <summary>Get whether the animation is currently active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override bool IsEnabled(int playerAnimationID)
    {
        return this.GetOpeningChest() != null;
    }

    /// <summary>Perform any logic needed on update while the animation is active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override void Update(int playerAnimationID)
    {
        this.ApplySkips(
            () => this.GetOpeningChest()!.updateWhenCurrentLocation(Game1.currentGameTime),
            () => this.GetOpeningChest() == null
        );
    }

    /*********
     ** Private methods
     *********/
    /// <summary>Get the chest in the current location which is currently opening.</summary>
    private Chest? GetOpeningChest()
    {
        return this.Chests.FirstOrDefault(chest => chest.frameCounter.Value > -1);
    }
}
