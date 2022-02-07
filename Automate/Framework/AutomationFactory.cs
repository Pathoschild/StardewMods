using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework.Machines.Buildings;
using Pathoschild.Stardew.Automate.Framework.Machines.Objects;
using Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures;
using Pathoschild.Stardew.Automate.Framework.Machines.Tiles;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Automate.Framework.Storage;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Constructs machines, containers, or connectors which can be added to a machine group.</summary>
    internal class AutomationFactory : IAutomationFactory
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private readonly Func<ModConfig> Config;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Whether the Better Junimos mod is installed.</summary>
        private readonly bool IsBetterJunimosLoaded;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="isBetterJunimosLoaded">Whether the Better Junimos mod is installed.</param>
        public AutomationFactory(Func<ModConfig> config, IMonitor monitor, IReflectionHelper reflection, bool isBetterJunimosLoaded)
        {
            this.Config = config;
            this.Monitor = monitor;
            this.Reflection = reflection;
            this.IsBetterJunimosLoaded = isBetterJunimosLoaded;
        }

        /// <summary>Get a machine, container, or connector instance for a given object.</summary>
        /// <param name="obj">The in-game object.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        public IAutomatable? GetFor(SObject obj, GameLocation location, in Vector2 tile)
        {
            // chest
            if (obj is Chest chest && chest.playerChest.Value)
            {
                switch (chest.SpecialChestType)
                {
                    case Chest.SpecialChestTypes.None:
                    case Chest.SpecialChestTypes.AutoLoader when !chest.modData.ContainsKey("spacechase0.SuperHopper"): // super hopper is used to transfer items between two chests without connecting them to the same group
                    case Chest.SpecialChestTypes.JunimoChest:
                        return new ChestContainer(chest, location, tile);

                    case Chest.SpecialChestTypes.MiniShippingBin:
                        return new MiniShippingBinMachine(chest, location);
                }
            }

            // indoor pot
            if (obj is IndoorPot indoorPot && BushMachine.CanAutomate(indoorPot.bush.Value))
                return new BushMachine(indoorPot, tile, location, this.Reflection);

            // tapper
            if (obj.IsTapper())
            {
                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainFeature) && terrainFeature is Tree tree)
                    return new TapperMachine(obj, location, tile, tree);
            }

            // machine by type
            switch (obj)
            {
                case Cask cask:
                    return new CaskMachine(cask, location, tile);

                case CrabPot crabPot:
                    return new CrabPotMachine(crabPot, location, tile, this.Monitor, this.Reflection);

                case WoodChipper woodChipper:
                    return new WoodChipperMachine(woodChipper, location, tile);
            }

            // machine by item ID
            switch (obj.QualifiedItemId)
            {
                case "(BC)165":
                    return new AutoGrabberMachine(obj, location, tile);

                case "(BC)10":
                    return new BeeHouseMachine(obj, location, tile);

                case "(BC)90":
                    return new BoneMillMachine(obj, location, tile);

                case "(BC)114":
                    return new CharcoalKilnMachine(obj, location, tile);

                case "(BC)16":
                    return new CheesePressMachine(obj, location, tile);

                case "(BC)246":
                    return new CoffeeMakerMachine(obj, location, tile);

                case "(BC)21":
                    return new CrystalariumMachine(obj, location, tile, this.Reflection);

                case "(BC)265":
                    return new DeconstructorMachine(obj, location, tile);

                case "(BC)99":
                    return new FeedHopperMachine(location, tile);

                case "(BC)13":
                    return new FurnaceMachine(obj, location, tile);

                case "(BC)182":
                    return new GeodeCrusherMachine(obj, location, tile);

                case "(BC)101":
                    return new CoopIncubatorMachine(obj, location, tile);

                case "(BC)12":
                    return new KegMachine(obj, location, tile);

                case "(BC)9":
                    return new LightningRodMachine(obj, location, tile);

                case "(BC)17":
                    return new LoomMachine(obj, location, tile);

                case "(BC)24":
                    return new MayonnaiseMachine(obj, location, tile);

                case "(BC)128":
                    return new MushroomBoxMachine(obj, location, tile);

                case "(BC)19":
                    return new OilMakerMachine(obj, location, tile);

                case "(BC)254":
                    return new OstrichIncubatorMachine(obj, location, tile);

                case "(BC)15":
                    return new PreservesJarMachine(obj, location, tile);

                case "(BC)20":
                    return new RecyclingMachine(obj, location, tile);

                case "(BC)25":
                    return new SeedMakerMachine(obj, location, tile);

                case "(BC)158":
                    return new SlimeEggPressMachine(obj, location, tile);

                case "(BC)156":
                    return new SlimeIncubatorMachine(obj, location, tile);

                case "(BC)117":
                    return new SodaMachine(obj, location, tile);

                case "(BC)231":
                    return new SolarPanelMachine(obj, location, tile);

                case "(BC)127":
                    return new StatueOfEndlessFortuneMachine(obj, location, tile);

                case "(BC)160":
                    return new StatueOfPerfectionMachine(obj, location, tile);

                case "(BC)280":
                    return new StatueOfTruePerfectionMachine(obj, location, tile);

                case "(BC)154":
                    return new WormBinMachine(obj, location, tile);
            }

            // connector
            if (this.IsConnector(obj))
                return new Connector(location, tile);

            return null;
        }

        /// <summary>Get a machine, container, or connector instance for a given terrain feature.</summary>
        /// <param name="feature">The terrain feature.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        public IAutomatable? GetFor(TerrainFeature feature, GameLocation location, in Vector2 tile)
        {
            // machine
            switch (feature)
            {
                case Bush bush when BushMachine.CanAutomate(bush):
                    return new BushMachine(bush, location);

                case FruitTree fruitTree:
                    return new FruitTreeMachine(fruitTree, location, tile);

                case Tree tree when TreeMachine.CanAutomate(tree) && tree.growthStage.Value >= Tree.treeStage: // avoid accidental machine links due to seeds spreading automatically
                    return new TreeMachine(tree, location, tile);
            }

            // connector
            if (this.IsConnector(feature))
                return new Connector(location, tile);

            return null;
        }

        /// <summary>Get a machine, container, or connector instance for a given building.</summary>
        /// <param name="building">The building.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        public IAutomatable? GetFor(Building building, BuildableGameLocation location, in Vector2 tile)
        {
            // building by type
            switch (building)
            {
                case FishPond pond:
                    return new FishPondMachine(pond, location);

                case JunimoHut hut:
                    {
                        ModConfig config = this.Config();

                        JunimoHutBehavior gemBehavior = config.JunimoHutBehaviorForGems;
                        if (gemBehavior is JunimoHutBehavior.AutoDetect)
                            gemBehavior = JunimoHutBehavior.Ignore;

                        JunimoHutBehavior fertilizerBehavior = config.JunimoHutBehaviorForFertilizer;
                        if (fertilizerBehavior is JunimoHutBehavior.AutoDetect)
                            fertilizerBehavior = this.IsBetterJunimosLoaded ? JunimoHutBehavior.Ignore : JunimoHutBehavior.MoveIntoChests;

                        JunimoHutBehavior seedBehavior = config.JunimoHutBehaviorForFertilizer;
                        if (seedBehavior is JunimoHutBehavior.AutoDetect)
                            seedBehavior = this.IsBetterJunimosLoaded ? JunimoHutBehavior.Ignore : JunimoHutBehavior.MoveIntoChests;

                        return new JunimoHutMachine(hut, location, gemBehavior, fertilizerBehavior, seedBehavior);
                    }

                case Mill mill:
                    return new MillMachine(mill, location);

                case ShippingBin bin:
                    return new ShippingBinMachine(bin, location);
            }

            // building by buildingType
            if (building.buildingType.Value == "Silo")
                return new FeedHopperMachine(building, location);

            return null;
        }

        /// <summary>Get a machine, container, or connector instance for a given tile position.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        /// <remarks>Shipping bin logic from <see cref="Farm.leftClick"/>, garbage can logic from <see cref="Town.checkAction"/>.</remarks>
        public IAutomatable? GetForTile(GameLocation location, in Vector2 tile)
        {
            // shipping bin on island farm
            if (location is IslandWest farm && (int)tile.X == farm.shippingBinPosition.X && (int)tile.Y == farm.shippingBinPosition.Y)
                return new ShippingBinMachine(Game1.getFarm(), new Rectangle(farm.shippingBinPosition.X, farm.shippingBinPosition.Y, 2, 1));

            // garbage can
            if (location is Town town)
            {
                string action = town.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Action", "Buildings");
                if (!string.IsNullOrWhiteSpace(action) && action.StartsWith("Garbage ") && int.TryParse(action.Split(' ')[1], out int trashCanIndex))
                    return new TrashCanMachine(town, tile, trashCanIndex, this.Reflection);
            }

            // fridge
            switch (location)
            {
                case FarmHouse house when (house.fridgePosition != Point.Zero && house.fridgePosition.X == (int)tile.X && house.fridgePosition.Y == (int)tile.Y):
                    return new ChestContainer(house.fridge.Value, location, tile, migrateLegacyOptions: false);

                case IslandFarmHouse house when (house.fridgePosition != Point.Zero && house.fridgePosition.X == (int)tile.X && house.fridgePosition.Y == (int)tile.Y):
                    return new ChestContainer(house.fridge.Value, location, tile, migrateLegacyOptions: false);
            }

            return null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a given in-game entity should be treated as a connector.</summary>
        /// <param name="entity">The in-game entity.</param>
        private bool IsConnector(object entity)
        {
            var config = this.Config();

            switch (entity)
            {
                case Item item:
                    return config.ConnectorNames.Contains(item.Name);

                case Flooring floor:
                    string? itemId = floor.GetData()?.ItemId;
                    string? itemName = ItemRegistry.GetData(itemId)?.InternalName;

                    return
                        !string.IsNullOrWhiteSpace(itemName)
                        && config.ConnectorNames.Contains(itemName);

                default:
                    return false;
            }
        }
    }
}
