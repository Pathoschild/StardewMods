using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.WildTrees;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Pathfinding;

namespace Pathoschild.Stardew.LookupAnything.Framework;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal partial class I18n
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get a separated list of values (like "A, B, C") using the separator for the current language.</summary>
    /// <param name="values">The values to list.</param>
    public static string List(IEnumerable<object> values)
    {
        return string.Join(I18n.Generic_ListSeparator(), values);
    }

    /// <summary>Get a translation for an enum value.</summary>
    /// <param name="stage">The tree growth stage.</param>
    public static string For(WildTreeGrowthStage stage)
    {
        string stageKey = stage == (WildTreeGrowthStage)4
            ? "smallTree"
            : stage.ToString();

        return I18n.GetByKey($"tree.stages.{stageKey}");
    }

    /// <summary>Get a translation for an enum value.</summary>
    /// <param name="quality">The item quality.</param>
    public static string For(ItemQuality quality)
    {
        return I18n.GetByKey($"quality.{quality.GetName()}");
    }

    /// <summary>Get a translation for an enum value.</summary>
    /// <param name="status">The friendship status.</param>
    /// <param name="wasHousemate">Whether the NPC is eligible to be a housemate, rather than spouse.</param>
    public static string For(FriendshipStatus status, bool wasHousemate)
    {
        if (wasHousemate && status == FriendshipStatus.Divorced)
            return I18n.FriendshipStatus_KickedOut();
        return I18n.GetByKey($"friendship-status.{status.ToString().ToLower()}");
    }

    /// <summary>Get a translation for an enum value.</summary>
    /// <param name="age">The child age.</param>
    public static string For(ChildAge age)
    {
        return I18n.GetByKey($"npc.child.age.{age.ToString().ToLower()}");
    }

    /// <summary>Get a value like <c>{{name}} loves this</c>, <c>{{name}} likes this</c>, etc.</summary>
    /// <param name="taste">The taste value returned by <see cref="StardewValley.Locations.MovieTheater.GetConcessionTasteForCharacter"/>.</param>
    /// <param name="name">The NPC name.</param>
    public static string ForMovieTasteLabel(string taste, string name)
    {
        return I18n.GetByKey($"item.movie-snack-preference.{taste}", new { name });
    }

    /// <summary>Select the correct translation based on the plural form.</summary>
    /// <param name="count">The number.</param>
    /// <param name="singleText">The singular form.</param>
    /// <param name="pluralText">The plural form.</param>
    public static string GetPlural(int count, string singleText, string pluralText)
    {
        return count == 1 ? singleText : pluralText;
    }

    /// <summary>Get a translated season name from the game.</summary>
    /// <param name="season">The English season name.</param>
    public static string GetSeasonName(Season season)
    {
        return Utility.getSeasonNameFromNumber((int)season);
    }

    /// <summary>Get translated season names from the game.</summary>
    /// <param name="seasons">The English season names.</param>
    public static IEnumerable<string> GetSeasonNames(IEnumerable<Season> seasons)
    {
        foreach (Season season in seasons)
            yield return I18n.GetSeasonName(season);
    }

    /// <summary>Get a human-readable representation of a value.</summary>
    /// <param name="value">The underlying value.</param>
    /// <param name="isNested">Whether the current value is nested within a larger value. This disables some multi-line formatting.</param>
    public static string? Stringify(object? value, bool isNested = false)
    {
        switch (value)
        {
            case null:
                return null;


            /****
            ** .NET types
            ****/
            case bool boolean:
                return boolean ? I18n.Generic_Yes() : I18n.Generic_No();

            case TimeSpan span:
                {
                    List<string> parts = [];
                    if (span.Days > 0)
                        parts.Add(I18n.Generic_Days(span.Days));
                    if (span.Hours > 0)
                        parts.Add(I18n.Generic_Hours(span.Hours));
                    if (span.Minutes > 0)
                        parts.Add(I18n.Generic_Minutes(span.Minutes));
                    return I18n.List(parts);
                }

            /****
            ** MonoGame types
            ****/
            case Color color:
                return color.A < 255
                    ? $"RGBA {color.R} {color.G} {color.B} {color.A}, hex #{Convert.ToHexString([color.R, color.G, color.B, color.A])}"
                    : $"RGB {color.R} {color.G} {color.B}, hex #{Convert.ToHexString([color.R, color.G, color.B])}";

            case Point point:
                return $"({point.X}, {point.Y})";

            case Vector2 vector:
                return $"({vector.X}, {vector.Y})";

            case Rectangle rect:
                return $"(x:{rect.X}, y:{rect.Y}, width:{rect.Width}, height:{rect.Height})";


            /****
            ** SMAPI types
            ****/
            case SDate date:
                return date.ToLocaleString(withYear: date.Year != Game1.year);

            /****
            ** Game types
            ****/
            case AnimatedSprite sprite:
                return $"(textureName: {sprite.textureName.Value}, currentFrame:{sprite.currentFrame}, loop:{sprite.loop}, sourceRect:{I18n.Stringify(sprite.sourceRect, isNested: true)})";

            case Friendship friendship:
                {
                    string?[] fields = [
                        friendship.IsRoommate() ? "status: Roommate" : $"status: {friendship.Status}",
                        $"points: {friendship.Points}",
                        $"giftsToday: {friendship.GiftsToday}",
                        $"giftsThisWeek: {friendship.GiftsThisWeek}",
                        friendship.LastGiftDate?.TotalDays > 0 ? $"lastGiftDate: {SDate.From(friendship.LastGiftDate)}" : null,
                        $"talkedToday: {friendship.TalkedToToday}",
                        friendship.ProposalRejected ? "proposalRejected: true" : null,
                        friendship.WeddingDate?.TotalDays > 0 ? $"weddingDate: {SDate.From(friendship.WeddingDate)}" : null,
                        friendship.NextBirthingDate?.TotalDays > 0 ? $"nextBirthingDate: {SDate.From(friendship.NextBirthingDate)}" : null,
                        friendship.Proposer > 0 ? $"proposer: {Game1.GetPlayer(friendship.Proposer)?.Name ?? friendship.Proposer.ToString()}" : null
                    ];

                    return $"({string.Join(", ", fields.Where(p => p is not null))})";
                }

            case Item item:
                return $"({item} {item.QualifiedItemId})";

            case MarriageDialogueReference dialogue:
                return $"(file: {dialogue.DialogueFile}, key: {dialogue.DialogueKey}, gendered: {dialogue.IsGendered}, substitutions: {I18n.Stringify(dialogue.Substitutions, isNested: true)})";

            case ModDataDictionary data when data.Any():
                {
                    StringBuilder str = new StringBuilder();
                    str.AppendLine();
                    foreach (var pair in data.Pairs.OrderBy(p => p.Key))
                        str.AppendLine($"- {pair.Key}: {pair.Value}");
                    return str.ToString().TrimEnd();
                }

            case NetBool net:
                return I18n.Stringify(net.Value, isNested);
            case NetByte net:
                return I18n.Stringify(net.Value, isNested);
            case NetColor net:
                return I18n.Stringify(net.Value, isNested);
            case NetDancePartner net:
                return I18n.Stringify(net.Value?.displayName, isNested);
            case NetDouble net:
                return I18n.Stringify(net.Value, isNested);
            case NetFloat net:
                return I18n.Stringify(net.Value, isNested);
            case NetGuid net:
                return I18n.Stringify(net.Value, isNested);
            case NetInt net:
                return I18n.Stringify(net.Value, isNested);
            case NetLocationRef net:
                return I18n.Stringify(net.Value?.NameOrUniqueName, isNested);
            case NetLong net:
                return I18n.Stringify(net.Value, isNested);
            case NetPoint net:
                return I18n.Stringify(net.Value, isNested);
            case NetPosition net:
                return I18n.Stringify(net.Value, isNested);
            case NetRectangle net:
                return I18n.Stringify(net.Value, isNested);
            case NetString net:
                return I18n.Stringify(net.Value, isNested);
            case NetVector2 net:
                return I18n.Stringify(net.Value, isNested);

            case SchedulePathDescription schedulePath:
                return $"{schedulePath.time / 100:00}:{schedulePath.time % 100:00} {schedulePath.targetLocationName} ({schedulePath.targetTile.X}, {schedulePath.targetTile.Y}) {schedulePath.facingDirection} {schedulePath.endOfRouteMessage}";

            case Stats stats:
                {
                    StringBuilder str = new StringBuilder();
                    str.AppendLine();
                    foreach ((string key, uint statValue) in stats.Values)
                        str.AppendLine($"- {key}: {I18n.Stringify(statValue, isNested: true)}");
                    return str.ToString().TrimEnd();
                }

            case Warp warp:
                return $"([{warp.X}, {warp.Y}] to {warp.TargetName}[{warp.TargetX}, {warp.TargetY}])";

            /****
            ** Heuristic fallbacks
            ****/
            default:
                {
                    Type type = value.GetType();

                    // net dictionary
                    if (value is INetSerializable)
                    {
                        object? dict = type.GetProperty("FieldDict")?.GetValue(value);
                        if (dict != null)
                            return I18n.Stringify(dict, isNested);
                    }

                    if (type.IsGenericType)
                    {
                        Type genericType = type.GetGenericTypeDefinition();

                        // net ref
                        if (genericType == typeof(NetRef<>))
                        {
                            PropertyInfo? refValue = type.GetProperty(nameof(NetRef<>.Value));
                            if (refValue != null)
                                return I18n.Stringify(refValue.GetValue(value), isNested);
                        }

                        // key/value pair
                        if (genericType == typeof(KeyValuePair<,>))
                        {
                            string? key = I18n.Stringify(type.GetProperty(nameof(KeyValuePair<,>.Key))?.GetValue(value), isNested: true);
                            string? val = I18n.Stringify(type.GetProperty(nameof(KeyValuePair<,>.Value))?.GetValue(value), isNested: true);
                            return $"[{key}]: {val}";
                        }
                    }

                    // enumerable
                    if (value is IEnumerable array and not string)
                    {
                        string[] values = (from item in array.Cast<object>() select I18n.Stringify(item, isNested: true) ?? "(null)").ToArray();
                        if (values.Length == 0)
                            return "[]";

                        if (isNested)
                            return $"[{I18n.List(values)}]";

                        return $"{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", values);
                    }

                    // anything else
                    return value.ToString();
                }
        }
    }
}
