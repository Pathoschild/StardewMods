using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.FarmExpansion;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.TractorMod.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod, IAssetLoader
    {
        /*********
        ** Properties
        *********/
        /****
        ** Constants
        ****/
        /// <summary>The <see cref="Building.maxOccupants"/> value which identifies a tractor garage.</summary>
        private readonly int MaxOccupantsID = -794739;

        /// <summary>The full type name for the Pelican Fiber mod's construction menu.</summary>
        private readonly string PelicanFiberMenuFullName = "PelicanFiber.Framework.ConstructionMenu";

        /// <summary>The number of days needed to build a tractor garage.</summary>
        private readonly int GarageConstructionDays = 3;

        /****
        ** State
        ****/
        /// <summary>The mod settings.</summary>
        private ModConfig Config;

        /// <summary>The tractor being ridden by the current player.</summary>
        private TractorManager TractorManager;

        /// <summary>The garage texture to apply.</summary>
        private Texture2D GarageTexture;

        /// <summary>The tractor texture to apply.</summary>
        private Texture2D TractorTexture;

        /// <summary>Whether the mod is enabled for the current farmhand.</summary>
        private bool IsEnabled = true;

        /// <summary>The mounted players in the player's current location, used to determine whether tractor textures need to be reapplied.</summary>
        private readonly HashSet<long> MountedPlayersInCurrentLocation = new HashSet<long>();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

            // init tractor logic
            StandardAttachmentsConfig attachmentConfig = this.Config.StandardAttachments;
            this.TractorManager = new TractorManager(this.Config, this.Helper.Translation, this.Helper.Reflection, attachments: new IAttachment[]
            {
                new CustomAttachment(this.Config.CustomAttachments), // should be first so it can override default attachments
                new AxeAttachment(attachmentConfig.Axe),
                new FertilizerAttachment(attachmentConfig.Fertilizer),
                new GrassStarterAttachment(attachmentConfig.GrassStarter),
                new HoeAttachment(attachmentConfig.Hoe),
                new MeleeWeaponAttachment(attachmentConfig.MeleeWeapon),
                new PickaxeAttachment(attachmentConfig.PickAxe),
                new ScytheAttachment(attachmentConfig.Scythe),
                new SeedAttachment(attachmentConfig.Seeds),
                new SeedBagAttachment(attachmentConfig.SeedBagMod),
                new SlingshotAttachment(attachmentConfig.Slingshot, this.Helper.Reflection),
                new WateringCanAttachment(attachmentConfig.WateringCan)
            });

            // hook events
            IModEvents events = helper.Events;
            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.GameLoop.DayStarted += this.OnDayStarted;
            events.GameLoop.DayEnding += this.OnDayEnding;
            events.GameLoop.Saving += this.OnSaving;
            if (this.Config.HighlightRadius)
                events.Display.Rendered += this.OnRendered;
            events.Display.MenuChanged += this.OnMenuChanged;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            events.World.NpcListChanged += this.OnNpcListChanged;
            events.World.LocationListChanged += this.OnLocationListChanged;
            events.World.BuildingListChanged += this.OnBuildingListChanged;
            events.Player.Warped += this.OnWarped;

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            // allow loading legacy data that might have been stored in the save file
            return asset.AssetNameEquals("Buildings/TractorGarage");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)this.GarageTexture;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The event called after the first game update, once all mods are loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // add to Farm Expansion carpenter menu
            FarmExpansionIntegration farmExpansion = new FarmExpansionIntegration(this.Helper.ModRegistry, this.Monitor);
            if (farmExpansion.IsLoaded)
            {
                farmExpansion.AddFarmBluePrint(this.GetBlueprint());
                farmExpansion.AddExpansionBluePrint(this.GetBlueprint());
            }
        }

        /// <summary>The event called after a save slot is loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // load legacy data
            if (Context.IsMainPlayer)
                this.LoadLegacyData();

            // check if mod should be enabled for the current player
            this.IsEnabled = true;
            if (!Context.IsMainPlayer)
            {
                IMultiplayerPeer host = this.Helper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID);
                this.IsEnabled = host?.GetMod(this.ModManifest.UniqueID) != null;
                if (!this.IsEnabled)
                    this.Monitor.Log("This mod is currently disabled because the host player doesn't have it installed.", LogLevel.Warn);
            }
        }

        /// <summary>The event called when a new day begins.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // reload textures
            this.TractorTexture = this.Helper.Content.Load<Texture2D>(this.GetTextureKey("tractor"));
            this.GarageTexture = this.Helper.Content.Load<Texture2D>(this.GetTextureKey("garage"));

            // host: apply changes in all locations
            if (Context.IsMainPlayer)
            {
                foreach (GameLocation location in this.GetLocations())
                    this.ApplyChanges(location);
            }
        }

        /// <summary>The event called when a building is added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            // host: apply changes to new locations
            if (Context.IsMainPlayer)
            {
                foreach (GameLocation location in e.Added)
                    this.ApplyChanges(location);
            }
        }

        /// <summary>The event called when a building is added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            // host (or farmhand in affected location): apply changes to new buildings
            if (Context.IsMainPlayer || (this.IsEnabled && object.ReferenceEquals(e.Location, Game1.player.currentLocation)))
            {
                foreach (Stable stable in e.Added.OfType<Stable>())
                    this.ApplyChanges(stable);
            }
        }

        /// <summary>The event called when the player warps to a new area.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnNpcListChanged(object sender, NpcListChangedEventArgs e)
        {
            // host (or farmhand in affected location): apply changes to new horses
            if (Context.IsMainPlayer || (this.IsEnabled && object.ReferenceEquals(e.Location, Game1.player.currentLocation)))
            {
                foreach (Horse horse in e.Added.OfType<Horse>())
                    this.ApplyChanges(horse);
            }
        }

        /// <summary>The event called when the player warps to a new area.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            this.MountedPlayersInCurrentLocation.Clear();

            // farmhand: apply changes to new location
            if (!Context.IsMainPlayer && this.IsEnabled)
                this.ApplyChanges(e.NewLocation);
        }

        /// <summary>The event called before the day ends.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            // host: move any tractors out of locations that Utility.findHorse can't find
            if (Context.IsMainPlayer)
            {
                HashSet<GameLocation> validLocations = new HashSet<GameLocation>(Game1.locations, new ObjectReferenceComparer<GameLocation>());
                foreach (GameLocation location in this.GetLocations())
                {
                    if (validLocations.Contains(location))
                        continue;

                    foreach (Horse horse in location.characters.OfType<Horse>().ToArray())
                    {
                        if (TractorManager.IsTractor(horse))
                            Game1.warpCharacter(horse, "Farm", new Point(0, 0));
                    }
                }
            }
        }

        /// <summary>The event called before the game starts saving.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            // host: remove legacy data
            if (Context.IsMainPlayer)
            {
                // remove legacy file (pre-4.6)
                FileInfo legacyFile = new FileInfo($"data/{Constants.SaveFolderName}.json");
                if (legacyFile.Exists)
                    legacyFile.Delete();

                // remove legacy save data (4.6)
                this.Helper.Data.WriteSaveData<LegacySaveData>("tractors", null);
            }
        }

        /// <summary>The event called after the game draws to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            // render debug radius
            if (this.IsEnabled)
            {
                if (this.Config.HighlightRadius && Context.IsWorldReady && Game1.activeClickableMenu == null && this.TractorManager?.IsCurrentPlayerRiding == true)
                    this.TractorManager.DrawRadius(Game1.spriteBatch);
            }
        }

        /// <summary>The event called after an active menu is opened or closed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // add blueprint to carpenter menu
            if (this.IsEnabled && Context.IsWorldReady && e.NewMenu != null)
            {
                // default menu
                if (e.NewMenu is CarpenterMenu)
                {
                    this.Helper.Reflection
                        .GetField<List<BluePrint>>(e.NewMenu, "blueprints")
                        .GetValue()
                        .Add(this.GetBlueprint());
                }

                // modded menus
                else if (e.NewMenu.GetType().FullName == this.PelicanFiberMenuFullName)
                {
                    this.Helper.Reflection
                        .GetField<List<BluePrint>>(e.NewMenu, "Blueprints")
                        .GetValue()
                        .Add(this.GetBlueprint());
                }
            }
        }

        /// <summary>The event called when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // summon available tractor
            if (this.IsEnabled && Context.IsPlayerFree && this.Config.Controls.SummonTractor.Contains(e.Button) && !Game1.player.isRidingHorse())
                this.SummonTractor();
        }

        /// <summary>The event called when the game updates (roughly sixty times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // In multiplayer, mounting a horse causes other players to construct a new horse instance for the player.mount field.
            // Track mounted players in the current location, and reapply the texture change when this happens.
            if (this.IsEnabled && Context.IsWorldReady && Context.IsMultiplayer)
            {
                foreach (Farmer player in Game1.currentLocation.farmers.Where(p => p.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID))
                {
                    // reapply on mount
                    if (player.mount != null)
                    {
                        if (this.MountedPlayersInCurrentLocation.Add(player.UniqueMultiplayerID))
                            this.ApplyChanges(player.mount);
                    }

                    // untrack on unmount (texture will be reapplied in OnNpcListChanged)
                    else
                        this.MountedPlayersInCurrentLocation.Remove(player.UniqueMultiplayerID);
                }
            }

            // update effects
            if (this.IsEnabled && Context.IsPlayerFree)
                this.TractorManager?.Update();
        }

        /****
        ** State methods
        ****/
        /// <summary>Apply mod changes to a location.</summary>
        /// <param name="location">The location whose buildings to check.</param>
        private void ApplyChanges(GameLocation location)
        {
            // track stables + horses
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Stable stable in buildableLocation.buildings.OfType<Stable>())
                    this.ApplyChanges(stable);
            }

            // update untracked horses if farmhand
            // This is needed because farmhands may not have access to the horse initially if it's in a different location than the stable.
            if (!Context.IsMainPlayer)
            {
                foreach (Horse horse in location.characters.OfType<Horse>())
                    this.ApplyChanges(horse);
            }
        }

        /// <summary>Apply mod changes to a building, if applicable.</summary>
        /// <param name="stable">The stable to change.</param>
        private void ApplyChanges(Stable stable)
        {
            // ignore non-tractor stable
            if (stable.maxOccupants.Value != this.MaxOccupantsID)
                return;

            // apply building
            stable.texture = new Lazy<Texture2D>(() => this.GarageTexture);
            if (!stable.isUnderConstruction())
            {
                Horse horse = this.FindHorse(stable.HorseId);
                this.ApplyChanges(horse, force: true);
            }
        }

        /// <summary>Apply changes to a horse, if applicable.</summary>
        /// <param name="horse">The horse to change.</param>
        /// <param name="force">Whether changes should be applied even if this doesn't look like a tractor.</param>
        private void ApplyChanges(Horse horse, bool force = false)
        {
            if (horse != null && (force || TractorManager.IsTractor(horse)))
            {
                horse.Name = TractorManager.GetTractorName(horse.HorseId);
                this.Helper.Reflection.GetField<Texture2D>(horse.Sprite, "spriteTexture").SetValue(this.TractorTexture);
            }
        }

        /// <summary>Summon an unused tractor to the player's current position, if any are available.</summary>
        private void SummonTractor()
        {
            // get nearest horse in player's current location
            {
                Horse horse =
                    (
                        from match in Game1.currentLocation.characters.OfType<Horse>()
                        where TractorManager.IsTractor(match) && match.rider == null
                        orderby Utility.distance(Game1.player.getTileX(), Game1.player.getTileY(), match.getTileX(), match.getTileY())
                        select match
                    )
                    .FirstOrDefault();
                if (horse != null)
                {
                    TractorManager.SetLocation(horse, Game1.currentLocation, Game1.player.getTileLocation());
                    return;
                }
            }

            // get horse from any accessible location
            foreach (GameLocation location in this.GetLocations())
            {
                foreach (Horse horse in location.characters.OfType<Horse>())
                {
                    if (TractorManager.IsTractor(horse) && horse.rider == null)
                    {
                        TractorManager.SetLocation(horse, Game1.currentLocation, Game1.player.getTileLocation());
                        return;
                    }
                }
            }
        }

        /****
        ** Save methods
        ****/
        /// <summary>Migrate tractors and garages from older versions of the mod.</summary>
        /// <remarks>The Robin construction logic is derived from <see cref="NPC.reloadSprite"/> and <see cref="Farm.resetForPlayerEntry"/>.</remarks>
        private void LoadLegacyData()
        {
            // get save data
            LegacySaveData saveData = this.Helper.Data.ReadSaveData<LegacySaveData>("tractors");
            if (saveData?.Buildings == null)
                saveData = this.Helper.Data.ReadJsonFile<LegacySaveData>($"data/{Constants.SaveFolderName}.json"); // pre-4.6 data
            if (saveData?.Buildings == null)
                return;

            // add tractor + garages
            BuildableGameLocation[] locations = CommonHelper.GetLocations().OfType<BuildableGameLocation>().ToArray();
            foreach (LegacySaveDataBuilding garageData in saveData.Buildings)
            {
                // get location
                BuildableGameLocation location = locations.FirstOrDefault(p => this.GetMapName(p) == (garageData.Map ?? "Farm"));
                if (location == null)
                {
                    this.Monitor.Log($"Ignored tractor garage in unknown location '{garageData.Map}'.");
                    continue;
                }

                // add garage
                Stable garage = location.buildings.OfType<Stable>().FirstOrDefault(p => p.tileX.Value == (int)garageData.Tile.X && p.tileY.Value == (int)garageData.Tile.Y);
                if (garage == null)
                {
                    garage = new Stable(garageData.TractorID, this.GetBlueprint(), garageData.Tile);
                    garage.daysOfConstructionLeft.Value = 0;
                    location.buildings.Add(garage);
                }
                garage.maxOccupants.Value = this.MaxOccupantsID;
                garage.load();
            }

            // fix building types
            foreach (BuildableGameLocation location in CommonHelper.GetLocations().OfType<BuildableGameLocation>())
            {
                foreach (Stable stable in location.buildings.OfType<Stable>())
                {
                    if (stable.buildingType.Value == "TractorGarage")
                    {
                        stable.buildingType.Value = "Stable";
                        stable.maxOccupants.Value = this.MaxOccupantsID;
                    }
                }
            }
        }

        /****
        ** Helper methods
        ****/
        /// <summary>Get all available locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations)
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

        /// <summary>Find all horses with a given ID.</summary>
        /// <param name="id">The unique horse ID.</param>
        private Horse FindHorse(Guid id)
        {
            foreach (GameLocation location in this.GetLocations())
            {
                foreach (NPC npc in location.characters)
                {
                    if (npc is Horse horse && horse.HorseId == id)
                        return horse;
                }
            }

            return null;
        }

        /// <summary>Get a blueprint to construct the tractor garage.</summary>
        private BluePrint GetBlueprint()
        {
            BluePrint blueprint = new BluePrint("Stable")
            {
                humanDoor = new Point(-1, -1),
                animalDoor = new Point(-2, -1),
                mapToWarpTo = null,
                displayName = this.Helper.Translation.Get("garage.name"),
                description = this.Helper.Translation.Get("garage.description"),
                blueprintType = "Buildings",
                daysToConstruct = this.GarageConstructionDays,
                nameOfBuildingToUpgrade = null,
                actionBehavior = null,
                maxOccupants = this.MaxOccupantsID,
                moneyRequired = this.Config.BuildPrice,
                tilesWidth = 4,
                tilesHeight = 2,
                sourceRectForMenuView = new Rectangle(0, 0, 64, 96),
                itemsRequired = this.Config.BuildUsesResources
                    ? new Dictionary<int, int> { [SObject.ironBar] = 20, [SObject.iridiumBar] = 5, [787/* battery pack */] = 5 }
                    : new Dictionary<int, int>()
            };

            this.Helper.Reflection.GetField<Texture2D>(blueprint, nameof(BluePrint.texture)).SetValue(this.GarageTexture);

            return blueprint;
        }

        /// <summary>Get the asset key for a texture from the assets folder (including seasonal logic if applicable).</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'tractor' or 'garage').</param>
        private string GetTextureKey(string spritesheet)
        {
            // try seasonal texture
            string seasonalKey = $"assets/{Game1.currentSeason}_{spritesheet}.png";
            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, seasonalKey)))
                return seasonalKey;

            // default to single texture
            return $"assets/{spritesheet}.png";
        }

        /// <summary>Get a unique map name for the given location.</summary>
        private string GetMapName(GameLocation location)
        {
            string uniqueName = location.uniqueName.Value;
            return uniqueName ?? location.Name;
        }
    }
}
