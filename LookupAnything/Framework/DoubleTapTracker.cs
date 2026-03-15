using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework;

/// <summary>Tracks double-taps for a key.</summary>
internal class DoubleTapTracker
{
    /*********
    ** Fields
    *********/
    /// <summary>The maximum milliseconds between two taps to count as a double-tap.</summary>
    private const double DoubleTapThresholdMs = 400;

    /// <summary>The <see cref="GameTime.TotalGameTime"/> milliseconds when the button was last pressed.</summary>
    private double LastTapTime;


    /*********
    ** Public methods
    *********/
    /// <summary>Receive a tap of the tracked button.</summary>
    /// <returns>Returns whether this constitutes the second tap in a double-tap.</returns>
    public bool ReceiveButtonPress()
    {
        double time = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
        double elapsed = time - this.LastTapTime;

        if (elapsed is <= DoubleTapThresholdMs and > 0)
        {
            this.LastTapTime = 0;
            return true;
        }

        this.LastTapTime = time;
        return false;
    }
}
