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
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /****
        ** Accumulators
        ****/
        /// <summary>When the regen was last applied.</summary>
        private double TimeSinceApplied;

        /// <summary>When the player last moved.</summary>
        private double TimeSinceMoved;

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
            // read config
            ModConfig config = helper.ReadConfig<ModConfig>();
            config.ConvertToMilliseconds();
            this.Config = config;

            // hook events
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
            if (!Context.IsPlayerFree)
            {
                this.TimeSinceApplied = 0;
                this.TimeSinceMoved = 0;
            }

            // collect info
            double elapsed = this.GetTimeSinceLastTick();
            bool isDoingSomething = this.IsPlayerDoingSomething();
            bool isResting = !isDoingSomething && this.TimeSinceMoved >= this.Config.RestDelay;
            bool isRunning = Context.IsPlayerFree && Game1.player.isMoving() && Game1.player.running;

            // update move timer
            if (isDoingSomething)
                this.TimeSinceMoved = 0;
            else
                this.TimeSinceMoved += elapsed;

            // accumulate regen
            if (isRunning)
            {
                this.AccumulatedHealth += this.Config.HealthRegen.Run * elapsed;
                this.AccumulatedStamina += this.Config.StaminaRegen.Run * elapsed;
            }
            if (isResting)
            {
                this.AccumulatedHealth += this.Config.HealthRegen.Rest * elapsed;
                this.AccumulatedStamina += this.Config.StaminaRegen.Rest * elapsed;
            }

            // apply regen
            if (this.TimeSinceApplied >= this.Config.RegenDelay)
            {
                Farmer player = Game1.player;
                if ((int)this.AccumulatedHealth != 0)
                {
                    player.health = (int)Math.Min(player.maxHealth, player.health + this.AccumulatedHealth);
                    this.AccumulatedHealth %= 1;
                }
                if ((int)this.AccumulatedStamina != 0)
                {
                    player.stamina = (int)Math.Min(player.maxStamina, player.stamina + this.AccumulatedStamina);
                    this.AccumulatedStamina %= 1;
                }
                this.TimeSinceApplied = 0;
            }
            else
                this.TimeSinceApplied += elapsed;
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
                !Context.CanPlayerMove
                || Game1.player.isMoving()
                || Game1.player.isEating
                || Game1.player.UsingTool
                || Game1.player.temporarilyInvincible; // took damage
        }
    }
}
