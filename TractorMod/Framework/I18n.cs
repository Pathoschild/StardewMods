using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "Tractor Power".</summary>
        public static string Buff_Name()
        {
            return I18n.GetByKey("buff.name");
        }

        /// <summary>Get a translation equivalent to "Tractor Garage".</summary>
        public static string Garage_Name()
        {
            return I18n.GetByKey("garage.name");
        }

        /// <summary>Get a translation equivalent to "A garage to store your tractor. Tractor included!".</summary>
        public static string Garage_Description()
        {
            return I18n.GetByKey("garage.description");
        }

        /// <summary>Get a translation equivalent to "Main Options".</summary>
        public static string Config_MainOptions()
        {
            return I18n.GetByKey("config.main-options");
        }

        /// <summary>Get a translation equivalent to "Controls".</summary>
        public static string Config_Controls()
        {
            return I18n.GetByKey("config.controls");
        }

        /// <summary>Get a translation equivalent to "Axe Features".</summary>
        public static string Config_Axe()
        {
            return I18n.GetByKey("config.axe");
        }

        /// <summary>Get a translation equivalent to "Hoe Features".</summary>
        public static string Config_Hoe()
        {
            return I18n.GetByKey("config.hoe");
        }

        /// <summary>Get a translation equivalent to "Pickaxe Features".</summary>
        public static string Config_Pickaxe()
        {
            return I18n.GetByKey("config.pickaxe");
        }

        /// <summary>Get a translation equivalent to "Scythe Features".</summary>
        public static string Config_Scythe()
        {
            return I18n.GetByKey("config.scythe");
        }

        /// <summary>Get a translation equivalent to "Melee Blunt Weapons Features".</summary>
        public static string Config_MeleeBlunt()
        {
            return I18n.GetByKey("config.melee-blunt");
        }

        /// <summary>Get a translation equivalent to "Melee Dagger Features".</summary>
        public static string Config_MeleeDagger()
        {
            return I18n.GetByKey("config.melee-dagger");
        }

        /// <summary>Get a translation equivalent to "Melee Sword Features".</summary>
        public static string Config_MeleeSword()
        {
            return I18n.GetByKey("config.melee-sword");
        }

        /// <summary>Get a translation equivalent to "Other Tools".</summary>
        public static string Config_OtherTools()
        {
            return I18n.GetByKey("config.other-tools");
        }

        /// <summary>Get a translation equivalent to "Custom Tools".</summary>
        public static string Config_CustomTools()
        {
            return I18n.GetByKey("config.custom-tools");
        }

        /// <summary>Get a translation equivalent to "Distance".</summary>
        public static string Config_Distance_Name()
        {
            return I18n.GetByKey("config.distance.name");
        }

        /// <summary>Get a translation equivalent to "The number of tiles in each direction around the tractor to affect (in addition to the tile under it). Default {{defaultValue}}; a value of {{maxRecommendedValue}} covers most of the visible screen, and higher values may negatively impact game performance.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        /// <param name="maxRecommendedValue">The value to inject for the <c>{{maxRecommendedValue}}</c> token.</param>
        public static string Config_Distance_Tooltip(object defaultValue, object maxRecommendedValue)
        {
            return I18n.GetByKey("config.distance.tooltip", new { defaultValue, maxRecommendedValue });
        }

        /// <summary>Get a translation equivalent to "Tractor Speed".</summary>
        public static string Config_Speed_Name()
        {
            return I18n.GetByKey("config.speed.name");
        }

        /// <summary>Get a translation equivalent to "The speed modifier when riding a tractor. Default {{defaultValue}}.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_Speed_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.speed.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Magnetic Radius".</summary>
        public static string Config_MagneticRadius_Name()
        {
            return I18n.GetByKey("config.magnetic-radius.name");
        }

        /// <summary>Get a translation equivalent to "The item magnetism amount (higher values attract items from father away). Default {{defaultValue}}.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_MagneticRadius_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.magnetic-radius.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Build Price".</summary>
        public static string Config_BuildPrice_Name()
        {
            return I18n.GetByKey("config.build-price.name");
        }

        /// <summary>Get a translation equivalent to "The gold price to buy a tractor garage. Default {{defaultValue}}g.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_BuildPrice_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.build-price.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Can Summon Without Garage".</summary>
        public static string Config_CanSummonWithoutGarage_Name()
        {
            return I18n.GetByKey("config.can-summon-without-garage.name");
        }

        /// <summary>Get a translation equivalent to "Whether you can summon a temporary tractor without building a garage first.".</summary>
        public static string Config_CanSummonWithoutGarage_Tooltip()
        {
            return I18n.GetByKey("config.can-summon-without-garage.tooltip");
        }

        /// <summary>Get a translation equivalent to "Invincible on Tractor".</summary>
        public static string Config_InvincibleOnTractor_Name()
        {
            return I18n.GetByKey("config.invincible-on-tractor.name");
        }

        /// <summary>Get a translation equivalent to "Whether you should be immune to damage from any source when riding the tractor.".</summary>
        public static string Config_InvincibleOnTractor_Tooltip()
        {
            return I18n.GetByKey("config.invincible-on-tractor.tooltip");
        }

        /// <summary>Get a translation equivalent to "Highlight Radius (Debug)".</summary>
        public static string Config_HighlightRadius_Name()
        {
            return I18n.GetByKey("config.highlight-radius.name");
        }

        /// <summary>Get a translation equivalent to "Whether to highlight the tractor radius when riding one, to help visualize the distance option.".</summary>
        public static string Config_HighlightRadius_Tooltip()
        {
            return I18n.GetByKey("config.highlight-radius.tooltip");
        }

        /// <summary>Get a translation equivalent to "Summon Tractor".</summary>
        public static string Config_SummonKey_Name()
        {
            return I18n.GetByKey("config.summon-key.name");
        }

        /// <summary>Get a translation equivalent to "Warp an available tractor to your position. Default backspace.".</summary>
        public static string Config_SummonKey_Tooltip()
        {
            return I18n.GetByKey("config.summon-key.tooltip");
        }

        /// <summary>Get a translation equivalent to "Dismiss Tractor".</summary>
        public static string Config_DismissKey_Name()
        {
            return I18n.GetByKey("config.dismiss-key.name");
        }

        /// <summary>Get a translation equivalent to "Return the tractor you're riding to its home.".</summary>
        public static string Config_DismissKey_Tooltip()
        {
            return I18n.GetByKey("config.dismiss-key.tooltip");
        }

        /// <summary>Get a translation equivalent to "Hold to Activate".</summary>
        public static string Config_HoldToActivateKey_Name()
        {
            return I18n.GetByKey("config.hold-to-activate-key.name");
        }

        /// <summary>Get a translation equivalent to "If specified, the tractor will only do something while you're holding this button. If nothing is specified, the tractor will work automatically while you're riding it.".</summary>
        public static string Config_HoldToActivateKey_Tooltip()
        {
            return I18n.GetByKey("config.hold-to-activate-key.tooltip");
        }

        /// <summary>Get a translation equivalent to "Chop Fruit Trees (Seeds)".</summary>
        public static string Config_ChopFruitTreesSeeds_Name()
        {
            return I18n.GetByKey("config.chop-fruit-trees-seeds.name");
        }

        /// <summary>Get a translation equivalent to "Whether to clear fruit tree seeds.".</summary>
        public static string Config_ChopFruitTreesSeeds_Tooltip()
        {
            return I18n.GetByKey("config.chop-fruit-trees-seeds.tooltip");
        }

        /// <summary>Get a translation equivalent to "Chop Fruit Trees (Saplings)".</summary>
        public static string Config_ChopFruitTreesSaplings_Name()
        {
            return I18n.GetByKey("config.chop-fruit-trees-saplings.name");
        }

        /// <summary>Get a translation equivalent to "Whether to clear fruit trees which aren't fully grown.".</summary>
        public static string Config_ChopFruitTreesSaplings_Tooltip()
        {
            return I18n.GetByKey("config.chop-fruit-trees-saplings.tooltip");
        }

        /// <summary>Get a translation equivalent to "Chop Fruit Trees (Grown)".</summary>
        public static string Config_ChopFruitTreesGrown_Name()
        {
            return I18n.GetByKey("config.chop-fruit-trees-grown.name");
        }

        /// <summary>Get a translation equivalent to "Whether to cut fully-grown fruit trees.".</summary>
        public static string Config_ChopFruitTreesGrown_Tooltip()
        {
            return I18n.GetByKey("config.chop-fruit-trees-grown.tooltip");
        }

        /// <summary>Get a translation equivalent to "Chop Trees (Seeds)".</summary>
        public static string Config_ChopTreesSeeds_Name()
        {
            return I18n.GetByKey("config.chop-trees-seeds.name");
        }

        /// <summary>Get a translation equivalent to "Whether to clear non-fruit tree seeds.".</summary>
        public static string Config_ChopTreesSeeds_Tooltip()
        {
            return I18n.GetByKey("config.chop-trees-seeds.tooltip");
        }

        /// <summary>Get a translation equivalent to "Chop Trees (Saplings)".</summary>
        public static string Config_ChopTreesSaplings_Name()
        {
            return I18n.GetByKey("config.chop-trees-saplings.name");
        }

        /// <summary>Get a translation equivalent to "Whether to clear non-fruit trees which aren't fully grown.".</summary>
        public static string Config_ChopTreesSaplings_Tooltip()
        {
            return I18n.GetByKey("config.chop-trees-saplings.tooltip");
        }

        /// <summary>Get a translation equivalent to "Chop Trees (Grown)".</summary>
        public static string Config_ChopTreesGrown_Name()
        {
            return I18n.GetByKey("config.chop-trees-grown.name");
        }

        /// <summary>Get a translation equivalent to "Whether to clear fully-grown non-fruit trees.".</summary>
        public static string Config_ChopTreesGrown_Tooltip()
        {
            return I18n.GetByKey("config.chop-trees-grown.tooltip");
        }

        /// <summary>Get a translation equivalent to "Chop Bushes".</summary>
        public static string Config_ChopBushes_Name()
        {
            return I18n.GetByKey("config.chop-bushes.name");
        }

        /// <summary>Get a translation equivalent to "Whether to cut bushes that can be chopped.".</summary>
        public static string Config_ChopBushes_Tooltip()
        {
            return I18n.GetByKey("config.chop-bushes.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Fruit Trees".</summary>
        public static string Config_HarvestFruitTrees_Name()
        {
            return I18n.GetByKey("config.harvest-fruit-trees.name");
        }

        /// <summary>Get a translation equivalent to "Whether to harvest fruit trees.".</summary>
        public static string Config_HarvestFruitTrees_Tooltip()
        {
            return I18n.GetByKey("config.harvest-fruit-trees.tooltip");
        }

        /// <summary>Get a translation equivalent to "Clear Dead Crops".</summary>
        public static string Config_ClearDeadCrops_Name()
        {
            return I18n.GetByKey("config.clear-dead-crops.name");
        }

        /// <summary>Get a translation equivalent to "Whether to destroy dead crops.".</summary>
        public static string Config_ClearDeadCrops_Tooltip()
        {
            return I18n.GetByKey("config.clear-dead-crops.tooltip");
        }

        /// <summary>Get a translation equivalent to "Clear Live Crops".</summary>
        public static string Config_ClearLiveCrops_Name()
        {
            return I18n.GetByKey("config.clear-live-crops.name");
        }

        /// <summary>Get a translation equivalent to "Whether to destroy live crops.".</summary>
        public static string Config_ClearLiveCrops_Tooltip()
        {
            return I18n.GetByKey("config.clear-live-crops.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Giant Crops".</summary>
        public static string Config_HarvestGiantCrops_Name()
        {
            return I18n.GetByKey("config.harvest-giant-crops.name");
        }

        /// <summary>Get a translation equivalent to "Whether to cut giant crops.".</summary>
        public static string Config_HarvestGiantCrops_Tooltip()
        {
            return I18n.GetByKey("config.harvest-giant-crops.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Crops".</summary>
        public static string Config_HarvestCrops_Name()
        {
            return I18n.GetByKey("config.harvest-crops.name");
        }

        /// <summary>Get a translation equivalent to "Whether to harvest crops.".</summary>
        public static string Config_HarvestCrops_Tooltip()
        {
            return I18n.GetByKey("config.Harvest Crops.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Flowers".</summary>
        public static string Config_HarvestFlowers_Name()
        {
            return I18n.GetByKey("config.harvest-flowers.name");
        }

        /// <summary>Get a translation equivalent to "Whether to harvest flowers.".</summary>
        public static string Config_HarvestFlowers_Tooltip()
        {
            return I18n.GetByKey("config.harvest-flowers.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Forage".</summary>
        public static string Config_HarvestForage_Name()
        {
            return I18n.GetByKey("config.harvest-forage.name");
        }

        /// <summary>Get a translation equivalent to "Whether to harvest forage.".</summary>
        public static string Config_HarvestForage_Tooltip()
        {
            return I18n.GetByKey("config.harvest-forage.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Grass".</summary>
        public static string Config_HarvestGrass_Name()
        {
            return I18n.GetByKey("config.harvest-grass.name");
        }

        /// <summary>Get a translation equivalent to "Whether to cut grass. If you have free silo space, this gives you hay as usual.".</summary>
        public static string Config_HarvestGrass_Tooltip()
        {
            return I18n.GetByKey("config.harvest-grass.tooltip");
        }

        /// <summary>Get a translation equivalent to "Clear Debris".</summary>
        public static string Config_ClearDebris_Name()
        {
            return I18n.GetByKey("config.clear-debris.name");
        }

        /// <summary>Get a translation equivalent to "Whether to destroy debris like weeds, twigs, giant stumps, and fallen logs.".</summary>
        public static string Config_ClearDebris_Tooltip()
        {
            return I18n.GetByKey("config.clear-debris.tooltip");
        }

        /// <summary>Get a translation equivalent to "Clear Weeds".</summary>
        public static string Config_ClearWeeds_Name()
        {
            return I18n.GetByKey("config.clear-weeds.name");
        }

        /// <summary>Get a translation equivalent to "Whether to destroy weeds.".</summary>
        public static string Config_ClearWeeds_Tooltip()
        {
            return I18n.GetByKey("config.clear-weeds.tooltip");
        }

        /// <summary>Get a translation equivalent to "Till Dirt".</summary>
        public static string Config_TillDirt_Name()
        {
            return I18n.GetByKey("config.till-dirt.name");
        }

        /// <summary>Get a translation equivalent to "Whether to till empty dirt.".</summary>
        public static string Config_TillDirt_Tooltip()
        {
            return I18n.GetByKey("config.till-dirt.tooltip");
        }

        /// <summary>Get a translation equivalent to "Dig Artifact Spots".</summary>
        public static string Config_DigArtifactSpots_Name()
        {
            return I18n.GetByKey("config.dig-artifact-spots.name");
        }

        /// <summary>Get a translation equivalent to "Whether to dig up artifact spots.".</summary>
        public static string Config_DigArtifactSpots_Tooltip()
        {
            return I18n.GetByKey("config.dig-artifact-spots.tooltip");
        }

        /// <summary>Get a translation equivalent to "Clear Tilled Dirt".</summary>
        public static string Config_ClearTilledDirt_Name()
        {
            return I18n.GetByKey("config.clear-tilled-dirt.name");
        }

        /// <summary>Get a translation equivalent to "Whether to clear tilled dirt.".</summary>
        public static string Config_ClearTilledDirt_Tooltip()
        {
            return I18n.GetByKey("config.clear-tilled-dirt.tooltip");
        }

        /// <summary>Get a translation equivalent to "Break Boulders and Meteorites".</summary>
        public static string Config_BreakBouldersAndMeteorites_Name()
        {
            return I18n.GetByKey("config.break-boulders-and-meteorites.name");
        }

        /// <summary>Get a translation equivalent to "Whether to break boulders and meteorites.".</summary>
        public static string Config_BreakBouldersAndMeteorites_Tooltip()
        {
            return I18n.GetByKey("config.break-boulders-and-meteorites.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Mine Spawns".</summary>
        public static string Config_HarvestMineSpawns_Name()
        {
            return I18n.GetByKey("config.harvest-mine-spawns.name");
        }

        /// <summary>Get a translation equivalent to "Whether to harvest spawned mine items like quartz and frozen tears.".</summary>
        public static string Config_HarvestMineSpawns_Tooltip()
        {
            return I18n.GetByKey("config.harvest-mine-spawns.tooltip");
        }

        /// <summary>Get a translation equivalent to "Break Flooring".</summary>
        public static string Config_BreakFlooring_Name()
        {
            return I18n.GetByKey("config.break-flooring.name");
        }

        /// <summary>Get a translation equivalent to "Whether to break placed flooring.".</summary>
        public static string Config_BreakFlooring_Tooltip()
        {
            return I18n.GetByKey("config.break-flooring.tooltip");
        }

        /// <summary>Get a translation equivalent to "Break Objects".</summary>
        public static string Config_BreakObjects_Name()
        {
            return I18n.GetByKey("config.break-objects.name");
        }

        /// <summary>Get a translation equivalent to "Whether to break placed objects.".</summary>
        public static string Config_BreakObjects_Tooltip()
        {
            return I18n.GetByKey("config.break-objects.tooltip");
        }

        /// <summary>Get a translation equivalent to "Break Mine Containers".</summary>
        public static string Config_BreakMineContainers_Name()
        {
            return I18n.GetByKey("config.break-mine-containers.name");
        }

        /// <summary>Get a translation equivalent to "Whether to break containers in the mine.".</summary>
        public static string Config_BreakMineContainers_Tooltip()
        {
            return I18n.GetByKey("config.break-mine-containers.tooltip");
        }

        /// <summary>Get a translation equivalent to "Harvest Machines".</summary>
        public static string Config_HarvestMachines_Name()
        {
            return I18n.GetByKey("config.harvest-machines.name");
        }

        /// <summary>Get a translation equivalent to "Whether to collect machine output.".</summary>
        public static string Config_HarvestMachines_Tooltip()
        {
            return I18n.GetByKey("config.harvest-machines.tooltip");
        }

        /// <summary>Get a translation equivalent to "Attack Monsters".</summary>
        public static string Config_AttackMonsters_Name()
        {
            return I18n.GetByKey("config.Attack-monsters.name");
        }

        /// <summary>Get a translation equivalent to "Whether to attack monsters. (This is massively overpowered due to the tractor tool speed.)".</summary>
        public static string Config_AttackMonsters_Tooltip()
        {
            return I18n.GetByKey("config.Attack-monsters.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Milk Pail".</summary>
        public static string Config_MilkPail_Name()
        {
            return I18n.GetByKey("config.milk-pail.name");
        }

        /// <summary>Get a translation equivalent to "Whether to collect milk from farm animals using the milk pail.".</summary>
        public static string Config_MilkPail_Tooltip()
        {
            return I18n.GetByKey("config.milk-pail.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Shears".</summary>
        public static string Config_Shears_Name()
        {
            return I18n.GetByKey("config.shears.name");
        }

        /// <summary>Get a translation equivalent to "Whether to collect wool from farm animals using the shears.".</summary>
        public static string Config_Shears_Tooltip()
        {
            return I18n.GetByKey("config.shears.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Watering Can".</summary>
        public static string Config_WateringCan_Name()
        {
            return I18n.GetByKey("config.watering-can.name");
        }

        /// <summary>Get a translation equivalent to "Whether to water nearby tiles using the watering can. This doesn't consume water in the watering can.".</summary>
        public static string Config_WateringCan_Tooltip()
        {
            return I18n.GetByKey("config.watering-can.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Fertilizer".</summary>
        public static string Config_Fertilizer_Name()
        {
            return I18n.GetByKey("config.fertilizer.name");
        }

        /// <summary>Get a translation equivalent to "Whether to apply fertilizer to crops and tilled dirt.".</summary>
        public static string Config_Fertilizer_Tooltip()
        {
            return I18n.GetByKey("config.fertilizer.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Grass Starters".</summary>
        public static string Config_GrassStarters_Name()
        {
            return I18n.GetByKey("config.grass-starters.name");
        }

        /// <summary>Get a translation equivalent to "Whether to plant grass starters.".</summary>
        public static string Config_GrassStarters_Tooltip()
        {
            return I18n.GetByKey("config.grass-starters.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Seeds".</summary>
        public static string Config_Seeds_Name()
        {
            return I18n.GetByKey("config.seeds.name");
        }

        /// <summary>Get a translation equivalent to "Whether to plant seeds.".</summary>
        public static string Config_Seeds_Tooltip()
        {
            return I18n.GetByKey("config.seeds.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Seed Bags".</summary>
        public static string Config_SeedBags_Name()
        {
            return I18n.GetByKey("config.seed-bags.name");
        }

        /// <summary>Get a translation equivalent to "Whether to plant seeds from the Seed Bag mod.".</summary>
        public static string Config_SeedBags_Tooltip()
        {
            return I18n.GetByKey("config.seed-bags.tooltip");
        }

        /// <summary>Get a translation equivalent to "Enable Slingshot".</summary>
        public static string Config_Slingshot_Name()
        {
            return I18n.GetByKey("config.slingshot.name");
        }

        /// <summary>Get a translation equivalent to "Whether to fire the slingshot towards the cursor. (This is massively overpowered and will consume ammo voraciously due to the tractor tool speed.)".</summary>
        public static string Config_Slingshot_Tooltip()
        {
            return I18n.GetByKey("config.slingshot.tooltip");
        }

        /// <summary>Get a translation equivalent to "Custom Tool Names".</summary>
        public static string Config_CustomToolNames_Name()
        {
            return I18n.GetByKey("config.custom-tool-names.name");
        }

        /// <summary>Get a translation equivalent to "The custom items/tools to enable while riding the tractor. Tools will be used on each surrounding tile, while items will be put down. If you specify something that's already supported (like the axe), this overrides all limitations on its use. You must specify the exact internal name (not the translated display name), like 'Axe' or 'Mega Bomb'. Separate multiple values with commas.".</summary>
        public static string Config_CustomToolNames_Tooltip()
        {
            return I18n.GetByKey("config.custom-tool-names.tooltip");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

