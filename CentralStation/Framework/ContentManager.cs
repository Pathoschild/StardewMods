using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.CentralStation.Framework.Constants;
using Pathoschild.Stardew.CentralStation.Framework.ContentModels;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TokenizableStrings;
using StardewValley.Util;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace Pathoschild.Stardew.CentralStation.Framework;

/// <summary>Manages the Central Station content provided by content packs.</summary>
internal class ContentManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The SMAPI API for loading and managing content assets.</summary>
    private readonly IGameContentHelper ContentHelper;

    /// <summary>The SMAPI API for fetching metadata about loaded mods.</summary>
    private readonly IModRegistry ModRegistry;

    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The messages shown when the player clicks a bookshelf.</summary>
    private readonly LiveMessageQueue BookshelfMessages;

    /// <summary>The dialogues shown when the player clicks a tourist, indexed by <c>{map id}#{tourist id}</c>.</summary>
    private readonly Dictionary<string, LiveMessageQueue> TouristDialogues = [];

    /// <summary>The 'strange occurrence' messages shown in rare cases.</summary>
    private readonly Dictionary<string, LiveMessageQueue> StrangeMessages = [];

    /// <summary>Whether the central station is showing the rare dark form (lighting dimmed, shops closed, etc.).</summary>
    private readonly PerScreen<bool> StationDark = new();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="contentHelper">The SMAPI API for loading and managing content assets.</param>
    /// <param name="modRegistry">The SMAPI API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public ContentManager(IGameContentHelper contentHelper, IModRegistry modRegistry, IMonitor monitor)
    {
        this.ContentHelper = contentHelper;
        this.ModRegistry = modRegistry;
        this.Monitor = monitor;

        this.BookshelfMessages = new LiveMessageQueue(loop: true, shuffle: true, this.GetBookshelfMessages);
    }

    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    [EventPriority(EventPriority.Low)] // let mods update tokens before we prepare the map
    public void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        // reset dialogue
        this.TouristDialogues.Clear();

        // reapply map edits (e.g. random tourists)
        this.StationDark.Value = false;
        this.ContentHelper.InvalidateCache($"Maps/{Constant.ModId}");

        // add ticket machine if player wakes up in a location
        this.AddTicketMachineForMapProperty(Game1.currentLocation);
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    public void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsDirectlyUnderPath("Maps"))
        {
            // edit vanilla locations
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/BusStop"))
                e.Edit(asset => this.EditBusStopMap(asset.AsMap()), AssetEditPriority.Late);
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/Railroad"))
                e.Edit(asset => this.EditRailroadMap(asset.AsMap()), AssetEditPriority.Late);

            // edit Central Station map
            else if (e.NameWithoutLocale.IsEquivalentTo($"Maps/{Constant.ModId}"))
                e.Edit(this.EditCentralStationMap, AssetEditPriority.Early);
        }
    }

    /// <inheritdoc cref="IContentEvents.AssetReady" />
    public void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        // re-edit current location's map if it's reloaded
        GameLocation location = Game1.currentLocation;
        if (location?.map != null && e.NameWithoutLocale.IsEquivalentTo(location.mapPath.Value))
        {
            this.ConvertPreviousTicketMachines(location);
            this.AddTicketMachineForMapProperty(location);
        }
    }

    /// <inheritdoc cref="IPlayerEvents.Warped" />
    public void OnWarped(object? sender, WarpedEventArgs e)
    {
        // apply ticket machines
        this.ConvertPreviousTicketMachines(e.NewLocation);
        this.AddTicketMachineForMapProperty(e.NewLocation);

        // entered central station
        if (e.NewLocation.NameOrUniqueName is Constant.CentralStationLocationId)
        {
            // increment stat
            Game1.stats.Increment(Constant.TimesVisitedStatKey);

            // rare dark station
            if (this.StationDark.Value)
            {
                this.StationDark.Value = false;
                this.ContentHelper.InvalidateCache($"Maps/{Constant.ModId}");
                e.NewLocation.resetForPlayerEntry();
            }
            else if (this.GetStationVisits() >= Constant.DarkStationMinVisits && Game1.timeOfDay >= Constant.DarkStationMinTime && Game1.random.NextBool(Constant.DarkStationChance))
            {
                this.StationDark.Value = true;
                Game1.stopMusicTrack(MusicContext.Default);
                this.ContentHelper.InvalidateCache($"Maps/{Constant.ModId}");
                e.NewLocation.resetForPlayerEntry();
            }
        }
    }

    /// <summary>Get the number of times the player has visited the Central Station.</summary>
    public uint GetStationVisits()
    {
        return Game1.stats.Get(Constant.TimesVisitedStatKey);
    }

    /// <summary>Get the stops which can be selected from the current location.</summary>
    /// <param name="shouldEnableStop">A filter which returns true for the stops to return.</param>
    public IEnumerable<Stop> GetStops(ShouldEnableStopDelegate shouldEnableStop)
    {
        foreach ((string id, StopModel? stop) in this.ContentHelper.Load<Dictionary<string, StopModel?>>(AssetNames.Stops))
        {
            if (stop is null)
                continue;

            // validate
            if (string.IsNullOrWhiteSpace(id))
            {
                this.Monitor.LogOnce($"Ignored {stop.Network} destination to {stop.ToLocation} with no ID field.", LogLevel.Warn);
                continue;
            }
            if (this.ModRegistry.GetFromNamespacedId(id, requirePrefix: true) is null)
            {
                this.Monitor.LogOnce($"Ignored {stop.Network} destination with ID '{id}': IDs must be prefixed with the exact unique mod ID, like `Example.ModId_StopId`.", LogLevel.Warn);
                continue;
            }
            if (string.IsNullOrWhiteSpace(stop.ToLocation))
            {
                this.Monitor.LogOnce($"Ignored {stop.Network} destination with ID '{id}' because it has no {nameof(stop.ToLocation)} field.", LogLevel.Warn);
                continue;
            }

            // match if applicable
            if (shouldEnableStop(id, stop.ToLocation, stop.Condition, stop.Network))
            {
                yield return new Stop(
                    Id: id,
                    DisplayName: () => stop.DisplayName ?? id,
                    DisplayNameInCombinedLists: stop.DisplayNameInCombinedLists != null
                        ? () => stop.DisplayNameInCombinedLists
                        : null,
                    ToLocation: stop.ToLocation,
                    ToTile: stop.ToTile,
                    ToFacingDirection: Utility.TryParseDirection(stop.ToFacingDirection, out int toFacingDirection)
                        ? toFacingDirection
                        : Game1.down,
                    Cost: stop.Cost,
                    Network: stop.Network,
                    Condition: stop.Condition
                );
            }
        }
    }

    /// <summary>Get a translation provided by the content pack.</summary>
    /// <param name="key">The translation key.</param>
    /// <param name="tokens">The tokens with which to format the text, if any.</param>
    public string GetTranslation(string key, params object[] tokens)
    {
        return Game1.content.LoadString($"Mods\\{Constant.ModId}\\InternalTranslations:{key}", tokens);
    }

    /// <summary>Get the formatted label to show for a stop in a destination menu.</summary>
    /// <param name="stop">The stop data.</param>
    /// <param name="networks">The stop networks in the destination list in which the label will be shown.</param>
    public string GetStopLabel(Stop stop, StopNetworks networks)
    {
        string rawDisplayName = networks.HasMultipleFlags()
            ? stop.DisplayNameInCombinedLists?.Invoke() ?? stop.DisplayName()
            : stop.DisplayName();

        if (string.IsNullOrWhiteSpace(rawDisplayName))
            rawDisplayName = stop.Id;

        string displayName = TokenParser.ParseText(rawDisplayName);

        return stop.Cost > 0
            ? Game1.content.LoadString("Strings\\Locations:MineCart_DestinationWithPrice", displayName, Utility.getNumberWithCommas(stop.Cost))
            : displayName;
    }

    /// <summary>Get a random bookshelf message.</summary>
    /// <param name="message">The next message to display.</param>
    /// <param name="hasMoreMessages">Whether there are more messages to show after this one (including repeats).</param>
    /// <returns>Returns whether a message was found.</returns>
    public bool TryGetBookshelfMessage([NotNullWhen(true)] out string? message, out bool hasMoreMessages)
    {
        return this.BookshelfMessages.TryGetNext(out message, out hasMoreMessages);
    }

    /// <summary>Get the next dialogue a tourist will speak, if they have any.</summary>
    /// <param name="mapId">The ID for the tourist map data which added the tourist.</param>
    /// <param name="touristId">The ID of the tourist within its tourist map data.</param>
    /// <param name="message">The next message to display.</param>
    /// <param name="hasMoreMessages">Whether there are more messages to show after this one (including repeats).</param>
    /// <returns>Returns whether a message was found.</returns>
    public bool TryGetTouristDialogue(string mapId, string touristId, [NotNullWhen(true)] out string? message, out bool hasMoreMessages)
    {
        // get message queue
        string key = $"{mapId}#{touristId}";
        if (!this.TouristDialogues.TryGetValue(key, out LiveMessageQueue? queue))
        {
            // get whether the dialogue should repeat
            // (If the tourist isn't valid, an error will be shown separately.)
            bool? dialogueRepeats =
                this.ContentHelper.Load<Dictionary<string, TouristMapModel?>>(AssetNames.Tourists)
                .GetValueOrDefault(mapId)
                ?.Tourists
                ?.FirstOrDefault(p => p.Key == touristId).Value
                ?.DialogueRepeats;

            this.TouristDialogues[key] = queue = new LiveMessageQueue(
                loop: dialogueRepeats ?? false,
                shuffle: false,
                () => this.GetTouristDialogues(mapId, touristId)
            );
        }

        // get next
        if (queue.TryGetNext(out message, out hasMoreMessages))
        {
            message = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, message);
            return true;
        }

        return false;
    }

    /// <summary>Get the next 'strange occurrence' message from a list of translations with keys in the form <c>{prefix}.{number suffix}</c>.</summary>
    /// <param name="prefix">The translation key prefix.</param>
    /// <param name="minSuffix">The min numeric key suffix.</param>
    /// <param name="maxSuffix">The max numeric key suffix.</param>
    /// <param name="shuffle">Whether to randomize the message order.</param>
    public string GetNextStrangeMessage(string prefix, int minSuffix, int maxSuffix, bool shuffle = true)
    {
        string cacheKey = $"{prefix}#{minSuffix}-{maxSuffix}#{shuffle}";
        if (!this.StrangeMessages.TryGetValue(cacheKey, out LiveMessageQueue? messages))
            this.StrangeMessages[cacheKey] = messages = new LiveMessageQueue(loop: true, shuffle: shuffle, fetchMessages: () => this.GetNumberedTranslations(prefix, minSuffix, maxSuffix));

        return messages.TryGetNext(out string? message, out _)
            ? message
            : string.Empty; // should never happen
    }

    /// <summary>Get the tile which contains an <c>Action</c> tile property which opens a given network's menu, if any.</summary>
    /// <param name="map">The map whose tiles to search.</param>
    /// <param name="network">The network to match.</param>
    /// <param name="tile">The tile position containing the property, if found.</param>
    /// <returns>Returns whether a tile was found.</returns>
    public bool TryGetActionTile(Map? map, StopNetworks network, out Point tile)
    {
        // scan layer
        Layer? buildingsLayer = map?.GetLayer("Buildings");
        if (buildingsLayer is not null)
        {
            int layerHeight = buildingsLayer.LayerHeight;
            int layerWidth = buildingsLayer.LayerWidth;

            for (int y = 0; y < layerHeight; y++)
            {
                for (int x = 0; x < layerWidth; x++)
                {
                    if (buildingsLayer.Tiles[x, y]?.Properties?.TryGetValue("Action", out string action) is true && action.StartsWithIgnoreCase(Constant.TicketsAction))
                    {
                        string foundRawNetwork = ArgUtility.SplitBySpaceAndGet(action, 1, StopNetworks.Train.ToString());
                        if (Utility.TryParseEnum(foundRawNetwork, out StopNetworks foundNetwork) && network.HasAnyFlag(foundNetwork))
                        {
                            tile = new Point(x, y);
                            return true;
                        }
                    }
                }
            }
        }

        // none found
        tile = Point.Zero;
        return false;
    }

    /// <summary>Get the tile which contains a given tile index, if any.</summary>
    /// <param name="map">The map whose tiles to search.</param>
    /// <param name="tileSheetId">The map tile sheet ID to match.</param>
    /// <param name="layerId">The map layer ID to match.</param>
    /// <param name="index">The tile index to match.</param>
    /// <param name="tile">The tile position containing the matched tile index, if found.</param>
    /// <returns>Returns whether a tile was found.</returns>
    public bool TryGetTileIndex(Map? map, string tileSheetId, string layerId, int index, out Point tile)
    {
        // scan layer
        Layer? layer = map?.GetLayer(layerId);
        if (layer is not null)
        {
            int layerHeight = layer.LayerHeight;
            int layerWidth = layer.LayerWidth;

            for (int y = 0; y < layerHeight; y++)
            {
                for (int x = 0; x < layerWidth; x++)
                {
                    var mapTile = layer.Tiles[x, y];
                    if (mapTile?.TileIndex == index && mapTile.TileSheet?.Id == tileSheetId)
                    {
                        tile = new Point(x, y);
                        return true;
                    }
                }
            }
        }

        // none found
        tile = Point.Zero;
        return false;
    }

    /// <summary>Try to parse a space-delimited list of networks from map or tile property arguments.</summary>
    /// <param name="args">The property arguments to read.</param>
    /// <param name="index">The index of the first argument to include within the <paramref name="args" />.</param>
    /// <param name="networks">The parsed networks value.</param>
    /// <param name="error">An error phrase indicating why getting the argument failed, if applicable.</param>
    /// <param name="defaultValue">The value to return if the index is out of bounds.</param>
    public bool TryParseOptionalSpaceDelimitedNetworks(string[] args, int index, out StopNetworks networks, [NotNullWhen(false)] out string? error, StopNetworks defaultValue)
    {
        // get default
        if (!ArgUtility.TryGetOptionalRemainder(args, index, out string? rawNetworks, delimiter: ',') || rawNetworks is null)
        {
            error = null;
            networks = defaultValue;
            return true;
        }

        // invalid
        if (!Utility.TryParseEnum(rawNetworks, out networks))
        {
            error = $"value '{rawNetworks.Replace(',', ' ')}' can't be parsed as a network type; should be '{string.Join("', '", Enum.GetNames(typeof(StopNetworks)))}', or a space-delimited list thereof";
            return false;
        }

        // else parsed
        error = null;
        return true;
    }

    /// <summary>Update the Central Station map when the rare wood is sold.</summary>
    /// <param name="map">The map to edit.</param>
    public void OnRareWoodSold(Map map)
    {
        IAssetDataForMap editor = this.ContentHelper.GetPatchHelper(map, map.assetPath).AsMap();

        editor.PatchMap(
            source: this.ContentHelper.Load<Map>($"Maps/{Constant.ModId}_EmptyWoodPedestal"),
            sourceArea: new Rectangle(0, 0, 2, 3),
            targetArea: new Rectangle(57, 25, 2, 3),
            PatchMapMode.ReplaceByLayer
        );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Add the Central Station action properties for vanilla or legacy ticket machines.</summary>
    /// <param name="location">The location whose map to change.</param>
    private void ConvertPreviousTicketMachines(GameLocation location)
    {
        // get map info
        Map map = location.Map;
        Layer? layer = map?.GetLayer("Buildings");
        if (map is null || layer is null)
            return;

        int layerHeight = layer.LayerHeight;
        int layerWidth = layer.LayerWidth;

        // edit tiles
        bool isBoatTunnel = location is BoatTunnel { Name: "BoatTunnel" };
        bool isBusStop = location is BusStop { Name: "BusStop" };
        for (int y = 0; y < layerHeight; y++)
        {
            for (int x = 0; x < layerWidth; x++)
            {
                // get tile
                Tile? tile = layer.Tiles[x, y];
                if (tile is null)
                    continue;

                // swap action properties
                if (tile.Properties.TryGetValue("Action", out string action))
                {
                    switch (action)
                    {
                        case "BoatTicket":
                            if (!isBoatTunnel || Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
                                tile.Properties["Action"] = "CentralStation Boat";
                            break;

                        case "TrainStation":
                            tile.Properties["Action"] = "CentralStation Train";
                            break;
                    }
                }

                // add to bus stop
                if (isBusStop && tile.TileIndex is 1057 && tile.TileSheet?.Id is "outdoors")
                    this.TryAddTicketMachine(map, x, y, StopNetworks.Bus);
            }
        }
    }

    /// <summary>Add a Central Station ticket machine if the location has a <see cref="Constant.TicketMachineMapProperty"/> map property.</summary>
    /// <param name="location">The location to edit.</param>
    private void AddTicketMachineForMapProperty(GameLocation location)
    {
        // get property
        if (!location.TryGetMapProperty(Constant.TicketMachineMapProperty, out string? rawProperty))
            return;

        // parse args
        string[] args = rawProperty.Split(' ');
        if (!ArgUtility.TryGetPoint(args, 0, out Point tile, out string? error) || !this.TryParseOptionalSpaceDelimitedNetworks(args, 2, out StopNetworks networks, out error, defaultValue: StopNetworks.Train))
        {
            this.Monitor.Log($"Location '{location.NameOrUniqueName}' has invalid property '{rawProperty}': {error}", LogLevel.Warn);
            return;
        }

        // add ticket machine
        this.TryAddTicketMachine(location.Map, tile.X, tile.Y, networks);
    }

    /// <summary>Apply edits to the Central Station map when it's loaded.</summary>
    /// <param name="assetData">The asset data.</param>
    private void EditCentralStationMap(IAssetData assetData)
    {
        var editor = assetData.AsMap();
        var map = editor.Data;

        // dark station
        if (this.StationDark.Value)
        {
            // make it darker
            map.Properties["AmbientLight"] = "200 200 100";

            // edit map tiles
            Layer backLayer = map.RequireLayer("Back");
            Layer buildingsLayer = map.RequireLayer("Buildings");
            Layer pathsLayer = map.RequireLayer("Paths");
            Layer frontLayer = map.RequireLayer("Front");
            const int lightPathIndex = 8;

            int layerHeight = pathsLayer.LayerHeight;
            int layerWidth = pathsLayer.LayerWidth;

            for (int y = 0; y < layerHeight; y++)
            {
                for (int x = 0; x < layerWidth; x++)
                {
                    // get tiles
                    Tile? backTile = backLayer.Tiles[x, y];
                    Tile? frontTile = frontLayer.Tiles[x, y];
                    Tile? pathTile = pathsLayer.Tiles[x, y];
                    Tile? buildingsTile = buildingsLayer.Tiles[x, y];

                    // get action property
                    if (buildingsTile?.Properties.TryGetValue("Action", out string? action) is not true)
                        action = null;

                    // remove lighting, but light up ticket booth & machine
                    if (pathTile?.TileIndex == lightPathIndex)
                        pathsLayer.Tiles[x, y] = null;
                    if (action != null && (action.StartsWithIgnoreCase($"{Constant.InternalAction} {MapSubActions.TicketBooth}") || action.StartsWithIgnoreCase($"{Constant.InternalAction} {MapSubActions.TicketMachine}")))
                        pathsLayer.Tiles[x, y - 1] ??= new StaticTile(pathsLayer, map.GetTileSheet("paths"), BlendMode.Alpha, lightPathIndex);

                    // remove gift shop clerk
                    if (frontTile?.TileIndex == 1910 && frontTile.TileSheet.Id == GameLocation.DefaultTileSheetId) // gift shop clerk's head
                        frontLayer.Tiles[x, y] = null;
                    else if (buildingsTile?.TileIndex == 1942 && buildingsTile.TileSheet.Id == GameLocation.DefaultTileSheetId)
                        buildingsLayer.Tiles[x, y] = null;

                    // close food court
                    if (backTile?.TileSheet.Id == "centralStation" && backTile.TileIndex is 152 or 153 or 154 or 155 or 172 or 173 or 174 or 175)
                        backLayer.Tiles[x, y] = new StaticTile(backTile.Layer, backTile.TileSheet, BlendMode.Alpha, backTile.TileIndex + 4);

                    // remove some interactions
                    if (action != null && (action.StartsWithIgnoreCase("OpenShop") || action.StartsWithIgnoreCase($"{Constant.InternalAction} {MapSubActions.Bookshelf}") || action.StartsWithIgnoreCase($"{Constant.InternalAction} {MapSubActions.PopUpShop}")))
                        buildingsTile!.Properties.Remove("Action");
                }
            }
        }

        // empty wood pedestal if sold
        SynchronizedShopStock syncedShop = Game1.player.team.synchronizedShopStock;
        string woodSyncId = $"{Constant.ModId}_GiftShop/{Game1.player.UniqueMultiplayerID}/Wood";
        var syncedStock = syncedShop.GetType().GetField("stockDictionary", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(syncedShop) as NetStringDictionary<int, NetInt>;
        if (syncedStock != null && syncedStock.TryGetValue(woodSyncId, out int stock) && stock <= 0)
            this.OnRareWoodSold(map);

        // add tourists
        this.AddCentralStationTourists(editor);
    }

    /// <summary>Add random tourist NPCs to the Central Station map.</summary>
    /// <param name="assetData">The Central Station map asset to edit.</param>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = "This is the method that validates the API contract.")]
    private void AddCentralStationTourists(IAssetDataForMap assetData)
    {
        // read tourist areas from map property
        Dictionary<string, Rectangle> touristAreas = [];
        {
            if (!assetData.Data.Properties.TryGetValue(Constant.TouristAreasMapProperty, out string rawProperty))
                return;

            string[] propertyArgs = ArgUtility.SplitBySpace(rawProperty);
            for (int i = 0; i < propertyArgs.Length; i += 5)
            {
                if (!ArgUtility.TryGet(propertyArgs, i, out string touristAreaId, out string error) || !ArgUtility.TryGetRectangle(propertyArgs, i + 1, out Rectangle touristArea, out error))
                {
                    this.Monitor.Log($"Can't add tourists to Central Station: map property '{Constant.TouristAreasMapProperty}' has invalid value '{rawProperty}': {error}", LogLevel.Warn);
                    return;
                }

                if (!touristAreas.TryAdd(touristAreaId, touristArea))
                {
                    this.Monitor.Log($"Can't add tourists to Central Station: map property '{Constant.TouristAreasMapProperty}' has invalid value '{rawProperty}': area ID '{touristAreaId}' is defined twice.", LogLevel.Warn);
                    return;
                }
            }
        }

        // collect available NPCs
        List<TouristSpawnOption> validTourists = [];
        foreach ((string mapId, TouristMapModel? touristMapData) in this.ContentHelper.Load<Dictionary<string, TouristMapModel?>>(AssetNames.Tourists))
        {
            // skip empty entry
            if (touristMapData?.Tourists?.Count is not > 0)
                continue;

            // validate
            if (string.IsNullOrWhiteSpace(mapId))
            {
                this.Monitor.LogOnce("Ignored tourist map with no ID field.", LogLevel.Warn);
                continue;
            }
            if (this.ModRegistry.GetFromNamespacedId(mapId) is null)
            {
                this.Monitor.LogOnce($"Ignored tourist map with ID '{mapId}': IDs must be prefixed with the exact unique mod ID, like `Example.ModId_TouristMapId`.", LogLevel.Warn);
                continue;
            }
            if (string.IsNullOrWhiteSpace(touristMapData.FromMap))
            {
                this.Monitor.LogOnce($"Ignored tourist map with ID '{mapId}' because it has no '{nameof(touristMapData.FromMap)}' value.", LogLevel.Warn);
                continue;
            }

            // add tourists to pool
            foreach ((string touristId, TouristModel? tourist) in touristMapData.Tourists)
            {
                if (tourist is null)
                    continue;

                // validate
                if (string.IsNullOrWhiteSpace(touristId))
                {
                    this.Monitor.LogOnce($"Ignored tourist from tourist map '{mapId}' with no ID field.", LogLevel.Warn);
                    continue;
                }

                // add to pool is available
                if (GameStateQuery.CheckConditions(tourist.Condition))
                    validTourists.Add(new(mapId, touristMapData, touristId, tourist));
            }
        }

        // shuffle tourists
        Random random = Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(Constant.ModId));
        Utility.Shuffle(random, validTourists);

        // spawn tourists on map
        LocalizedContentManager contentManager = Game1.content.CreateTemporary();
        Map map = assetData.Data;
        Layer buildingsLayer = map.RequireLayer("Buildings");
        Layer pathsLayer = map.RequireLayer("Paths");

        foreach ((string areaId, Rectangle area) in touristAreas)
        {
            for (int y = area.Y, maxY = area.Bottom - 1; y <= maxY; y++)
            {
                for (int x = area.X, maxX = area.Right - 1; x <= maxX; x++)
                {
                    // check preconditions
                    if (pathsLayer.Tiles[x, y]?.TileIndex is not 7) // red circle marks spawn points
                        continue;
                    if (validTourists.Count is 0)
                        return; // no further tourists can spawn
                    if (!random.NextBool(Constant.TouristSpawnChance))
                        continue;

                    // get tourist to spawn
                    TouristSpawnOption? spawn = null;
                    for (int i = validTourists.Count - 1; i >= 0; i--)
                    {
                        TouristSpawnOption candidate = validTourists[i];
                        if (candidate.Tourist.OnlyInAreas?.Count is null or 0 || candidate.Tourist.OnlyInAreas.Any(areaId.EqualsIgnoreCase))
                        {
                            spawn = candidate;
                            validTourists.RemoveAt(i);
                            break;
                        }
                    }
                    if (spawn is null)
                        continue;

                    // load map
                    Map touristMap;
                    try
                    {
                        touristMap = contentManager.Load<Map>(spawn.Map.FromMap);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"Ignored tourist '{spawn.MapId}' > '{spawn.TouristId}' because its map could not be loaded.\nTechnical details: {ex}", LogLevel.Warn);
                        continue;
                    }

                    // remove disallowed layers
                    for (int i = touristMap.Layers.Count - 1; i >= 0; i--)
                    {
                        Layer layer = touristMap.Layers[i];
                        if (layer.Id is not ("Buildings" or "Front"))
                            touristMap.RemoveLayer(layer);
                    }

                    // patch into map
                    Rectangle sourceRect = Utility.getSourceRectWithinRectangularRegion(
                        regionX: 0,
                        regionY: 0,
                        regionWidth: touristMap.GetSizeInTiles().Width,
                        sourceIndex: spawn.Tourist.Index,
                        sourceWidth: 1,
                        sourceHeight: 2
                    );
                    assetData.PatchMap(touristMap, sourceRect, new Rectangle(x, y - 1, 1, 2));

                    // add dialogue action
                    if (spawn.Tourist.Dialogue?.Count > 0)
                    {
                        Tile? buildingTile = buildingsLayer.Tiles[x, y];
                        if (buildingTile is null)
                            buildingsLayer.Tiles[x, y] = buildingTile = new StaticTile(buildingsLayer, map.GetTileSheet(GameLocation.DefaultTileSheetId), BlendMode.Alpha, 0);

                        buildingTile.Properties["Action"] = $"{Constant.InternalAction} {MapSubActions.TouristDialogue} {spawn.MapId} {spawn.TouristId}";
                    }
                }
            }
        }
    }

    /// <summary>Edit the vanilla bus stop map.</summary>
    /// <param name="asset">The map asset to edit.</param>
    private void EditBusStopMap(IAssetDataForMap asset)
    {
        // replace ticket machine
        // This reduces headaches due to the vanilla game's hardcode tile index checks applying before Central Station's action property
        Layer? layer = asset.Data.GetLayer("Buildings");
        if (layer != null)
        {
            int layerHeight = layer.LayerHeight;
            int layerWidth = layer.LayerWidth;

            for (int y = 0; y < layerHeight; y++)
            {
                for (int x = 0; x < layerWidth; x++)
                {
                    // get tile
                    Tile? tile = layer.Tiles[x, y];
                    if (tile is null)
                        continue;

                    if (tile.TileIndex is 1057 && tile.TileSheet?.Id is "outdoors")
                        this.TryAddTicketMachine(asset, x, y, StopNetworks.Bus);
                }
            }
        }
    }

    /// <summary>Edit the vanilla railroad map.</summary>
    /// <param name="asset">The map asset to edit.</param>
    private void EditRailroadMap(IAssetDataForMap asset)
    {
        // add ticket machine if not already present
        if (!this.TryGetActionTile(asset.Data, StopNetworks.Train, out _))
            this.TryAddTicketMachine(asset, tileX: 32, tileY: 40, StopNetworks.Train);
    }

    /// <summary>Add a ticket machine to a map.</summary>
    /// <param name="map">The map to edit.</param>
    /// <param name="tileX">The tile X position at which to place the machine.</param>
    /// <param name="tileY">The tile Y position at which to place the bottom of the machine.</param>
    /// <param name="networks">The networks to which the ticket machine is connected.</param>
    /// <returns>Returns whether the ticket machine was successfully applied.</returns>
    private void TryAddTicketMachine(Map map, int tileX, int tileY, StopNetworks networks)
    {
        IAssetDataForMap asset = this.ContentHelper.GetPatchHelper(map, map.assetPath).AsMap();

        this.TryAddTicketMachine(asset, tileX, tileY, networks);
    }

    /// <summary>Add a ticket machine to a map.</summary>
    /// <param name="asset">The map asset to edit.</param>
    /// <param name="tileX">The tile X position at which to place the machine.</param>
    /// <param name="tileY">The tile Y position at which to place the bottom of the machine.</param>
    /// <param name="networks">The networks to which the ticket machine is connected.</param>
    /// <returns>Returns whether the ticket machine was successfully applied.</returns>
    private void TryAddTicketMachine(IAssetDataForMap asset, int tileX, int tileY, StopNetworks networks)
    {
        // load ticket machine patch
        Map ticketMachine;
        try
        {
            ticketMachine = this.ContentHelper.Load<Map>(AssetNames.TicketMachine);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't load ticket machine to apply it to {asset.Name}. Is the mod installed correctly?\nTechnical details: {ex}", LogLevel.Error);
            return;
        }

        // apply patch
        asset.PatchMap(ticketMachine, targetArea: new Rectangle(tileX, tileY - 1, 1, 2));

        // set action property
        Layer buildingsLayer = asset.Data.RequireLayer("Buildings");
        Tile? tile = buildingsLayer.Tiles[tileX, tileY];
        if (tile is null)
        {
            this.Monitor.Log($"Couldn't set Central Station action property after adding machine to {asset.Name}.", LogLevel.Error);
            return;
        }

        tile.Properties["Action"] = $"{Constant.TicketsAction} {networks.ToString().Replace(",", " ")}";
    }

    /// <summary>Get the available bookshelf messages from the live asset.</summary>
    private IEnumerable<LiveMessageQueue.Message> GetBookshelfMessages()
    {
        foreach ((string id, List<string?>? dialogues) in this.ContentHelper.Load<Dictionary<string, List<string?>?>>(AssetNames.Bookshelf))
        {
            if (this.ModRegistry.GetFromNamespacedId(id) is null)
            {
                this.Monitor.LogOnce($"Ignored bookshelf messages with ID '{id}': IDs must be prefixed with the exact unique mod ID, like `Example.ModId_StopId`.", LogLevel.Warn);
                continue;
            }

            if (dialogues?.Count is not > 0)
            {
                this.Monitor.Log($"Can't get bookshelf messages with ID '{id}' because its dialogue list is empty.");
                yield break;
            }

            foreach (string? text in dialogues)
            {
                if (!string.IsNullOrWhiteSpace(text))
                    yield return new LiveMessageQueue.Message(Key: $"{id}#{text}", Text: text);
            }
        }
    }

    /// <summary>Get the available tourist dialogues from the live asset.</summary>
    /// <param name="mapId">The ID for the tourist map data which added the tourist.</param>
    /// <param name="touristId">The ID of the tourist within its tourist map data.</param>
    private IEnumerable<LiveMessageQueue.Message> GetTouristDialogues(string mapId, string touristId)
    {
        // get tourist map entry
        Dictionary<string, TouristMapModel?> data = this.ContentHelper.Load<Dictionary<string, TouristMapModel?>>(AssetNames.Tourists);
        if (!data.TryGetValue(mapId, out TouristMapModel? mapData))
        {
            this.Monitor.Log($"Can't get tourist dialogue '{mapId}' > '{touristId}' because that map ID wasn't found in the data.");
            yield break;
        }

        // get tourist entry
        TouristModel? tourist = mapData?.Tourists?.FirstOrDefault(p => p.Key == touristId).Value;
        if (tourist is null)
        {
            this.Monitor.Log($"Can't get tourist dialogue '{mapId}' > '{touristId}' because that tourist ID wasn't found in its tourist map data.");
            yield break;
        }
        if (tourist.Dialogue?.Count is not > 0)
        {
            this.Monitor.Log($"Can't get tourist dialogue '{mapId}' > '{touristId}' because that tourist has no dialogue.");
            yield break;
        }

        // get dialogue
        for (int i = 0; i < tourist.Dialogue.Count; i++)
        {
            string text = tourist.Dialogue[i] ?? string.Empty;
            yield return new LiveMessageQueue.Message($"{mapId}#{touristId}#{i}#{text}", text); // include index: tourists can repeat dialogue text (e.g. the flamingo's "..." lines)
        }
    }

    /// <summary>Get all translations with a prefix and suffix.</summary>
    /// <param name="prefix">The translation key prefix.</param>
    /// <param name="min">The min numeric key suffix.</param>
    /// <param name="max">The max numeric key suffix.</param>
    private IEnumerable<LiveMessageQueue.Message> GetNumberedTranslations(string prefix, int min, int max)
    {
        for (int i = min; i <= max; i++)
        {
            string key = $"{prefix}.{i}";
            string text = this.GetTranslation(key);

            yield return new LiveMessageQueue.Message(key, text);
        }
    }

    /// <summary>A tourist which may spawn when parsing map data.</summary>
    /// <param name="MapId">The entry key for the tourist map which adds the tourist.</param>
    /// <param name="Map">The tourist map data.</param>
    /// <param name="TouristId">The entry key for the tourist within the map.</param>
    /// <param name="Tourist">The tourist data.</param>
    private record TouristSpawnOption(string MapId, TouristMapModel Map, string TouristId, TouristModel Tourist);
}
