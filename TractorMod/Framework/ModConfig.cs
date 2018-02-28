using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/

        public StandardAttachmentSet StandardAttachments { get; set; } = new ModConfig.StandardAttachmentSet();

        /// <summary>Whether the axe clears fruit trees.</summary>
        public bool AxeCutsFruitTrees { get; set; } = false;

        /// <summary>Whether the axe clears non-fruit trees.</summary>
        public bool AxeCutsTrees { get; set; } = false;

        /// <summary>Whether the axe clears live crops.</summary>
        public bool AxeClearsCrops { get; set; } = false;

        /// <summary>Whether the tractor can clear hoed dirt tiles when the pickaxe is selected.</summary>
        public bool PickaxeClearsDirt { get; set; } = true;

        /// <summary>Whether the tractor can break paths and flooring when the pickaxe is selected.</summary>
        public bool PickaxeBreaksFlooring { get; set; } = false;

        /// <summary>Whether to use the experimental feature which lets the tractor pass through trellis crops.</summary>
        public bool PassThroughTrellisCrops { get; set; }

        /// <summary>The custom tools or items to allow. These must match the exact internal tool/item names (not the display names).</summary>
        public string[] CustomAttachments { get; set; } = new string[0];

        /// <summary>The number of tiles on each side of the tractor to affect (in addition to the tile under it).</summary>
        public int Distance { get; set; } = 1;

        /// <summary>The speed modifier when riding the tractor.</summary>
        public int TractorSpeed { get; set; } = -2;

        /// <summary>The magnetic radius when riding the tractor.</summary>
        public int MagneticRadius { get; set; } = 384;

        /// <summary>Whether you need to provide building resources to buy the garage.</summary>
        public bool BuildUsesResources { get; set; } = true;

        /// <summary>The gold price to buy a tractor garage.</summary>
        public int BuildPrice { get; set; } = 150000;

        /// <summary>Whether to highlight the tractor radius when riding it.</summary>
        public bool HighlightRadius { get; set; }

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();


        /*********
        ** Nested models
        *********/
        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The control which toggles the chest UI.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] SummonTractor { get; set; } = { SButton.T };

            /// <summary>A button which activates the tractor when held, or none to activate automatically.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] HoldToActivate { get; set; } = new SButton[0];
        }

        /// <summary>A set of control bindings.</summary>
        internal class StandardAttachmentSet
        {
            public AxeAttachments Axe = new AxeAttachments();

            public FertilizerAttachments Fertilizer = new FertilizerAttachments();

            public GrassStarterAttachments GrassStarter = new GrassStarterAttachments();

            public HoeAttachments Hoe = new HoeAttachments();

            public PickAxeAttachments PickAxe = new PickAxeAttachments();

            public ScytheAttachments Scythe = new ScytheAttachments();

            public SeedsAttachment Seeds = new SeedsAttachment();

            public WateringCanAttachment WateringCan = new WateringCanAttachment();

            public SeedBagModAttachment SeedBag = new SeedBagModAttachment();
        }

        /// <summary>A set of Axe attachments.</summary>
        internal class AxeAttachments
        {
            /// <summary>Whether or not to cut down Fruit Trees.</summary>
            public bool CutFruitTrees{ get; set; } = false;

            /// <summary>Whether or not to cut down Tapped Trees.</summary>
            public bool CutTappedTrees { get; set; } = false;

            /// <summary>Whether or not to cut down Trees.</summary>
            public bool CutTrees { get; set; } = false;

            /// <summary>Whether or not to cut down Live Crops.</summary>
            public bool ClearLiveCrops { get; set; } = false;

            /// <summary>Whether or not to cut down Dead Crops.</summary>
            public bool ClearDeadCrops { get; set; } = true;

            /// <summary>Whether or not to clear Debris.</summary>
            public bool ClearDebris { get; set; } = true;
        }

        /// <summary>A set of Fertilizer attachments.</summary>
        internal class FertilizerAttachments
        {
           /// <summary>Whether or not to use Fertilizer.</summary>
           public bool Enable { get; set; } = true;
        }

        /// <summary>A set of Grass Starter attachments.</summary>
        internal class GrassStarterAttachments
        {
            /// <summary>Whether or not to plant Grass Staters.</summary>
            public bool Enable { get; set; } = true;
        }

        /// <summary>A set of Hoe attachments.</summary>
        internal class HoeAttachments
        {
            /// <summary>Whether or not to Till Dirt.</summary>
            public bool TillDirt { get; set; } = true;
        }

        /// <summary>A set of PickAxe attachments.</summary>
        internal class PickAxeAttachments
        {
            /// <summary>Whether or not to clear Debris.</summary>
            public bool ClearDebris { get; set; } = true;

            /// <summary>Whether or not to clear Dead Crops.</summary>
            public bool ClearDeadCrops { get; set; } = true;

            /// <summary>Whether or not to clear Tilled Dirt.</summary>
            public bool ClearDirt { get; set; } = true;

            /// <summary>Whether or not to clear Flooring.</summary>
            public bool ClearFlooring { get; set; } = false;
        }

        /// <summary>A set of Scythe attachments.</summary>
        internal class ScytheAttachments
        {
            /// <summary>Whether or not to harvest Forage.</summary>
            public bool HarvestForage { get; set; } = true;

            /// <summary>Whether or not to harvest Crops.</summary>
            public bool HarvestCrops { get; set; } = true;

            /// <summary>Whether or not to harvest Flowers.</summary>
            public bool HarvestFlowers { get; set; } = true;

            /// <summary>Whether or not to harvest Fruit Trees.</summary>
            public bool HarvestFruitTrees { get; set; } = true;

            /// <summary>Whether or not to cut down Grass.</summary>
            public bool HarvestGrass { get; set; } = true;

            /// <summary>Whether or not to clear Dead Crops.</summary>
            public bool ClearDeadCrops { get; set; } = true;

            /// <summary>Whether or not to clear Debris.</summary>
            public bool ClearWeeds { get; set; } = true;
        }

        /// <summary>A set of Seed attachments.</summary>
        internal class SeedsAttachment
        {
            /// <summary>Whether or not to plant Seeds.</summary>
            //[JsonConverter(typeof(bool))]
            public bool Enable { get; set; } = true;
        }

        /// <summary>A set of Watering Can attachments.</summary>
        internal class WateringCanAttachment
        {
            /// <summary>Whether or not to water Crops.</summary>
            //[JsonConverter(typeof(bool))]
            public bool Enable { get; set; } = true;
        }

        /// <summary>A set of Seed Bag attachments.</summary>
        internal class SeedBagModAttachment
        {
            /// <summary>Whether or not to use the SeedBag.</summary>
            //[JsonConverter(typeof(bool))]
            public bool Enable { get; set; } = true;
        }
    }
}
