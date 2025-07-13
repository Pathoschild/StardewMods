using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Integrations.IconicFramework;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.TractorMod.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;

namespace Pathoschild.Stardew.TractorMod;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /****
    ** Constants
    ****/
    /// <summary>The update rate when only one player is in a location (as a frame multiple).</summary>
    private readonly uint TextureUpdateRateWithSinglePlayer = 30;

    /// <summary>The update rate when multiple players are in the same location (as a frame multiple). This should be more frequent due to sprite broadcasts, new horses instances being created during NetRef&lt;Horse&gt; syncs, etc.</summary>
    private readonly uint TextureUpdateRateWithMultiplePlayers = 3;

    /// <summary>The unique ID for the stable building in <c>Data/Buildings</c>.</summary>
    private readonly string GarageBuildingId = "Pathoschild.TractorMod_Stable";

    /// <summary>The minimum version the host must have for the mod to be enabled on a farmhand.</summary>
    private readonly string MinHostVersion = "4.15.0";

    /// <summary>The base path for assets loaded through the game's content pipeline so other mods can edit them.</summary>
    private readonly string PublicAssetBasePath = "Mods/Pathoschild.TractorMod";

    /// <summary>The message ID for a request to warp a tractor to the given farmhand.</summary>
    private readonly string RequestTractorMessageID = "TractorRequest";

    /****
    ** State
    ****/
    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry

    /// <summary>The configured key bindings.</summary>
    private ModConfigKeys Keys => this.Config.Controls;

    /// <summary>Manages audio effects for the tractor.</summary>
    private AudioManager AudioManager = null!; // set in Entry

    /// <summary>Manages textures loaded for the tractor and garage.</summary>
    private TextureManager TextureManager = null!; // set in Entry

    /// <summary>The backing field for <see cref="TractorManager"/>.</summary>
    private PerScreen<TractorManager> TractorManagerImpl = null!; // set in Entry

    /// <summary>The tractor being ridden by the current player.</summary>
    private TractorManager TractorManager => this.TractorManagerImpl.Value;

    /// <summary>Whether the mod is enabled for the current farmhand.</summary>
    private bool IsEnabled = true;

    /// <summary>When the <c>Data/AudioChanges</c> asset was last edited, whether Tractor Mod added its custom audio.</summary>
    /// <remarks>Due to how that asset works, audio changes are applicable for the remainder of the session even if we later remove the entries from the asset.</remarks>
    private bool AppliedAudioChanges;

    /// <summary>The current opacity of the tractor radius if it's being temporarily displayed, as a value between 0 (transparent) and 1 (default).</summary>
    private readonly PerScreen<float> HighlightDistanceAlpha = new();

    /// <summary>The temporary radius, or -1 for the default radius.</summary>
    private readonly PerScreen<int> TemporaryDistance = new(() => -1);


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        CommonHelper.RemoveObsoleteFiles(this, "TractorMod.pdb"); // removed in 4.16.5

        // read config
        this.Config = helper.ReadConfig<ModConfig>();

        // init
        I18n.Init(helper.Translation);
        this.AudioManager = new AudioManager(
            directoryPath: this.Helper.DirectoryPath,
            isActive: () => this.Config.SoundEffects == TractorSoundType.Tractor,
            getVolume: () => this.Config.SoundEffectsVolume
        );
        this.TextureManager = new(
            directoryPath: this.Helper.DirectoryPath,
            publicAssetBasePath: this.PublicAssetBasePath,
            gameContentHelper: helper.GameContent,
            modContentHelper: helper.ModContent,
            monitor: this.Monitor
        );
        this.TractorManagerImpl = new(() =>
        {
            var manager = new TractorManager(this.Config, this.GetDistance, this.Keys, this.Helper.Reflection, () => this.TextureManager.BuffIconTexture, this.AudioManager);
            this.UpdateConfigFor(manager);
            return manager;
        });
        this.UpdateConfig();

        // hook events
        IModEvents events = helper.Events;
        events.Content.AssetRequested += this.OnAssetRequested;
        events.Content.LocaleChanged += this.OnLocaleChanged;
        events.GameLoop.GameLaunched += this.OnGameLaunched;
        events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        events.GameLoop.DayStarted += this.OnDayStarted;
        events.GameLoop.DayEnding += this.OnDayEnding;
        events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        events.GameLoop.Saved += this.OnSaved;
        events.Display.RenderedWorld += this.OnRenderedWorld;
        events.Input.ButtonsChanged += this.OnButtonsChanged;
        events.World.NpcListChanged += this.OnNpcListChanged;
        events.World.LocationListChanged += this.OnLocationListChanged;
        events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
        events.Player.Warped += this.OnWarped;

        // validate translations
        if (!helper.Translation.GetTranslations().Any())
            this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
    }


    /*********
    ** Private methods
    *********/
    /****
    ** Event handlers
    ****/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // warn about incompatible mods
        if (this.Helper.ModRegistry.IsLoaded("bcmpinc.HarvestWithScythe"))
            this.Monitor.Log("The 'Harvest With Scythe' mod is compatible with Tractor Mod, but it may break some tractor scythe features. You can ignore this warning if you don't have any scythe issues.", LogLevel.Warn);

        // add config UI
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForTractor(this.Helper.ModRegistry),
            get: () => this.Config,
            set: config => this.Config = config,
            onSaved: this.UpdateConfig
        );

        // add Iconic Framework icon
        IconicFrameworkIntegration iconicFramework = new(this.Helper.ModRegistry, this.Monitor);
        if (iconicFramework.IsLoaded)
        {
            iconicFramework.AddToolbarIcon(
                this.Helper.ModContent.GetInternalAssetName("assets/icon.png").BaseName,
                new Rectangle(0, 0, 16, 16),
                I18n.Icon_SummonTractor_Name,
                I18n.Icon_SummonTractor_Desc,
                this.SummonTractor
            );
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // load legacy data
        Migrator.AfterLoad(this.Helper, this.Monitor, this.ModManifest.Version);

        // check if mod should be enabled for the current player
        this.IsEnabled = Context.IsMainPlayer;
        if (!this.IsEnabled)
        {
            ISemanticVersion? hostVersion = this.Helper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?.GetMod(this.ModManifest.UniqueID)?.Version;
            if (hostVersion == null)
            {
                this.IsEnabled = false;
                this.Monitor.Log("This mod is disabled because the host player doesn't have it installed.", LogLevel.Warn);
            }
            else if (hostVersion.IsOlderThan(this.MinHostVersion))
            {
                this.IsEnabled = false;
                this.Monitor.Log($"This mod is disabled because the host player has {this.ModManifest.Name} {hostVersion}, but the minimum compatible version is {this.MinHostVersion}.", LogLevel.Warn);
            }
            else
                this.IsEnabled = true;
        }

        this.TemporaryDistance.Value = -1;
        this.HighlightDistanceAlpha.Value = 0;
    }

    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        // reload textures
        this.TextureManager.UpdateTextures();

        if (Context.IsMainPlayer)
        {
            // init garages + tractors
            Dictionary<Guid, Horse?> validTractors = [];
            foreach (GameLocation location in this.GetLocations())
            {
                foreach (Stable garage in this.GetGaragesIn(location))
                {
                    // fix duplicate ID
                    if (validTractors.ContainsKey(garage.HorseId))
                    {
                        garage.HorseId = Guid.NewGuid();
                        garage.id.Value = garage.HorseId;
                    }

                    // spawn new tractor if needed
                    Horse? tractor = this.FindHorse(garage.HorseId);
                    if (!garage.isUnderConstruction())
                    {
                        Vector2 tractorTile = this.GetDefaultTractorTile(garage);
                        if (tractor == null)
                        {
                            tractor = new Horse(garage.HorseId, (int)tractorTile.X, (int)tractorTile.Y);
                            location.addCharacter(tractor);
                        }
                        tractor.DefaultPosition = tractorTile;
                    }

                    // normalize tractor
                    if (tractor != null)
                        TractorManager.SetTractorInfo(tractor, this.Config.SoundEffects);

                    // normalize ownership
                    garage.owner.Value = 0;
                    if (tractor != null)
                        tractor.ownerId.Value = 0;

                    // apply textures
                    this.TextureManager.ApplyTextures(tractor, this.IsTractor);

                    // track tractor
                    validTractors[garage.HorseId] = tractor;
                }
            }

            // clean up duplicate/invalid tractors
            foreach (GameLocation location in this.GetLocations())
            {
                location.characters.RemoveWhere(npc =>
                {
                    if (npc is Horse tractor && this.IsTractor(tractor))
                    {
                        if (!validTractors.TryGetValue(tractor.HorseId, out Horse? validTractor))
                        {
                            this.Monitor.Log($"Removed unknown tractor in {location.NameOrUniqueName} (ID: {tractor.HorseId}).");
                            return true;
                        }

                        if (!object.ReferenceEquals(tractor, validTractor))
                        {
                            this.Monitor.Log($"Removed duplicate tractor in {location.NameOrUniqueName} (ID: {tractor.HorseId}).");
                            return true;
                        }
                    }

                    return false;
                });
            }
        }
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        this.AppliedAudioChanges |= this.AudioManager.OnAssetRequested(e);
        this.TextureManager.OnAssetRequested(e);

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
        {
            e.Edit(editor =>
            {
                var data = editor.AsDictionary<string, BuildingData>().Data;

                data[this.GarageBuildingId] = new BuildingData
                {
                    Name = I18n.Garage_Name(),
                    Description = I18n.Garage_Description(),
                    Texture = $"{this.PublicAssetBasePath}/Garage",
                    BuildingType = typeof(Stable).FullName,
                    SortTileOffset = 1,

                    Builder = Game1.builder_robin,
                    BuildCost = this.Config.BuildPrice,
                    BuildMaterials = this.Config.RequireBuildMaterials
                        ? this.Config.BuildMaterials
                            .Select(p => new BuildingMaterial
                            {
                                ItemId = p.Key,
                                Amount = p.Value
                            })
                            .ToList()
                        : [],
                    BuildDays = 2,

                    Size = new Point(4, 2),
                    CollisionMap = "XXXX\nXOOX"
                };
            });
        }
    }

    /// <inheritdoc cref="IContentEvents.LocaleChanged" />
    private void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
    {
        this.Helper.GameContent.InvalidateCache("Data/Buildings");
    }

    /// <inheritdoc cref="IWorldEvents.LocationListChanged" />
    private void OnLocationListChanged(object? sender, LocationListChangedEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        // rescue lost tractors
        if (Context.IsMainPlayer)
        {
            foreach (GameLocation location in e.Removed)
            {
                foreach (Horse tractor in this.GetTractorsIn(location).ToArray())
                    this.DismissTractor(tractor);
            }
        }
    }

    /// <inheritdoc cref="IWorldEvents.NpcListChanged" />
    private void OnNpcListChanged(object? sender, NpcListChangedEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        // workaround for instantly-built tractors spawning a horse
        if (Context.IsMainPlayer && e.Location.buildings.Any())
        {
            Horse[] horses = e.Added.OfType<Horse>().ToArray();
            if (horses.Any())
            {
                HashSet<Guid> tractorIDs = [.. this.GetGaragesIn(e.Location).Select(p => p.HorseId)];
                foreach (Horse horse in horses)
                {
                    if (tractorIDs.Contains(horse.HorseId) && !TractorManager.IsTractor(horse))
                        TractorManager.SetTractorInfo(horse, this.Config.SoundEffects);
                }
            }
        }
    }

    /// <inheritdoc cref="IPlayerEvents.Warped" />
    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer || !this.TractorManager.IsCurrentPlayerRiding)
            return;

        // fix: warping onto a magic warp while mounted causes an infinite warp loop
        Vector2 tile = CommonHelper.GetPlayerTile(Game1.player);
        string touchAction = Game1.player.currentLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "TouchAction", "Back");
        if (this.TractorManager.IsCurrentPlayerRiding && touchAction?.Split(' ', 2).First() is "MagicWarp" or "Warp")
            Game1.currentLocation.lastTouchActionLocation = tile;

        // fix: warping into an event may break the event (e.g. Mr Qi's event on mine level event for the 'Cryptic Note' quest)
        if (Game1.CurrentEvent != null)
            Game1.player.mount.dismount();
    }

    /// <inheritdoc cref="IGameLoopEvents.UpdateTicked" />
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        // multiplayer: override textures in the current location
        if (Context.IsWorldReady && Game1.currentLocation != null)
        {
            uint updateRate = Game1.currentLocation.farmers.Count > 1 ? this.TextureUpdateRateWithMultiplePlayers : this.TextureUpdateRateWithSinglePlayer;
            if (e.IsMultipleOf(updateRate))
            {
                foreach (Horse horse in this.GetTractorsIn(Game1.currentLocation))
                    this.TextureManager.ApplyTextures(horse, this.IsTractor);
            }
        }

        // update tractor effects
        if (Context.IsPlayerFree)
            this.TractorManager.Update();
        else if (Game1.CurrentEvent is not null)
            this.AudioManager.SetEngineState(EngineState.Stop);
    }

    /// <inheritdoc cref="IGameLoopEvents.DayEnding" />
    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        // move tractors out of locations which Utility.findHorse can't find
        if (Context.IsMainPlayer)
        {
            HashSet<GameLocation> vanillaLocations = new(Game1.locations, new ObjectReferenceComparer<GameLocation>());

            foreach (GameLocation location in this.GetLocations())
            {
                if (vanillaLocations.Contains(location))
                    continue;

                foreach (Horse tractor in this.GetTractorsIn(location).ToArray())
                    Game1.warpCharacter(tractor, "Farm", new Point(0, 0));
            }
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.ReturnedToTitle" />
    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.AudioManager.SetEngineState(EngineState.Stop);
    }

    /// <inheritdoc cref="IGameLoopEvents.Saved" />
    private void OnSaved(object? sender, SavedEventArgs e)
    {
        Migrator.AfterSave();
    }

    /// <inheritdoc cref="IDisplayEvents.RenderedWorld" />
    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        // render debug radius
        if ((this.Config.HighlightRadius || this.HighlightDistanceAlpha.Value > 0) && Context.IsWorldReady && Game1.activeClickableMenu == null && this.TractorManager.IsCurrentPlayerRiding)
        {
            if (this.Config.HighlightRadius)
                this.TractorManager.DrawRadius(Game1.spriteBatch, 1f);
            else
            {
                this.TractorManager.DrawRadius(Game1.spriteBatch, Math.Min(this.HighlightDistanceAlpha.Value, 1f));
                this.HighlightDistanceAlpha.Value -= ModConstants.HighlightDistanceFade;
            }
        }
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.IsEnabled || !Context.IsPlayerFree)
            return;

        if (this.Keys.SummonTractor.JustPressed() && !Game1.player.isRidingHorse())
            this.SummonTractor();
        else if (this.Keys.DismissTractor.JustPressed() && Game1.player.isRidingHorse())
            this.DismissTractor(Game1.player.mount);
        else if (this.Keys.IncreaseDistance.JustPressed() && this.TractorManager.IsCurrentPlayerRiding)
            this.OffsetTemporaryDistance(1);
        else if (this.Keys.ReduceDistance.JustPressed() && this.TractorManager.IsCurrentPlayerRiding)
            this.OffsetTemporaryDistance(-1);
    }

    /// <inheritdoc cref="IMultiplayerEvents.ModMessageReceived" />
    private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        // tractor request from a farmhand
        if (e.Type == this.RequestTractorMessageID && Context.IsMainPlayer && e.FromModID == this.ModManifest.UniqueID)
        {
            Farmer? player = Game1.GetPlayer(e.FromPlayerID);
            if (player is { IsMainPlayer: false })
            {
                this.Monitor.Log(this.SummonLocalTractorTo(player)
                    ? $"Summon tractor for {player.Name} ({e.FromPlayerID})."
                    : $"Received tractor request for {player.Name} ({e.FromPlayerID}), but no tractor is available."
                );
            }
            else
                this.Monitor.Log($"Received tractor request for {e.FromPlayerID}, but no such player was found.");
        }
    }

    /****
    ** Helper methods
    ****/
    /// <summary>Reapply the mod configuration.</summary>
    private void UpdateConfig()
    {
        // update building data
        this.Helper.GameContent.InvalidateCache("Data/Buildings");

        // apply audio changes
        if (this.Config.SoundEffects == TractorSoundType.Tractor)
        {
            if (!this.AppliedAudioChanges)
                this.Helper.GameContent.InvalidateCache("Data/AudioChanges");
        }
        else
            this.AppliedAudioChanges = false;

        // update current tractors
        foreach (var pair in this.TractorManagerImpl.GetActiveValues())
            this.UpdateConfigFor(pair.Value);

        // set engine volume
        this.AudioManager.UpdateVolume();
    }

    /// <summary>Apply the mod configuration to a tractor manager instance.</summary>
    /// <param name="manager">The tractor manager to update.</param>
    private void UpdateConfigFor(TractorManager manager)
    {
        var modRegistry = this.Helper.ModRegistry;
        var reflection = this.Helper.Reflection;
        var toolConfig = this.Config.StandardAttachments;

        manager.UpdateConfig(
            this.Config,
            this.Keys,
            new IAttachment?[]
            {
                    new CustomAttachment(this.Config.CustomAttachments, modRegistry), // should be first so it can override default attachments
                    new AxeAttachment(toolConfig.Axe, modRegistry, reflection),
                    new FertilizerAttachment(toolConfig.Fertilizer, modRegistry, reflection),
                    new GrassStarterAttachment(toolConfig.GrassStarter, modRegistry),
                    new HoeAttachment(toolConfig.Hoe, modRegistry),
                    new MeleeBluntAttachment(toolConfig.MeleeBlunt, modRegistry),
                    new MeleeDaggerAttachment(toolConfig.MeleeDagger, modRegistry),
                    new MeleeSwordAttachment(toolConfig.MeleeSword, modRegistry),
                    new MilkPailAttachment(toolConfig.MilkPail, modRegistry),
                    new PickaxeAttachment(toolConfig.PickAxe, modRegistry, reflection),
                    new ScytheAttachment(toolConfig.Scythe, modRegistry, reflection),
                    new SeedAttachment(toolConfig.Seeds, modRegistry, reflection),
                    modRegistry.IsLoaded(SeedBagAttachment.ModId) ? new SeedBagAttachment(toolConfig.SeedBagMod, modRegistry) : null,
                    new ShearsAttachment(toolConfig.Shears, modRegistry),
                    new SlingshotAttachment(toolConfig.Slingshot, modRegistry),
                    new WateringCanAttachment(toolConfig.WateringCan, modRegistry)
            }.WhereNotNull().ToArray()
        );
    }

    /// <summary>Summon an unused tractor to the player's current position, if any are available.</summary>
    private void SummonTractor()
    {
        bool summoned = this.SummonLocalTractorTo(Game1.player);
        if (!summoned && !Context.IsMainPlayer)
        {
            this.Monitor.Log("Sending tractor request to host player.");
            this.Helper.Multiplayer.SendMessage(
                message: true,
                messageType: this.RequestTractorMessageID,
                modIDs: [this.ModManifest.UniqueID],
                playerIDs: [Game1.MasterPlayer.UniqueMultiplayerID]
            );
        }
    }

    /// <summary>Summon an unused tractor to a player's current position, if any are available. If the player is a farmhand in multiplayer, only tractors in synced locations can be found by this method.</summary>
    /// <param name="player">The target player.</param>
    /// <returns>Returns whether a tractor was successfully summoned.</returns>
    private bool SummonLocalTractorTo(Farmer? player)
    {
        // get player info
        if (player == null)
            return false;
        GameLocation location = player.currentLocation;
        Vector2 tile = player.Tile;

        // find nearest tractor in player's current location (if available), else any location
        Horse? tractor = this
            .GetTractorsIn(location, includeMounted: false)
            .MinBy(match => Utility.distance(tile.X, tile.Y, match.TilePoint.X, match.TilePoint.Y));
        tractor ??= this
            .GetLocations()
            .SelectMany(loc => this.GetTractorsIn(loc, includeMounted: false))
            .FirstOrDefault();

        // create a tractor if needed
        if (tractor == null && this.Config.CanSummonWithoutGarage && Context.IsMainPlayer)
        {
            tractor = new Horse(Guid.NewGuid(), 0, 0);
            TractorManager.SetTractorInfo(tractor, this.Config.SoundEffects);
            this.TextureManager.ApplyTextures(tractor, this.IsTractor);
        }

        // warp to player
        if (tractor != null)
        {
            TractorManager.SetLocation(tractor, location, tile);
            return true;
        }
        return false;
    }

    /// <summary>Send a tractor back home.</summary>
    /// <param name="tractor">The tractor to dismiss.</param>
    private void DismissTractor(Horse? tractor)
    {
        if (tractor == null || !this.IsTractor(tractor))
            return;

        // dismount
        if (tractor.rider != null)
            tractor.dismount();

        // get home position (garage may have been moved since the tractor was spawned)
        Farm location = Game1.getFarm();
        Stable? garage = location.buildings.OfType<Stable>().FirstOrDefault(p => p.HorseId == tractor.HorseId);
        Vector2 tile = garage != null
            ? this.GetDefaultTractorTile(garage)
            : tractor.DefaultPosition;

        // warp home
        TractorManager.SetLocation(tractor, location, tile);
    }

    /// <summary>Temporarily increase or reduce the tractor tool distance by the given amount, and highlight the resulting radius.</summary>
    /// <param name="offset">The amount to add to the distance.</param>
    private void OffsetTemporaryDistance(int offset)
    {
        // get current distance
        int distance = this.TemporaryDistance.Value;
        if (distance < 0)
            distance = this.Config.Distance;

        // apply offset
        distance = MathHelper.Clamp(distance + offset, 1, ModConstants.MaxRecommendedDistance);
        if (distance == this.Config.Distance)
            distance = -1;

        // update
        this.TemporaryDistance.Value = distance;
        this.HighlightDistanceAlpha.Value = ModConstants.HighlightDistanceTicks * ModConstants.HighlightDistanceFade;
    }

    /// <summary>Get the tool effect distance to apply.</summary>
    private int GetDistance()
    {
        return this.TemporaryDistance.Value > -1
            ? this.TemporaryDistance.Value
            : this.Config.Distance;
    }

    /// <summary>Get all available locations.</summary>
    private IEnumerable<GameLocation> GetLocations()
    {
        GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

        foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels))
        {
            yield return location;

            foreach (GameLocation indoors in location.GetInstancedBuildingInteriors())
                yield return indoors;
        }
    }

    /// <summary>Get all tractors in the given location.</summary>
    /// <param name="location">The location to scan.</param>
    /// <param name="includeMounted">Whether to include horses that are currently being ridden.</param>
    private IEnumerable<Horse> GetTractorsIn(GameLocation location, bool includeMounted = true)
    {
        // single-player
        if (!Context.IsMultiplayer || !includeMounted)
            return location.characters.OfType<Horse>().Where(this.IsTractor);

        // multiplayer
        return
            location.characters.OfType<Horse>().Where(this.IsTractor)
                .Concat(
                    from player in location.farmers
                    where this.IsTractor(player.mount)
                    select player.mount
                )
                .Distinct(new ObjectReferenceComparer<Horse>());
    }

    /// <summary>Get all tractor garages in the given location.</summary>
    /// <param name="location">The location to scan.</param>
    private IEnumerable<Stable> GetGaragesIn(GameLocation location)
    {
        return location.buildings
            .OfType<Stable>()
            .Where(this.IsGarage);
    }

    /// <summary>Find all horses with a given ID.</summary>
    /// <param name="id">The unique horse ID.</param>
    private Horse? FindHorse(Guid id)
    {
        foreach (GameLocation location in this.GetLocations())
        {
            foreach (Horse horse in location.characters.OfType<Horse>())
            {
                if (horse.HorseId == id)
                    return horse;
            }
        }

        return null;
    }

    /// <summary>Get whether a stable is a tractor garage.</summary>
    /// <param name="stable">The stable to check.</param>
    private bool IsGarage([NotNullWhen(true)] Stable? stable)
    {
        return stable?.buildingType.Value == this.GarageBuildingId;
    }

    /// <summary>Get whether a horse is a tractor.</summary>
    /// <param name="horse">The horse to check.</param>
    private bool IsTractor([NotNullWhen(true)] Horse? horse)
    {
        return TractorManager.IsTractor(horse);
    }

    /// <summary>Get the default tractor tile position in a garage.</summary>
    /// <param name="garage">The tractor's home garage.</param>
    private Vector2 GetDefaultTractorTile(Stable garage)
    {
        return new Vector2(garage.tileX.Value + 1, garage.tileY.Value + 1);
    }
}
