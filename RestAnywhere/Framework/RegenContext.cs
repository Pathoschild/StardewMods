using System;
using StardewModdingAPI;
using StardewValley;

namespace RestAnywhere.Framework
{
    /// <summary>The current regen context.</summary>
    internal class RegenContext
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The current player.</summary>
        public Farmer Player { get; private set; }

        /// <summary>Whether the player is doing something.</summary>
        public bool IsDoingSomething { get; private set; }

        /// <summary>Whether the player is moving.</summary>
        public bool IsMoving { get; private set; }

        /// <summary>The milliseconds elapsed since the last tick.</summary>
        public double TickTime { get; private set; }

        /// <summary>When the player last moved.</summary>
        public double TimeSinceMoved { get; private set; }

        /// <summary>Whether the player is walking.</summary>
        public bool IsWalking => this.IsMoving && !this.Player.running;

        /// <summary>Whether the player is running.</summary>
        public bool IsRunning => this.IsMoving && this.Player.running;

        /// <summary>Whether the player is resting.</summary>
        public bool IsResting { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Reset all timers.</summary>
        public void ResetTimers()
        {
            this.TickTime = 0;
            this.TimeSinceMoved = 0;
        }

        /// <summary>Update the context for the current tick.</summary>
        /// <param name="restDelay">The number of seconds the player should stop doing anything before they're considered to be resting.</param>
        public void Tick(double restDelay)
        {
            // main context
            this.Player = Game1.player;
            this.IsDoingSomething = this.GetIsDoingSomething();
            this.IsMoving = this.Player.isMoving();
            this.TickTime = this.GetTimeSinceLastTick();

            // derived context
            this.TimeSinceMoved = !this.IsDoingSomething
                ? this.TimeSinceMoved + this.TickTime
                : 0;
            this.IsResting = !this.IsDoingSomething && this.TimeSinceMoved >= restDelay;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the player is currently doing something.</summary>
        private bool GetIsDoingSomething()
        {
            return
                !Context.CanPlayerMove
                || Game1.player.isMoving()
                || Game1.player.isEating
                || Game1.player.UsingTool
                || Game1.player.temporarilyInvincible; // took damage
        }

        /// <summary>Get the number of milliseconds elapsed since the last update tick.</summary>
        private double GetTimeSinceLastTick()
        {
            return Game1.currentGameTime != null
                ? Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds
                : 0;
        }
    }
}
