using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Pathoschild.Stardew.CentralStation.Framework;
using Pathoschild.Stardew.CentralStation.Framework.Constants;
using Pathoschild.Stardew.CentralStation.Framework.Integrations;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;

namespace Pathoschild.Stardew.CentralStation;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>Manages the Central Station content provided by content packs.</summary>
    private ContentManager ContentManager = null!; // set in Entry

    /// <summary>Manages the available destinations, including destinations provided through other frameworks like Train Station.</summary>
    private StopManager StopManager = null!; // set in Entry

    /// <summary>Whether the Bus Locations mod is installed, regardless of whether it has any stops loaded.</summary>
    private bool HasBusLocationsMod;

    /// <summary>Whether the player received a free item from a cola machine since they arrived in the Central Station.</summary>
    private readonly PerScreen<bool> GotRareColaDrop = new();

    /// <summary>Whether the player saw a rare strange occurrence since they arrived in the Central Station, aside from a strange cola machine or the dark station.</summary>
    private readonly PerScreen<bool> SawStrangeOccurrence = new();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // validate
        if (!this.ValidateInstall())
            return;

        // init
        this.ContentManager = new(helper.GameContent, helper.ModRegistry, this.Monitor);
        this.StopManager = new(this.ContentManager, this.Monitor, helper.ModRegistry);
        this.HasBusLocationsMod = helper.ModRegistry.IsLoaded(BusLocationsStopProvider.ModId);

        // hook events
        helper.Events.GameLoop.DayStarted += this.ContentManager.OnDayStarted;
        helper.Events.Content.AssetRequested += this.ContentManager.OnAssetRequested;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.Player.Warped += this.OnWarped;

        // hook tile actions
        GameLocation.RegisterTileAction(Constant.TicketsAction, this.OnTicketsAction);
        GameLocation.RegisterTileAction(Constant.InternalAction, this.OnCentralAction);
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod)
    {
        return new CentralStationApi(mod.Manifest, this.StopManager);
    }


    /*********
    ** Private methods
    *********/
    /****
    ** Handle map actions
    ****/
    /// <summary>Handle the player activating the map property which opens a destination menu.</summary>
    /// <param name="location">The location containing the property.</param>
    /// <param name="args">The action arguments.</param>
    /// <param name="who">The player who activated it.</param>
    /// <param name="tile">The tile containing the action property.</param>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnTicketsAction(GameLocation location, string[] args, Farmer who, Point tile)
    {
        if (!this.ContentManager.TryParseOptionalSpaceDelimitedNetworks(args, 1, out StopNetworks networks, out string? error, StopNetworks.Train))
        {
            this.Monitor.LogOnce($"Location {location.NameOrUniqueName} has invalid CentralStation property: {error}", LogLevel.Warn);
            return false;
        }

        this.OpenMenu(networks);
        return true;
    }

    /// <summary>Handle the player activating the map property in the Central Station which performs an internal sub-action identified by a <see cref="MapSubActions"/> value.</summary>
    /// <param name="location">The location containing the property.</param>
    /// <param name="args">The action arguments.</param>
    /// <param name="who">The player who activated it.</param>
    /// <param name="tile">The tile containing the action property.</param>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnCentralAction(GameLocation location, string[] args, Farmer who, Point tile)
    {
        if (location.NameOrUniqueName is not Constant.CentralStationLocationId)
            return false;

        string subAction = ArgUtility.Get(args, 1);
        switch (subAction)
        {
            case MapSubActions.TicketBooth:
            case MapSubActions.TicketMachine:
                return this.OnCentralTicketAction(isTicketBooth: subAction is MapSubActions.TicketBooth);

            case MapSubActions.Bookshelf:
                return this.OnCentralBookshelfAction();

            case MapSubActions.ColaMachine:
                return this.OnCentralColaAction(location, who, tile);

            case MapSubActions.ExitDoor:
                return this.OnCentralExitDoorAction();

            case MapSubActions.PopUpShop:
                return this.OnCentralPopupShopAction();

            case MapSubActions.TouristDialogue:
                return this.OnCentralTouristAction(location, args, tile);

            default:
                return false;
        }
    }

    /// <summary>Handle the player activating a <see cref="MapSubActions.TicketBooth"/> or <see cref="MapSubActions.TicketMachine"/> action in the Central Station.</summary>
    /// <param name="isTicketBooth">Whether the player interacted with the ticket booth (<c>true</c>) or machine (<c>false</c>).</param>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnCentralTicketAction(bool isTicketBooth)
    {
        void ShowTickets() => this.OpenMenu(StopNetworks.Boat | StopNetworks.Bus | StopNetworks.Train);

        // rare chance of showing a secret message before the ticket menu
        if (this.ContentManager.GetStationVisits() >= Constant.StrangeMessageMinVisits && !this.SawStrangeOccurrence.Value && Game1.random.NextBool(Constant.StrangeMessageChance))
        {
            this.SawStrangeOccurrence.Value = true;

            string message = isTicketBooth
                ? this.ContentManager.GetNextStrangeMessage("location.ticket-counter", 1, 3)
                : this.ContentManager.GetNextStrangeMessage("location.ticket-machine", 1, 3);

            Game1.drawDialogueNoTyping(message);
            Game1.PerformActionWhenPlayerFree(ShowTickets);
        }
        else
            ShowTickets();

        return true;
    }

    /// <summary>Handle the player activating a <see cref="MapSubActions.Bookshelf"/> action in the Central Station.</summary>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnCentralBookshelfAction()
    {
        if (this.ContentManager.TryGetBookshelfMessage(out string? message, out _))
        {
            Game1.drawDialogueNoTyping(message);
            return true;
        }

        return false;
    }

    /// <summary>Handle the player activating a <see cref="MapSubActions.ColaMachine"/> action in the Central Station.</summary>
    /// <param name="location">The location containing the property.</param>
    /// <param name="who">The player who activated it.</param>
    /// <param name="tile">The tile containing the action property.</param>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnCentralColaAction(GameLocation location, Farmer who, Point tile)
    {
        const string jojaColaId = "(O)167";

        // rare chance of free item, else show dialogue to buy Joja cola
        if (this.ContentManager.GetStationVisits() >= Constant.StrangeColaMachineMinVisits && !this.GotRareColaDrop.Value && Game1.random.NextBool(Constant.StrangeColaMachineChance))
        {
            this.GotRareColaDrop.Value = true;

            Item drink;
            string message;

            if (Game1.random.NextBool())
            {
                drink = ItemRegistry.Create(jojaColaId);
                message = this.ContentManager.GetNextStrangeMessage("location.cola-machine", 2, 3); // skip variant 1, which suggests a non-Joja Cola item
            }
            else
            {
                ParsedItemData[] drinks = ItemRegistry
                    .GetObjectTypeDefinition()
                    .GetAllData()
                    .Where(p => p.RawData is ObjectData { IsDrink: true } && p.QualifiedItemId is not jojaColaId)
                    .ToArray();

                drink = ItemRegistry.Create(Game1.random.ChooseFrom(drinks).QualifiedItemId);
                message = this.ContentManager.GetNextStrangeMessage("location.cola-machine", 1, 3);
            }

            Game1.drawDialogueNoTyping(message);
            Game1.PerformActionWhenPlayerFree(() => Game1.player.addItemByMenuIfNecessary(drink));
        }
        else
            location.performAction(["ColaMachine"], who, new Location(tile.X, tile.Y));

        return true;
    }

    /// <summary>Handle the player activating a <see cref="MapSubActions.ExitDoor"/> action in the Central Station.</summary>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnCentralExitDoorAction()
    {
        // rare chance of strange sounds, else locked-door sound
        if (this.ContentManager.GetStationVisits() >= Constant.StrangeSoundsMinVisits && !this.SawStrangeOccurrence.Value && Game1.random.NextBool(Constant.StrangeSoundsChance))
        {
            this.SawStrangeOccurrence.Value = true;

            Game1.playSound("sipTea");
            DelayedAction.playSoundAfterDelay("sipTea", 200);
            DelayedAction.playSoundAfterDelay("sipTea", 400);
            DelayedAction.playSoundAfterDelay("seeds", 600);
            DelayedAction.playSoundAfterDelay("Duggy", 800);

            DelayedAction.functionAfterDelay(() => Game1.drawDialogueNoTyping(this.ContentManager.GetTranslation("location.exit-door.strange-sounds")), 800);
        }
        else
        {
            Game1.playSound("doorOpen", out ICue cue);
            cue.Volume = 0.5f;
        }

        return true;
    }

    /// <summary>Handle the player activating a <see cref="MapSubActions.PopUpShop"/> action in the Central Station.</summary>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnCentralPopupShopAction()
    {
        Game1.drawDialogueNoTyping(this.ContentManager.GetTranslation("vendor-shop.dialogue.coming-soon"));
        return true;
    }

    /// <summary>Handle the player activating a <see cref="MapSubActions.TouristDialogue"/> action in the Central Station.</summary>
    /// <param name="location">The location containing the property.</param>
    /// <param name="args">The action arguments.</param>
    /// <param name="tile">The tile containing the action property.</param>
    /// <returns>Returns whether the action was handled.</returns>
    private bool OnCentralTouristAction(GameLocation location, string[] args, Point tile)
    {
        // read args
        if (!ArgUtility.TryGet(args, 2, out string mapId, out string error) || !ArgUtility.TryGet(args, 3, out string touristId, out error))
        {
            this.Monitor.LogOnce($"Location {location.NameOrUniqueName} has invalid {args[0]} property: {error}.", LogLevel.Warn);
            return false;
        }

        // get dialogue
        bool hasMessage = this.ContentManager.TryGetTouristDialogue(mapId, touristId, out string? dialogue, out bool hasMoreMessages);
        if (hasMessage)
            Game1.drawObjectDialogue(dialogue);

        // if we're viewing their last dialogue, remove the property to avoid a ghost hand cursor
        if (!hasMoreMessages)
            location.removeTileProperty(tile.X, tile.Y, "Buildings", "Action");

        return hasMessage;
    }


    /****
    ** Handle SMAPI events
    ****/
    /// <inheritdoc cref="IDisplayEvents.MenuChanged" />
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Bus Locations handles any action click on the ticket machine coordinates and replaces Central Station's
        // menu even if it's shown first. Since we include Bus Locations' stops in our menu, reopen ours instead.
        if (this.HasBusLocationsMod && Game1.currentLocation is BusStop busStop && e.NewMenu is DialogueBox dialogueBox && dialogueBox.dialogues.FirstOrDefault() is "Where would you like to go?" or "Out of service")
        {
            busStop.lastQuestionKey = null;
            busStop.afterQuestion = null;
            Game1.objectDialoguePortraitPerson = null;

            this.OpenMenu(StopNetworks.Bus);
        }
    }

    /// <inheritdoc cref="IPlayerEvents.Warped" />
    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        this.GotRareColaDrop.Value = false;
        this.SawStrangeOccurrence.Value = false;

        this.ContentManager.OnWarped(sender, e);
    }


    /****
    ** Helper methods
    ****/
    /// <summary>Open the menu to choose a destination.</summary>
    /// <param name="networks">The networks for which to get stops.</param>
    private void OpenMenu(StopNetworks networks)
    {
        // get stops
        // Central Station first, then Stardew Valley, then any others in alphabetical order
        var choices = this.StopManager
            .GetAvailableStops(networks)
            .Select(stop => (Stop: stop, Label: this.ContentManager.GetStopLabel(stop, networks)))
            .OrderBy(choice => choice.Stop.Id switch
            {
                DestinationIds.CentralStation => 0,
                DestinationIds.BoatTunnel or DestinationIds.BusStop or DestinationIds.Railroad => 1,
                _ => 2
            })
            .ThenBy(choice => choice.Label, HumanSortComparer.DefaultIgnoreCase)
            .ToArray();
        if (choices.Length == 0)
        {
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
            return;
        }

        // show menu
        Game1.currentLocation.ShowPagedResponses(
            prompt: Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"),
             responses: [.. choices.Select(choice => KeyValuePair.Create(choice.Stop.Id, choice.Label))],
            on_response: OnRawDestinationPicked,
            itemsPerPage: 6 // largest page size used in vanilla, barely fits on smallest screen
        );
        void OnRawDestinationPicked(string selectedId)
        {
            Stop? stop = choices.FirstOrDefault(stop => stop.Stop.Id == selectedId).Stop;
            if (stop != null)
                this.OnDestinationPicked(stop, networks);
        }
    }

    /// <summary>Handle the player choosing a destination in the UI.</summary>
    /// <param name="stop">The selected stop.</param>
    /// <param name="networks">The networks containing the stop.</param>
    private void OnDestinationPicked(Stop stop, StopNetworks networks)
    {
        // apply vanilla behavior for default routes
        switch (stop.Id)
        {
            // boat to Ginger Island
            case DestinationIds.GingerIsland:
                if (Game1.currentLocation is BoatTunnel tunnel && networks.HasFlag(StopNetworks.Boat))
                {
                    if (this.TryDeductCost(tunnel.TicketPrice))
                        tunnel.StartDeparture();
                    else
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
                    return;
                }
                break;

            // bus to desert
            case DestinationIds.Desert:
                if (Game1.currentLocation is BusStop busStop && networks.HasFlag(StopNetworks.Bus))
                {
                    busStop.lastQuestionKey = "Bus";
                    busStop.afterQuestion = null;
                    busStop.answerDialogue(new Response("Yes", ""));
                    return;
                }
                break;
        }

        // charge ticket price
        if (!this.TryDeductCost(stop.Cost))
        {
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
            return;
        }

        // warp
        LocationRequest request = Game1.getLocationRequest(stop.ToLocation);
        request.OnWarp += () => this.OnWarped(stop, networks);
        Game1.warpFarmer(request, stop.ToTile?.X ?? 0, stop.ToTile?.Y ?? 0, stop.ToFacingDirection);
    }

    /// <summary>The action to perform when the player arrives at the destination.</summary>
    /// <param name="stop">The stop that the player warped to.</param>
    /// <param name="fromNetwork">The networks of the stop where the player embarked to reach this one.</param>
    private void OnWarped(Stop stop, StopNetworks fromNetwork)
    {
        GameLocation location = Game1.currentLocation;

        // choose network travelled
        StopNetworks network = stop.Network & fromNetwork;
        if (network == 0)
            network = stop.Network;
        network = network.GetPreferred();

        // auto-detect arrival spot if needed
        if (stop.ToTile is null)
        {
            int tileX = 0;
            int tileY = 0;
            if (this.ContentManager.TryGetActionTile(location?.Map, network, out Point machineTile))
            {
                tileX = machineTile.X;
                tileY = machineTile.Y + 1;
            }
            else if (location is BusStop { Name: "BusStop" } && this.ContentManager.TryGetTileIndex(location.Map, "outdoors", "Buildings", 1057, out machineTile))
            {
                tileX = machineTile.X;
                tileY = machineTile.Y + 1;
            }
            else
                Utility.getDefaultWarpLocation(location?.Name, ref tileX, ref tileY);

            Game1.player.Position = new Vector2(tileX * Game1.tileSize, tileY * Game1.tileSize);
        }

        // pause fade to simulate travel
        // (setting a null message pauses without showing a message afterward)
        const int pauseTime = 1500;
        Game1.pauseThenMessage(pauseTime, null);

        // play transit effects mid-fade
        switch (network)
        {
            case StopNetworks.Bus:
                Game1.playSound("busDriveOff");
                break;

            case StopNetworks.Boat:
                Game1.playSound("waterSlosh");
                DelayedAction.playSoundAfterDelay("waterSlosh", 500);
                DelayedAction.playSoundAfterDelay("waterSlosh", 1000);
                break;

            case StopNetworks.Train:
                {
                    Game1.playSound("trainLoop", out ICue cue);
                    cue.SetVariable("Volume", 100f); // default volume is zero
                    DelayedAction.functionAfterDelay(
                        () =>
                        {
                            Game1.playSound("trainWhistle"); // disguise end of looping sounds
                            cue.Stop(AudioStopOptions.Immediate);
                        },
                        pauseTime
                    );
                }
                break;
        }
    }

    /// <summary>Deduct the cost of a ticket from the player's money, if they have enough.</summary>
    /// <param name="cost">The ticket cost.</param>
    private bool TryDeductCost(int cost)
    {
        if (Game1.player.Money >= cost)
        {
            Game1.player.Money -= cost;
            return true;
        }

        return false;
    }

    /// <summary>Validate that Central Station is installed correctly.</summary>
    private bool ValidateInstall()
    {
        IModInfo? contentPack = this.Helper.ModRegistry.Get("Pathoschild.CentralStation.Content");

        if (contentPack is null)
        {
            this.Monitor.Log("Central Station is missing its content files, so it won't work. Please delete and reinstall the mod to fix this.", LogLevel.Error);
            return false;
        }

        if (contentPack.Manifest.Version.ToString() != this.ModManifest.Version.ToString())
        {
            this.Monitor.Log($"Central Station was updated incorrectly, so it won't work. (It has code version {this.ModManifest.Version} and content version {contentPack.Manifest.Version}.) Please delete and reinstall the mod to fix this.", LogLevel.Error);
            return false;
        }

        return true;
    }
}
