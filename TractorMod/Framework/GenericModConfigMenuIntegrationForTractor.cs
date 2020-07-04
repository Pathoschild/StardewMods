using System;
using System.Collections.Generic;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Input;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForTractor
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

        /// <summary>Get the parsed key bindings.</summary>
        private readonly Func<ModConfigKeys> GetKeys;

        /// <summary>An API for fetching metadata about loaded mods.</summary>
        private readonly IModRegistry ModRegistry;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="getKeys">Get the parsed key bindings.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForTractor(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Func<ModConfigKeys> getKeys, Action reset, Action saveAndApply)
        {
            this.ModRegistry = modRegistry;
            this.GetKeys = getKeys;
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            // get control label
            string controlLabel = "Configure the key bindings for the tractor.";
            {
                var keys = this.GetKeys();

                List<string> complexKeys = new List<string>();
                if (!this.IsEditable(keys.SummonTractor))
                    complexKeys.Add("Summon Tractor");
                if (!this.IsEditable(keys.DismissTractor))
                    complexKeys.Add("Dismiss Tractor");
                if (!this.IsEditable(keys.HoldToActivate))
                    complexKeys.Add("Hold to Activate");

                if (complexKeys.Any())
                    controlLabel += $" Some key bindings ({string.Join(", ", complexKeys)}) can't be edited because they contain multiple keys; you'll need to edit them through the config.json file.";

                controlLabel += " To disable a control, edit the config.json and set it to a empty string.";
            }

            // register
            menu
                .RegisterConfig()

                // main options
                .AddLabel("Main Options")
                .AddNumberField(
                    label: "Distance",
                    description: "The number of tiles in each direction around the tractor to affect (in addition to the tile under it). Default 1; a value of 15 covers most of the visible screen, and higher values may negatively impact game performance.",
                    get: config => config.Distance,
                    set: (config, value) => config.Distance = value,
                    min: 1,
                    max: 16
                )
                .AddNumberField(
                    label: "Tractor Speed",
                    description: "The speed modifier when riding a tractor. Default -2.",
                    get: config => config.TractorSpeed,
                    set: (config, value) => config.TractorSpeed = value,
                    min: -5,
                    max: 10
                )
                .AddNumberField(
                    label: "Magnetic Radius",
                    description: "The item magnetism amount (higher values attract items from father away). Default 384.",
                    get: config => config.MagneticRadius,
                    set: (config, value) => config.MagneticRadius = value,
                    min: 0,
                    max: 1000
                )
                .AddNumberField(
                    label: "Build Price",
                    description: "The gold price to buy a tractor garage. Default 150,000g.",
                    get: config => config.BuildPrice,
                    set: (config, value) => config.BuildPrice = value,
                    min: 0,
                    max: 1_000_000
                )
                .AddCheckbox(
                    label: "Can Summon Without Garage",
                    description: "Whether you can summon a temporary tractor without building a garage first. Default false.",
                    get: config => config.CanSummonWithoutGarage,
                    set: (config, value) => config.CanSummonWithoutGarage = value
                )
                .AddCheckbox(
                    label: "Invincible on Tractor",
                    description: "Whether you should be immune to damage from any source when riding the tractor. Default true.",
                    get: config => config.InvincibleOnTractor,
                    set: (config, value) => config.InvincibleOnTractor = value
                )
                .AddCheckbox(
                    label: "Highlight Radius (Debug)",
                    description: "Whether to highlight the tractor radius when riding one, to help visualize the distance option. Default false.",
                    get: config => config.HighlightRadius,
                    set: (config, value) => config.HighlightRadius = value
                )

                // controls
                .AddLabel("Controls", controlLabel)
                .AddKeyBinding(
                    label: "Summon Tractor",
                    description: "Warp an available tractor to your position. Default backspace.",
                    get: config => this.GetSingleButton(this.GetKeys().SummonTractor),
                    set: (config, value) => config.Controls.SummonTractor = value.ToString(),
                    enable: this.IsEditable(this.GetKeys().SummonTractor)
                )
                .AddKeyBinding(
                    label: "Dismiss Tractor",
                    description: "Return the tractor you're riding to its home.",
                    get: config => this.GetSingleButton(this.GetKeys().DismissTractor),
                    set: (config, value) => config.Controls.DismissTractor = value.ToString(),
                    enable: this.IsEditable(this.GetKeys().DismissTractor)
                )
                .AddKeyBinding(
                    label: "Hold to Activate",
                    description: "If specified, the tractor will only do something while you're holding this button. If nothing is specified, the tractor will work automatically while you're riding it.",
                    get: config => this.GetSingleButton(this.GetKeys().HoldToActivate),
                    set: (config, value) => config.Controls.HoldToActivate = value.ToString(),
                    enable: this.IsEditable(this.GetKeys().HoldToActivate)
                )

                // axe
                .AddLabel("Axe Features")
                .AddCheckbox(
                    label: "Chop Fruit Trees (Seeds)",
                    description: "Whether the axe clears fruit tree seeds. Default false.",
                    get: config => config.StandardAttachments.Axe.ClearFruitTreeSeeds,
                    set: (config, value) => config.StandardAttachments.Axe.ClearFruitTreeSeeds = value
                )
                .AddCheckbox(
                    label: "Chop Fruit Trees (Saplings)",
                    description: "Whether the axe clears fruit trees which aren't fully grown. Default false.",
                    get: config => config.StandardAttachments.Axe.ClearFruitTreeSaplings,
                    set: (config, value) => config.StandardAttachments.Axe.ClearFruitTreeSaplings = value
                )
                .AddCheckbox(
                    label: "Chop Fruit Trees (Grown)",
                    description: "Whether the axe cuts fully-grown fruit trees. Default false.",
                    get: config => config.StandardAttachments.Axe.CutGrownFruitTrees,
                    set: (config, value) => config.StandardAttachments.Axe.CutGrownFruitTrees = value
                )
                .AddCheckbox(
                    label: "Chop Trees (Seeds)",
                    description: "Whether the axe clears non-fruit tree seeds. Default false.",
                    get: config => config.StandardAttachments.Axe.ClearTreeSeeds,
                    set: (config, value) => config.StandardAttachments.Axe.ClearTreeSeeds = value
                )
                .AddCheckbox(
                    label: "Chop Trees (Saplings)",
                    description: "Whether the axe clears non-fruit trees which aren't fully grown. Default false.",
                    get: config => config.StandardAttachments.Axe.ClearTreeSaplings,
                    set: (config, value) => config.StandardAttachments.Axe.ClearTreeSaplings = value
                )
                .AddCheckbox(
                    label: "Chop Trees (Grown)",
                    description: "Whether the axe clears fully-grown non-fruit trees. Default false.",
                    get: config => config.StandardAttachments.Axe.CutGrownTrees,
                    set: (config, value) => config.StandardAttachments.Axe.CutGrownTrees = value
                )
                .AddCheckbox(
                    label: "Chop Bushes",
                    description: "Whether the axe cuts choppable bushes. Default false.",
                    get: config => config.StandardAttachments.Axe.CutBushes,
                    set: (config, value) => config.StandardAttachments.Axe.CutBushes = value
                )
                .AddCheckbox(
                    label: "Chop Crops (Dead)",
                    description: "Whether the axe clears dead crops. Default true.",
                    get: config => config.StandardAttachments.Axe.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.Axe.ClearDeadCrops = value
                )
                .AddCheckbox(
                    label: "Chop Crops (Live)",
                    description: "Whether the axe clears live crops. Default false.",
                    get: config => config.StandardAttachments.Axe.ClearLiveCrops,
                    set: (config, value) => config.StandardAttachments.Axe.ClearLiveCrops = value
                )
                .AddCheckbox(
                    label: "Chop Crops (Giant)",
                    description: "Whether the axe cuts giant crops. Default true.",
                    get: config => config.StandardAttachments.Axe.CutGiantCrops,
                    set: (config, value) => config.StandardAttachments.Axe.CutGiantCrops = value
                )
                .AddCheckbox(
                    label: "Chop Debris",
                    description: "Whether the axe clears debris like weeds, twigs, giant stumps, and fallen logs. Default true.",
                    get: config => config.StandardAttachments.Axe.ClearDebris,
                    set: (config, value) => config.StandardAttachments.Axe.ClearDebris = value
                )

                // hoe
                .AddLabel("Hoe Features")
                .AddCheckbox(
                    label: "Till Dirt",
                    description: "Whether the hoe tills empty dirt. Default true.",
                    get: config => config.StandardAttachments.Hoe.TillDirt,
                    set: (config, value) => config.StandardAttachments.Hoe.TillDirt = value
                )
                .AddCheckbox(
                    label: "Clear Weeds",
                    description: "Whether the hoe clears weeds. Default true.",
                    get: config => config.StandardAttachments.Hoe.ClearWeeds,
                    set: (config, value) => config.StandardAttachments.Hoe.ClearWeeds = value
                )
                .AddCheckbox(
                    label: "Dig Artifact Spots",
                    description: "Whether the hoe digs artifact spots. Default true.",
                    get: config => config.StandardAttachments.Hoe.DigArtifactSpots,
                    set: (config, value) => config.StandardAttachments.Hoe.DigArtifactSpots = value
                )

                // pickaxe
                .AddLabel("Pickaxe Features")
                .AddCheckbox(
                    label: "Clear Debris",
                    description: "Whether the pickaxe clears debris. Default true.",
                    get: config => config.StandardAttachments.PickAxe.ClearDebris,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearDebris = value
                )
                .AddCheckbox(
                    label: "Clear Dead Crops",
                    description: "Whether the pickaxe clears dead crops. Default true.",
                    get: config => config.StandardAttachments.PickAxe.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearDeadCrops = value
                )
                .AddCheckbox(
                    label: "Clear Tilled Dirt",
                    description: "Whether the pickaxe clears tilled dirt. Default true.",
                    get: config => config.StandardAttachments.PickAxe.ClearDirt,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearDirt = value
                )
                .AddCheckbox(
                    label: "Clear Weeds",
                    description: "Whether the pickaxe clears weeds. Default true.",
                    get: config => config.StandardAttachments.PickAxe.ClearWeeds,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearWeeds = value
                )
                .AddCheckbox(
                    label: "Break Flooring",
                    description: "Whether the pickaxe breaks placed flooring. Default false.",
                    get: config => config.StandardAttachments.PickAxe.ClearFlooring,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearFlooring = value
                )
                .AddCheckbox(
                    label: "Break Boulders and Meteorites",
                    description: "Whether the pickaxe breaks boulders and meteorites. Default true.",
                    get: config => config.StandardAttachments.PickAxe.ClearBouldersAndMeteorites,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearBouldersAndMeteorites = value
                )
                .AddCheckbox(
                    label: "Break Objects",
                    description: "Whether the pickaxe breaks placed objects. Default false.",
                    get: config => config.StandardAttachments.PickAxe.ClearObjects,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearObjects = value
                )
                .AddCheckbox(
                    label: "Break Mine Containers",
                    description: "Whether the pickaxe breaks containers in the mine. Default true.",
                    get: config => config.StandardAttachments.PickAxe.BreakMineContainers,
                    set: (config, value) => config.StandardAttachments.PickAxe.BreakMineContainers = value
                )

                // scythe
                .AddLabel("Scythe Features")
                .AddCheckbox(
                    label: "Harvest Crops",
                    description: "Whether the scythe harvests crops. Default true.",
                    get: config => config.StandardAttachments.Scythe.HarvestCrops,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestCrops = value
                )
                .AddCheckbox(
                    label: "Harvest Flowers",
                    description: "Whether the scythe harvests flowers. Default true.",
                    get: config => config.StandardAttachments.Scythe.HarvestFlowers,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestFlowers = value
                )
                .AddCheckbox(
                    label: "Harvest Forage",
                    description: "Whether the scythe harvests forage. Default true.",
                    get: config => config.StandardAttachments.Scythe.HarvestForage,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestForage = value
                )
                .AddCheckbox(
                    label: "Harvest Fruit Trees",
                    description: "Whether the scythe harvests fruit trees. Default true.",
                    get: config => config.StandardAttachments.Scythe.HarvestFruitTrees,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestFruitTrees = value
                )
                .AddCheckbox(
                    label: "Harvest Machines",
                    description: "Whether the scythe collects machine output. Default false.",
                    get: config => config.StandardAttachments.Scythe.HarvestMachines,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestMachines = value
                )
                .AddCheckbox(
                    label: "Harvest Grass",
                    description: "Whether the scythe cuts grass. If you have free silo space, this gives you hay as usual. Default true.",
                    get: config => config.StandardAttachments.Scythe.HarvestGrass,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestGrass = value
                )
                .AddCheckbox(
                    label: "Clear Dead Crops",
                    description: "Whether the scythe clears dead crops. Default true.",
                    get: config => config.StandardAttachments.Scythe.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.Scythe.ClearDeadCrops = value
                )
                .AddCheckbox(
                    label: "Clear Weeds",
                    description: "Whether the scythe clears weeds. Default true.",
                    get: config => config.StandardAttachments.Scythe.ClearWeeds,
                    set: (config, value) => config.StandardAttachments.Scythe.ClearWeeds = value
                )

                // melee weapon
                .AddLabel("Melee Weapon Features")
                .AddCheckbox(
                    label: "Attack Monsters",
                    description: "Whether melee weapons attack monsters. (This is massively overpowered due to the tractor tool speed.) Default false.",
                    get: config => config.StandardAttachments.MeleeWeapon.AttackMonsters,
                    set: (config, value) => config.StandardAttachments.MeleeWeapon.AttackMonsters = value
                )
                .AddCheckbox(
                    label: "Clear Dead Crops",
                    description: "Whether melee weapons clear dead crops. Default true.",
                    get: config => config.StandardAttachments.MeleeWeapon.AttackMonsters,
                    set: (config, value) => config.StandardAttachments.MeleeWeapon.AttackMonsters = value
                )
                .AddCheckbox(
                    label: "Break Mine Containers",
                    description: "Whether melee weapons break containers in the mine. Default true.",
                    get: config => config.StandardAttachments.MeleeWeapon.BreakMineContainers,
                    set: (config, value) => config.StandardAttachments.MeleeWeapon.BreakMineContainers = value
                )

                // other
                .AddLabel("Other Tools")
                .AddCheckbox(
                    label: "Enable Milk Pail",
                    description: "Whether to collect milk from farm animals using the milk pail. Default true.",
                    get: config => config.StandardAttachments.GrassStarter.Enable,
                    set: (config, value) => config.StandardAttachments.GrassStarter.Enable = value
                )
                .AddCheckbox(
                    label: "Enable Shears",
                    description: "Whether to collect wool from farm animals using the shears. Default true.",
                    get: config => config.StandardAttachments.Shears.Enable,
                    set: (config, value) => config.StandardAttachments.Shears.Enable = value
                )
                .AddCheckbox(
                    label: "Enable Watering Can",
                    description: "Whether to water nearby tiles. This doesn't consume water in the watering can. Default true.",
                    get: config => config.StandardAttachments.WateringCan.Enable,
                    set: (config, value) => config.StandardAttachments.WateringCan.Enable = value
                )
                .AddCheckbox(
                    label: "Enable Fertilizer",
                    description: "Whether to apply fertilizer to crops and tilled dirt. Default true.",
                    get: config => config.StandardAttachments.Fertilizer.Enable,
                    set: (config, value) => config.StandardAttachments.Fertilizer.Enable = value
                )
                .AddCheckbox(
                    label: "Enable Grass Starters",
                    description: "Whether to plant grass starters. Default true.",
                    get: config => config.StandardAttachments.GrassStarter.Enable,
                    set: (config, value) => config.StandardAttachments.GrassStarter.Enable = value
                )
                .AddCheckbox(
                    label: "Enable Seeds",
                    description: "Whether to plant seeds. Default true.",
                    get: config => config.StandardAttachments.Seeds.Enable,
                    set: (config, value) => config.StandardAttachments.Seeds.Enable = value
                )
                .AddCheckbox(
                    label: "Enable Seed Bags",
                    description: "Whether to plant seeds from the Seed Bag mod. Default true.",
                    get: config => config.StandardAttachments.SeedBagMod.Enable,
                    set: (config, value) => config.StandardAttachments.SeedBagMod.Enable = value,
                    enable: this.ModRegistry.IsLoaded(SeedBagAttachment.ModId)
                )
                .AddCheckbox(
                    label: "Enable Slingshot",
                    description: "Whether to fire the slingshot towards the cursor. (This is massively overpowered and will consume ammo voraciously due to the tractor tool speed.) Default false.",
                    get: config => config.StandardAttachments.Slingshot.Enable,
                    set: (config, value) => config.StandardAttachments.Slingshot.Enable = value
                )

                // custom tools
                .AddLabel("Custom Tools")
                .AddTextbox(
                    label: "Custom Tool Names",
                    description: "The custom items/tools to enable while riding the tractor. Tools will be used on each surrounding tile, while items will be put down. If you specify something that's already supported (like the axe), this overrides all limitations on its use. You must specify the exact internal name (not the translated display name), like 'Axe' or 'Mega Bomb'. Separate multiple values with commas.",
                    get: config => string.Join(", ", config.CustomAttachments),
                    set: (config, value) => config.CustomAttachments = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray()
                );
        }

        /// <summary>Get whether a key binding consists of a single key.</summary>
        /// <param name="binding">The key binding.</param>
        private bool IsEditable(KeyBinding binding)
        {
            SButton[][] sets = binding.ButtonSets;
            return
                sets.Length == 0
                || (sets.Length == 1 && sets[0].Length == 1);
        }

        /// <summary>Get the first button in a key binding, if any.</summary>
        /// <param name="binding">The key binding.</param>
        private SButton GetSingleButton(KeyBinding binding)
        {
            SButton[] set = binding.ButtonSets.FirstOrDefault();
            return set?.FirstOrDefault() ?? SButton.None;
        }
    }
}
