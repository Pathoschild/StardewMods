using System;
using RestAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace RestAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /****
        ** Configuration
        ****/
        /// <summary>The number of milliseconds the player should stop doing anything before they're considered to be resting.</summary>
        private double RestDelay;

        /// <summary>The number of milliseconds to accumulate regen before applying it.</summary>
        private readonly double ApplyDelay = 1000;

        /// <summary>The health regeneration to apply per millisecond when resting.</summary>
        private double HealthRegen;

        /// <summary>The stamina regeneration to apply per millisecond when resting.</summary>
        private double StaminaRegen;

        /****
        ** Accumulators
        ****/
        /// <summary>When the regen was last applied.</summary>
        private double LastApplied;

        /// <summary>When the player last moved.</summary>
        private double LastMoved;

        /// <summary>The accumulated health regen to apply.</summary>
        private double AccumulatedHealth;

        /// <summary>The accumulated stamina regen to apply.</summary>
        private double AccumulatedStamina;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            ModConfig config = helper.ReadConfig<ModConfig>();
            this.RestDelay = config.RestDelay * 1000;
            this.HealthRegen = config.HealthRegen / 1000;
            this.StaminaRegen = config.StaminaRegen / 1000;

            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method called when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            double now = this.GetGameEpoch();

            // reset if player is doing something
            if (this.IsPlayerDoingSomething())
            {
                this.LastApplied = now;
                this.LastMoved = now;
                this.AccumulatedHealth = 0;
                this.AccumulatedStamina = 0;
                return;
            }

            // accumulate regen
            bool isResting = (now - this.LastMoved) > this.RestDelay;
            if (isResting)
            {
                double elapsed = this.GetTimeSinceLastTick();
                this.AccumulatedHealth += this.HealthRegen * elapsed;
                this.AccumulatedStamina += this.StaminaRegen * elapsed;
            }

            // apply regen
            double timeSinceApplied = now - this.LastApplied;
            if (timeSinceApplied >= this.ApplyDelay)
            {
                Farmer player = Game1.player;
                player.health = (int)Math.Min(player.maxHealth, player.health + this.AccumulatedHealth);
                player.stamina = (int)Math.Min(player.maxStamina, player.stamina + this.AccumulatedStamina);

                this.LastApplied = now;
                this.AccumulatedStamina = 0;
                this.AccumulatedHealth = 0;
            }
        }

        /// <summary>Get the number of milliseconds elapsed since the game started.</summary>
        private double GetGameEpoch()
        {
            return Game1.currentGameTime != null
                ? Game1.currentGameTime.TotalGameTime.TotalMilliseconds
                : 0;
        }

        /// <summary>Get the number of milliseconds elapsed since the last update tick.</summary>
        private double GetTimeSinceLastTick()
        {
            return Game1.currentGameTime != null
                ? Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds
                : 0;
        }

        /// <summary>Get whether the player is currently doing something.</summary>
        private bool IsPlayerDoingSomething()
        {
            return
                !Context.IsPlayerFree
                || Game1.player.isMoving()
                || Game1.player.isEating
                || Game1.player.UsingTool
                || Game1.player.temporarilyInvincible; // took damage
        }
    }
}
