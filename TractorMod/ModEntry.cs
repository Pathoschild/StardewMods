using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.FarmExpansion;
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
using StardewValley.Locations;
using StardewValley.Menus;

namespace Pathoschild.Stardew.TractorMod
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /****
        ** Constants
        ****/
        /// <summary>The <see cref="Building.maxOccupants"/> value which identifies a tractor garage.</summary>
        private readonly int MaxOccupantsID = -794739;

        /// <summary>The update rate when only one player is in a location (as a frame multiple).</summary>
        private readonly uint TextureUpdateRateWithSinglePlayer = 30;

        /// <summary>The update rate when multiple players are in the same location (as a frame multiple). This should be more frequent due to sprite broadcasts, new horses instances being created during NetRef&lt;Horse&gt; syncs, etc.</summary>
        private readonly uint TextureUpdateRateWithMultiplePlayers = 3;

        /// <summary>The full type name for the Farm Expansion's construction menu.</summary>
        private readonly string FarmExpansionMenuFullName = "FarmExpansion.Menus.FECarpenterMenu";

        /// <summary>The full type name for the Pelican Fiber mod's construction menu.</summary>
        private readonly string PelicanFiberMenuFullName = "PelicanFiber.Framework.ConstructionMenu";

        /// <summary>The building type for the garage blueprint.</summary>
        private readonly string BlueprintBuildingType = "TractorGarage";

        /// <summary>The minimum version the host must have for the mod to be enabled on a farmhand.</summary>
        private readonly string MinHostVersion = "4.7.0";

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

        /// <summary>Manages textures loaded for the tractor and garage.</summary>
        private TextureManager TextureManager = null!; // set in Entry

        /// <summary>The backing field for <see cref="TractorManager"/>.</summary>
        private PerScreen<TractorManager> TractorManagerImpl = null!; // set in Entry

        /// <summary>The tractor being ridden by the current player.</summary>
        private TractorManager TractorManager => this.TractorManagerImpl.Value;

        /// <summary>Whether the mod is enabled for the current farmhand.</summary>
        private bool IsEnabled = true;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

            // init
            I18n.Init(helper.Translation);
            this.TextureManager = new(
                directoryPath: this.Helper.DirectoryPath,
                publicAssetBasePath: this.PublicAssetBasePath,
                contentHelper: helper.ModContent,
                monitor: this.Monitor
            );
            this.TractorManagerImpl = new(() =>
            {
                var manager = new TractorManager(this.Config, this.Keys, this.Helper.Reflection);
                this.UpdateConfigFor(manager);
                return manager;
            });
            this.UpdateConfig();

            // hook events
            IModEvents events = helper.Events;
            events.Content.AssetRequested += this.OnAssetRequested;
            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.GameLoop.DayStarted += this.OnDayStarted;
            events.GameLoop.DayEnding += this.OnDayEnding;
            events.GameLoop.Saved += this.OnSaved;
            events.Display.RenderedWorld += this.OnRenderedWorld;
            events.Display.MenuChanged += this.OnMenuChanged;
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
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // add to Farm Expansion carpenter menu
            FarmExpansionIntegration farmExpansion = new FarmExpansionIntegration(this.Helper.ModRegistry, this.Monitor);
            if (farmExpansion.IsLoaded)
            {
                farmExpansion.AddFarmBluePrint(this.GetBlueprint());
                farmExpansion.AddExpansionBluePrint(this.GetBlueprint());
            }

            // add Generic Mod Config Menu integration
            new GenericModConfigMenuIntegrationForTractor(
                getConfig: () => this.Config,
                reset: () =>
                {
                    this.Config = new ModConfig();
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                saveAndApply: () =>
                {
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Register();

            // warn about incompatible mods
            if (this.Helper.ModRegistry.IsLoaded("bcmpinc.HarvestWithScythe"))
                this.Monitor.Log("The 'Harvest With Scythe' mod is compatible with Tractor Mod, but it may break some tractor scythe features. You can ignore this warning if you don't have any scythe issues.", LogLevel.Warn);
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // load legacy data
            Migrator.AfterLoad(this.Helper, this.Monitor, this.ModManifest.Version, this.GetBlueprint);

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
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // reload textures
            this.TextureManager.UpdateTextures();

            // init garages + tractors
            if (Context.IsMainPlayer)
            {
                foreach (BuildableGameLocation location in this.GetBuildableLocations())
                {
                    foreach (Stable garage in this.GetGaragesIn(location))
                    {
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
                            TractorManager.SetTractorInfo(tractor);

                        // normalize ownership
                        garage.owner.Value = 0;
                        if (tractor != null)
                            tractor.ownerId.Value = 0;

                        // apply textures
                        this.TextureManager.ApplyTextures(garage, this.IsGarage);
                        this.TextureManager.ApplyTextures(tractor, this.IsTractor);
                    }
                }
            }
        }

        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            this.TextureManager.OnAssetRequested(e);
        }

        /// <inheritdoc cref="IWorldEvents.LocationListChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
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

        /// <inheritdoc cref="IWorldEvents.NpcListChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnNpcListChanged(object? sender, NpcListChangedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // workaround for instantly-built tractors spawning a horse
            if (Context.IsMainPlayer && e.Location is BuildableGameLocation buildableLocation)
            {
                Horse[] horses = e.Added.OfType<Horse>().ToArray();
                if (horses.Any())
                {
                    HashSet<Guid> tractorIDs = new HashSet<Guid>(this.GetGaragesIn(buildableLocation).Select(p => p.HorseId));
                    foreach (Horse horse in horses)
                    {
                        if (tractorIDs.Contains(horse.HorseId) && !TractorManager.IsTractor(horse))
                            TractorManager.SetTractorInfo(horse);
                    }
                }
            }
        }

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
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

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
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
                    foreach (Stable stable in this.GetGaragesIn(Game1.currentLocation))
                        this.TextureManager.ApplyTextures(stable, this.IsGarage);
                }
            }

            // override blueprint texture
            if (Game1.activeClickableMenu != null)
            {
                IClickableMenu menu = Game1.activeClickableMenu;
                bool isFarmExpansion = menu.GetType().FullName == this.FarmExpansionMenuFullName;
                bool isPelicanFiber = !isFarmExpansion && menu.GetType().FullName == this.PelicanFiberMenuFullName;

                this.TextureManager.ApplyTextures(
                    menu: menu,
                    isFarmExpansion: isFarmExpansion,
                    isPelicanFiber: isPelicanFiber,
                    isGarage: blueprint => blueprint.maxOccupants == this.MaxOccupantsID,
                    reflection: this.Helper.Reflection
                );
            }

            // update tractor effects
            if (Context.IsPlayerFree)
                this.TractorManager.Update();
        }

        /// <inheritdoc cref="IGameLoopEvents.DayEnding"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            if (Context.IsMainPlayer)
            {
                // collect valid stable IDs
                HashSet<Guid> validStableIDs = new HashSet<Guid>(
                    from location in this.GetBuildableLocations()
                    from garage in this.GetGaragesIn(location)
                    select garage.HorseId
                );

                // get locations reachable by Utility.findHorse
                HashSet<GameLocation> vanillaLocations = new HashSet<GameLocation>(Game1.locations, new ObjectReferenceComparer<GameLocation>());

                // clean up
                foreach (GameLocation location in this.GetLocations())
                {
                    bool isValidLocation = vanillaLocations.Contains(location);

                    foreach (Horse tractor in this.GetTractorsIn(location).ToArray())
                    {
                        // remove invalid tractor (e.g. building demolished)
                        if (!validStableIDs.Contains(tractor.HorseId))
                        {
                            location.characters.Remove(tractor);
                            continue;
                        }

                        // move tractor out of location that Utility.findHorse can't find
                        if (!isValidLocation)
                            Game1.warpCharacter(tractor, "Farm", new Point(0, 0));
                    }
                }
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.Saved"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaved(object? sender, SavedEventArgs e)
        {
            Migrator.AfterSave();
        }

        /// <inheritdoc cref="IDisplayEvents.RenderedWorld"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // render debug radius
            if (this.Config.HighlightRadius && Context.IsWorldReady && Game1.activeClickableMenu == null && this.TractorManager.IsCurrentPlayerRiding)
                this.TractorManager.DrawRadius(Game1.spriteBatch);
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (!this.IsEnabled || !Context.IsWorldReady)
                return;

            // add blueprints
            if (e.NewMenu is CarpenterMenu || e.NewMenu?.GetType().FullName == this.PelicanFiberMenuFullName)
            {
                // get field
                IList<BluePrint> blueprints = this.Helper.Reflection
                    .GetField<List<BluePrint>>(e.NewMenu, "blueprints")
                    .GetValue();

                // add garage blueprint
                blueprints.Add(this.GetBlueprint());

                // add stable blueprint if needed
                // (If player built a tractor garage first, the game won't let them build a stable since it thinks they already have one. Derived from the CarpenterMenu constructor.)
                if (!blueprints.Any(p => p.name == "Stable" && p.maxOccupants != this.MaxOccupantsID))
                {
                    Farm farm = Game1.getFarm();

                    int cabins = farm.getNumberBuildingsConstructed("Cabin");
                    int stables = farm.getNumberBuildingsConstructed("Stable") - Game1.getFarm().buildings.OfType<Stable>().Count(this.IsGarage);
                    if (stables < cabins + 1)
                        blueprints.Add(new BluePrint("Stable"));
                }
            }
        }

        /// <inheritdoc cref="IInputEvents.ButtonsChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!this.IsEnabled || !Context.IsPlayerFree)
                return;

            if (this.Keys.SummonTractor.JustPressed() && !Game1.player.isRidingHorse())
                this.SummonTractor();
            else if (this.Keys.DismissTractor.JustPressed() && Game1.player.isRidingHorse())
                this.DismissTractor(Game1.player.mount);
        }

        /// <inheritdoc cref="IMultiplayerEvents.ModMessageReceived"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            // tractor request from a farmhand
            if (e.Type == this.RequestTractorMessageID && Context.IsMainPlayer && e.FromModID == this.ModManifest.UniqueID)
            {
                Farmer player = Game1.getFarmer(e.FromPlayerID);
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
            foreach (var pair in this.TractorManagerImpl.GetActiveValues())
                this.UpdateConfigFor(pair.Value);
        }

        /// <summary>Apply the mod configuration to a tractor manager instance.</summary>
        /// <param name="manager">The tractor manager to update.</param>
        private void UpdateConfigFor(TractorManager manager)
        {
            var modRegistry = this.Helper.ModRegistry;
            var reflection = this.Helper.Reflection;
            var toolConfig = this.Config.StandardAttachments;

            manager.UpdateConfig(this.Config, this.Keys, new IAttachment?[]
            {
                new CustomAttachment(this.Config.CustomAttachments, modRegistry, reflection), // should be first so it can override default attachments
                new AxeAttachment(toolConfig.Axe, modRegistry, reflection),
                new FertilizerAttachment(toolConfig.Fertilizer, modRegistry, reflection),
                new GrassStarterAttachment(toolConfig.GrassStarter, modRegistry, reflection),
                new HoeAttachment(toolConfig.Hoe, modRegistry, reflection),
                new MeleeBluntAttachment(toolConfig.MeleeBlunt, modRegistry, reflection),
                new MeleeDaggerAttachment(toolConfig.MeleeDagger, modRegistry, reflection),
                new MeleeSwordAttachment(toolConfig.MeleeSword, modRegistry, reflection),
                new MilkPailAttachment(toolConfig.MilkPail, modRegistry, reflection),
                new PickaxeAttachment(toolConfig.PickAxe, modRegistry, reflection),
                new ScytheAttachment(toolConfig.Scythe, modRegistry, reflection),
                new SeedAttachment(toolConfig.Seeds, modRegistry, reflection),
                modRegistry.IsLoaded(SeedBagAttachment.ModId) ? new SeedBagAttachment(toolConfig.SeedBagMod, modRegistry, reflection) : null,
                new ShearsAttachment(toolConfig.Shears, modRegistry, reflection),
                new SlingshotAttachment(toolConfig.Slingshot, modRegistry, reflection),
                new WateringCanAttachment(toolConfig.WateringCan, modRegistry, reflection)
            });
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
                    modIDs: new[] { this.ModManifest.UniqueID },
                    playerIDs: new[] { Game1.MasterPlayer.UniqueMultiplayerID }
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
            Vector2 tile = player.getTileLocation();

            // find nearest tractor in player's current location (if available), else any location
            Horse? tractor = this
                .GetTractorsIn(location, includeMounted: false)
                .OrderBy(match => Utility.distance(tile.X, tile.Y, match.getTileX(), match.getTileY()))
                .FirstOrDefault();
            tractor ??= this
                .GetLocations()
                .SelectMany(loc => this.GetTractorsIn(loc, includeMounted: false))
                .FirstOrDefault();

            // create a tractor if needed
            if (tractor == null && this.Config.CanSummonWithoutGarage && Context.IsMainPlayer)
            {
                tractor = new Horse(Guid.NewGuid(), 0, 0);
                TractorManager.SetTractorInfo(tractor);
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

        /// <summary>Get all available locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels))
            {
                yield return location;

                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        if (building.indoors.Value != null)
                            yield return building.indoors.Value;
                    }
                }
            }
        }

        /// <summary>Get all available buildable locations.</summary>
        private IEnumerable<BuildableGameLocation> GetBuildableLocations()
        {
            return this.GetLocations().OfType<BuildableGameLocation>();
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
            return location is BuildableGameLocation buildableLocation
                ? buildableLocation.buildings.OfType<Stable>().Where(this.IsGarage)
                : Enumerable.Empty<Stable>();
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
            return
                stable != null
                && (
                    stable.maxOccupants.Value == this.MaxOccupantsID
                    || stable.buildingType.Value == this.BlueprintBuildingType // freshly constructed, not yet normalized
                );
        }

        /// <summary>Get whether a horse is a tractor.</summary>
        /// <param name="horse">The horse to check.</param>
        private bool IsTractor([NotNullWhen(true)] Horse? horse)
        {
            return TractorManager.IsTractor(horse);
        }

        /// <summary>Get a blueprint to construct the tractor garage.</summary>
        private BluePrint GetBlueprint()
        {
            return new BluePrint("Stable")
            {
                displayName = I18n.Garage_Name(),
                description = I18n.Garage_Description(),
                maxOccupants = this.MaxOccupantsID,
                moneyRequired = this.Config.BuildPrice,
                tilesWidth = 4,
                tilesHeight = 2,
                sourceRectForMenuView = new Rectangle(0, 0, 64, 96),
                itemsRequired = this.Config.BuildMaterials
            };
        }

        /// <summary>Get the default tractor tile position in a garage.</summary>
        /// <param name="garage">The tractor's home garage.</param>
        private Vector2 GetDefaultTractorTile(Stable garage)
        {
            return new Vector2(garage.tileX.Value + 1, garage.tileY.Value + 1);
        }
    }
}
