using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Commands;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework.Commands
{
    /// <summary>A console command which sets the current farm type.</summary>
    internal class SetFarmTypeCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>The vanilla farm type IDs.</summary>
        private readonly ISet<int> VanillaFarmTypes = new HashSet<int>(
            Enumerable.Range(0, Farm.layout_max + 1)
        );


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="config">The mod configuration.</param>
        public SetFarmTypeCommand(IMonitor monitor, ModConfig config)
            : base(monitor, "set_farm_type")
        {
            this.Config = config;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return $@"
                small_beach_farm set_farm_type

                   Usage: small_beach_farm set_farm_type
                   If a save is loaded, migrates the current farm to the Small Beach Farm type.

                   Usage: small_beach_farm set_farm_type <id>
                   If a save is loaded, migrates the current farm to a vanilla farm type. Possible values: {string.Join(", ", this.VanillaFarmTypes.Select(id => $"{id} ({this.GetFarmLabel(id)}"))}.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            // validation checks
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("You must load a save to use this command.", LogLevel.Error);
                return;
            }

            switch (args.Length)
            {
                // migrate to Small Beach Farm
                case 0:
                    {
                        if (Game1.whichFarm == this.Config.ReplaceFarmID)
                        {
                            this.Monitor.Log("Your current farm is already a Small Beach Farm.", LogLevel.Info);
                            return;
                        }

                        this.SetFarmType(this.Config.ReplaceFarmID);
                        this.Monitor.Log("Your current farm has been converted into a Small Beach Farm.", LogLevel.Warn);
                    }
                    break;

                // migrate to specified farm type
                case 1:
                    {
                        if (!int.TryParse(args[0], out int type) || !this.VanillaFarmTypes.Contains(type))
                        {
                            this.Monitor.Log($"Invalid farm type '{args[0]}'. Enter `small_beach_farm help` for more info.", LogLevel.Error);
                            return;
                        }
                        if (Game1.whichFarm == type)
                        {
                            this.Monitor.Log($"Your current farm is already set to {type} ({this.GetFarmLabel(type)}).", LogLevel.Info);
                            return;
                        }

                        this.SetFarmType(type);
                        this.Monitor.Log($"Your current farm has been converted to {type} ({this.GetFarmLabel(type)}).", LogLevel.Warn);
                    }
                    break;

                default:
                    this.Monitor.Log("Invalid usage. Enter `small_beach_farm help` for more info.", LogLevel.Error);
                    break;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Change the farm type to the given value.</summary>
        /// <param name="type">The farm type ID.</param>
        private void SetFarmType(int type)
        {
            Game1.whichFarm = type;

            Farm farm = Game1.getFarm();
            farm.mapPath.Value = $@"Maps\{Farm.getMapNameFromTypeInt(Game1.whichFarm)}";
            farm.reloadMap();
        }

        /// <summary>Get the display name for a vanilla farm type.</summary>
        /// <param name="type">The farm type.</param>
        private string GetFarmLabel(int type)
        {
            string translationKey = type switch
            {
                Farm.default_layout => "Character_FarmStandard",
                Farm.riverlands_layout => "Character_FarmFishing",
                Farm.forest_layout => "Character_FarmForaging",
                Farm.mountains_layout => "Character_FarmMining",
                Farm.combat_layout => "Character_FarmCombat",
                Farm.fourCorners_layout => "Character_FarmFourCorners",
                Farm.beach_layout => "Character_FarmBeach",
                _ => null
            };

            return translationKey != null
                ? Game1.content.LoadString(@$"Strings\UI:{translationKey}").Split('_')[0]
                : type.ToString();
        }
    }
}
