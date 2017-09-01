using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Manages a spawned tractor.</summary>
    internal class TractorManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>The unique buff ID for the tractor speed.</summary>
        private readonly int BuffUniqueID = 58012397;

        /// <summary>The number of ticks between each tractor action check.</summary>
        private readonly int TicksPerAction = 12; // roughly five times per second

        /// <summary>Provides translations from the mod's i18n folder.</summary>
        private readonly ITranslationHelper Translation;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The tractor attachments to apply.</summary>
        private readonly IAttachment[] Attachments;

        /// <summary>The mod settings.</summary>
        private readonly ModConfig Config;

        /// <summary>The number of ticks since the tractor last checked for an action to perform.</summary>
        private int SkippedActionTicks;


        /*********
        ** Accessors
        *********/
        /// <summary>A static tractor NPC for display in the world.</summary>
        /// <remarks>This is deliberately separate to avoid conflicting with logic for summoning or managing the player horse.</remarks>
        public TractorStatic Static { get; }

        /// <summary>A tractor horse for riding.</summary>
        public TractorMount Mount { get; }

        /// <summary>The currently active tractor instance.</summary>
        public NPC Current => this.IsRiding ? (NPC)this.Mount : this.Static;

        /// <summary>Whether the player is currently riding the tractor.</summary>
        public bool IsRiding => this.Mount?.rider == Game1.player;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tileX">The initial tile X position.</param>
        /// <param name="tileY">The initial tile Y position.</param>
        /// <param name="config">The mod settings.</param>
        /// <param name="attachments">The tractor attachments to apply.</param>
        /// <param name="content">The content helper with which to load the tractor sprite.</param>
        /// <param name="translation">Provides translations from the mod's i18n folder.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public TractorManager(int tileX, int tileY, ModConfig config, IEnumerable<IAttachment> attachments, IContentHelper content, ITranslationHelper translation, IReflectionHelper reflection)
        {
            AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(@"assets\tractor.png"), 0, 32, 32)
            {
                textureUsesFlippedRightForLeft = true,
                loop = true
            };

            this.Static = new TractorStatic(typeof(TractorStatic).Name, tileX, tileY, sprite, () => this.SetMounted(true));
            this.Mount = new TractorMount(typeof(TractorMount).Name, tileX, tileY, sprite, () => this.SetMounted(false));
            this.Config = config;
            this.Attachments = attachments.ToArray();
            this.Translation = translation;
            this.Reflection = reflection;
        }

        /// <summary>Move the tractor to the given location.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="tile">The tile coordinate in the given location.</param>
        public void SetLocation(GameLocation location, Vector2 tile)
        {
            Game1.warpCharacter(this.Current, location.name, tile, false, true);
        }

        /// <summary>Move the tractor to a specific pixel position within its current location.</summary>
        /// <param name="position">The pixel coordinate in the current location.</param>
        public void SetPixelPosition(Vector2 position)
        {
            this.Current.Position = position;
        }

        /// <summary>Remove all tractors from the game.</summary>
        public void RemoveTractors()
        {
            // find all locations
            IEnumerable<GameLocation> locations = Game1.locations
                .Union(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors != null
                    select building.indoors
                );

            // remove tractors
            foreach (GameLocation location in locations)
                location.characters.RemoveAll(p => p is TractorStatic || p is TractorMount);
        }

        /// <summary>Update tractor effects and actions in the game.</summary>
        public void Update()
        {
            if (!this.IsRiding || Game1.activeClickableMenu != null)
                return; // tractor isn't enabled

            // apply tractor speed buff
            Buff buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == this.BuffUniqueID);
            if (buff == null)
            {
                buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, this.Config.MagneticRadius, this.Config.TractorSpeed, 0, 0, 1, "Tractor Power", this.Translation.Get("buff.name")) { which = this.BuffUniqueID };
                Game1.buffsDisplay.addOtherBuff(buff);
            }
            buff.millisecondsDuration = 100;

            // apply action cooldown
            this.SkippedActionTicks++;
            if (this.SkippedActionTicks % this.TicksPerAction != 0)
                return;
            this.SkippedActionTicks = 0;

            // get context
            SFarmer player = Game1.player;
            GameLocation location = Game1.currentLocation;
            Tool tool = player.CurrentTool;
            Item item = player.CurrentItem;

            // get active attachments
            IAttachment[] attachments = this.Attachments
                .Where(attachment => attachment.IsEnabled(player, tool, item, location))
                .ToArray();
            if (!attachments.Any())
                return;

            // apply tools
            this.TemporarilyFakeInteraction(() =>
            {
                foreach (Vector2 tile in this.GetTileGrid(Game1.player.getTileLocation(), this.Config.Distance))
                {
                    location.objects.TryGetValue(tile, out SObject tileObj);
                    location.terrainFeatures.TryGetValue(tile, out TerrainFeature tileFeature);
                    foreach (IAttachment attachment in attachments)
                    {
                        if (attachment.Apply(tile, tileObj, tileFeature, Game1.player, tool, item, Game1.currentLocation))
                            break;
                    }
                }
            });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Temporarily dismount and set up the player to interact with a tile, then return it to the previous state afterwards.</summary>
        /// <param name="action">The action to perform.</param>
        private void TemporarilyFakeInteraction(Action action)
        {
            // get references
            SFarmer player = Game1.player;
            IPrivateField<Horse> mountField = this.Reflection.GetPrivateField<Horse>(Game1.player, "mount");

            // save current state
            Horse mount = mountField.GetValue();
            Vector2 mountPosition = this.Current.position;
            WateringCan wateringCan = player.CurrentTool as WateringCan;
            int waterInCan = wateringCan?.WaterLeft ?? 0;
            float stamina = player.stamina;
            Vector2 position = player.position;
            int currentToolIndex = player.CurrentToolIndex;
            bool canMove = Game1.player.canMove; // fix player frozen due to animations when performing an action

            // move mount out of the way
            mountField.SetValue(null);
            this.Current.position = new Vector2(-5, -5);

            // perform action
            try
            {
                action();
            }
            finally
            {
                // move mount back
                this.Current.position = mountPosition;
                mountField.SetValue(mount);

                // restore previous state
                if (wateringCan != null)
                    wateringCan.WaterLeft = waterInCan;
                player.stamina = stamina;
                player.position = position;
                player.CurrentToolIndex = currentToolIndex;
                Game1.player.canMove = canMove;
            }
        }

        /// <summary>Get a grid of tiles.</summary>
        /// <param name="origin">The center of the grid.</param>
        /// <param name="distance">The number of tiles in each direction to include.</param>
        private IEnumerable<Vector2> GetTileGrid(Vector2 origin, int distance)
        {
            for (int x = -distance; x <= distance; x++)
            {
                for (int y = -distance; y <= distance; y++)
                    yield return new Vector2(origin.X + x, origin.Y + y);
            }
        }

        /// <summary>Set whether the player should be riding the tractor.</summary>
        /// <param name="mount">Whether the player should be riding the tractor.</param>
        private void SetMounted(bool mount)
        {
            // swap tractors
            NPC newTractor = mount ? (NPC)this.Mount : this.Static;
            NPC oldTractor = mount ? (NPC)this.Static : this.Mount;

            Game1.removeCharacterFromItsLocation(oldTractor.name);
            Game1.removeCharacterFromItsLocation(newTractor.name);
            Game1.currentLocation.addCharacter(newTractor);

            newTractor.position = oldTractor.position;
            newTractor.facingDirection = oldTractor.facingDirection;

            // mount
            if (mount)
                this.Mount.checkAction(Game1.player, Game1.currentLocation);
        }
    }
}
