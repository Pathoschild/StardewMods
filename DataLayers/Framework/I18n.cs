using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
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
        ** Accessors
        *********/
        /// <summary>A lookup of available translation keys.</summary>
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "Using the same key is deliberate.")]
        public static class Keys
        {
            /// <summary>The unique key for a translation equivalent to "Accessible".</summary>
            public const string Accessible_Name = "accessible.name";

            /// <summary>The unique key for a translation equivalent to "Clear".</summary>
            public const string Accessible_Clear = "accessible.clear";

            /// <summary>The unique key for a translation equivalent to "Occupied".</summary>
            public const string Accessible_Occupied = "accessible.occupied";

            /// <summary>The unique key for a translation equivalent to "Impassable".</summary>
            public const string Accessible_Impassable = "accessible.impassable";

            /// <summary>The unique key for a translation equivalent to "Warp".</summary>
            public const string Accessible_Warp = "accessible.warp";

            /// <summary>The unique key for a translation equivalent to "Buildable".</summary>
            public const string Buildable_Name = "buildable.name";

            /// <summary>The unique key for a translation equivalent to "Can Build here".</summary>
            public const string Buildable_Buildable = "buildable.buildable";

            /// <summary>The unique key for a translation equivalent to "Occupied".</summary>
            public const string Buildable_Occupied = "buildable.occupied";

            /// <summary>The unique key for a translation equivalent to "Can't Build Here".</summary>
            public const string Buildable_NotBuildable = "buildable.not-buildable";

            /// <summary>The unique key for a translation equivalent to "Coverage: Bee Houses".</summary>
            public const string BeeHouses_Name = "bee-houses.name";

            /// <summary>The unique key for a translation equivalent to "Flower Range".</summary>
            public const string BeeHouses_Range = "bee-houses.range";

            /// <summary>The unique key for a translation equivalent to "Coverage: Junimo Huts".</summary>
            public const string JunimoHuts_Name = "junimo-huts.name";

            /// <summary>The unique key for a translation equivalent to "Can Harvest".</summary>
            public const string JunimoHuts_CanHarvest = "junimo-huts.can-harvest";

            /// <summary>The unique key for a translation equivalent to "Can't Harvest".</summary>
            public const string JunimoHuts_CannotHarvest = "junimo-huts.cannot-harvest";

            /// <summary>The unique key for a translation equivalent to "Coverage: Scarecrows".</summary>
            public const string Scarecrows_Name = "scarecrows.name";

            /// <summary>The unique key for a translation equivalent to "Protected".</summary>
            public const string Scarecrows_Protected = "scarecrows.protected";

            /// <summary>The unique key for a translation equivalent to "Exposed".</summary>
            public const string Scarecrows_Exposed = "scarecrows.exposed";

            /// <summary>The unique key for a translation equivalent to "Coverage: Sprinklers".</summary>
            public const string Sprinklers_Name = "sprinklers.name";

            /// <summary>The unique key for a translation equivalent to "Covered".</summary>
            public const string Sprinklers_Covered = "sprinklers.covered";

            /// <summary>The unique key for a translation equivalent to "Dry Crops".</summary>
            public const string Sprinklers_DryCrops = "sprinklers.dry-crops";

            /// <summary>The unique key for a translation equivalent to "Crops: Ready to Harvest".</summary>
            public const string CropHarvest_Name = "crop-harvest.name";

            /// <summary>The unique key for a translation equivalent to "Ready".</summary>
            public const string CropHarvest_Ready = "crop-harvest.ready";

            /// <summary>The unique key for a translation equivalent to "Not Ready".</summary>
            public const string CropHarvest_NotReady = "crop-harvest.not-ready";

            /// <summary>The unique key for a translation equivalent to "Not Enough Time".</summary>
            public const string CropHarvest_NotEnoughTime = "crop-harvest.not-enough-time";

            /// <summary>The unique key for a translation equivalent to "Crops: Watered".</summary>
            public const string CropWater_Name = "crop-water.name";

            /// <summary>The unique key for a translation equivalent to "Watered Crop".</summary>
            public const string CropWater_Watered = "crop-water.watered";

            /// <summary>The unique key for a translation equivalent to "Dry Crop".</summary>
            public const string CropWater_Dry = "crop-water.dry";

            /// <summary>The unique key for a translation equivalent to "Crops: Water for Paddy Crops".</summary>
            public const string CropPaddyWater_Name = "crop-paddy-water.name";

            /// <summary>The unique key for a translation equivalent to "Near Water".</summary>
            public const string CropPaddyWater_InRange = "crop-paddy-water.in-range";

            /// <summary>The unique key for a translation equivalent to "Dry Land".</summary>
            public const string CropPaddyWater_NotInRange = "crop-paddy-water.not-in-range";

            /// <summary>The unique key for a translation equivalent to "Crops: Fertilized".</summary>
            public const string CropFertilizer_Name = "crop-fertilizer.name";

            /// <summary>The unique key for a translation equivalent to "Fertilizer".</summary>
            public const string CropFertilizer_Fertilizer = "crop-fertilizer.fertilizer";

            /// <summary>The unique key for a translation equivalent to "Retaining Soil".</summary>
            public const string CropFertilizer_RetainingSoil = "crop-fertilizer.retaining-soil";

            /// <summary>The unique key for a translation equivalent to "Speed-Gro".</summary>
            public const string CropFertilizer_SpeedGro = "crop-fertilizer.speed-gro";

            /// <summary>The unique key for a translation equivalent to "Machine Processing".</summary>
            public const string Machines_Name = "machines.name";

            /// <summary>The unique key for a translation equivalent to "Empty".</summary>
            public const string Machines_Empty = "machines.empty";

            /// <summary>The unique key for a translation equivalent to "Processing".</summary>
            public const string Machines_Processing = "machines.processing";

            /// <summary>The unique key for a translation equivalent to "Finished".</summary>
            public const string Machines_Finished = "machines.finished";

            /// <summary>The unique key for a translation equivalent to "Tile Grid".</summary>
            public const string Grid_Name = "grid.name";

            /// <summary>The unique key for a translation equivalent to "Tillable".</summary>
            public const string Tillable_Name = "tillable.name";

            /// <summary>The unique key for a translation equivalent to "Tillable".</summary>
            public const string Tillable_Tillable = "tillable.tillable";

            /// <summary>The unique key for a translation equivalent to "Tilled".</summary>
            public const string Tillable_Tilled = "tillable.tilled";

            /// <summary>The unique key for a translation equivalent to "Occupied".</summary>
            public const string Tillable_Occupied = "tillable.occupied";

            /// <summary>The unique key for a translation equivalent to "Not Tillable".</summary>
            public const string Tillable_NotTillable = "tillable.not-tillable";
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "Accessible".</summary>
        public static string Accessible_Name()
        {
            return I18n.GetByKey(Keys.Accessible_Name);
        }

        /// <summary>Get a translation equivalent to "Clear".</summary>
        public static string Accessible_Clear()
        {
            return I18n.GetByKey(Keys.Accessible_Clear);
        }

        /// <summary>Get a translation equivalent to "Occupied".</summary>
        public static string Accessible_Occupied()
        {
            return I18n.GetByKey(Keys.Accessible_Occupied);
        }

        /// <summary>Get a translation equivalent to "Impassable".</summary>
        public static string Accessible_Impassable()
        {
            return I18n.GetByKey(Keys.Accessible_Impassable);
        }

        /// <summary>Get a translation equivalent to "Warp".</summary>
        public static string Accessible_Warp()
        {
            return I18n.GetByKey(Keys.Accessible_Warp);
        }

        /// <summary>Get a translation equivalent to "Buildable".</summary>
        public static string Buildable_Name()
        {
            return I18n.GetByKey(Keys.Buildable_Name);
        }

        /// <summary>Get a translation equivalent to "Can Build here".</summary>
        public static string Buildable_Buildable()
        {
            return I18n.GetByKey(Keys.Buildable_Buildable);
        }

        /// <summary>Get a translation equivalent to "Occupied".</summary>
        public static string Buildable_Occupied()
        {
            return I18n.GetByKey(Keys.Buildable_Occupied);
        }

        /// <summary>Get a translation equivalent to "Can't Build Here".</summary>
        public static string Buildable_NotBuildable()
        {
            return I18n.GetByKey(Keys.Buildable_NotBuildable);
        }

        /// <summary>Get a translation equivalent to "Coverage: Bee Houses".</summary>
        public static string BeeHouses_Name()
        {
            return I18n.GetByKey(Keys.BeeHouses_Name);
        }

        /// <summary>Get a translation equivalent to "Flower Range".</summary>
        public static string BeeHouses_Range()
        {
            return I18n.GetByKey(Keys.BeeHouses_Range);
        }

        /// <summary>Get a translation equivalent to "Coverage: Junimo Huts".</summary>
        public static string JunimoHuts_Name()
        {
            return I18n.GetByKey(Keys.JunimoHuts_Name);
        }

        /// <summary>Get a translation equivalent to "Can Harvest".</summary>
        public static string JunimoHuts_CanHarvest()
        {
            return I18n.GetByKey(Keys.JunimoHuts_CanHarvest);
        }

        /// <summary>Get a translation equivalent to "Can't Harvest".</summary>
        public static string JunimoHuts_CannotHarvest()
        {
            return I18n.GetByKey(Keys.JunimoHuts_CannotHarvest);
        }

        /// <summary>Get a translation equivalent to "Coverage: Scarecrows".</summary>
        public static string Scarecrows_Name()
        {
            return I18n.GetByKey(Keys.Scarecrows_Name);
        }

        /// <summary>Get a translation equivalent to "Protected".</summary>
        public static string Scarecrows_Protected()
        {
            return I18n.GetByKey(Keys.Scarecrows_Protected);
        }

        /// <summary>Get a translation equivalent to "Exposed".</summary>
        public static string Scarecrows_Exposed()
        {
            return I18n.GetByKey(Keys.Scarecrows_Exposed);
        }

        /// <summary>Get a translation equivalent to "Coverage: Sprinklers".</summary>
        public static string Sprinklers_Name()
        {
            return I18n.GetByKey(Keys.Sprinklers_Name);
        }

        /// <summary>Get a translation equivalent to "Covered".</summary>
        public static string Sprinklers_Covered()
        {
            return I18n.GetByKey(Keys.Sprinklers_Covered);
        }

        /// <summary>Get a translation equivalent to "Dry Crops".</summary>
        public static string Sprinklers_DryCrops()
        {
            return I18n.GetByKey(Keys.Sprinklers_DryCrops);
        }

        /// <summary>Get a translation equivalent to "Crops: Ready to Harvest".</summary>
        public static string CropHarvest_Name()
        {
            return I18n.GetByKey(Keys.CropHarvest_Name);
        }

        /// <summary>Get a translation equivalent to "Ready".</summary>
        public static string CropHarvest_Ready()
        {
            return I18n.GetByKey(Keys.CropHarvest_Ready);
        }

        /// <summary>Get a translation equivalent to "Not Ready".</summary>
        public static string CropHarvest_NotReady()
        {
            return I18n.GetByKey(Keys.CropHarvest_NotReady);
        }

        /// <summary>Get a translation equivalent to "Not Enough Time".</summary>
        public static string CropHarvest_NotEnoughTime()
        {
            return I18n.GetByKey(Keys.CropHarvest_NotEnoughTime);
        }

        /// <summary>Get a translation equivalent to "Crops: Watered".</summary>
        public static string CropWater_Name()
        {
            return I18n.GetByKey(Keys.CropWater_Name);
        }

        /// <summary>Get a translation equivalent to "Watered Crop".</summary>
        public static string CropWater_Watered()
        {
            return I18n.GetByKey(Keys.CropWater_Watered);
        }

        /// <summary>Get a translation equivalent to "Dry Crop".</summary>
        public static string CropWater_Dry()
        {
            return I18n.GetByKey(Keys.CropWater_Dry);
        }

        /// <summary>Get a translation equivalent to "Crops: Water for Paddy Crops".</summary>
        public static string CropPaddyWater_Name()
        {
            return I18n.GetByKey(Keys.CropPaddyWater_Name);
        }

        /// <summary>Get a translation equivalent to "Near Water".</summary>
        public static string CropPaddyWater_InRange()
        {
            return I18n.GetByKey(Keys.CropPaddyWater_InRange);
        }

        /// <summary>Get a translation equivalent to "Dry Land".</summary>
        public static string CropPaddyWater_NotInRange()
        {
            return I18n.GetByKey(Keys.CropPaddyWater_NotInRange);
        }

        /// <summary>Get a translation equivalent to "Crops: Fertilized".</summary>
        public static string CropFertilizer_Name()
        {
            return I18n.GetByKey(Keys.CropFertilizer_Name);
        }

        /// <summary>Get a translation equivalent to "Fertilizer".</summary>
        public static string CropFertilizer_Fertilizer()
        {
            return I18n.GetByKey(Keys.CropFertilizer_Fertilizer);
        }

        /// <summary>Get a translation equivalent to "Retaining Soil".</summary>
        public static string CropFertilizer_RetainingSoil()
        {
            return I18n.GetByKey(Keys.CropFertilizer_RetainingSoil);
        }

        /// <summary>Get a translation equivalent to "Speed-Gro".</summary>
        public static string CropFertilizer_SpeedGro()
        {
            return I18n.GetByKey(Keys.CropFertilizer_SpeedGro);
        }

        /// <summary>Get a translation equivalent to "Machine Processing".</summary>
        public static string Machines_Name()
        {
            return I18n.GetByKey(Keys.Machines_Name);
        }

        /// <summary>Get a translation equivalent to "Empty".</summary>
        public static string Machines_Empty()
        {
            return I18n.GetByKey(Keys.Machines_Empty);
        }

        /// <summary>Get a translation equivalent to "Processing".</summary>
        public static string Machines_Processing()
        {
            return I18n.GetByKey(Keys.Machines_Processing);
        }

        /// <summary>Get a translation equivalent to "Finished".</summary>
        public static string Machines_Finished()
        {
            return I18n.GetByKey(Keys.Machines_Finished);
        }

        /// <summary>Get a translation equivalent to "Tile Grid".</summary>
        public static string Grid_Name()
        {
            return I18n.GetByKey(Keys.Grid_Name);
        }

        /// <summary>Get a translation equivalent to "Tillable".</summary>
        public static string Tillable_Name()
        {
            return I18n.GetByKey(Keys.Tillable_Name);
        }

        /// <summary>Get a translation equivalent to "Tillable".</summary>
        public static string Tillable_Tillable()
        {
            return I18n.GetByKey(Keys.Tillable_Tillable);
        }

        /// <summary>Get a translation equivalent to "Tilled".</summary>
        public static string Tillable_Tilled()
        {
            return I18n.GetByKey(Keys.Tillable_Tilled);
        }

        /// <summary>Get a translation equivalent to "Occupied".</summary>
        public static string Tillable_Occupied()
        {
            return I18n.GetByKey(Keys.Tillable_Occupied);
        }

        /// <summary>Get a translation equivalent to "Not Tillable".</summary>
        public static string Tillable_NotTillable()
        {
            return I18n.GetByKey(Keys.Tillable_NotTillable);
        }

        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        public static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

