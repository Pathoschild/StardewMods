using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Pathfinding;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows an NPC's schedule.</summary>
internal class ScheduleField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The sentences a screen reader speaks for the whole field.</summary>
    private readonly string[] SpokenParts;

    /// <summary>The scheduled stops, one entry per stop.</summary>
    private readonly SpokenEntry[] SpokenStops;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="npc">The NPC whose schedule to display.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    public ScheduleField(NPC npc, GameHelper gameHelper)
        : this(npc, gameHelper, new ScheduleSpeech()) { }

    /// <inheritdoc />
    public override string? GetSpokenText()
    {
        string text = SpokenText.Sentences(this.SpokenParts);
        return text.Length > 0 ? text : null;
    }

    /// <inheritdoc />
    public override IReadOnlyList<SpokenEntry> GetSpokenEntries()
    {
        return this.SpokenStops;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="npc">The NPC whose schedule to display.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="speech">Collects the spoken text while the displayed text is built.</param>
    private ScheduleField(NPC npc, GameHelper gameHelper, ScheduleSpeech speech)
        : base(I18n.Npc_Schedule(), ScheduleField.GetText(npc, gameHelper, speech))
    {
        this.SpokenParts = speech.Parts.ToArray();
        this.SpokenStops = speech.Stops.ToArray();
    }

    /// <summary>Get the text to display.</summary>
    /// <param name="npc">The NPC whose schedule to display.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="speech">Collects the spoken text while the displayed text is built.</param>
    private static IEnumerable<IFormattedText> GetText(NPC npc, GameHelper gameHelper, ScheduleSpeech speech)
    {
        bool isFarmhand = !Context.IsMainPlayer;

        // current location
        GameLocation? location = npc.currentLocation;
        if (isFarmhand && location?.IsActiveLocation() is not true)
        {
            speech.Parts.Add(I18n.Npc_Schedule_Farmhand_UnknownPosition());
            yield return new FormattedText(I18n.Npc_Schedule_Farmhand_UnknownPosition());
        }
        else
        {
            string locationName = location is not null
                ? gameHelper.GetLocationDisplayName(location.Name, location.GetData())
                : "???";

            speech.Parts.Add(I18n.ScreenReader_ScheduleCurrentPosition(locationName: locationName, x: npc.TilePoint.X, y: npc.TilePoint.Y));
            yield return new FormattedText(I18n.Npc_Schedule_CurrentPosition(locationName: locationName, x: npc.TilePoint.X, y: npc.TilePoint.Y));
        }
        yield return new FormattedText(Environment.NewLine + Environment.NewLine);

        // schedule
        if (isFarmhand)
        {
            speech.Parts.Add(I18n.Npc_Schedule_Farmhand_UnknownSchedule());
            yield return new FormattedText(I18n.Npc_Schedule_Farmhand_UnknownSchedule());
        }
        else
        {
            // validate schedule
            ScheduleEntry[] schedule = ScheduleField.FormatSchedule(npc.Schedule).ToArray();
            if (schedule.Length is 0)
            {
                speech.Parts.Add(I18n.Npc_Schedule_NoEntries());
                yield return new FormattedText(I18n.Npc_Schedule_NoEntries());
                yield break;
            }
            if (npc.ignoreScheduleToday || !npc.followSchedule)
            {
                speech.Parts.Add(I18n.Npc_Schedule_NotFollowingSchedule());
                yield return new FormattedText(I18n.Npc_Schedule_NotFollowingSchedule());
                yield break;
            }

            // show schedule entries
            for (int i = 0; i < schedule.Length; i++)
            {
                (int time, SchedulePathDescription entry) = schedule[i];

                string locationName = gameHelper.GetLocationDisplayName(entry.targetLocationName, Game1.getLocationFromName(entry.targetLocationName)?.GetData());
                bool isStarted = Game1.timeOfDay >= time;
                bool isFinished = i < schedule.Length - 1 && Game1.timeOfDay >= schedule[i + 1].Time;

                Color textColor = isStarted
                    ? (isFinished ? Color.Gray : Color.Green)
                    : Color.Black;

                // the sighted view colours past stops grey, the stop happening now green, and later stops black
                string stop = I18n.ScreenReader_ScheduleStop(time: CommonHelper.FormatTime(time), locationName: locationName, x: entry.targetTile.X, y: entry.targetTile.Y);
                if (isStarted)
                    stop = isFinished ? I18n.ScreenReader_ScheduleFinished(stop) : I18n.ScreenReader_ScheduleNow(stop);
                speech.Parts.Add(stop);
                speech.Stops.Add(new SpokenEntry(stop));

                if (i > 0)
                    yield return new FormattedText(Environment.NewLine);
                yield return new FormattedText(I18n.Npc_Schedule_Entry(time: CommonHelper.FormatTime(time), locationName: locationName, x: entry.targetTile.X, y: entry.targetTile.Y), textColor);
            }
        }
    }

    /// <summary>Returns a collection of schedule entries sorted by time. Consecutive entries with the same target location are omitted.</summary>
    /// <param name="schedule">The schedule to format.</param>
    private static IEnumerable<ScheduleEntry> FormatSchedule(Dictionary<int, SchedulePathDescription>? schedule)
    {
        if (schedule is null)
            yield break;

        List<int> sortedKeys = [.. schedule.Keys.OrderBy(key => key)];
        string prevTargetLocationName = string.Empty;

        foreach (int time in sortedKeys)
        {
            // skip if the entry does not exist or the previous entry was for the same location
            if (!schedule.TryGetValue(time, out SchedulePathDescription? entry) || entry.targetLocationName == prevTargetLocationName)
                continue;

            prevTargetLocationName = entry.targetLocationName;
            yield return new ScheduleEntry(time, entry);
        }
    }

    /// <summary>An entry in an NPC's schedule.</summary>
    /// <param name="Time">The time that the event starts.</param>
    /// <param name="Description">A description of the event.</param>
    private record ScheduleEntry(int Time, SchedulePathDescription Description);

    /// <summary>Collects the spoken text while the displayed schedule text is built.</summary>
    private class ScheduleSpeech
    {
        /// <summary>The sentences spoken for the whole field.</summary>
        public List<string> Parts { get; } = [];

        /// <summary>The scheduled stops.</summary>
        public List<SpokenEntry> Stops { get; } = [];
    }
}
