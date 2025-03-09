using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Integrations.CustomBush;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

/// <summary>Describes a bush.</summary>
internal class BushSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying target.</summary>
    private readonly Bush Target;

    /// <summary>Provides subject entries.</summary>
    private readonly ISubjectRegistry Codex;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="codex">Provides subject entries.</param>
    /// <param name="bush">The lookup target.</param>
    public BushSubject(GameHelper gameHelper, ISubjectRegistry codex, Bush bush)
        : base(gameHelper)
    {
        this.Target = bush;
        this.Codex = codex;

        if (this.TryGetCustomBush(bush, out ICustomBush? customBush))
            this.Initialize(TokenParser.ParseText(customBush.DisplayName), TokenParser.ParseText(customBush.Description), I18n.Type_Bush());
        else if (this.IsBerryBush(bush))
            this.Initialize(I18n.Bush_Name_Berry(), I18n.Bush_Description_Berry(), I18n.Type_Bush());
        else if (this.IsTeaBush(bush))
            this.Initialize(I18n.Bush_Name_Tea(), I18n.Bush_Description_Tea(), I18n.Type_Bush());
        else
            this.Initialize(I18n.Bush_Name_Plain(), I18n.Bush_Description_Plain(), I18n.Type_Bush());
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        // get basic info
        Bush bush = this.Target;
        bool isBerryBush = this.IsBerryBush(bush);
        bool isTeaBush = this.IsTeaBush(bush);
        SDate today = SDate.Now();

        if (isBerryBush && this.TryGetBushBloomSchedules(bush, out var bushBloomSchedule))
        {
            List<Item> itemList = [];
            Dictionary<Item, string> displayText = new(new ObjectReferenceComparer<Item>());

            foreach (var entry in bushBloomSchedule.OrderBy(p => p.StartDay.TotalDays).ThenBy(p => p.EndDay.TotalDays))
            {
                SDate lastDay = SDate.From(entry.EndDay);
                SDate firstDay = SDate.From(entry.StartDay);
                Item item = ItemRegistry.Create(entry.UnqualifiedItemId);
                itemList.Add(item);

                if (firstDay < today)
                    firstDay = today;

                if (lastDay < today)
                    continue;

                displayText[item] = firstDay == lastDay
                    ? $"{item.DisplayName}: {this.Stringify(firstDay)}"
                    : $"{item.DisplayName}: {this.Stringify(firstDay)} - {this.Stringify(lastDay)}";
            }

            yield return new ItemIconListField(this.GameHelper, I18n.Bush_NextHarvest(), itemList, false, item => displayText.GetValueOrDefault(item));
        }
        else
        {
            if (isBerryBush || isTeaBush)
            {
                SDate nextHarvest = this.GetNextHarvestDate(bush);
                string nextHarvestStr = nextHarvest == today
                    ? I18n.Generic_Now()
                    : $"{this.Stringify(nextHarvest)} ({this.GetRelativeDateStr(nextHarvest)})";
                if (this.TryGetCustomBushDrops(bush, out IList<ItemDropData>? drops))
                    yield return new ItemDropListField(this.GameHelper, this.Codex, I18n.Bush_NextHarvest(), drops, preface: nextHarvestStr);
                else
                {
                    string harvestSchedule = isTeaBush ? I18n.Bush_Schedule_Tea() : I18n.Bush_Schedule_Berry();
                    yield return new GenericField(I18n.Bush_NextHarvest(), $"{nextHarvestStr}{Environment.NewLine}{harvestSchedule}");
                }
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
    }

    /// <inheritdoc />
    public override IEnumerable<IDebugField> GetDebugFields()
    {
        Bush target = this.Target;

        // pinned fields
        yield return new GenericDebugField("health", target.health, pinned: true);
        yield return new GenericDebugField("is town bush", this.Stringify(target.townBush.Value), pinned: true);
        yield return new GenericDebugField("is in bloom", this.Stringify(target.inBloom()), pinned: true);

        // raw fields
        foreach (IDebugField field in this.GetDebugFieldsFrom(target))
            yield return field;
    }

    /// <inheritdoc />
    public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
    {
        Bush bush = this.Target;

        // get source info
        Rectangle sourceArea = bush.sourceRect.Value;
        Point spriteSize = new(sourceArea.Width * Game1.pixelZoom, sourceArea.Height * Game1.pixelZoom);
        SpriteEffects spriteEffects = bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        // calculate target area
        float scale = Math.Min(size.X / spriteSize.X, size.Y / spriteSize.Y);
        Point targetSize = new((int)(spriteSize.X * scale), (int)(spriteSize.Y * scale));
        Vector2 offset = new Vector2(size.X - targetSize.X, size.Y - targetSize.Y) / 2;

        // get texture
        Texture2D texture;
        if (this.TryGetCustomBush(bush, out ICustomBush? customBush))
        {
            texture = bush.IsSheltered()
                ? Game1.content.Load<Texture2D>(customBush.IndoorTexture)
                : Game1.content.Load<Texture2D>(customBush.Texture);
        }
        else
            texture = Bush.texture.Value;

        // draw portrait
        spriteBatch.Draw(
            texture: texture,
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
        return bush.size.Value == Bush.mediumBush && !bush.townBush.Value && !bush.Location.InIslandContext();
    }

    /// <summary>Get whether a given bush produces tea.</summary>
    /// <param name="bush">The bush to check.</param>
    private bool IsTeaBush(Bush bush)
    {
        return bush.size.Value == Bush.greenTeaBush;
    }

    /// <summary>Get bush data from the Custom Bush mod if applicable.</summary>
    /// <param name="bush">The bush to check.</param>
    /// <param name="customBush">The resulting custom bush, if applicable.</param>
    /// <returns>Returns whether a custom bush was found.</returns>
    private bool TryGetCustomBush(Bush bush, [NotNullWhen(true)] out ICustomBush? customBush)
    {
        customBush = null;
        return
            this.GameHelper.CustomBush.IsLoaded
            && this.GameHelper.CustomBush.ModApi.TryGetCustomBush(bush, out customBush);
    }

    /// <summary>Get bush drops from the Custom Bush mod if applicable.</summary>
    /// <param name="bush">The bush to check.</param>
    /// <param name="drops">The items produced by the custom bush, if applicable.</param>
    /// <returns>Returns whether custom bush drops were found.</returns>
    private bool TryGetCustomBushDrops(Bush bush, [NotNullWhen(true)] out IList<ItemDropData>? drops)
    {
        CustomBushIntegration customBush = this.GameHelper.CustomBush;

        if (customBush.IsLoaded && customBush.ModApi.TryGetCustomBush(bush, out _, out string? id) && customBush.ModApi.TryGetDrops(id, out IList<ICustomBushDrop>? rawDrops))
        {
            drops = new List<ItemDropData>(rawDrops.Count);

            foreach (ICustomBushDrop drop in rawDrops)
                drops.Add(new ItemDropData(drop.ItemId, drop.MinStack, drop.MaxStack, drop.Chance, drop.Condition));

            return true;
        }

        drops = null;
        return false;
    }

    /// <summary>Get the berry schedules from Bush Bloom Mod for a given bush, if applicable.</summary>
    /// <param name="bush">The bush to check.</param>
    /// <param name="schedule">The berry schedule for this bush.</param>
    /// <returns>Returns whether Bush Bloom Mod provided custom schedules.</returns>
    private bool TryGetBushBloomSchedules(Bush bush, [NotNullWhen(true)] out (string UnqualifiedItemId, WorldDate StartDay, WorldDate EndDay)[]? schedule)
    {
        if (this.GameHelper.BushBloomMod.IsLoaded && this.GameHelper.BushBloomMod.ModApi.IsReady())
        {
            SDate today = SDate.Now();
            schedule = this.GameHelper.BushBloomMod.ModApi.GetActiveSchedules(today.Season.ToString(), today.Day, today.Year, bush.Location, bush.Tile);
            return true;
        }

        schedule = null;
        return false;
    }

    /// <summary>Get the date when the bush was planted.</summary>
    /// <param name="bush">The bush to check.</param>
    private SDate GetDatePlanted(Bush bush)
    {
        SDate date = new(1, Season.Spring, 1);
        if (this.IsTeaBush(bush) && bush.datePlanted.Value > 0) // Caroline's sun room bush has datePlanted = -999
            date = date.AddDays(bush.datePlanted.Value);
        return date;
    }

    /// <summary>Get the date when the bush will be fully grown.</summary>
    /// <param name="bush">The bush to check.</param>
    private SDate GetDateFullyGrown(Bush bush)
    {
        SDate date = this.GetDatePlanted(bush);

        if (this.TryGetCustomBush(bush, out ICustomBush? customBush))
            date = date.AddDays(customBush.AgeToProduce);
        else if (this.IsTeaBush(bush))
            date = date.AddDays(Bush.daysToMatureGreenTeaBush);

        return date;
    }

    /// <summary>Get the day of season when this bush will start producing berries.</summary>
    /// <param name="bush">The bush to check.</param>
    private int GetDayToBeginProducing(Bush bush)
    {
        if (this.TryGetCustomBush(bush, out ICustomBush? customBush))
            return customBush.DayToBeginProducing;

        if (this.IsTeaBush(bush))
            return 22; // tea bushes produce on day 22+ of season

        return -1;
    }

    /// <summary>Get the seasons during which this bush produces berries when not sheltered.</summary>
    /// <param name="bush">The bush to check.</param>
    private List<Season> GetProducingSeasons(Bush bush)
    {
        if (this.TryGetCustomBush(bush, out ICustomBush? customBush))
            return customBush.Seasons;

        if (this.IsTeaBush(bush))
            return [Season.Spring, Season.Summer, Season.Fall];

        return [Season.Spring, Season.Fall];
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

        // tea bush and custom bush
        int dayToBegin = this.GetDayToBeginProducing(bush);
        if (dayToBegin >= 0)
        {
            SDate readyDate = this.GetDateFullyGrown(bush);
            if (readyDate < tomorrow)
                readyDate = tomorrow;

            if (!bush.IsSheltered())
            {
                // bush not sheltered, must check producing seasons
                List<Season> producingSeasons = this.GetProducingSeasons(bush);
                SDate seasonDate = new(Math.Max(1, dayToBegin), readyDate.Season, readyDate.Year);
                while (!producingSeasons.Contains(seasonDate.Season))
                    seasonDate = seasonDate.AddDays(28);

                if (readyDate < seasonDate)
                    return seasonDate;
            }

            if (readyDate.Day < dayToBegin)
                readyDate = new(dayToBegin, readyDate.Season, readyDate.Year);

            return readyDate;
        }

        // wild bushes produce salmonberries in spring 15-18, and blackberries in fall 8-11
        SDate springStart = new(15, Season.Spring);
        SDate springEnd = new(18, Season.Spring);
        SDate fallStart = new(8, Season.Fall);
        SDate fallEnd = new(11, Season.Fall);

        if (tomorrow < springStart)
            return springStart;
        if (tomorrow > springEnd && tomorrow < fallStart)
            return fallStart;
        if (tomorrow > fallEnd)
            return new(springStart.Day, springStart.Season, springStart.Year + 1);
        return tomorrow;
    }
}
