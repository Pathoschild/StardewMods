using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static partial class I18n
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

        /// <summary>Get a translation equivalent to "now".</summary>
        public static string Generic_Now()
        {
            return I18n.GetByKey("generic.now");
        }

        /// <summary>Get a translation equivalent to "tomorrow".</summary>
        public static string Generic_Tomorrow()
        {
            return I18n.GetByKey("generic.tomorrow");
        }

        /// <summary>Get a translation equivalent to "yesterday".</summary>
        public static string Generic_Yesterday()
        {
            return I18n.GetByKey("generic.yesterday");
        }

        /// <summary>Get a translation equivalent to "{{percent}}%".</summary>
        /// <param name="percent">The value to inject for the <c>{{percent}}</c> token.</param>
        public static string Generic_Percent(object percent)
        {
            return I18n.GetByKey("generic.percent", new { percent });
        }

        /// <summary>Get a translation equivalent to "{{percent}}% chance of {{label}}".</summary>
        /// <param name="percent">The value to inject for the <c>{{percent}}</c> token.</param>
        /// <param name="label">The value to inject for the <c>{{label}}</c> token.</param>
        public static string Generic_PercentChanceOf(object percent, object label)
        {
            return I18n.GetByKey("generic.percent-chance-of", new { percent, label });
        }

        /// <summary>Get a translation equivalent to "{{percent}}% ({{value}} of {{max}})".</summary>
        /// <param name="percent">The value to inject for the <c>{{percent}}</c> token.</param>
        /// <param name="value">The value to inject for the <c>{{value}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Generic_PercentRatio(object percent, object value, object max)
        {
            return I18n.GetByKey("generic.percent-ratio", new { percent, value, max });
        }

        /// <summary>Get a translation equivalent to "{{value}} of {{max}}".</summary>
        /// <param name="value">The value to inject for the <c>{{value}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Generic_Ratio(object value, object max)
        {
            return I18n.GetByKey("generic.ratio", new { value, max });
        }

        /// <summary>Get a translation equivalent to "{{min}} to {{max}}".</summary>
        /// <param name="min">The value to inject for the <c>{{min}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Generic_Range(object min, object max)
        {
            return I18n.GetByKey("generic.range", new { min, max });
        }

        /// <summary>Get a translation equivalent to "yes".</summary>
        public static string Generic_Yes()
        {
            return I18n.GetByKey("generic.yes");
        }

        /// <summary>Get a translation equivalent to "no".</summary>
        public static string Generic_No()
        {
            return I18n.GetByKey("generic.no");
        }

        /// <summary>Get a translation equivalent to "{{count}} seconds".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Generic_Seconds(object count)
        {
            return I18n.GetByKey("generic.seconds", new { count });
        }

        /// <summary>Get a translation equivalent to "{{count}} minutes".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Generic_Minutes(object count)
        {
            return I18n.GetByKey("generic.minutes", new { count });
        }

        /// <summary>Get a translation equivalent to "{{count}} hours".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Generic_Hours(object count)
        {
            return I18n.GetByKey("generic.hours", new { count });
        }

        /// <summary>Get a translation equivalent to "{{count}} days".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Generic_Days(object count)
        {
            return I18n.GetByKey("generic.days", new { count });
        }

        /// <summary>Get a translation equivalent to "in {{count}} days".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Generic_InXDays(object count)
        {
            return I18n.GetByKey("generic.in-x-days", new { count });
        }

        /// <summary>Get a translation equivalent to "{{count}} days ago".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Generic_XDaysAgo(object count)
        {
            return I18n.GetByKey("generic.x-days-ago", new { count });
        }

        /// <summary>Get a translation equivalent to "{{price}}g".</summary>
        /// <param name="price">The value to inject for the <c>{{price}}</c> token.</param>
        public static string Generic_Price(object price)
        {
            return I18n.GetByKey("generic.price", new { price });
        }

        /// <summary>Get a translation equivalent to "{{price}}g ({{quality}})".</summary>
        /// <param name="price">The value to inject for the <c>{{price}}</c> token.</param>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        public static string Generic_PriceForQuality(object price, object quality)
        {
            return I18n.GetByKey("generic.price-for-quality", new { price, quality });
        }

        /// <summary>Get a translation equivalent to "{{price}}g for stack of {{count}}".</summary>
        /// <param name="price">The value to inject for the <c>{{price}}</c> token.</param>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Generic_PriceForStack(object price, object count)
        {
            return I18n.GetByKey("generic.price-for-stack", new { price, count });
        }

        /// <summary>Get a translation equivalent to "Building".</summary>
        public static string Type_Building()
        {
            return I18n.GetByKey("type.building");
        }

        /// <summary>Get a translation equivalent to "Bush".</summary>
        public static string Type_Bush()
        {
            return I18n.GetByKey("type.bush");
        }

        /// <summary>Get a translation equivalent to "Fruit tree".</summary>
        public static string Type_FruitTree()
        {
            return I18n.GetByKey("type.fruit-tree");
        }

        /// <summary>Get a translation equivalent to "Map tile".</summary>
        public static string Type_MapTile()
        {
            return I18n.GetByKey("type.map-tile");
        }

        /// <summary>Get a translation equivalent to "Monster".</summary>
        public static string Type_Monster()
        {
            return I18n.GetByKey("type.monster");
        }

        /// <summary>Get a translation equivalent to "Player".</summary>
        public static string Type_Player()
        {
            return I18n.GetByKey("type.player");
        }

        /// <summary>Get a translation equivalent to "Tree".</summary>
        public static string Type_Tree()
        {
            return I18n.GetByKey("type.tree");
        }

        /// <summary>Get a translation equivalent to "Villager".</summary>
        public static string Type_Villager()
        {
            return I18n.GetByKey("type.villager");
        }

        /// <summary>Get a translation equivalent to "Other".</summary>
        public static string Type_Other()
        {
            return I18n.GetByKey("type.other");
        }

        /// <summary>Get a translation equivalent to "normal".</summary>
        public static string Quality_Normal()
        {
            return I18n.GetByKey("quality.normal");
        }

        /// <summary>Get a translation equivalent to "silver".</summary>
        public static string Quality_Silver()
        {
            return I18n.GetByKey("quality.silver");
        }

        /// <summary>Get a translation equivalent to "gold".</summary>
        public static string Quality_Gold()
        {
            return I18n.GetByKey("quality.gold");
        }

        /// <summary>Get a translation equivalent to "iridium".</summary>
        public static string Quality_Iridium()
        {
            return I18n.GetByKey("quality.iridium");
        }

        /// <summary>Get a translation equivalent to "Friendly".</summary>
        public static string FriendshipStatus_Friendly()
        {
            return I18n.GetByKey("friendship-status.friendly");
        }

        /// <summary>Get a translation equivalent to "Dating".</summary>
        public static string FriendshipStatus_Dating()
        {
            return I18n.GetByKey("friendship-status.dating");
        }

        /// <summary>Get a translation equivalent to "Engaged".</summary>
        public static string FriendshipStatus_Engaged()
        {
            return I18n.GetByKey("friendship-status.engaged");
        }

        /// <summary>Get a translation equivalent to "Married".</summary>
        public static string FriendshipStatus_Married()
        {
            return I18n.GetByKey("friendship-status.married");
        }

        /// <summary>Get a translation equivalent to "Divorced".</summary>
        public static string FriendshipStatus_Divorced()
        {
            return I18n.GetByKey("friendship-status.divorced");
        }

        /// <summary>Get a translation equivalent to "Kicked out".</summary>
        public static string FriendshipStatus_KickedOut()
        {
            return I18n.GetByKey("friendship-status.kicked-out");
        }

        /// <summary>Get a translation equivalent to "Pantry".</summary>
        public static string BundleArea_Pantry()
        {
            return I18n.GetByKey("bundle-area.pantry");
        }

        /// <summary>Get a translation equivalent to "Crafts Room".</summary>
        public static string BundleArea_CraftsRoom()
        {
            return I18n.GetByKey("bundle-area.crafts-room");
        }

        /// <summary>Get a translation equivalent to "Fish Tank".</summary>
        public static string BundleArea_FishTank()
        {
            return I18n.GetByKey("bundle-area.fish-tank");
        }

        /// <summary>Get a translation equivalent to "Boiler Room".</summary>
        public static string BundleArea_BoilerRoom()
        {
            return I18n.GetByKey("bundle-area.boiler-room");
        }

        /// <summary>Get a translation equivalent to "Vault".</summary>
        public static string BundleArea_Vault()
        {
            return I18n.GetByKey("bundle-area.vault");
        }

        /// <summary>Get a translation equivalent to "Bulletin Board".</summary>
        public static string BundleArea_BulletinBoard()
        {
            return I18n.GetByKey("bundle-area.bulletin-board");
        }

        /// <summary>Get a translation equivalent to "Abandoned Joja Mart".</summary>
        public static string BundleArea_AbandonedJojaMart()
        {
            return I18n.GetByKey("bundle-area.abandoned-joja-mart");
        }

        /// <summary>Get a translation equivalent to "Adventure Guild".</summary>
        public static string Shop_AdventureGuild()
        {
            return I18n.GetByKey("shop.adventure-guild");
        }

        /// <summary>Get a translation equivalent to "Clint".</summary>
        public static string Shop_Clint()
        {
            return I18n.GetByKey("shop.clint");
        }

        /// <summary>Get a translation equivalent to "Marnie".</summary>
        public static string Shop_Marnie()
        {
            return I18n.GetByKey("shop.marnie");
        }

        /// <summary>Get a translation equivalent to "Pierre".</summary>
        public static string Shop_Pierre()
        {
            return I18n.GetByKey("shop.pierre");
        }

        /// <summary>Get a translation equivalent to "Robin".</summary>
        public static string Shop_Robin()
        {
            return I18n.GetByKey("shop.robin");
        }

        /// <summary>Get a translation equivalent to "volcano shop".</summary>
        public static string Shop_Volcano()
        {
            return I18n.GetByKey("shop.volcano");
        }

        /// <summary>Get a translation equivalent to "Willy".</summary>
        public static string Shop_Willy()
        {
            return I18n.GetByKey("shop.willy");
        }

        /// <summary>Get a translation equivalent to "Cooking".</summary>
        public static string RecipeType_Cooking()
        {
            return I18n.GetByKey("recipe-type.cooking");
        }

        /// <summary>Get a translation equivalent to "Crafting".</summary>
        public static string RecipeType_Crafting()
        {
            return I18n.GetByKey("recipe-type.crafting");
        }

        /// <summary>Get a translation equivalent to "Tailoring".</summary>
        public static string RecipeType_Tailoring()
        {
            return I18n.GetByKey("recipe-type.tailoring");
        }

        /// <summary>Get a translation equivalent to "Your loyal pet.".</summary>
        public static string Data_Npc_Pet_Description()
        {
            return I18n.GetByKey("data.npc.pet.description");
        }

        /// <summary>Get a translation equivalent to "Your loyal steed.".</summary>
        public static string Data_Npc_Horse_Description()
        {
            return I18n.GetByKey("data.npc.horse.description");
        }

        /// <summary>Get a translation equivalent to "A forest spirit from the spirit world. Very little is known about them.".</summary>
        public static string Data_Npc_Junimo_Description()
        {
            return I18n.GetByKey("data.npc.junimo.description");
        }

        /// <summary>Get a translation equivalent to "A friendly spirit who cleans up trash.".</summary>
        public static string Data_Npc_TrashBear_Description()
        {
            return I18n.GetByKey("data.npc.trash-bear.description");
        }

        /// <summary>Get a translation equivalent to "Debris".</summary>
        public static string Data_Type_Debris()
        {
            return I18n.GetByKey("data.type.debris");
        }

        /// <summary>Get a translation equivalent to "Stone".</summary>
        public static string Data_Type_Stone()
        {
            return I18n.GetByKey("data.type.stone");
        }

        /// <summary>Get a translation equivalent to "Container".</summary>
        public static string Data_Type_Container()
        {
            return I18n.GetByKey("data.type.container");
        }

        /// <summary>Get a translation equivalent to "Hatches eggs into baby chickens and ducks.".</summary>
        public static string Data_Item_EggIncubator_Description()
        {
            return I18n.GetByKey("data.item.egg-incubator.description");
        }

        /// <summary>Get a translation equivalent to "Broken Branch".</summary>
        public static string Data_Item_BrokenBranch_Name()
        {
            return I18n.GetByKey("data.item.broken-branch.name");
        }

        /// <summary>Get a translation equivalent to "A broken branch. Chop with an axe to obtain wood.".</summary>
        public static string Data_Item_BrokenBranch_Description()
        {
            return I18n.GetByKey("data.item.broken-branch.description");
        }

        /// <summary>Get a translation equivalent to "Stone".</summary>
        public static string Data_Item_Stone_Name()
        {
            return I18n.GetByKey("data.item.stone.name");
        }

        /// <summary>Get a translation equivalent to "A nondescript gray stone. Break apart to obtain stone.".</summary>
        public static string Data_Item_Stone_Description()
        {
            return I18n.GetByKey("data.item.stone.description");
        }

        /// <summary>Get a translation equivalent to "Geode Node".</summary>
        public static string Data_Item_Geode_Name()
        {
            return I18n.GetByKey("data.item.geode.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain geodes.".</summary>
        public static string Data_Item_Geode_Description()
        {
            return I18n.GetByKey("data.item.geode.description");
        }

        /// <summary>Get a translation equivalent to "Frozen Geode Node".</summary>
        public static string Data_Item_FrozenGeode_Name()
        {
            return I18n.GetByKey("data.item.frozen-geode.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain frozen geodes.".</summary>
        public static string Data_Item_FrozenGeode_Description()
        {
            return I18n.GetByKey("data.item.frozen-geode.description");
        }

        /// <summary>Get a translation equivalent to "Magma Geode Node".</summary>
        public static string Data_Item_MagmaGeode_Name()
        {
            return I18n.GetByKey("data.item.magma-geode.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain magma geodes.".</summary>
        public static string Data_Item_MagmaGeode_Description()
        {
            return I18n.GetByKey("data.item.magma-geode.description");
        }

        /// <summary>Get a translation equivalent to "Weed".</summary>
        public static string Data_Item_Weed_Name()
        {
            return I18n.GetByKey("data.item.weed.name");
        }

        /// <summary>Get a translation equivalent to "A nondescript weed. Might contain fibers or mixed seeds.".</summary>
        public static string Data_Item_Weed_Description()
        {
            return I18n.GetByKey("data.item.weed.description");
        }

        /// <summary>Get a translation equivalent to "Fertile Weed".</summary>
        public static string Data_Item_FertileWeed_Name()
        {
            return I18n.GetByKey("data.item.fertile-weed.name");
        }

        /// <summary>Get a translation equivalent to "Guaranteed to drop wild seeds.".</summary>
        public static string Data_Item_FertileWeed_Description()
        {
            return I18n.GetByKey("data.item.fertile-weed.description");
        }

        /// <summary>Get a translation equivalent to "Ice Crystal".</summary>
        public static string Data_Item_IceCrystal_Name()
        {
            return I18n.GetByKey("data.item.ice-crystal.name");
        }

        /// <summary>Get a translation equivalent to "A nondescript ice crystal. Very small chance of containing refined quartz.".</summary>
        public static string Data_Item_IceCrystal_Description()
        {
            return I18n.GetByKey("data.item.ice-crystal.description");
        }

        /// <summary>Get a translation equivalent to "Amethyst Cluster".</summary>
        public static string Data_Item_AmethystNode_Name()
        {
            return I18n.GetByKey("data.item.amethyst-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain amethysts.".</summary>
        public static string Data_Item_AmethystNode_Description()
        {
            return I18n.GetByKey("data.item.amethyst-node.description");
        }

        /// <summary>Get a translation equivalent to "Aquamarine Node".</summary>
        public static string Data_Item_AquamarineNode_Name()
        {
            return I18n.GetByKey("data.item.aquamarine-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain aquamarines.".</summary>
        public static string Data_Item_AquamarineNode_Description()
        {
            return I18n.GetByKey("data.item.aquamarine-node.description");
        }

        /// <summary>Get a translation equivalent to "Diamond Node".</summary>
        public static string Data_Item_DiamondNode_Name()
        {
            return I18n.GetByKey("data.item.diamond-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain diamonds.".</summary>
        public static string Data_Item_DiamondNode_Description()
        {
            return I18n.GetByKey("data.item.diamond-node.description");
        }

        /// <summary>Get a translation equivalent to "Emerald Node".</summary>
        public static string Data_Item_EmeraldNode_Name()
        {
            return I18n.GetByKey("data.item.emerald-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain emeralds.".</summary>
        public static string Data_Item_EmeraldNode_Description()
        {
            return I18n.GetByKey("data.item.emerald-node.description");
        }

        /// <summary>Get a translation equivalent to "Gem Node".</summary>
        public static string Data_Item_GemNode_Name()
        {
            return I18n.GetByKey("data.item.gem-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain emeralds, aquamarines, rubies, amethysts, topaz, jade, or diamonds.".</summary>
        public static string Data_Item_GemNode_Description()
        {
            return I18n.GetByKey("data.item.gem-node.description");
        }

        /// <summary>Get a translation equivalent to "Jade Node".</summary>
        public static string Data_Item_JadeNode_Name()
        {
            return I18n.GetByKey("data.item.jade-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain jade.".</summary>
        public static string Data_Item_JadeNode_Description()
        {
            return I18n.GetByKey("data.item.jade-node.description");
        }

        /// <summary>Get a translation equivalent to "Mystic Stone".</summary>
        public static string Data_Item_MysticStone_Name()
        {
            return I18n.GetByKey("data.item.mystic-stone.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain rare gems and ores, including prismatic shards.".</summary>
        public static string Data_Item_MysticStone_Description()
        {
            return I18n.GetByKey("data.item.mystic-stone.description");
        }

        /// <summary>Get a translation equivalent to "Prismatic Node".</summary>
        public static string Data_Item_PrismaticNode_Name()
        {
            return I18n.GetByKey("data.item.prismatic-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain prismatic shards.".</summary>
        public static string Data_Item_PrismaticNode_Description()
        {
            return I18n.GetByKey("data.item.prismatic-node.description");
        }

        /// <summary>Get a translation equivalent to "Ruby Node".</summary>
        public static string Data_Item_RubyNode_Name()
        {
            return I18n.GetByKey("data.item.ruby-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain rubies.".</summary>
        public static string Data_Item_RubyNode_Description()
        {
            return I18n.GetByKey("data.item.ruby-node.description");
        }

        /// <summary>Get a translation equivalent to "Topaz Node".</summary>
        public static string Data_Item_TopazNode_Name()
        {
            return I18n.GetByKey("data.item.topaz-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain topaz.".</summary>
        public static string Data_Item_TopazNode_Description()
        {
            return I18n.GetByKey("data.item.topaz-node.description");
        }

        /// <summary>Get a translation equivalent to "Cinder Shard Node".</summary>
        public static string Data_Item_CinderShardNode_Name()
        {
            return I18n.GetByKey("data.item.cinder-shard-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain cinder shards.".</summary>
        public static string Data_Item_CinderShardNode_Description()
        {
            return I18n.GetByKey("data.item.cinder-shard-node.description");
        }

        /// <summary>Get a translation equivalent to "Copper Node".</summary>
        public static string Data_Item_CopperNode_Name()
        {
            return I18n.GetByKey("data.item.copper-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain copper ore.".</summary>
        public static string Data_Item_CopperNode_Description()
        {
            return I18n.GetByKey("data.item.copper-node.description");
        }

        /// <summary>Get a translation equivalent to "Gold Node".</summary>
        public static string Data_Item_GoldNode_Name()
        {
            return I18n.GetByKey("data.item.gold-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain gold ore.".</summary>
        public static string Data_Item_GoldNode_Description()
        {
            return I18n.GetByKey("data.item.gold-node.description");
        }

        /// <summary>Get a translation equivalent to "Iridium Node".</summary>
        public static string Data_Item_IridiumNode_Name()
        {
            return I18n.GetByKey("data.item.iridium-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain iridium ore.".</summary>
        public static string Data_Item_IridiumNode_Description()
        {
            return I18n.GetByKey("data.item.iridium-node.description");
        }

        /// <summary>Get a translation equivalent to "Iron Node".</summary>
        public static string Data_Item_IronNode_Name()
        {
            return I18n.GetByKey("data.item.iron-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain iron ore.".</summary>
        public static string Data_Item_IronNode_Description()
        {
            return I18n.GetByKey("data.item.iron-node.description");
        }

        /// <summary>Get a translation equivalent to "Omni Geode Node".</summary>
        public static string Data_Item_OmniGeodeNode_Name()
        {
            return I18n.GetByKey("data.item.omni-geode-node.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain an omni geode.".</summary>
        public static string Data_Item_OmniGeodeNode_Description()
        {
            return I18n.GetByKey("data.item.omni-geode-node.description");
        }

        /// <summary>Get a translation equivalent to "Colored Stone".</summary>
        public static string Data_Item_ColoredStone_Name()
        {
            return I18n.GetByKey("data.item.colored-stone.name");
        }

        /// <summary>Get a translation equivalent to "A nondescript stone. It might contain stone, ores, or geodes.".</summary>
        public static string Data_Item_ColoredStone_Description()
        {
            return I18n.GetByKey("data.item.colored-stone.description");
        }

        /// <summary>Get a translation equivalent to "Stone".</summary>
        public static string Data_Item_MineStone_Name()
        {
            return I18n.GetByKey("data.item.mine-stone.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain stone with a chance of coal.".</summary>
        public static string Data_Item_MineStone_Description()
        {
            return I18n.GetByKey("data.item.mine-stone.description");
        }

        /// <summary>Get a translation equivalent to "Clay Stone".</summary>
        public static string Data_Item_ClayStone_Name()
        {
            return I18n.GetByKey("data.item.clay-stone.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain clay.".</summary>
        public static string Data_Item_ClayStone_Description()
        {
            return I18n.GetByKey("data.item.clay-stone.description");
        }

        /// <summary>Get a translation equivalent to "Fossil Stone".</summary>
        public static string Data_Item_FossilStone_Name()
        {
            return I18n.GetByKey("data.item.fossil-stone.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain fossils and bones.".</summary>
        public static string Data_Item_FossilStone_Description()
        {
            return I18n.GetByKey("data.item.fossil-stone.description");
        }

        /// <summary>Get a translation equivalent to "Barrel".</summary>
        public static string Data_Item_Barrel_Name()
        {
            return I18n.GetByKey("data.item.barrel.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain sundry goods.".</summary>
        public static string Data_Item_Barrel_Description()
        {
            return I18n.GetByKey("data.item.barrel.description");
        }

        /// <summary>Get a translation equivalent to "Box".</summary>
        public static string Data_Item_Box_Name()
        {
            return I18n.GetByKey("data.item.box.name");
        }

        /// <summary>Get a translation equivalent to "Break apart to obtain sundry goods.".</summary>
        public static string Data_Item_Box_Description()
        {
            return I18n.GetByKey("data.item.box.description");
        }

        /// <summary>Get a translation equivalent to "backwoods".</summary>
        public static string Location_Backwoods()
        {
            return I18n.GetByKey("location.backwoods");
        }

        /// <summary>Get a translation equivalent to "beach".</summary>
        public static string Location_Beach()
        {
            return I18n.GetByKey("location.beach");
        }

        /// <summary>Get a translation equivalent to "beach (eastern pier)".</summary>
        public static string Location_Beach_EastPier()
        {
            return I18n.GetByKey("location.beach.east-pier");
        }

        /// <summary>Get a translation equivalent to "mutant bug lair".</summary>
        public static string Location_BugLand()
        {
            return I18n.GetByKey("location.bugLand");
        }

        /// <summary>Get a translation equivalent to "bus stop".</summary>
        public static string Location_BusStop()
        {
            return I18n.GetByKey("location.busStop");
        }

        /// <summary>Get a translation equivalent to "desert".</summary>
        public static string Location_Desert()
        {
            return I18n.GetByKey("location.desert");
        }

        /// <summary>Get a translation equivalent to "farm".</summary>
        public static string Location_Farm()
        {
            return I18n.GetByKey("location.farm");
        }

        /// <summary>Get a translation equivalent to "farm cave".</summary>
        public static string Location_FarmCave()
        {
            return I18n.GetByKey("location.farmCave");
        }

        /// <summary>Get a translation equivalent to "farmhouse".</summary>
        public static string Location_FarmHouse()
        {
            return I18n.GetByKey("location.farmHouse");
        }

        /// <summary>Get a translation equivalent to "forest".</summary>
        public static string Location_Forest()
        {
            return I18n.GetByKey("location.forest");
        }

        /// <summary>Get a translation equivalent to "forest river".</summary>
        public static string Location_Forest_FishArea0()
        {
            return I18n.GetByKey("location.forest.fish-area-0");
        }

        /// <summary>Get a translation equivalent to "forest pond".</summary>
        public static string Location_Forest_FishArea1()
        {
            return I18n.GetByKey("location.forest.fish-area-1");
        }

        /// <summary>Get a translation equivalent to "forest river (south tip of large island)".</summary>
        public static string Location_Forest_IslandTip()
        {
            return I18n.GetByKey("location.forest.island-tip");
        }

        /// <summary>Get a translation equivalent to "mountain".</summary>
        public static string Location_Mountain()
        {
            return I18n.GetByKey("location.mountain");
        }

        /// <summary>Get a translation equivalent to "railroad".</summary>
        public static string Location_Railroad()
        {
            return I18n.GetByKey("location.railroad");
        }

        /// <summary>Get a translation equivalent to "sewers".</summary>
        public static string Location_Sewer()
        {
            return I18n.GetByKey("location.sewer");
        }

        /// <summary>Get a translation equivalent to "Night Market submarine".</summary>
        public static string Location_Submarine()
        {
            return I18n.GetByKey("location.submarine");
        }

        /// <summary>Get a translation equivalent to "town".</summary>
        public static string Location_Town()
        {
            return I18n.GetByKey("location.town");
        }

        /// <summary>Get a translation equivalent to "town (northmost bridge)".</summary>
        public static string Location_Town_NorthmostBridge()
        {
            return I18n.GetByKey("location.town.northmost-bridge");
        }

        /// <summary>Get a translation equivalent to "bus tunnel".</summary>
        public static string Location_Tunnel()
        {
            return I18n.GetByKey("location.tunnel");
        }

        /// <summary>Get a translation equivalent to "mines".</summary>
        public static string Location_UndergroundMine()
        {
            return I18n.GetByKey("location.undergroundMine");
        }

        /// <summary>Get a translation equivalent to "mine level {{level}}".</summary>
        /// <param name="level">The value to inject for the <c>{{level}}</c> token.</param>
        public static string Location_UndergroundMine_Level(object level)
        {
            return I18n.GetByKey("location.undergroundMine.level", new { level });
        }

        /// <summary>Get a translation equivalent to "witch's swamp".</summary>
        public static string Location_WitchSwamp()
        {
            return I18n.GetByKey("location.witchSwamp");
        }

        /// <summary>Get a translation equivalent to "secret woods".</summary>
        public static string Location_Woods()
        {
            return I18n.GetByKey("location.woods");
        }

        /// <summary>Get a translation equivalent to "{{locationName}} (fishing area {{id}})".</summary>
        /// <param name="locationName">The value to inject for the <c>{{locationName}}</c> token.</param>
        /// <param name="id">The value to inject for the <c>{{id}}</c> token.</param>
        public static string Location_UnknownFishArea(object locationName, object id)
        {
            return I18n.GetByKey("location.unknown-fish-area", new { locationName, id });
        }

        /// <summary>Get a translation equivalent to "Love".</summary>
        public static string Animal_Love()
        {
            return I18n.GetByKey("animal.love");
        }

        /// <summary>Get a translation equivalent to "Happiness".</summary>
        public static string Animal_Happiness()
        {
            return I18n.GetByKey("animal.happiness");
        }

        /// <summary>Get a translation equivalent to "Mood today".</summary>
        public static string Animal_Mood()
        {
            return I18n.GetByKey("animal.mood");
        }

        /// <summary>Get a translation equivalent to "Complaints".</summary>
        public static string Animal_Complaints()
        {
            return I18n.GetByKey("animal.complaints");
        }

        /// <summary>Get a translation equivalent to "Produce ready".</summary>
        public static string Animal_ProduceReady()
        {
            return I18n.GetByKey("animal.produce-ready");
        }

        /// <summary>Get a translation equivalent to "Growth".</summary>
        public static string Animal_Growth()
        {
            return I18n.GetByKey("animal.growth");
        }

        /// <summary>Get a translation equivalent to "Sells for".</summary>
        public static string Animal_SellsFor()
        {
            return I18n.GetByKey("animal.sells-for");
        }

        /// <summary>Get a translation equivalent to "wasn't fed yesterday".</summary>
        public static string Animal_Complaints_Hungry()
        {
            return I18n.GetByKey("animal.complaints.hungry");
        }

        /// <summary>Get a translation equivalent to "was left outside last night".</summary>
        public static string Animal_Complaints_LeftOut()
        {
            return I18n.GetByKey("animal.complaints.left-out");
        }

        /// <summary>Get a translation equivalent to "moved into new home".</summary>
        public static string Animal_Complaints_NewHome()
        {
            return I18n.GetByKey("animal.complaints.new-home");
        }

        /// <summary>Get a translation equivalent to "no heater in winter".</summary>
        public static string Animal_Complaints_NoHeater()
        {
            return I18n.GetByKey("animal.complaints.no-heater");
        }

        /// <summary>Get a translation equivalent to "hasn't been petted today".</summary>
        public static string Animal_Complaints_NotPetted()
        {
            return I18n.GetByKey("animal.complaints.not-petted");
        }

        /// <summary>Get a translation equivalent to "attacked by wild animals during the night".</summary>
        public static string Animal_Complaints_WildAnimalAttack()
        {
            return I18n.GetByKey("animal.complaints.wild-animal-attack");
        }

        /// <summary>Get a translation equivalent to "Animals".</summary>
        public static string Building_Animals()
        {
            return I18n.GetByKey("building.animals");
        }

        /// <summary>Get a translation equivalent to "Construction".</summary>
        public static string Building_Construction()
        {
            return I18n.GetByKey("building.construction");
        }

        /// <summary>Get a translation equivalent to "Feed trough".</summary>
        public static string Building_FeedTrough()
        {
            return I18n.GetByKey("building.feed-trough");
        }

        /// <summary>Get a translation equivalent to "Horse".</summary>
        public static string Building_Horse()
        {
            return I18n.GetByKey("building.horse");
        }

        /// <summary>Get a translation equivalent to "Horse location".</summary>
        public static string Building_HorseLocation()
        {
            return I18n.GetByKey("building.horse-location");
        }

        /// <summary>Get a translation equivalent to "Owner".</summary>
        public static string Building_Owner()
        {
            return I18n.GetByKey("building.owner");
        }

        /// <summary>Get a translation equivalent to "Slimes".</summary>
        public static string Building_Slimes()
        {
            return I18n.GetByKey("building.slimes");
        }

        /// <summary>Get a translation equivalent to "Stored hay".</summary>
        public static string Building_StoredHay()
        {
            return I18n.GetByKey("building.stored-hay");
        }

        /// <summary>Get a translation equivalent to "Upgrades".</summary>
        public static string Building_Upgrades()
        {
            return I18n.GetByKey("building.upgrades");
        }

        /// <summary>Get a translation equivalent to "Water trough".</summary>
        public static string Building_WaterTrough()
        {
            return I18n.GetByKey("building.water-trough");
        }

        /// <summary>Get a translation equivalent to "{{count}} of max {{max}} animals".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Building_Animals_Summary(object count, object max)
        {
            return I18n.GetByKey("building.animals.summary", new { count, max });
        }

        /// <summary>Get a translation equivalent to "ready on {{date}}".</summary>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string Building_Construction_Summary(object date)
        {
            return I18n.GetByKey("building.construction.summary", new { date });
        }

        /// <summary>Get a translation equivalent to "automated".</summary>
        public static string Building_FeedTrough_Automated()
        {
            return I18n.GetByKey("building.feed-trough.automated");
        }

        /// <summary>Get a translation equivalent to "{{filled}} of {{max}} feed slots filled".</summary>
        /// <param name="filled">The value to inject for the <c>{{filled}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Building_FeedTrough_Summary(object filled, object max)
        {
            return I18n.GetByKey("building.feed-trough.summary", new { filled, max });
        }

        /// <summary>Get a translation equivalent to "{{location}} ({{x}}, {{y}})".</summary>
        /// <param name="location">The value to inject for the <c>{{location}}</c> token.</param>
        /// <param name="x">The value to inject for the <c>{{x}}</c> token.</param>
        /// <param name="y">The value to inject for the <c>{{y}}</c> token.</param>
        public static string Building_HorseLocation_Summary(object location, object x, object y)
        {
            return I18n.GetByKey("building.horse-location.summary", new { location, x, y });
        }

        /// <summary>Get a translation equivalent to "Harvesting enabled".</summary>
        public static string Building_JunimoHarvestingEnabled()
        {
            return I18n.GetByKey("building.junimo-harvesting-enabled");
        }

        /// <summary>Get a translation equivalent to "no owner".</summary>
        public static string Building_Owner_None()
        {
            return I18n.GetByKey("building.owner.none");
        }

        /// <summary>Get a translation equivalent to "Output processing".</summary>
        public static string Building_OutputProcessing()
        {
            return I18n.GetByKey("building.output-processing");
        }

        /// <summary>Get a translation equivalent to "Output ready".</summary>
        public static string Building_OutputReady()
        {
            return I18n.GetByKey("building.output-ready");
        }

        /// <summary>Get a translation equivalent to "{{count}} of max {{max}} slimes".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Building_Slimes_Summary(object count, object max)
        {
            return I18n.GetByKey("building.slimes.summary", new { count, max });
        }

        /// <summary>Get a translation equivalent to "{{hayCount}} hay (max capacity: {{maxHay}})".</summary>
        /// <param name="hayCount">The value to inject for the <c>{{hayCount}}</c> token.</param>
        /// <param name="maxHay">The value to inject for the <c>{{maxHay}}</c> token.</param>
        public static string Building_StoredHay_SummaryOneSilo(object hayCount, object maxHay)
        {
            return I18n.GetByKey("building.stored-hay.summary-one-silo", new { hayCount, maxHay });
        }

        /// <summary>Get a translation equivalent to "{{hayCount}} hay in {{siloCount}} silos (max capacity: {{maxHay}})".</summary>
        /// <param name="hayCount">The value to inject for the <c>{{hayCount}}</c> token.</param>
        /// <param name="siloCount">The value to inject for the <c>{{siloCount}}</c> token.</param>
        /// <param name="maxHay">The value to inject for the <c>{{maxHay}}</c> token.</param>
        public static string Building_StoredHay_SummaryMultipleSilos(object hayCount, object siloCount, object maxHay)
        {
            return I18n.GetByKey("building.stored-hay.summary-multiple-silos", new { hayCount, siloCount, maxHay });
        }

        /// <summary>Get a translation equivalent to "up to 4 animals, add cows".</summary>
        public static string Building_Upgrades_Barn_0()
        {
            return I18n.GetByKey("building.upgrades.barn.0");
        }

        /// <summary>Get a translation equivalent to "up to 8 animals, add pregnancy and goats".</summary>
        public static string Building_Upgrades_Barn_1()
        {
            return I18n.GetByKey("building.upgrades.barn.1");
        }

        /// <summary>Get a translation equivalent to "up to 12 animals, add autofeed, pigs, and sheep".</summary>
        public static string Building_Upgrades_Barn_2()
        {
            return I18n.GetByKey("building.upgrades.barn.2");
        }

        /// <summary>Get a translation equivalent to "initial cabin".</summary>
        public static string Building_Upgrades_Cabin_0()
        {
            return I18n.GetByKey("building.upgrades.cabin.0");
        }

        /// <summary>Get a translation equivalent to "add kitchen, enable marriage".</summary>
        public static string Building_Upgrades_Cabin_1()
        {
            return I18n.GetByKey("building.upgrades.cabin.1");
        }

        /// <summary>Get a translation equivalent to "enable children".</summary>
        public static string Building_Upgrades_Cabin_2()
        {
            return I18n.GetByKey("building.upgrades.cabin.2");
        }

        /// <summary>Get a translation equivalent to "up to 4 animals; add chickens".</summary>
        public static string Building_Upgrades_Coop_0()
        {
            return I18n.GetByKey("building.upgrades.coop.0");
        }

        /// <summary>Get a translation equivalent to "up to 8 animals; add incubator, dinosaurs, and ducks".</summary>
        public static string Building_Upgrades_Coop_1()
        {
            return I18n.GetByKey("building.upgrades.coop.1");
        }

        /// <summary>Get a translation equivalent to "up to 12 animals; add autofeed and rabbits".</summary>
        public static string Building_Upgrades_Coop_2()
        {
            return I18n.GetByKey("building.upgrades.coop.2");
        }

        /// <summary>Get a translation equivalent to "{{filled}} of {{max}} water troughs filled".</summary>
        /// <param name="filled">The value to inject for the <c>{{filled}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Building_WaterTrough_Summary(object filled, object max)
        {
            return I18n.GetByKey("building.water-trough.summary", new { filled, max });
        }

        /// <summary>Get a translation equivalent to "Population".</summary>
        public static string Building_FishPond_Population()
        {
            return I18n.GetByKey("building.fish-pond.population");
        }

        /// <summary>Get a translation equivalent to "Drops".</summary>
        public static string Building_FishPond_Drops()
        {
            return I18n.GetByKey("building.fish-pond.drops");
        }

        /// <summary>Get a translation equivalent to "Quests".</summary>
        public static string Building_FishPond_Quests()
        {
            return I18n.GetByKey("building.fish-pond.quests");
        }

        /// <summary>Get a translation equivalent to "Add a fish to start this pond".</summary>
        public static string Building_FishPond_Population_Empty()
        {
            return I18n.GetByKey("building.fish-pond.population.empty");
        }

        /// <summary>Get a translation equivalent to "New fish spawns {{relativeDate}}".</summary>
        /// <param name="relativeDate">The value to inject for the <c>{{relativeDate}}</c> token.</param>
        public static string Building_FishPond_Population_NextSpawn(object relativeDate)
        {
            return I18n.GetByKey("building.fish-pond.population.next-spawn", new { relativeDate });
        }

        /// <summary>Get a translation equivalent to "{{chance}}% chance each day of producing the first matched item:".</summary>
        /// <param name="chance">The value to inject for the <c>{{chance}}</c> token.</param>
        public static string Building_FishPond_Drops_Preface(object chance)
        {
            return I18n.GetByKey("building.fish-pond.drops.preface", new { chance });
        }

        /// <summary>Get a translation equivalent to "With {{count}} fish:".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Building_FishPond_Drops_MinFish(object count)
        {
            return I18n.GetByKey("building.fish-pond.drops.min-fish", new { count });
        }

        /// <summary>Get a translation equivalent to "{{count}} fish".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Building_FishPond_Quests_Done(object count)
        {
            return I18n.GetByKey("building.fish-pond.quests.done", new { count });
        }

        /// <summary>Get a translation equivalent to "{{count}} fish: will need {{itemName}}".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        /// <param name="itemName">The value to inject for the <c>{{itemName}}</c> token.</param>
        public static string Building_FishPond_Quests_IncompleteOne(object count, object itemName)
        {
            return I18n.GetByKey("building.fish-pond.quests.incomplete-one", new { count, itemName });
        }

        /// <summary>Get a translation equivalent to "{{count}} fish: will need one of {{itemList}}".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        /// <param name="itemList">The value to inject for the <c>{{itemList}}</c> token.</param>
        public static string Building_FishPond_Quests_IncompleteRandom(object count, object itemList)
        {
            return I18n.GetByKey("building.fish-pond.quests.incomplete-random", new { count, itemList });
        }

        /// <summary>Get a translation equivalent to "available {{relativeDate}}".</summary>
        /// <param name="relativeDate">The value to inject for the <c>{{relativeDate}}</c> token.</param>
        public static string Building_FishPond_Quests_Available(object relativeDate)
        {
            return I18n.GetByKey("building.fish-pond.quests.available", new { relativeDate });
        }

        /// <summary>Get a translation equivalent to "Berry Bush".</summary>
        public static string Bush_Name_Berry()
        {
            return I18n.GetByKey("bush.name.berry");
        }

        /// <summary>Get a translation equivalent to "Plain Bush".</summary>
        public static string Bush_Name_Plain()
        {
            return I18n.GetByKey("bush.name.plain");
        }

        /// <summary>Get a translation equivalent to "Tea Bush".</summary>
        public static string Bush_Name_Tea()
        {
            return I18n.GetByKey("bush.name.tea");
        }

        /// <summary>Get a translation equivalent to "A bush that grows salmonberries and blackberries.".</summary>
        public static string Bush_Description_Berry()
        {
            return I18n.GetByKey("bush.description.berry");
        }

        /// <summary>Get a translation equivalent to "A plain bush that doesn't grow anything.".</summary>
        public static string Bush_Description_Plain()
        {
            return I18n.GetByKey("bush.description.plain");
        }

        /// <summary>Get a translation equivalent to "A bush that grows tea leaves.".</summary>
        public static string Bush_Description_Tea()
        {
            return I18n.GetByKey("bush.description.Tea");
        }

        /// <summary>Get a translation equivalent to "Date planted".</summary>
        public static string Bush_DatePlanted()
        {
            return I18n.GetByKey("bush.date-planted");
        }

        /// <summary>Get a translation equivalent to "Growth".</summary>
        public static string Bush_Growth()
        {
            return I18n.GetByKey("bush.growth");
        }

        /// <summary>Get a translation equivalent to "Next harvest".</summary>
        public static string Bush_NextHarvest()
        {
            return I18n.GetByKey("bush.next-harvest");
        }

        /// <summary>Get a translation equivalent to "mature on {{date}}".</summary>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string Bush_Growth_Summary(object date)
        {
            return I18n.GetByKey("bush.growth.summary", new { date });
        }

        /// <summary>Get a translation equivalent to "20% chance of salmonberries in spring 15-18 and blackberries in fall 8-11.".</summary>
        public static string Bush_Schedule_Berry()
        {
            return I18n.GetByKey("bush.schedule.berry");
        }

        /// <summary>Get a translation equivalent to "Grows tea leaves from the 22nd of each season (except winter if not in the greenhouse).".</summary>
        public static string Bush_Schedule_Tea()
        {
            return I18n.GetByKey("bush.schedule.tea");
        }

        /// <summary>Get a translation equivalent to "Complaints".</summary>
        public static string FruitTree_Complaints()
        {
            return I18n.GetByKey("fruit-tree.complaints");
        }

        /// <summary>Get a translation equivalent to "{{fruitName}} Tree".</summary>
        /// <param name="fruitName">The value to inject for the <c>{{fruitName}}</c> token.</param>
        public static string FruitTree_Name(object fruitName)
        {
            return I18n.GetByKey("fruit-tree.name", new { fruitName });
        }

        /// <summary>Get a translation equivalent to "Growth".</summary>
        public static string FruitTree_Growth()
        {
            return I18n.GetByKey("fruit-tree.growth");
        }

        /// <summary>Get a translation equivalent to "Next fruit".</summary>
        public static string FruitTree_NextFruit()
        {
            return I18n.GetByKey("fruit-tree.next-fruit");
        }

        /// <summary>Get a translation equivalent to "Season".</summary>
        public static string FruitTree_Season()
        {
            return I18n.GetByKey("fruit-tree.season");
        }

        /// <summary>Get a translation equivalent to "Quality".</summary>
        public static string FruitTree_Quality()
        {
            return I18n.GetByKey("fruit-tree.quality");
        }

        /// <summary>Get a translation equivalent to "can't grow because there are adjacent objects".</summary>
        public static string FruitTree_Complaints_AdjacentObjects()
        {
            return I18n.GetByKey("fruit-tree.complaints.adjacent-objects");
        }

        /// <summary>Get a translation equivalent to "mature on {{date}}".</summary>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string FruitTree_Growth_Summary(object date)
        {
            return I18n.GetByKey("fruit-tree.growth.summary", new { date });
        }

        /// <summary>Get a translation equivalent to "won't grow any more fruit until you harvest those it has".</summary>
        public static string FruitTree_NextFruit_MaxFruit()
        {
            return I18n.GetByKey("fruit-tree.next-fruit.max-fruit");
        }

        /// <summary>Get a translation equivalent to "out of season".</summary>
        public static string FruitTree_NextFruit_OutOfSeason()
        {
            return I18n.GetByKey("fruit-tree.next-fruit.out-of-season");
        }

        /// <summary>Get a translation equivalent to "struck by lightning! Will recover in {{count}} days.".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string FruitTree_NextFruit_StruckByLightning(object count)
        {
            return I18n.GetByKey("fruit-tree.next-fruit.struck-by-lightning", new { count });
        }

        /// <summary>Get a translation equivalent to "too young to bear fruit".</summary>
        public static string FruitTree_NextFruit_TooYoung()
        {
            return I18n.GetByKey("fruit-tree.next-fruit.too-young");
        }

        /// <summary>Get a translation equivalent to "{{quality}} now".</summary>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        public static string FruitTree_Quality_Now(object quality)
        {
            return I18n.GetByKey("fruit-tree.quality.now", new { quality });
        }

        /// <summary>Get a translation equivalent to "{{quality}} on {{date}}".</summary>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string FruitTree_Quality_OnDate(object quality, object date)
        {
            return I18n.GetByKey("fruit-tree.quality.on-date", new { quality, date });
        }

        /// <summary>Get a translation equivalent to "{{quality}} on {{date}} next year".</summary>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string FruitTree_Quality_OnDateNextYear(object quality, object date)
        {
            return I18n.GetByKey("fruit-tree.quality.on-date-next-year", new { quality, date });
        }

        /// <summary>Get a translation equivalent to "{{season}} (or anytime in greenhouse)".</summary>
        /// <param name="season">The value to inject for the <c>{{season}}</c> token.</param>
        public static string FruitTree_Season_Summary(object season)
        {
            return I18n.GetByKey("fruit-tree.season.summary", new { season });
        }

        /// <summary>Get a translation equivalent to "Crop".</summary>
        public static string Crop_Summary()
        {
            return I18n.GetByKey("crop.summary");
        }

        /// <summary>Get a translation equivalent to "Harvest".</summary>
        public static string Crop_Harvest()
        {
            return I18n.GetByKey("crop.harvest");
        }

        /// <summary>Get a translation equivalent to "This crop is dead.".</summary>
        public static string Crop_Summary_Dead()
        {
            return I18n.GetByKey("crop.summary.dead");
        }

        /// <summary>Get a translation equivalent to "drops {{count}}".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Crop_Summary_DropsX(object count)
        {
            return I18n.GetByKey("crop.summary.drops-X", new { count });
        }

        /// <summary>Get a translation equivalent to "drops {{min}} to {{max}} ({{percent}}% chance of extra crops)".</summary>
        /// <param name="min">The value to inject for the <c>{{min}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        /// <param name="percent">The value to inject for the <c>{{percent}}</c> token.</param>
        public static string Crop_Summary_DropsXToY(object min, object max, object percent)
        {
            return I18n.GetByKey("crop.summary.drops-X-to-Y", new { min, max, percent });
        }

        /// <summary>Get a translation equivalent to "harvest after {{daysToFirstHarvest}} days".</summary>
        /// <param name="daysToFirstHarvest">The value to inject for the <c>{{daysToFirstHarvest}}</c> token.</param>
        public static string Crop_Summary_HarvestOnce(object daysToFirstHarvest)
        {
            return I18n.GetByKey("crop.summary.harvest-once", new { daysToFirstHarvest });
        }

        /// <summary>Get a translation equivalent to "harvest after {{daysToFirstHarvest}} days, then every {{daysToNextHarvests}} days".</summary>
        /// <param name="daysToFirstHarvest">The value to inject for the <c>{{daysToFirstHarvest}}</c> token.</param>
        /// <param name="daysToNextHarvests">The value to inject for the <c>{{daysToNextHarvests}}</c> token.</param>
        public static string Crop_Summary_HarvestMulti(object daysToFirstHarvest, object daysToNextHarvests)
        {
            return I18n.GetByKey("crop.summary.harvest-multi", new { daysToFirstHarvest, daysToNextHarvests });
        }

        /// <summary>Get a translation equivalent to "grows in {{seasons}}".</summary>
        /// <param name="seasons">The value to inject for the <c>{{seasons}}</c> token.</param>
        public static string Crop_Summary_Seasons(object seasons)
        {
            return I18n.GetByKey("crop.summary.seasons", new { seasons });
        }

        /// <summary>Get a translation equivalent to "sells for {{price}}".</summary>
        /// <param name="price">The value to inject for the <c>{{price}}</c> token.</param>
        public static string Crop_Summary_SellsFor(object price)
        {
            return I18n.GetByKey("crop.summary.sells-for", new { price });
        }

        /// <summary>Get a translation equivalent to "too late in the season for the next harvest (would be on {{date}})".</summary>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string Crop_Harvest_TooLate(object date)
        {
            return I18n.GetByKey("crop.harvest.too-late", new { date });
        }

        /// <summary>Get a translation equivalent to "Contents".</summary>
        public static string Item_Contents()
        {
            return I18n.GetByKey("item.contents");
        }

        /// <summary>Get a translation equivalent to "Loves this".</summary>
        public static string Item_LovesThis()
        {
            return I18n.GetByKey("item.loves-this");
        }

        /// <summary>Get a translation equivalent to "Likes this".</summary>
        public static string Item_LikesThis()
        {
            return I18n.GetByKey("item.likes-this");
        }

        /// <summary>Get a translation equivalent to "Neutral about this".</summary>
        public static string Item_NeutralAboutThis()
        {
            return I18n.GetByKey("item.neutral-about-this");
        }

        /// <summary>Get a translation equivalent to "Dislikes this".</summary>
        public static string Item_DislikesThis()
        {
            return I18n.GetByKey("item.dislikes-this");
        }

        /// <summary>Get a translation equivalent to "Hates this".</summary>
        public static string Item_HatesThis()
        {
            return I18n.GetByKey("item.hates-this");
        }

        /// <summary>Get a translation equivalent to "Needed for".</summary>
        public static string Item_NeededFor()
        {
            return I18n.GetByKey("item.needed-for");
        }

        /// <summary>Get a translation equivalent to "Owned".</summary>
        public static string Item_NumberOwned()
        {
            return I18n.GetByKey("item.number-owned");
        }

        /// <summary>Get a translation equivalent to "Cooked".</summary>
        public static string Item_NumberCooked()
        {
            return I18n.GetByKey("item.number-cooked");
        }

        /// <summary>Get a translation equivalent to "Crafted".</summary>
        public static string Item_NumberCrafted()
        {
            return I18n.GetByKey("item.number-crafted");
        }

        /// <summary>Get a translation equivalent to "Recipes".</summary>
        public static string Item_Recipes()
        {
            return I18n.GetByKey("item.recipes");
        }

        /// <summary>Get a translation equivalent to "See also".</summary>
        public static string Item_SeeAlso()
        {
            return I18n.GetByKey("item.see-also");
        }

        /// <summary>Get a translation equivalent to "Sells for".</summary>
        public static string Item_SellsFor()
        {
            return I18n.GetByKey("item.sells-for");
        }

        /// <summary>Get a translation equivalent to "Sells to".</summary>
        public static string Item_SellsTo()
        {
            return I18n.GetByKey("item.sells-to");
        }

        /// <summary>Get a translation equivalent to "Can be dyed".</summary>
        public static string Item_CanBeDyed()
        {
            return I18n.GetByKey("item.can-be-dyed");
        }

        /// <summary>Get a translation equivalent to "Produces dye".</summary>
        public static string Item_ProducesDye()
        {
            return I18n.GetByKey("item.produces-dye");
        }

        /// <summary>Get a translation equivalent to "{{name}} placed".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        public static string Item_Contents_Placed(object name)
        {
            return I18n.GetByKey("item.contents.placed", new { name });
        }

        /// <summary>Get a translation equivalent to "{{name}} ready".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        public static string Item_Contents_Ready(object name)
        {
            return I18n.GetByKey("item.contents.ready", new { name });
        }

        /// <summary>Get a translation equivalent to "{{name}} in {{time}}".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        /// <param name="time">The value to inject for the <c>{{time}}</c> token.</param>
        public static string Item_Contents_Partial(object name, object time)
        {
            return I18n.GetByKey("item.contents.partial", new { name, time });
        }

        /// <summary>Get a translation equivalent to "{{count}} unrevealed villagers".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_UndiscoveredGiftTaste(object count)
        {
            return I18n.GetByKey("item.undiscovered-gift-taste", new { count });
        }

        /// <summary>Get a translation equivalent to ", and {{count}} unrevealed villagers".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_UndiscoveredGiftTasteAppended(object count)
        {
            return I18n.GetByKey("item.undiscovered-gift-taste-appended", new { count });
        }

        /// <summary>Get a translation equivalent to "community center ({{bundles}})".</summary>
        /// <param name="bundles">The value to inject for the <c>{{bundles}}</c> token.</param>
        public static string Item_NeededFor_CommunityCenter(object bundles)
        {
            return I18n.GetByKey("item.needed-for.community-center", new { bundles });
        }

        /// <summary>Get a translation equivalent to "full shipment achievement (ship one)".</summary>
        public static string Item_NeededFor_FullShipment()
        {
            return I18n.GetByKey("item.needed-for.full-shipment");
        }

        /// <summary>Get a translation equivalent to "polyculture achievement (ship {{count}} more)".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_NeededFor_Polyculture(object count)
        {
            return I18n.GetByKey("item.needed-for.polyculture", new { count });
        }

        /// <summary>Get a translation equivalent to "full collection achievement (donate one to museum)".</summary>
        public static string Item_NeededFor_FullCollection()
        {
            return I18n.GetByKey("item.needed-for.full-collection");
        }

        /// <summary>Get a translation equivalent to "gourmet chef achievement (cook {{recipes}})".</summary>
        /// <param name="recipes">The value to inject for the <c>{{recipes}}</c> token.</param>
        public static string Item_NeededFor_GourmetChef(object recipes)
        {
            return I18n.GetByKey("item.needed-for.gourmet-chef", new { recipes });
        }

        /// <summary>Get a translation equivalent to "craft master achievement (make {{recipes}})".</summary>
        /// <param name="recipes">The value to inject for the <c>{{recipes}}</c> token.</param>
        public static string Item_NeededFor_CraftMaster(object recipes)
        {
            return I18n.GetByKey("item.needed-for.craft-master", new { recipes });
        }

        /// <summary>Get a translation equivalent to "shipping box".</summary>
        public static string Item_SellsTo_ShippingBox()
        {
            return I18n.GetByKey("item.sells-to.shipping-box");
        }

        /// <summary>Get a translation equivalent to "{{name}} (needs {{count}})".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_RecipesForIngredient_Entry(object name, object count)
        {
            return I18n.GetByKey("item.recipes-for-ingredient.entry", new { name, count });
        }

        /// <summary>Get a translation equivalent to "{{name}} x{{count}}".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_RecipesForMachine_MultipleItems(object name, object count)
        {
            return I18n.GetByKey("item.recipes-for-machine.multiple-items", new { name, count });
        }

        /// <summary>Get a translation equivalent to "you own {{count}} of these".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_NumberOwned_Summary(object count)
        {
            return I18n.GetByKey("item.number-owned.summary", new { count });
        }

        /// <summary>Get a translation equivalent to "you made {{count}} of these".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_NumberCrafted_Summary(object count)
        {
            return I18n.GetByKey("item.number-crafted.summary", new { count });
        }

        /// <summary>Get a translation equivalent to "Contents".</summary>
        public static string Item_CaskContents()
        {
            return I18n.GetByKey("item.cask-contents");
        }

        /// <summary>Get a translation equivalent to "Aging".</summary>
        public static string Item_CaskSchedule()
        {
            return I18n.GetByKey("item.cask-schedule");
        }

        /// <summary>Get a translation equivalent to "{{quality}} ready now".</summary>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        public static string Item_CaskSchedule_Now(object quality)
        {
            return I18n.GetByKey("item.cask-schedule.now", new { quality });
        }

        /// <summary>Get a translation equivalent to "{{quality}} now (use pickaxe to stop aging)".</summary>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        public static string Item_CaskSchedule_NowPartial(object quality)
        {
            return I18n.GetByKey("item.cask-schedule.now-partial", new { quality });
        }

        /// <summary>Get a translation equivalent to "{{quality}} tomorrow".</summary>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        public static string Item_CaskSchedule_Tomorrow(object quality)
        {
            return I18n.GetByKey("item.cask-schedule.tomorrow", new { quality });
        }

        /// <summary>Get a translation equivalent to "{{quality}} in {{count}} days ({{date}})".</summary>
        /// <param name="quality">The value to inject for the <c>{{quality}}</c> token.</param>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string Item_CaskSchedule_InXDays(object quality, object count, object date)
        {
            return I18n.GetByKey("item.cask-schedule.in-x-days", new { quality, count, date });
        }

        /// <summary>Get a translation equivalent to "Bait".</summary>
        public static string Item_CrabpotBait()
        {
            return I18n.GetByKey("item.crabpot-bait");
        }

        /// <summary>Get a translation equivalent to "Needs bait!".</summary>
        public static string Item_CrabpotBaitNeeded()
        {
            return I18n.GetByKey("item.crabpot-bait-needed");
        }

        /// <summary>Get a translation equivalent to "Not needed due to Luremaster profession.".</summary>
        public static string Item_CrabpotBaitNotNeeded()
        {
            return I18n.GetByKey("item.crabpot-bait-not-needed");
        }

        /// <summary>Get a translation equivalent to "Health".</summary>
        public static string Item_FenceHealth()
        {
            return I18n.GetByKey("item.fence-health");
        }

        /// <summary>Get a translation equivalent to "no decay with Gold Clock".</summary>
        public static string Item_FenceHealth_GoldClock()
        {
            return I18n.GetByKey("item.fence-health.gold-clock");
        }

        /// <summary>Get a translation equivalent to "{{percent}}% (roughly {{count}} days left)".</summary>
        /// <param name="percent">The value to inject for the <c>{{percent}}</c> token.</param>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Item_FenceHealth_Summary(object percent, object count)
        {
            return I18n.GetByKey("item.fence-health.summary", new { percent, count });
        }

        /// <summary>Get a translation equivalent to "Fish pond drops".</summary>
        public static string Item_FishPondDrops()
        {
            return I18n.GetByKey("item.fish-pond-drops");
        }

        /// <summary>Get a translation equivalent to "Spawn rules".</summary>
        public static string Item_FishSpawnRules()
        {
            return I18n.GetByKey("item.fish-spawn-rules");
        }

        /// <summary>Get a translation equivalent to "min fishing level: {{level}}".</summary>
        /// <param name="level">The value to inject for the <c>{{level}}</c> token.</param>
        public static string Item_FishSpawnRules_MinFishingLevel(object level)
        {
            return I18n.GetByKey("item.fish-spawn-rules.min-fishing-level", new { level });
        }

        /// <summary>Get a translation equivalent to "not caught yet (can only be caught once)".</summary>
        public static string Item_FishSpawnRules_NotCaughtYet()
        {
            return I18n.GetByKey("item.fish-spawn-rules.not-caught-yet");
        }

        /// <summary>Get a translation equivalent to "locations: {{locations}}".</summary>
        /// <param name="locations">The value to inject for the <c>{{locations}}</c> token.</param>
        public static string Item_FishSpawnRules_Locations(object locations)
        {
            return I18n.GetByKey("item.fish-spawn-rules.locations", new { locations });
        }

        /// <summary>Get a translation equivalent to "locations:".</summary>
        public static string Item_FishSpawnRules_LocationsBySeason_Label()
        {
            return I18n.GetByKey("item.fish-spawn-rules.locations-by-season.label");
        }

        /// <summary>Get a translation equivalent to "{{season}}: {{locations}}".</summary>
        /// <param name="season">The value to inject for the <c>{{season}}</c> token.</param>
        /// <param name="locations">The value to inject for the <c>{{locations}}</c> token.</param>
        public static string Item_FishSpawnRules_LocationsBySeason_SeasonLocations(object season, object locations)
        {
            return I18n.GetByKey("item.fish-spawn-rules.locations-by-season.season-locations", new { season, locations });
        }

        /// <summary>Get a translation equivalent to "any season".</summary>
        public static string Item_FishSpawnRules_SeasonAny()
        {
            return I18n.GetByKey("item.fish-spawn-rules.season-any");
        }

        /// <summary>Get a translation equivalent to "seasons: {{seasons}}".</summary>
        /// <param name="seasons">The value to inject for the <c>{{seasons}}</c> token.</param>
        public static string Item_FishSpawnRules_SeasonList(object seasons)
        {
            return I18n.GetByKey("item.fish-spawn-rules.season-list", new { seasons });
        }

        /// <summary>Get a translation equivalent to "time of day: {{times}}".</summary>
        /// <param name="times">The value to inject for the <c>{{times}}</c> token.</param>
        public static string Item_FishSpawnRules_Time(object times)
        {
            return I18n.GetByKey("item.fish-spawn-rules.time", new { times });
        }

        /// <summary>Get a translation equivalent to "weather: sunny".</summary>
        public static string Item_FishSpawnRules_WeatherSunny()
        {
            return I18n.GetByKey("item.fish-spawn-rules.weather-sunny");
        }

        /// <summary>Get a translation equivalent to "weather: raining".</summary>
        public static string Item_FishSpawnRules_WeatherRainy()
        {
            return I18n.GetByKey("item.fish-spawn-rules.weather-rainy");
        }

        /// <summary>Get a translation equivalent to "Preference".</summary>
        public static string Item_MovieSnackPreference()
        {
            return I18n.GetByKey("item.movie-snack-preference");
        }

        /// <summary>Get a translation equivalent to "Movie this week".</summary>
        public static string Item_MovieTicket_MovieThisWeek()
        {
            return I18n.GetByKey("item.movie-ticket.movie-this-week");
        }

        /// <summary>Get a translation equivalent to "Loves movie".</summary>
        public static string Item_MovieTicket_LovesMovie()
        {
            return I18n.GetByKey("item.movie-ticket.loves-movie");
        }

        /// <summary>Get a translation equivalent to "Likes movie".</summary>
        public static string Item_MovieTicket_LikesMovie()
        {
            return I18n.GetByKey("item.movie-ticket.likes-movie");
        }

        /// <summary>Get a translation equivalent to "Dislikes movie".</summary>
        public static string Item_MovieTicket_DislikesMovie()
        {
            return I18n.GetByKey("item.movie-ticket.dislikes-movie");
        }

        /// <summary>Get a translation equivalent to "{{name}} loves this".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        public static string Item_MovieSnackPreference_Love(object name)
        {
            return I18n.GetByKey("item.movie-snack-preference.love", new { name });
        }

        /// <summary>Get a translation equivalent to "{{name}} likes this".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        public static string Item_MovieSnackPreference_Like(object name)
        {
            return I18n.GetByKey("item.movie-snack-preference.like", new { name });
        }

        /// <summary>Get a translation equivalent to "{{name}} dislikes this".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        public static string Item_MovieSnackPreference_Dislike(object name)
        {
            return I18n.GetByKey("item.movie-snack-preference.dislike", new { name });
        }

        /// <summary>Get a translation equivalent to "No movie this week".</summary>
        public static string Item_MovieTicket_MovieThisWeek_None()
        {
            return I18n.GetByKey("item.movie-ticket.movie-this-week.none");
        }

        /// <summary>Get a translation equivalent to "Pitch".</summary>
        public static string Item_MusicBlock_Pitch()
        {
            return I18n.GetByKey("item.music-block.pitch");
        }

        /// <summary>Get a translation equivalent to "Drum type".</summary>
        public static string Item_MusicBlock_DrumType()
        {
            return I18n.GetByKey("item.music-block.drum-type");
        }

        /// <summary>Get a translation equivalent to "Invincible".</summary>
        public static string Monster_Invincible()
        {
            return I18n.GetByKey("monster.invincible");
        }

        /// <summary>Get a translation equivalent to "Health".</summary>
        public static string Monster_Health()
        {
            return I18n.GetByKey("monster.health");
        }

        /// <summary>Get a translation equivalent to "Drops".</summary>
        public static string Monster_Drops()
        {
            return I18n.GetByKey("monster.drops");
        }

        /// <summary>Get a translation equivalent to "XP".</summary>
        public static string Monster_Experience()
        {
            return I18n.GetByKey("monster.experience");
        }

        /// <summary>Get a translation equivalent to "Defense".</summary>
        public static string Monster_Defense()
        {
            return I18n.GetByKey("monster.defense");
        }

        /// <summary>Get a translation equivalent to "Attack".</summary>
        public static string Monster_Attack()
        {
            return I18n.GetByKey("monster.attack");
        }

        /// <summary>Get a translation equivalent to "Adventure Guild".</summary>
        public static string Monster_AdventureGuild()
        {
            return I18n.GetByKey("monster.adventure-guild");
        }

        /// <summary>Get a translation equivalent to "nothing".</summary>
        public static string Monster_Drops_Nothing()
        {
            return I18n.GetByKey("monster.drops.nothing");
        }

        /// <summary>Get a translation equivalent to "eradication goal: {{name}} (killed {{count}} of {{requiredCount}})".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        /// <param name="requiredCount">The value to inject for the <c>{{requiredCount}}</c> token.</param>
        public static string Monster_AdventureGuild_EradicationGoal(object name, object count, object requiredCount)
        {
            return I18n.GetByKey("monster.adventure-guild.eradication-goal", new { name, count, requiredCount });
        }

        /// <summary>Get a translation equivalent to "Birthday".</summary>
        public static string Npc_Birthday()
        {
            return I18n.GetByKey("npc.birthday");
        }

        /// <summary>Get a translation equivalent to "Can romance".</summary>
        public static string Npc_CanRomance()
        {
            return I18n.GetByKey("npc.can-romance");
        }

        /// <summary>Get a translation equivalent to "Friendship".</summary>
        public static string Npc_Friendship()
        {
            return I18n.GetByKey("npc.friendship");
        }

        /// <summary>Get a translation equivalent to "Talked today".</summary>
        public static string Npc_TalkedToday()
        {
            return I18n.GetByKey("npc.talked-today");
        }

        /// <summary>Get a translation equivalent to "Gifted today".</summary>
        public static string Npc_GiftedToday()
        {
            return I18n.GetByKey("npc.gifted-today");
        }

        /// <summary>Get a translation equivalent to "Gifted this week".</summary>
        public static string Npc_GiftedThisWeek()
        {
            return I18n.GetByKey("npc.gifted-this-week");
        }

        /// <summary>Get a translation equivalent to "Kissed today".</summary>
        public static string Npc_KissedToday()
        {
            return I18n.GetByKey("npc.kissed-today");
        }

        /// <summary>Get a translation equivalent to "Hugged today".</summary>
        public static string Npc_HuggedToday()
        {
            return I18n.GetByKey("npc.hugged-today");
        }

        /// <summary>Get a translation equivalent to "Loves gifts".</summary>
        public static string Npc_LovesGifts()
        {
            return I18n.GetByKey("npc.loves-gifts");
        }

        /// <summary>Get a translation equivalent to "Likes gifts".</summary>
        public static string Npc_LikesGifts()
        {
            return I18n.GetByKey("npc.likes-gifts");
        }

        /// <summary>Get a translation equivalent to "Neutral gifts".</summary>
        public static string Npc_NeutralGifts()
        {
            return I18n.GetByKey("npc.neutral-gifts");
        }

        /// <summary>Get a translation equivalent to "Dislikes gifts".</summary>
        public static string Npc_DislikesGifts()
        {
            return I18n.GetByKey("npc.dislikes-gifts");
        }

        /// <summary>Get a translation equivalent to "Hates gifts".</summary>
        public static string Npc_HatesGifts()
        {
            return I18n.GetByKey("npc.hates-gifts");
        }

        /// <summary>Get a translation equivalent to "You're married! &lt;".</summary>
        public static string Npc_CanRomance_Married()
        {
            return I18n.GetByKey("npc.can-romance.married");
        }

        /// <summary>Get a translation equivalent to "You're housemates!".</summary>
        public static string Npc_CanRomance_Housemate()
        {
            return I18n.GetByKey("npc.can-romance.housemate");
        }

        /// <summary>Get a translation equivalent to "You haven't met them yet.".</summary>
        public static string Npc_Friendship_NotMet()
        {
            return I18n.GetByKey("npc.friendship.not-met");
        }

        /// <summary>Get a translation equivalent to "need bouquet for next".</summary>
        public static string Npc_Friendship_NeedBouquet()
        {
            return I18n.GetByKey("npc.friendship.need-bouquet");
        }

        /// <summary>Get a translation equivalent to "next in {{count}} pts".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Npc_Friendship_NeedPoints(object count)
        {
            return I18n.GetByKey("npc.friendship.need-points", new { count });
        }

        /// <summary>Get a translation equivalent to "{{count}} unrevealed items".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Npc_UndiscoveredGiftTaste(object count)
        {
            return I18n.GetByKey("npc.undiscovered-gift-taste", new { count });
        }

        /// <summary>Get a translation equivalent to ", and {{count}} unrevealed items".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Npc_UndiscoveredGiftTasteAppended(object count)
        {
            return I18n.GetByKey("npc.undiscovered-gift-taste-appended", new { count });
        }

        /// <summary>Get a translation equivalent to "Age".</summary>
        public static string Npc_Child_Age()
        {
            return I18n.GetByKey("npc.child.age");
        }

        /// <summary>Get a translation equivalent to "{{label}} ({{count}} days to {{nextLabel}})".</summary>
        /// <param name="label">The value to inject for the <c>{{label}}</c> token.</param>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        /// <param name="nextLabel">The value to inject for the <c>{{nextLabel}}</c> token.</param>
        public static string Npc_Child_Age_DescriptionPartial(object label, object count, object nextLabel)
        {
            return I18n.GetByKey("npc.child.age.description-partial", new { label, count, nextLabel });
        }

        /// <summary>Get a translation equivalent to "{{label}}".</summary>
        /// <param name="label">The value to inject for the <c>{{label}}</c> token.</param>
        public static string Npc_Child_Age_DescriptionGrown(object label)
        {
            return I18n.GetByKey("npc.child.age.description-grown", new { label });
        }

        /// <summary>Get a translation equivalent to "newborn".</summary>
        public static string Npc_Child_Age_Newborn()
        {
            return I18n.GetByKey("npc.child.age.newborn");
        }

        /// <summary>Get a translation equivalent to "baby".</summary>
        public static string Npc_Child_Age_Baby()
        {
            return I18n.GetByKey("npc.child.age.baby");
        }

        /// <summary>Get a translation equivalent to "crawler".</summary>
        public static string Npc_Child_Age_Crawler()
        {
            return I18n.GetByKey("npc.child.age.crawler");
        }

        /// <summary>Get a translation equivalent to "toddler".</summary>
        public static string Npc_Child_Age_Toddler()
        {
            return I18n.GetByKey("npc.child.age.toddler");
        }

        /// <summary>Get a translation equivalent to "Love".</summary>
        public static string Pet_Love()
        {
            return I18n.GetByKey("pet.love");
        }

        /// <summary>Get a translation equivalent to "Petted today".</summary>
        public static string Pet_PettedToday()
        {
            return I18n.GetByKey("pet.petted-today");
        }

        /// <summary>Get a translation equivalent to "Last petted".</summary>
        public static string Pet_LastPetted()
        {
            return I18n.GetByKey("pet.last-petted");
        }

        /// <summary>Get a translation equivalent to "Water bowl".</summary>
        public static string Pet_WaterBowl()
        {
            return I18n.GetByKey("pet.water-bowl");
        }

        /// <summary>Get a translation equivalent to "yes (+12 love)".</summary>
        public static string Pet_LastPetted_Yes()
        {
            return I18n.GetByKey("pet.last-petted.yes");
        }

        /// <summary>Get a translation equivalent to "{{days}} days ago".</summary>
        /// <param name="days">The value to inject for the <c>{{days}}</c> token.</param>
        public static string Pet_LastPetted_DaysAgo(object days)
        {
            return I18n.GetByKey("pet.last-petted.days-ago", new { days });
        }

        /// <summary>Get a translation equivalent to "never".</summary>
        public static string Pet_LastPetted_Never()
        {
            return I18n.GetByKey("pet.last-petted.never");
        }

        /// <summary>Get a translation equivalent to "empty".</summary>
        public static string Pet_WaterBowl_Empty()
        {
            return I18n.GetByKey("pet.water-bowl.empty");
        }

        /// <summary>Get a translation equivalent to "filled (+6 love)".</summary>
        public static string Pet_WaterBowl_Filled()
        {
            return I18n.GetByKey("pet.water-bowl.filled");
        }

        /// <summary>Get a translation equivalent to "Farm name".</summary>
        public static string Player_FarmName()
        {
            return I18n.GetByKey("player.farm-name");
        }

        /// <summary>Get a translation equivalent to "Farm map".</summary>
        public static string Player_FarmMap()
        {
            return I18n.GetByKey("player.farm-map");
        }

        /// <summary>Get a translation equivalent to "Favorite thing".</summary>
        public static string Player_FavoriteThing()
        {
            return I18n.GetByKey("player.favorite-thing");
        }

        /// <summary>Get a translation equivalent to "Gender".</summary>
        public static string Player_Gender()
        {
            return I18n.GetByKey("player.gender");
        }

        /// <summary>Get a translation equivalent to "Housemate".</summary>
        public static string Player_Housemate()
        {
            return I18n.GetByKey("player.housemate");
        }

        /// <summary>Get a translation equivalent to "Spouse".</summary>
        public static string Player_Spouse()
        {
            return I18n.GetByKey("player.spouse");
        }

        /// <summary>Get a translation equivalent to "Watched movie this week".</summary>
        public static string Player_WatchedMovieThisWeek()
        {
            return I18n.GetByKey("player.watched-movie-this-week");
        }

        /// <summary>Get a translation equivalent to "Combat".</summary>
        public static string Player_CombatSkill()
        {
            return I18n.GetByKey("player.combat-skill");
        }

        /// <summary>Get a translation equivalent to "Farming".</summary>
        public static string Player_FarmingSkill()
        {
            return I18n.GetByKey("player.farming-skill");
        }

        /// <summary>Get a translation equivalent to "Fishing".</summary>
        public static string Player_FishingSkill()
        {
            return I18n.GetByKey("player.fishing-skill");
        }

        /// <summary>Get a translation equivalent to "Foraging".</summary>
        public static string Player_ForagingSkill()
        {
            return I18n.GetByKey("player.foraging-skill");
        }

        /// <summary>Get a translation equivalent to "Mining".</summary>
        public static string Player_MiningSkill()
        {
            return I18n.GetByKey("player.mining-skill");
        }

        /// <summary>Get a translation equivalent to "Luck".</summary>
        public static string Player_Luck()
        {
            return I18n.GetByKey("player.luck");
        }

        /// <summary>Get a translation equivalent to "Save format".</summary>
        public static string Player_SaveFormat()
        {
            return I18n.GetByKey("player.save-format");
        }

        /// <summary>Get a translation equivalent to "Custom".</summary>
        public static string Player_FarmMap_Custom()
        {
            return I18n.GetByKey("player.farm-map.custom");
        }

        /// <summary>Get a translation equivalent to "male".</summary>
        public static string Player_Gender_Male()
        {
            return I18n.GetByKey("player.gender.male");
        }

        /// <summary>Get a translation equivalent to "female".</summary>
        public static string Player_Gender_Female()
        {
            return I18n.GetByKey("player.gender.female");
        }

        /// <summary>Get a translation equivalent to "{{percent}}% to many random checks".</summary>
        /// <param name="percent">The value to inject for the <c>{{percent}}</c> token.</param>
        public static string Player_Luck_Summary(object percent)
        {
            return I18n.GetByKey("player.luck.summary", new { percent });
        }

        /// <summary>Get a translation equivalent to "level {{level}} ({{expNeeded}} XP to next)".</summary>
        /// <param name="level">The value to inject for the <c>{{level}}</c> token.</param>
        /// <param name="expNeeded">The value to inject for the <c>{{expNeeded}}</c> token.</param>
        public static string Player_Skill_Progress(object level, object expNeeded)
        {
            return I18n.GetByKey("player.skill.progress", new { level, expNeeded });
        }

        /// <summary>Get a translation equivalent to "level {{level}}".</summary>
        /// <param name="level">The value to inject for the <c>{{level}}</c> token.</param>
        public static string Player_Skill_ProgressLast(object level)
        {
            return I18n.GetByKey("player.skill.progress-last", new { level });
        }

        /// <summary>Get a translation equivalent to "Solution".</summary>
        public static string Puzzle_Solution()
        {
            return I18n.GetByKey("puzzle.solution");
        }

        /// <summary>Get a translation equivalent to "Puzzle solved!".</summary>
        public static string Puzzle_Solution_Solved()
        {
            return I18n.GetByKey("puzzle.solution.solved");
        }

        /// <summary>Get a translation equivalent to "Puzzle solutions are hidden in progression mode.".</summary>
        public static string Puzzle_Solution_Hidden()
        {
            return I18n.GetByKey("puzzle.solution.hidden");
        }

        /// <summary>Get a translation equivalent to "Crystal Cave Puzzle".</summary>
        public static string Puzzle_IslandCrystalCave_Title()
        {
            return I18n.GetByKey("puzzle.island-crystal-cave.title");
        }

        /// <summary>Get a translation equivalent to "Crystal ID".</summary>
        public static string Puzzle_IslandCrystalCave_CrystalId()
        {
            return I18n.GetByKey("puzzle.island-crystal-cave.crystal-id");
        }

        /// <summary>Get a translation equivalent to "Activate the statue to start the puzzle.".</summary>
        public static string Puzzle_IslandCrystalCave_Solution_NotActivated()
        {
            return I18n.GetByKey("puzzle.island-crystal-cave.solution.not-activated");
        }

        /// <summary>Get a translation equivalent to "Waiting for game to generate sequence...".</summary>
        public static string Puzzle_IslandCrystalCave_Solution_Waiting()
        {
            return I18n.GetByKey("puzzle.island-crystal-cave.solution.waiting");
        }

        /// <summary>Get a translation equivalent to "Repeat the pattern of activated crystals:".</summary>
        public static string Puzzle_IslandCrystalCave_Solution_Activated()
        {
            return I18n.GetByKey("puzzle.island-crystal-cave.solution.activated");
        }

        /// <summary>Get a translation equivalent to "Island Mermaid Puzzle".</summary>
        public static string Puzzle_IslandMermaid_Title()
        {
            return I18n.GetByKey("puzzle.island-mermaid.title");
        }

        /// <summary>Get a translation equivalent to "Use flute blocks to play the Night Market mermaid song. Pitch values (you can lookup a flute block to see its pitch):".</summary>
        public static string Puzzle_IslandMermaid_Solution_Intro()
        {
            return I18n.GetByKey("puzzle.island-mermaid.solution.intro");
        }

        /// <summary>Get a translation equivalent to "Island Shrine Puzzle".</summary>
        public static string Puzzle_IslandShrine_Title()
        {
            return I18n.GetByKey("puzzle.island-shrine.title");
        }

        /// <summary>Get a translation equivalent to "For each pedestal, place the item dropped by gem birds in the corresponding island location when it rains. The items are randomized for each save.".</summary>
        public static string Puzzle_IslandShrine_Solution()
        {
            return I18n.GetByKey("puzzle.island-shrine.solution");
        }

        /// <summary>Get a translation equivalent to "East: {{item}}".</summary>
        /// <param name="item">The value to inject for the <c>{{item}}</c> token.</param>
        public static string Puzzle_IslandShrine_Solution_East(object item)
        {
            return I18n.GetByKey("puzzle.island-shrine.solution.east", new { item });
        }

        /// <summary>Get a translation equivalent to "North: {{item}}".</summary>
        /// <param name="item">The value to inject for the <c>{{item}}</c> token.</param>
        public static string Puzzle_IslandShrine_Solution_North(object item)
        {
            return I18n.GetByKey("puzzle.island-shrine.solution.north", new { item });
        }

        /// <summary>Get a translation equivalent to "South: {{item}}".</summary>
        /// <param name="item">The value to inject for the <c>{{item}}</c> token.</param>
        public static string Puzzle_IslandShrine_Solution_South(object item)
        {
            return I18n.GetByKey("puzzle.island-shrine.solution.south", new { item });
        }

        /// <summary>Get a translation equivalent to "West: {{item}}".</summary>
        /// <param name="item">The value to inject for the <c>{{item}}</c> token.</param>
        public static string Puzzle_IslandShrine_Solution_West(object item)
        {
            return I18n.GetByKey("puzzle.island-shrine.solution.west", new { item });
        }

        /// <summary>Get a translation equivalent to "A tile position on the map. This is displayed because you enabled tile lookups in the configuration.".</summary>
        public static string Tile_Description()
        {
            return I18n.GetByKey("tile.description");
        }

        /// <summary>Get a translation equivalent to "Map name".</summary>
        public static string Tile_MapName()
        {
            return I18n.GetByKey("tile.map-name");
        }

        /// <summary>Get a translation equivalent to "Tile".</summary>
        public static string Tile_Tile()
        {
            return I18n.GetByKey("tile.tile");
        }

        /// <summary>Get a translation equivalent to "{{layerName}}: tile index".</summary>
        /// <param name="layerName">The value to inject for the <c>{{layerName}}</c> token.</param>
        public static string Tile_TileIndex(object layerName)
        {
            return I18n.GetByKey("tile.tile-index", new { layerName });
        }

        /// <summary>Get a translation equivalent to "{{layerName}}: tilesheet".</summary>
        /// <param name="layerName">The value to inject for the <c>{{layerName}}</c> token.</param>
        public static string Tile_Tilesheet(object layerName)
        {
            return I18n.GetByKey("tile.tilesheet", new { layerName });
        }

        /// <summary>Get a translation equivalent to "{{layerName}}: blend mode".</summary>
        /// <param name="layerName">The value to inject for the <c>{{layerName}}</c> token.</param>
        public static string Tile_BlendMode(object layerName)
        {
            return I18n.GetByKey("tile.blend-mode", new { layerName });
        }

        /// <summary>Get a translation equivalent to "{{layerName}}: ix props: {{propertyName}}".</summary>
        /// <param name="layerName">The value to inject for the <c>{{layerName}}</c> token.</param>
        /// <param name="propertyName">The value to inject for the <c>{{propertyName}}</c> token.</param>
        public static string Tile_IndexProperty(object layerName, object propertyName)
        {
            return I18n.GetByKey("tile.index-property", new { layerName, propertyName });
        }

        /// <summary>Get a translation equivalent to "{{layerName}}: props: {{propertyName}}".</summary>
        /// <param name="layerName">The value to inject for the <c>{{layerName}}</c> token.</param>
        /// <param name="propertyName">The value to inject for the <c>{{propertyName}}</c> token.</param>
        public static string Tile_TileProperty(object layerName, object propertyName)
        {
            return I18n.GetByKey("tile.tile-property", new { layerName, propertyName });
        }

        /// <summary>Get a translation equivalent to "no tile here".</summary>
        public static string Tile_Tile_NoneHere()
        {
            return I18n.GetByKey("tile.tile.none-here");
        }

        /// <summary>Get a translation equivalent to "Item wanted".</summary>
        public static string TrashBearOrGourmand_ItemWanted()
        {
            return I18n.GetByKey("trash-bear-or-gourmand.item-wanted");
        }

        /// <summary>Get a translation equivalent to "Quest progress".</summary>
        public static string TrashBearOrGourmand_QuestProgress()
        {
            return I18n.GetByKey("trash-bear-or-gourmand.quest-progress");
        }

        /// <summary>Get a translation equivalent to "Growth stage".</summary>
        public static string Tree_Stage()
        {
            return I18n.GetByKey("tree.stage");
        }

        /// <summary>Get a translation equivalent to "Next growth".</summary>
        public static string Tree_NextGrowth()
        {
            return I18n.GetByKey("tree.next-growth");
        }

        /// <summary>Get a translation equivalent to "Has seed".</summary>
        public static string Tree_HasSeed()
        {
            return I18n.GetByKey("tree.has-seed");
        }

        /// <summary>Get a translation equivalent to "Is fertilized".</summary>
        public static string Tree_IsFertilized()
        {
            return I18n.GetByKey("tree.is-fertilized");
        }

        /// <summary>Get a translation equivalent to "Big Mushroom".</summary>
        public static string Tree_Name_BigMushroom()
        {
            return I18n.GetByKey("tree.name.big-mushroom");
        }

        /// <summary>Get a translation equivalent to "Mahogany Tree".</summary>
        public static string Tree_Name_Mahogany()
        {
            return I18n.GetByKey("tree.name.mahogany");
        }

        /// <summary>Get a translation equivalent to "Maple Tree".</summary>
        public static string Tree_Name_Maple()
        {
            return I18n.GetByKey("tree.name.maple");
        }

        /// <summary>Get a translation equivalent to "Oak Tree".</summary>
        public static string Tree_Name_Oak()
        {
            return I18n.GetByKey("tree.name.oak");
        }

        /// <summary>Get a translation equivalent to "Palm Tree".</summary>
        public static string Tree_Name_Palm()
        {
            return I18n.GetByKey("tree.name.palm");
        }

        /// <summary>Get a translation equivalent to "Pine Tree".</summary>
        public static string Tree_Name_Pine()
        {
            return I18n.GetByKey("tree.name.pine");
        }

        /// <summary>Get a translation equivalent to "Unknown Tree".</summary>
        public static string Tree_Name_Unknown()
        {
            return I18n.GetByKey("tree.name.unknown");
        }

        /// <summary>Get a translation equivalent to "Fully grown".</summary>
        public static string Tree_Stage_Done()
        {
            return I18n.GetByKey("tree.stage.done");
        }

        /// <summary>Get a translation equivalent to "{{stageName}} ({{step}} of {{max}})".</summary>
        /// <param name="stageName">The value to inject for the <c>{{stageName}}</c> token.</param>
        /// <param name="step">The value to inject for the <c>{{step}}</c> token.</param>
        /// <param name="max">The value to inject for the <c>{{max}}</c> token.</param>
        public static string Tree_Stage_Partial(object stageName, object step, object max)
        {
            return I18n.GetByKey("tree.stage.partial", new { stageName, step, max });
        }

        /// <summary>Get a translation equivalent to "can't grow in winter outside greenhouse".</summary>
        public static string Tree_NextGrowth_Winter()
        {
            return I18n.GetByKey("tree.next-growth.winter");
        }

        /// <summary>Get a translation equivalent to "can't grow because other trees are too close".</summary>
        public static string Tree_NextGrowth_AdjacentTrees()
        {
            return I18n.GetByKey("tree.next-growth.adjacent-trees");
        }

        /// <summary>Get a translation equivalent to "{{chance}}% chance to grow into {{stage}} tomorrow".</summary>
        /// <param name="chance">The value to inject for the <c>{{chance}}</c> token.</param>
        /// <param name="stage">The value to inject for the <c>{{stage}}</c> token.</param>
        public static string Tree_NextGrowth_Chance(object chance, object stage)
        {
            return I18n.GetByKey("tree.next-growth.chance", new { chance, stage });
        }

        /// <summary>Get a translation equivalent to "guarantees daily growth, even in winter".</summary>
        public static string Tree_IsFertilized_Effects()
        {
            return I18n.GetByKey("tree.is-fertilized.effects");
        }

        /// <summary>Get a translation equivalent to "seed".</summary>
        public static string Tree_Stages_Seed()
        {
            return I18n.GetByKey("tree.stages.seed");
        }

        /// <summary>Get a translation equivalent to "sprout".</summary>
        public static string Tree_Stages_Sprout()
        {
            return I18n.GetByKey("tree.stages.sprout");
        }

        /// <summary>Get a translation equivalent to "sapling".</summary>
        public static string Tree_Stages_Sapling()
        {
            return I18n.GetByKey("tree.stages.sapling");
        }

        /// <summary>Get a translation equivalent to "bush".</summary>
        public static string Tree_Stages_Bush()
        {
            return I18n.GetByKey("tree.stages.bush");
        }

        /// <summary>Get a translation equivalent to "small tree".</summary>
        public static string Tree_Stages_SmallTree()
        {
            return I18n.GetByKey("tree.stages.smallTree");
        }

        /// <summary>Get a translation equivalent to "tree".</summary>
        public static string Tree_Stages_Tree()
        {
            return I18n.GetByKey("tree.stages.tree");
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

