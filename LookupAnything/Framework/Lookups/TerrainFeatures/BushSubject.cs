using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures
{
    /// <summary>Describes a bush.</summary>
    internal class BushSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying target.</summary>
        private readonly Bush Target;

        /// <summary>The location which contains the bush.</summary>
        private readonly GameLocation Location;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="bush">The lookup target.</param>
        /// <param name="location">The location which contains the bush.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public BushSubject(GameHelper gameHelper, Bush bush, GameLocation location, IReflectionHelper reflection)
            : base(gameHelper)
        {
            this.Target = bush;
            this.Location = location;
            this.Reflection = reflection;

            if (this.IsBerryBush(bush))
                this.Initialize(I18n.Bush_Name_Berry(), I18n.Bush_Description_Berry(), I18n.Type_Bush());
            else if (this.IsTeaBush(bush))
                this.Initialize(I18n.Bush_Name_Tea(), I18n.Bush_Description_Tea(), I18n.Type_Bush());
            else
                this.Initialize(I18n.Bush_Name_Plain(), I18n.Bush_Description_Plain(), I18n.Type_Bush());
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // get basic info
            Bush bush = this.Target;
            bool isBerryBush = this.IsBerryBush(bush);
            bool isTeaBush = this.IsTeaBush(bush);
            SDate today = SDate.Now();

            // next harvest
            if (isBerryBush || isTeaBush)
            {
                SDate nextHarvest = this.GetNextHarvestDate(bush);
                string nextHarvestStr = nextHarvest == today
                    ? I18n.Generic_Now()
                    : $"{this.Stringify(nextHarvest)} ({this.GetRelativeDateStr(nextHarvest)})";
                string harvestSchedule = isTeaBush ? I18n.Bush_Schedule_Tea() : I18n.Bush_Schedule_Berry();

                yield return new GenericField(I18n.Bush_NextHarvest(), $"{nextHarvestStr}{Environment.NewLine}{harvestSchedule}");
            }

            // date planted + grown
            if (isTeaBush)
            {
                SDate datePlanted = this.GetDatePlanted(bush);
                int daysOld = SDate.Now().DaysSinceStart - datePlanted.DaysSinceStart; // bush.getAge() not reliable, e.g. for Caroline's tea bush
                SDate dateGrown = this.GetDateFullyGrown(bush);

                yield return new GenericField(I18n.Bush_DatePlanted(), $"{this.Stringify(datePlanted)} ({this.GetRelativeDateStr(-daysOld)})");
                if (dateGrown > today)
                {
                    string grownOnDateText = I18n.Bush_Growth_Summary(date: this.Stringify(dateGrown));
                    yield return new GenericField(I18n.Bush_Growth(), $"{grownOnDateText} ({this.GetRelativeDateStr(dateGrown)})");
                }
            }
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<IDebugField> GetDebugFields()
        {
            Bush target = this.Target;

            // pinned fields
            yield return new GenericDebugField("health", target.health, pinned: true);
            yield return new GenericDebugField("is greenhouse bush", this.Stringify(target.greenhouseBush.Value), pinned: true);
            yield return new GenericDebugField("is town bush", this.Stringify(target.townBush.Value), pinned: true);
            yield return new GenericDebugField("is in bloom", this.Stringify(target.inBloom(Game1.currentSeason, Game1.dayOfMonth)), pinned: true);

            // raw fields
            foreach (IDebugField field in this.GetDebugFieldsFrom(target))
                yield return field;
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            Bush bush = this.Target;

            // get source info
            Rectangle sourceArea = this.Reflection.GetField<NetRectangle>(bush, "sourceRect").GetValue().Value;
            Point spriteSize = new(sourceArea.Width * Game1.pixelZoom, sourceArea.Height * Game1.pixelZoom);
            SpriteEffects spriteEffects = bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // calculate target area
            float scale = Math.Min(size.X / spriteSize.X, size.Y / spriteSize.Y);
            Point targetSize = new((int)(spriteSize.X * scale), (int)(spriteSize.Y * scale));
            Vector2 offset = new Vector2(size.X - targetSize.X, size.Y - targetSize.Y) / 2;

            // draw portrait
            spriteBatch.Draw(
                texture: Bush.texture.Value,
                destinationRectangle: new((int)(position.X + offset.X), (int)(position.Y + offset.Y), targetSize.X, targetSize.Y),
                sourceRectangle: sourceArea,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                effects: spriteEffects,
                layerDepth: 0
            );
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the given bush produces berries.</summary>
        /// <param name="bush">The berry busy.</param>
        private bool IsBerryBush(Bush bush)
        {
            return bush.size.Value == Bush.mediumBush && !bush.townBush.Value;
        }

        /// <summary>Get whether a given bush produces tea.</summary>
        /// <param name="bush">The bush to check.</param>
        private bool IsTeaBush(Bush bush)
        {
            return bush.size.Value == Bush.greenTeaBush;
        }

        /// <summary>Get the date when the bush was planted.</summary>
        /// <param name="bush">The bush to check.</param>
        private SDate GetDatePlanted(Bush bush)
        {
            SDate date = new(1, "spring", 1);
            if (this.IsTeaBush(bush) && bush.datePlanted.Value > 0) // Caroline's sun room bush has datePlanted = -999
                date = date.AddDays(bush.datePlanted.Value);
            return date;
        }

        /// <summary>Get the date when the bush will be fully grown.</summary>
        /// <param name="bush">The bush to check.</param>
        private SDate GetDateFullyGrown(Bush bush)
        {
            SDate date = this.GetDatePlanted(bush);
            if (this.IsTeaBush(bush))
                date = date.AddDays(Bush.daysToMatureGreenTeaBush);
            return date;
        }

        /// <summary>Get the next date when the bush will produce forage.</summary>
        /// <param name="bush">The bush to check.</param>
        /// <remarks>Derived from <see cref="Bush.inBloom"/>.</remarks>
        private SDate GetNextHarvestDate(Bush bush)
        {
            SDate today = SDate.Now();
            var tomorrow = today.AddDays(1);

            // currently has produce
            if (bush.tileSheetOffset.Value == 1)
                return today;

            // tea bushes take 20 days to grow, then produce leaves on day 22+ of each season (except winter if not in the greenhouse)
            if (this.IsTeaBush(bush))
            {
                SDate minDate = this.GetDateFullyGrown(bush);
                if (minDate < tomorrow)
                    minDate = tomorrow;

                if (minDate.Season == "winter" && !bush.greenhouseBush.Value && this.Location.locationContext != GameLocation.LocationContext.Island)
                    return new(22, "spring", minDate.Year + 1);
                if (minDate.Day < 22)
                    return new(22, minDate.Season);
                return minDate;
            }

            // wild bushes produce salmonberries in spring 15-18, and blackberries in fall 8-11
            SDate springStart = new(15, "spring");
            SDate springEnd = new(18, "spring");
            SDate fallStart = new(8, "fall");
            SDate fallEnd = new(11, "fall");

            if (tomorrow < springStart)
                return springStart;
            if (tomorrow > springEnd && tomorrow < fallStart)
                return fallStart;
            if (tomorrow > fallEnd)
                return new(springStart.Day, springStart.Season, springStart.Year + 1);
            return tomorrow;
        }
    }
}
