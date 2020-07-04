using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Manages tractor effects when the current player is riding a tractor horse.</summary>
    internal sealed class TractorManager
    {
        /*********
        ** Fields
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
        private IAttachment[] Attachments;

        /// <summary>The attachment cooldowns in ticks for each rate-limited attachment.</summary>
        private IDictionary<IAttachment, int> AttachmentCooldowns;

        /// <summary>The mod settings.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys;

        /// <summary>The number of ticks since the tractor last checked for an action to perform.</summary>
        private int SkippedActionTicks;

        /// <summary>Whether the player was riding the tractor during the last tick.</summary>
        private bool WasRiding;

        /// <summary>Whether the tractor effects were enabled during the last tick.</summary>
        private bool WasEnabled;

        /// <summary>The tractor location during the last tick.</summary>
        private GameLocation WasLocation;

        /// <summary>The rider health to maintain if they're invincible.</summary>
        private int RiderHealth;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the current player is riding the tractor.</summary>
        public bool IsCurrentPlayerRiding => TractorManager.IsTractor(Game1.player?.mount);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod settings.</param>
        /// <param name="keys">The configured key bindings.</param>
        /// <param name="translation">Provides translations from the mod's i18n folder.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public TractorManager(ModConfig config, ModConfigKeys keys, ITranslationHelper translation, IReflectionHelper reflection)
        {
            this.Config = config;
            this.Keys = keys;
            this.Translation = translation;
            this.Reflection = reflection;
        }

        /// <summary>Get the unique name for a tractor horse.</summary>
        /// <param name="id">The horse ID.</param>
        public static string GetTractorName(Guid id)
        {
            return $"tractor/{id:N}";
        }

        /// <summary>Get whether the given horse should be treated as a tractor.</summary>
        /// <param name="horse">The horse to check.</param>
        public static bool IsTractor(Horse horse)
        {
            return horse != null && horse.Name.StartsWith("tractor/");
        }

        /// <summary>Move a horse to the given location.</summary>
        /// <param name="horse">The horse to move.</param>
        /// <param name="location">The game location.</param>
        /// <param name="tile">The tile coordinate in the given location.</param>
        /// <remarks>The default <see cref="Game1.warpCharacter(NPC,GameLocation,Vector2)"/> logic doesn't work in the mines, so this method reimplements it with better logic.</remarks>
        public static void SetLocation(Horse horse, GameLocation location, Vector2 tile)
        {
            // remove horse from its current location
            // (The default logic in Game1.removeCharacterFromItsLocation doesn't support the mines. since they're not in Game1.locations.)
            horse.currentLocation?.characters.Remove(horse);
            horse.currentLocation = null;

            // add to new location
            location.addCharacter(horse);
            horse.currentLocation = location;

            horse.isCharging = false;
            horse.speed = 2;
            horse.blockedInterval = 0;
            horse.position.X = tile.X * Game1.tileSize;
            horse.position.Y = tile.Y * Game1.tileSize;
        }

        /// <summary>Update tractor effects and actions in the game.</summary>
        public void Update()
        {
            // update when player mounts or unmounts
            if (this.IsCurrentPlayerRiding != this.WasRiding)
            {
                this.WasRiding = this.IsCurrentPlayerRiding;

                // track health for invincibility
                if (this.Config.InvincibleOnTractor && this.IsCurrentPlayerRiding)
                    this.RiderHealth = Game1.player.health;

                // reset held-down tool power
                Game1.player.toolPower = 0;
            }

            // detect activation or location change
            bool enabled = this.IsEnabled();
            if (enabled && (!this.WasEnabled || !object.ReferenceEquals(this.WasLocation, Game1.currentLocation)))
                this.OnActivated(Game1.currentLocation);
            this.WasEnabled = enabled;
            this.WasLocation = Game1.currentLocation;

            // apply riding effects
            if (this.IsCurrentPlayerRiding && Game1.activeClickableMenu == null)
            {
                // apply invincibility
                if (this.Config.InvincibleOnTractor)
                {
                    if (Game1.player.health > this.RiderHealth)
                        this.RiderHealth = Game1.player.health;
                    else
                        Game1.player.health = this.RiderHealth;
                }

                // apply tractor buff
                this.UpdateBuff();

                // apply tool effects
                if (this.UpdateCooldown())
                {
                    if (enabled)
                        this.UpdateAttachmentEffects();
                }
            }
        }

        /// <summary>Draw a radius around the player.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public void DrawRadius(SpriteBatch spriteBatch)
        {
            bool enabled = this.IsEnabled();

            foreach (Vector2 tile in this.GetTileGrid(Game1.player.getTileLocation(), this.Config.Distance))
            {
                // get tile area in screen pixels
                Rectangle area = new Rectangle((int)(tile.X * Game1.tileSize - Game1.viewport.X), (int)(tile.Y * Game1.tileSize - Game1.viewport.Y), Game1.tileSize, Game1.tileSize);

                // choose tile color
                Color color = enabled ? Color.Green : Color.Red;

                // draw background
                spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, area.Height), color * 0.2f);

                // draw border
                int borderSize = 1;
                Color borderColor = color * 0.5f;
                spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, borderSize), borderColor); // top
                spriteBatch.DrawLine(area.X, area.Y, new Vector2(borderSize, area.Height), borderColor); // left
                spriteBatch.DrawLine(area.X + area.Width, area.Y, new Vector2(borderSize, area.Height), borderColor); // right
                spriteBatch.DrawLine(area.X, area.Y + area.Height, new Vector2(area.Width, borderSize), borderColor); // bottom
            }
        }

        /// <summary>Update mod configuration if it changed.</summary>
        /// <param name="config">The new mod configuration.</param>
        /// <param name="keys">The new key bindings.</param>
        /// <param name="attachments">The tractor attachments to apply.</param>
        public void UpdateConfig(ModConfig config, ModConfigKeys keys, IEnumerable<IAttachment> attachments)
        {
            // update config
            this.Config = config;
            this.Keys = keys;
            this.Attachments = attachments.Where(p => p != null).ToArray();
            this.AttachmentCooldowns = this.Attachments.Where(p => p.RateLimit > this.TicksPerAction).ToDictionary(p => p, p => 0);

            // clear buff so it's reapplied with new values
            Game1.buffsDisplay?.otherBuffs?.RemoveAll(p => p.which == this.BuffUniqueID);

            // reset cooldowns
            this.SkippedActionTicks = 0;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the tractor is toggled on by the player.</summary>
        private bool IsEnabled()
        {
            if (!this.IsCurrentPlayerRiding)
                return false;

            // automatic mode
            if (!this.Keys.HoldToActivate.HasAny())
                return true;

            // hold-to-activate mode
            return this.Keys.HoldToActivate.IsDown();
        }

        /// <summary>Apply the tractor buff to the current player.</summary>
        private void UpdateBuff()
        {
            Buff buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == this.BuffUniqueID);
            if (buff == null)
            {
                buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, this.Config.MagneticRadius, this.Config.TractorSpeed, 0, 0, 1, "Tractor Power", this.Translation.Get("buff.name")) { which = this.BuffUniqueID };
                Game1.buffsDisplay.addOtherBuff(buff);
            }
            buff.millisecondsDuration = 100;
        }

        /// <summary>Update the attachment cooldown.</summary>
        /// <returns>Returns whether the cooldown has ended.</returns>
        private bool UpdateCooldown()
        {
            this.SkippedActionTicks++;

            if (this.SkippedActionTicks % this.TicksPerAction != 0)
                return false;

            this.SkippedActionTicks = 0;
            return true;
        }

        /// <summary>Notify attachments that effects have been enabled for a location.</summary>
        /// <param name="location">The current tractor location.</param>
        private void OnActivated(GameLocation location)
        {
            foreach (IAttachment attachment in this.Attachments)
                attachment.OnActivated(location);
        }

        /// <summary>Apply any effects for the current tractor attachment.</summary>
        private void UpdateAttachmentEffects()
        {
            // get context
            Farmer player = Game1.player;
            GameLocation location = Game1.currentLocation;
            Tool tool = player.CurrentTool;
            Item item = player.CurrentItem;

            // get active attachments
            IAttachment[] attachments = this.GetApplicableAttachmentsAfterCooldown(player, tool, item, location).ToArray();
            if (!attachments.Any())
                return;

            // get tile grid to affect
            // This must be done outside the temporary interaction block below, since that dismounts
            // the player which changes their position from what the player may expect.
            Vector2 origin = Game1.player.getTileLocation();
            Vector2[] grid = this.GetTileGrid(origin, this.Config.Distance).ToArray();

            // apply tools
            this.TemporarilyFakeInteraction(() =>
            {
                foreach (Vector2 tile in grid)
                {
                    // face tile to avoid game skipping interaction
                    this.GetRadialAdjacentTile(origin, tile, out Vector2 adjacentTile, out int facingDirection);
                    player.Position = adjacentTile * Game1.tileSize;
                    player.FacingDirection = facingDirection;

                    // apply attachment effects
                    location.objects.TryGetValue(tile, out SObject tileObj);
                    location.terrainFeatures.TryGetValue(tile, out TerrainFeature tileFeature);
                    foreach (IAttachment attachment in attachments)
                    {
                        if (attachment.Apply(tile, tileObj, tileFeature, Game1.player, tool, item, Game1.currentLocation))
                        {
                            this.ResetCooldown(attachment);
                            break;
                        }
                    }
                }
            });
        }

        /// <summary>Get the attachments which are ready and can be applied to the given tile, after applying cooldown.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        private IEnumerable<IAttachment> GetApplicableAttachmentsAfterCooldown(Farmer player, Tool tool, Item item, GameLocation location)
        {
            foreach (IAttachment attachment in this.Attachments)
            {
                // run cooldown
                if (attachment.RateLimit > this.TicksPerAction)
                {
                    int cooldown = this.AttachmentCooldowns[attachment];
                    if (cooldown > this.TicksPerAction)
                    {
                        this.AttachmentCooldowns[attachment] -= this.TicksPerAction;
                        continue;
                    }
                }

                // yield attachment
                if (attachment.IsEnabled(player, tool, item, location))
                    yield return attachment;
            }
        }

        /// <summary>Reset the cooldown for an attachment.</summary>
        /// <param name="attachment">The attachment to reset.</param>
        private void ResetCooldown(IAttachment attachment)
        {
            if (attachment.RateLimit > 0)
                this.AttachmentCooldowns[attachment] = attachment.RateLimit;
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

        /// <summary>Get the tile coordinate which is adjacent to the given <paramref name="tile"/> along a radial line from the tractor position.</summary>
        /// <param name="origin">The tile containing the tractor.</param>
        /// <param name="tile">The tile to face.</param>
        /// <param name="adjacent">The tile radially adjacent to the <paramref name="tile"/>.</param>
        /// <param name="facingDirection">The direction to face.</param>
        private void GetRadialAdjacentTile(Vector2 origin, Vector2 tile, out Vector2 adjacent, out int facingDirection)
        {
            facingDirection = Utility.getDirectionFromChange(tile, origin);
            switch (facingDirection)
            {
                case Game1.up:
                    adjacent = new Vector2(tile.X, tile.Y + 1);
                    break;

                case Game1.down:
                    adjacent = new Vector2(tile.X, tile.Y - 1);
                    break;

                case Game1.left:
                    adjacent = new Vector2(tile.X + 1, tile.Y);
                    break;

                case Game1.right:
                    adjacent = new Vector2(tile.X - 1, tile.Y);
                    break;

                default:
                    adjacent = tile;
                    break;
            }
        }

        /// <summary>Temporarily dismount and set up the player to interact with a tile, then return it to the previous state afterwards.</summary>
        /// <param name="action">The action to perform.</param>
        [SuppressMessage("SMAPI", "AvoidImplicitNetFieldCast", Justification = "Deliberately accesses net field instance.")]
        private void TemporarilyFakeInteraction(Action action)
        {
            // get references
            // (Note: change net values directly to avoid sync bugs, since the value will be reset when we're done.)
            Farmer player = Game1.player;
            NetRef<Horse> mountField = this.Reflection.GetField<NetRef<Horse>>(Game1.player, "netMount").GetValue();
            IReflectedField<Horse> mountFieldValue = this.Reflection.GetField<Horse>(mountField, "value");
            IReflectedField<Vector2> mountPositionValue = this.Reflection.GetField<Vector2>(player.mount.position.Field, "value");

            // save current state
            Horse mount = mountField.Value;
            Vector2 mountPosition = mount.Position;
            WateringCan wateringCan = player.CurrentTool as WateringCan;
            int waterInCan = wateringCan?.WaterLeft ?? 0;
            float stamina = player.stamina;
            Vector2 position = player.Position;
            int facingDirection = player.FacingDirection;
            int currentToolIndex = player.CurrentToolIndex;
            bool canMove = player.canMove; // fix player frozen due to animations when performing an action

            // move mount out of the way
            mountFieldValue.SetValue(null);
            mountPositionValue.SetValue(new Vector2(-5, -5));

            // perform action
            try
            {
                action();
            }
            finally
            {
                // move mount back
                mountPositionValue.SetValue(mountPosition);
                mountFieldValue.SetValue(mount);

                // restore previous state
                if (wateringCan != null)
                    wateringCan.WaterLeft = waterInCan;
                player.stamina = stamina;
                player.Position = position;
                player.FacingDirection = facingDirection;
                player.CurrentToolIndex = currentToolIndex;
                player.canMove = canMove;
            }
        }
    }
}
