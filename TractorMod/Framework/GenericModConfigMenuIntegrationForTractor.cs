using System;
using System.Linq;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
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
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForTractor(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ModRegistry = modRegistry;
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            var defaultConfig = new ModConfig();

            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            // register
            menu.Register();

            if (this.ModRegistry.IsLoaded("NermNermNerm.QuestableTractor"))
                menu.AddParagraph(I18n.Config_QuestableTractorWarning);

            menu
                // main options
                .AddSectionTitle(I18n.Config_MainOptions)
                .AddNumberField(
                    name: I18n.Config_Distance_Name,
                    tooltip: () => I18n.Config_Distance_Tooltip(defaultValue: defaultConfig.Distance, maxRecommendedValue: 15),
                    get: config => config.Distance,
                    set: (config, value) => config.Distance = value,
                    min: 0,
                    max: 16
                )
                .AddNumberField(
                    name: I18n.Config_Speed_Name,
                    tooltip: () => I18n.Config_Speed_Tooltip(defaultValue: defaultConfig.TractorSpeed),
                    get: config => config.TractorSpeed,
                    set: (config, value) => config.TractorSpeed = value,
                    min: -5,
                    max: 10
                )
                .AddNumberField(
                    name: I18n.Config_MagneticRadius_Name,
                    tooltip: () => I18n.Config_MagneticRadius_Tooltip(defaultConfig.MagneticRadius),
                    get: config => config.MagneticRadius,
                    set: (config, value) => config.MagneticRadius = value,
                    min: 0,
                    max: 1000
                )
                .AddNumberField(
                    name: I18n.Config_BuildPrice_Name,
                    tooltip: () => I18n.Config_BuildPrice_Tooltip(defaultValue: defaultConfig.BuildPrice),
                    get: config => config.BuildPrice,
                    set: (config, value) => config.BuildPrice = value,
                    min: 0,
                    max: 1_000_000
                )
                .AddCheckbox(
                    name: I18n.Config_CanSummonWithoutGarage_Name,
                    tooltip: I18n.Config_CanSummonWithoutGarage_Tooltip,
                    get: config => config.CanSummonWithoutGarage,
                    set: (config, value) => config.CanSummonWithoutGarage = value
                )
                .AddCheckbox(
                    name: I18n.Config_InvincibleOnTractor_Name,
                    tooltip: I18n.Config_InvincibleOnTractor_Tooltip,
                    get: config => config.InvincibleOnTractor,
                    set: (config, value) => config.InvincibleOnTractor = value
                )
                .AddCheckbox(
                    name: I18n.Config_HighlightRadius_Name,
                    tooltip: I18n.Config_HighlightRadius_Tooltip,
                    get: config => config.HighlightRadius,
                    set: (config, value) => config.HighlightRadius = value
                )

                // audio
                .AddSectionTitle(I18n.Config_Audio)
                .AddDropdown(
                    name: I18n.Config_TractorSounds_Name,
                    tooltip: () => I18n.Config_TractorSounds_Tooltip(defaultValue: I18n.GetByKey($"config.tractor-sounds.value.{defaultConfig.SoundEffects}")),
                    formatAllowedValue: value => I18n.GetByKey($"config.tractor-sounds.value.{value}"),
                    allowedValues: Enum.GetNames<TractorSoundType>(),
                    get: config => config.SoundEffects.ToString(),
                    set: (config, value) => config.SoundEffects = Enum.Parse<TractorSoundType>(value)
                )
                .AddNumberField(
                    name: I18n.Config_TractorVolume_Name,
                    tooltip: () => I18n.Config_TractorVolume_Tooltip(defaultValue: defaultConfig.SoundEffectsVolume),
                    get: config => config.SoundEffectsVolume,
                    set: (config, value) => config.SoundEffectsVolume = value,
                    min: 0,
                    max: 100
                )

                // controls
                .AddSectionTitle(I18n.Config_Controls)
                .AddKeyBinding(
                    name: I18n.Config_SummonKey_Name,
                    tooltip: I18n.Config_SummonKey_Tooltip,
                    get: config => config.Controls.SummonTractor,
                    set: (config, value) => config.Controls.SummonTractor = value
                )
                .AddKeyBinding(
                    name: I18n.Config_DismissKey_Name,
                    tooltip: I18n.Config_DismissKey_Tooltip,
                    get: config => config.Controls.DismissTractor,
                    set: (config, value) => config.Controls.DismissTractor = value
                )
                .AddKeyBinding(
                    name: I18n.Config_HoldToActivateKey_Name,
                    tooltip: I18n.Config_HoldToActivateKey_Tooltip,
                    get: config => config.Controls.HoldToActivate,
                    set: (config, value) => config.Controls.HoldToActivate = value
                )

                // axe
                .AddSectionTitle(I18n.Config_Axe)
                .AddCheckbox(
                    name: I18n.Config_ChopFruitTreesSeeds_Name,
                    tooltip: I18n.Config_ChopFruitTreesSeeds_Tooltip,
                    get: config => config.StandardAttachments.Axe.ClearFruitTreeSeeds,
                    set: (config, value) => config.StandardAttachments.Axe.ClearFruitTreeSeeds = value
                )
                .AddCheckbox(
                    name: I18n.Config_ChopFruitTreesSaplings_Name,
                    tooltip: I18n.Config_ChopFruitTreesSaplings_Tooltip,
                    get: config => config.StandardAttachments.Axe.ClearFruitTreeSaplings,
                    set: (config, value) => config.StandardAttachments.Axe.ClearFruitTreeSaplings = value
                )
                .AddCheckbox(
                    name: I18n.Config_ChopFruitTreesGrown_Name,
                    tooltip: I18n.Config_ChopFruitTreesGrown_Tooltip,
                    get: config => config.StandardAttachments.Axe.CutGrownFruitTrees,
                    set: (config, value) => config.StandardAttachments.Axe.CutGrownFruitTrees = value
                )
                .AddCheckbox(
                    name: I18n.Config_ChopTreesSeeds_Name,
                    tooltip: I18n.Config_ChopTreesSeeds_Tooltip,
                    get: config => config.StandardAttachments.Axe.ClearTreeSeeds,
                    set: (config, value) => config.StandardAttachments.Axe.ClearTreeSeeds = value
                )
                .AddCheckbox(
                    name: I18n.Config_ChopTreesSaplings_Name,
                    tooltip: I18n.Config_ChopTreesSaplings_Tooltip,
                    get: config => config.StandardAttachments.Axe.ClearTreeSaplings,
                    set: (config, value) => config.StandardAttachments.Axe.ClearTreeSaplings = value
                )
                .AddCheckbox(
                    name: I18n.Config_ChopTreesGrown_Name,
                    tooltip: I18n.Config_ChopTreesGrown_Tooltip,
                    get: config => config.StandardAttachments.Axe.CutGrownTrees,
                    set: (config, value) => config.StandardAttachments.Axe.CutGrownTrees = value
                )
                .AddCheckbox(
                    name: I18n.Config_ChopTreesStumps_Name,
                    tooltip: I18n.Config_ChopTreesStumps_Tooltip,
                    get: config => config.StandardAttachments.Axe.CutTreeStumps,
                    set: (config, value) => config.StandardAttachments.Axe.CutTreeStumps = value
                )
                .AddCheckbox(
                    name: I18n.Config_ChopBushes_Name,
                    tooltip: I18n.Config_ChopBushes_Tooltip,
                    get: config => config.StandardAttachments.Axe.CutBushes,
                    set: (config, value) => config.StandardAttachments.Axe.CutBushes = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearDeadCrops_Name,
                    tooltip: I18n.Config_ClearDeadCrops_Tooltip,
                    get: config => config.StandardAttachments.Axe.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.Axe.ClearDeadCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearLiveCrops_Name,
                    tooltip: I18n.Config_ClearLiveCrops_Tooltip,
                    get: config => config.StandardAttachments.Axe.ClearLiveCrops,
                    set: (config, value) => config.StandardAttachments.Axe.ClearLiveCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestGiantCrops_Name,
                    tooltip: I18n.Config_HarvestGiantCrops_Tooltip,
                    get: config => config.StandardAttachments.Axe.CutGiantCrops,
                    set: (config, value) => config.StandardAttachments.Axe.CutGiantCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearDebris_Name,
                    tooltip: I18n.Config_ClearDebris_Tooltip,
                    get: config => config.StandardAttachments.Axe.ClearDebris,
                    set: (config, value) => config.StandardAttachments.Axe.ClearDebris = value
                )

                // hoe
                .AddSectionTitle(I18n.Config_Hoe)
                .AddCheckbox(
                    name: I18n.Config_TillDirt_Name,
                    tooltip: I18n.Config_TillDirt_Tooltip,
                    get: config => config.StandardAttachments.Hoe.TillDirt,
                    set: (config, value) => config.StandardAttachments.Hoe.TillDirt = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearWeeds_Name,
                    tooltip: I18n.Config_ClearWeeds_Tooltip,
                    get: config => config.StandardAttachments.Hoe.ClearWeeds,
                    set: (config, value) => config.StandardAttachments.Hoe.ClearWeeds = value
                )
                .AddCheckbox(
                    name: I18n.Config_DigArtifactSpots_Name,
                    tooltip: I18n.Config_DigArtifactSpots_Tooltip,
                    get: config => config.StandardAttachments.Hoe.DigArtifactSpots,
                    set: (config, value) => config.StandardAttachments.Hoe.DigArtifactSpots = value
                )
                .AddCheckbox(
                    name: I18n.Config_DigSeedSpots_Name,
                    tooltip: I18n.Config_DigSeedSpots_Tooltip,
                    get: config => config.StandardAttachments.Hoe.DigSeedSpots,
                    set: (config, value) => config.StandardAttachments.Hoe.DigSeedSpots = value
                )

                // pickaxe
                .AddSectionTitle(I18n.Config_Pickaxe)
                .AddCheckbox(
                    name: I18n.Config_ClearDebris_Name,
                    tooltip: I18n.Config_ClearDebris_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.ClearDebris,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearDebris = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearDeadCrops_Name,
                    tooltip: I18n.Config_ClearDeadCrops_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearDeadCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearTilledDirt_Name,
                    tooltip: I18n.Config_ClearTilledDirt_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.ClearDirt,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearDirt = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearWeeds_Name,
                    tooltip: I18n.Config_ClearWeeds_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.ClearWeeds,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearWeeds = value
                )
                .AddCheckbox(
                    name: I18n.Config_BreakFlooring_Name,
                    tooltip: I18n.Config_BreakFlooring_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.ClearFlooring,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearFlooring = value
                )
                .AddCheckbox(
                    name: I18n.Config_BreakBouldersAndMeteorites_Name,
                    tooltip: I18n.Config_BreakBouldersAndMeteorites_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.ClearBouldersAndMeteorites,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearBouldersAndMeteorites = value
                )
                .AddCheckbox(
                    name: I18n.Config_BreakObjects_Name,
                    tooltip: I18n.Config_BreakObjects_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.ClearObjects,
                    set: (config, value) => config.StandardAttachments.PickAxe.ClearObjects = value
                )
                .AddCheckbox(
                    name: I18n.Config_BreakMineContainers_Name,
                    tooltip: I18n.Config_BreakMineContainers_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.BreakMineContainers,
                    set: (config, value) => config.StandardAttachments.PickAxe.BreakMineContainers = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestMineSpawns_Name,
                    tooltip: I18n.Config_HarvestMineSpawns_Tooltip,
                    get: config => config.StandardAttachments.PickAxe.HarvestMineSpawns,
                    set: (config, value) => config.StandardAttachments.PickAxe.HarvestMineSpawns = value
                )

                // scythe
                .AddSectionTitle(I18n.Config_Scythe)
                .AddCheckbox(
                    name: I18n.Config_HarvestCrops_Name,
                    tooltip: I18n.Config_HarvestCrops_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestCrops,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestFlowers_Name,
                    tooltip: I18n.Config_HarvestFlowers_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestFlowers,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestFlowers = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestForage_Name,
                    tooltip: I18n.Config_HarvestForage_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestForage,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestForage = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestFruitTrees_Name,
                    tooltip: I18n.Config_HarvestFruitTrees_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestFruitTrees,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestFruitTrees = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestTreeMoss_Name,
                    tooltip: I18n.Config_HarvestTreeMoss_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestTreeMoss,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestTreeMoss = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestTreeSeeds_Name,
                    tooltip: I18n.Config_HarvestTreeSeeds_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestTreeSeeds,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestTreeSeeds = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestMachines_Name,
                    tooltip: I18n.Config_HarvestMachines_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestMachines,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestMachines = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestBlueGrass_Name,
                    tooltip: I18n.Config_HarvestBlueGrass_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestBlueGrass,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestBlueGrass = value
                )
                .AddCheckbox(
                    name: I18n.Config_HarvestNonBlueGrass_Name,
                    tooltip: I18n.Config_HarvestNonBlueGrass_Tooltip,
                    get: config => config.StandardAttachments.Scythe.HarvestNonBlueGrass,
                    set: (config, value) => config.StandardAttachments.Scythe.HarvestNonBlueGrass = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearDeadCrops_Name,
                    tooltip: I18n.Config_ClearDeadCrops_Tooltip,
                    get: config => config.StandardAttachments.Scythe.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.Scythe.ClearDeadCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearWeeds_Name,
                    tooltip: I18n.Config_ClearWeeds_Tooltip,
                    get: config => config.StandardAttachments.Scythe.ClearWeeds,
                    set: (config, value) => config.StandardAttachments.Scythe.ClearWeeds = value
                )

                // melee blunt weapons
                .AddSectionTitle(I18n.Config_MeleeBlunt)
                .AddCheckbox(
                    name: I18n.Config_AttackMonsters_Name,
                    tooltip: I18n.Config_AttackMonsters_Tooltip,
                    get: config => config.StandardAttachments.MeleeBlunt.AttackMonsters,
                    set: (config, value) => config.StandardAttachments.MeleeBlunt.AttackMonsters = value
                )
                .AddCheckbox(
                    name: I18n.Config_BreakMineContainers_Name,
                    tooltip: I18n.Config_BreakMineContainers_Tooltip,
                    get: config => config.StandardAttachments.MeleeBlunt.BreakMineContainers,
                    set: (config, value) => config.StandardAttachments.MeleeBlunt.BreakMineContainers = value
                )

                // melee daggers
                .AddSectionTitle(I18n.Config_MeleeDagger)
                .AddCheckbox(
                    name: I18n.Config_AttackMonsters_Name,
                    tooltip: I18n.Config_AttackMonsters_Tooltip,
                    get: config => config.StandardAttachments.MeleeDagger.AttackMonsters,
                    set: (config, value) => config.StandardAttachments.MeleeDagger.AttackMonsters = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearDeadCrops_Name,
                    tooltip: I18n.Config_ClearDeadCrops_Tooltip,
                    get: config => config.StandardAttachments.MeleeDagger.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.MeleeDagger.ClearDeadCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_BreakMineContainers_Name,
                    tooltip: I18n.Config_BreakMineContainers_Tooltip,
                    get: config => config.StandardAttachments.MeleeDagger.BreakMineContainers,
                    set: (config, value) => config.StandardAttachments.MeleeDagger.BreakMineContainers = value
                )

                // melee sword
                .AddSectionTitle(I18n.Config_MeleeSword)
                .AddCheckbox(
                    name: I18n.Config_AttackMonsters_Name,
                    tooltip: I18n.Config_AttackMonsters_Tooltip,
                    get: config => config.StandardAttachments.MeleeSword.AttackMonsters,
                    set: (config, value) => config.StandardAttachments.MeleeSword.AttackMonsters = value
                )
                .AddCheckbox(
                    name: I18n.Config_ClearDeadCrops_Name,
                    tooltip: I18n.Config_ClearDeadCrops_Tooltip,
                    get: config => config.StandardAttachments.MeleeSword.ClearDeadCrops,
                    set: (config, value) => config.StandardAttachments.MeleeSword.ClearDeadCrops = value
                )
                .AddCheckbox(
                    name: I18n.Config_BreakMineContainers_Name,
                    tooltip: I18n.Config_BreakMineContainers_Tooltip,
                    get: config => config.StandardAttachments.MeleeSword.BreakMineContainers,
                    set: (config, value) => config.StandardAttachments.MeleeSword.BreakMineContainers = value
                )

                // other
                .AddSectionTitle(I18n.Config_OtherTools)
                .AddCheckbox(
                    name: I18n.Config_MilkPail_Name,
                    tooltip: I18n.Config_MilkPail_Tooltip,
                    get: config => config.StandardAttachments.MilkPail.Enable,
                    set: (config, value) => config.StandardAttachments.MilkPail.Enable = value
                )
                .AddCheckbox(
                    name: I18n.Config_Shears_Name,
                    tooltip: I18n.Config_Shears_Tooltip,
                    get: config => config.StandardAttachments.Shears.Enable,
                    set: (config, value) => config.StandardAttachments.Shears.Enable = value
                )
                .AddCheckbox(
                    name: I18n.Config_WateringCan_Name,
                    tooltip: I18n.Config_WateringCan_Tooltip,
                    get: config => config.StandardAttachments.WateringCan.Enable,
                    set: (config, value) => config.StandardAttachments.WateringCan.Enable = value
                )
                .AddCheckbox(
                    name: I18n.Config_Fertilizer_Name,
                    tooltip: I18n.Config_Fertilizer_Tooltip,
                    get: config => config.StandardAttachments.Fertilizer.Enable,
                    set: (config, value) => config.StandardAttachments.Fertilizer.Enable = value
                )
                .AddCheckbox(
                    name: I18n.Config_GrassStarters_Name,
                    tooltip: I18n.Config_GrassStarters_Tooltip,
                    get: config => config.StandardAttachments.GrassStarter.Enable,
                    set: (config, value) => config.StandardAttachments.GrassStarter.Enable = value
                )
                .AddCheckbox(
                    name: I18n.Config_Seeds_Name,
                    tooltip: I18n.Config_Seeds_Tooltip,
                    get: config => config.StandardAttachments.Seeds.Enable,
                    set: (config, value) => config.StandardAttachments.Seeds.Enable = value
                )
                .AddCheckbox(
                    name: I18n.Config_SeedBags_Name,
                    tooltip: I18n.Config_SeedBags_Tooltip,
                    get: config => config.StandardAttachments.SeedBagMod.Enable,
                    set: (config, value) => config.StandardAttachments.SeedBagMod.Enable = value,
                    enable: this.ModRegistry.IsLoaded(SeedBagAttachment.ModId)
                )
                .AddCheckbox(
                    name: I18n.Config_Slingshot_Name,
                    tooltip: I18n.Config_Slingshot_Tooltip,
                    get: config => config.StandardAttachments.Slingshot.Enable,
                    set: (config, value) => config.StandardAttachments.Slingshot.Enable = value
                )

                // custom tools
                .AddSectionTitle(I18n.Config_CustomTools)
                .AddTextbox(
                    name: I18n.Config_CustomToolNames_Name,
                    tooltip: I18n.Config_CustomToolNames_Tooltip,
                    get: config => string.Join(", ", config.CustomAttachments),
                    set: (config, value) => config.CustomAttachments = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray()
                );
        }
    }
}
