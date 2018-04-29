using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>The current regen context.</summary>
        private readonly RegenContext RegenContext = new RegenContext();

        /// <summary>When the regen was last applied.</summary>
        private double TimeSinceApplied;

        /// <summary>The accumulated health regen to apply.</summary>
        private double AccumulatedHealth;

        /// <summary>The accumulated stamina regen to apply.</summary>
        private double AccumulatedStamina;

        /// <summary>The stamina rules to apply.</summary>
        private RegenRule[] Rules;


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

            // init rules
            this.Rules = this.GetRules(config).ToArray();

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
            // update context
            var context = this.RegenContext;
            if (!Context.IsPlayerFree)
            {
                context.ResetTimers();
                return;
            }
            context.Tick(this.Config.RestDelay);

            // accumulate regen
            double elapsed = context.TickTime;
            foreach (RegenRule rule in this.Rules)
            {
                if (rule.Applies(this.RegenContext))
                {
                    this.AccumulatedHealth += rule.HealthRegen * elapsed;
                    this.AccumulatedStamina += rule.StaminaRegen * elapsed;
                }
            }

            // apply regen
            if (this.TimeSinceApplied >= this.Config.RegenDelay)
            {
                Farmer player = context.Player;
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

        /// <summary>Get the regen rules to apply.</summary>
        /// <param name="config">The mod configuration.</param>
        private IEnumerable<RegenRule> GetRules(ModConfig config)
        {
            // note: regen is converted to milliseconds at this point
            yield return new RegenRule(config.HealthRegenPerSecond.Rest, config.StaminaRegenPerSecond.Rest, applies: context => Context.IsPlayerFree && context.IsResting);
            yield return new RegenRule(config.HealthRegenPerSecond.Walk, config.StaminaRegenPerSecond.Walk, applies: context => Context.IsPlayerFree && context.IsWalking);
            yield return new RegenRule(config.HealthRegenPerSecond.Run, config.StaminaRegenPerSecond.Run, applies: context => Context.IsPlayerFree && context.IsRunning);
        }
    }
}
