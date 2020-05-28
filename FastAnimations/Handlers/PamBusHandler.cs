using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles Pam's bus arriving/departing animation.</summary>
    /// <remarks>See game logic in <see cref="BusStop.UpdateWhenCurrentLocation"/> and <see cref="Desert.UpdateWhenCurrentLocation"/>.</remarks>
    internal class PamBusHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public PamBusHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            if (Game1.currentLocation is BusStop stop)
                return stop.drivingOff || stop.drivingBack;
            if (Game1.currentLocation is Desert desert)
                return desert.drivingOff || desert.drivingBack;

            return false;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            GameLocation location = Game1.currentLocation;

            this.ApplySkips(
                run: () => location.UpdateWhenCurrentLocation(Game1.currentGameTime),
                until: () => !this.IsEnabled(playerAnimationID)
            );
        }
    }
}
