using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework.Machines;
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

        /// <summary>Whether the Better Junimos mod is installed.</summary>
        private readonly bool IsBetterJunimosLoaded;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="isBetterJunimosLoaded">Whether the Better Junimos mod is installed.</param>
        public AutomationFactory(Func<ModConfig> config, IMonitor monitor, bool isBetterJunimosLoaded)
        {
            this.Config = config;
            this.Monitor = monitor;
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
                return new BushMachine(indoorPot, tile, location);

            // tapper
            if (obj.IsTapper())
            {
                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainFeature) && terrainFeature is Tree tree)
                    return new TapperMachine(obj, location, tile, tree);
            }

            // crab pot
            if (obj is CrabPot crabPot)
                return new CrabPotMachine(crabPot, location, tile, this.Monitor);

            // machine by item ID
            switch (obj.QualifiedItemId)
            {
                case "(BC)165":
                    return new AutoGrabberMachine(obj, location, tile);

                case "(BC)99":
                    return new FeedHopperMachine(location, tile);
            }

            // machine in Data/Machines
            if (obj.GetMachineData() != null)
                return new DataBasedMachine(obj, location, tile, () => this.Config().MinMinutesForFairyDust);

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

                case Tree tree when TreeMachine.CanAutomate(tree, location) && tree.growthStage.Value >= Tree.treeStage: // avoid accidental machine links due to seeds spreading automatically
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
        public IAutomatable? GetFor(Building building, GameLocation location, in Vector2 tile)
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

                case ShippingBin bin:
                    return new ShippingBinMachine(bin, location);

                default:
                    switch (building.buildingType.Value)
                    {
                        case "Mill":
                            return new MillMachine(building, location);
                    }
                    break;
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
                if (action != null)
                {
                    string[] fields = ArgUtility.SplitBySpace(action);
                    if (string.Equals(fields[0], "Garbage", StringComparison.OrdinalIgnoreCase) && ArgUtility.HasIndex(fields, 1))
                        return new TrashCanMachine(town, tile, fields[1]);
                }
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
