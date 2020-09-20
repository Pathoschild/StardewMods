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
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal partial class L10n
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="stage">The tree growth stage.</param>
        public static string For(WildTreeGrowthStage stage)
        {
            return L10n.GetRaw($"tree.stages.{stage}");
        }

        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="quality">The item quality.</param>
        public static string For(ItemQuality quality)
        {
            return L10n.GetRaw($"quality.{quality.GetName()}");
        }

        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="status">The friendship status.</param>
        /// <param name="wasHousemate">Whether the NPC is eligible to be a housemate, rather than spouse.</param>
        public static string For(FriendshipStatus status, bool wasHousemate)
        {
            if (wasHousemate && status == FriendshipStatus.Divorced)
                return L10n.GetRaw("friendship-status.kicked-out");
            return L10n.GetRaw($"friendship-status.{status.ToString().ToLower()}");
        }

        /// <summary>Get a translation for an enum value.</summary>
        /// <param name="age">The child age.</param>
        public static string For(ChildAge age)
        {
            return L10n.GetRaw($"npc.child.age.{age.ToString().ToLower()}");
        }

        /// <summary>Get a value like <c>{{name}} loves this</c>, <c>{{name}} likes this</c>, etc.</summary>
        /// <param name="taste">The taste value returned by <see cref="StardewValley.Locations.MovieTheater.GetConcessionTasteForCharacter"/>.</param>
        /// <param name="name">The NPC name.</param>
        public static string ForMovieTasteLabel(string taste, string name)
        {
            return L10n.GetRaw($"item.movie-snack-preference.{taste}", new { name });
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
        public static string GetSeasonName(string season)
        {
            if (string.IsNullOrWhiteSpace(season))
                return season;

            int id = Utility.getSeasonNumber(season);
            if (id == -1)
                throw new InvalidOperationException($"Can't translate unknown season '{season}'.");
            return Utility.getSeasonNameFromNumber(id);
        }

        /// <summary>Get translated season names from the game.</summary>
        /// <param name="seasons">The English season names.</param>
        public static IEnumerable<string> GetSeasonNames(IEnumerable<string> seasons)
        {
            foreach (string season in seasons)
                yield return L10n.GetSeasonName(season);
        }

        /// <summary>The overridden translations for location names.</summary>
        public static class LocationOverrides
        {
            /// <summary>The translated name for a location, or the internal name if no translation is available.</summary>
            public static string LocationName(string locationName)
            {
                return L10n.Translations.Get($"location.{locationName}").Default(locationName);
            }

            /// <summary>The translated name for a fishing area.</summary>
            public static string AreaName(string locationName, string id)
            {
                // mine level
                if (string.Equals(locationName, "UndergroundMine", StringComparison.OrdinalIgnoreCase))
                    return L10n.Location_UndergroundMine_Level(level: id);

                // dynamic area override
                Translation areaTranslation = L10n.Translations.Get(int.TryParse(id, out int _)
                    ? $"location.{locationName}.fish-area-{id}"
                    : $"location.{locationName}.{id}");
                return areaTranslation
                    .Default(L10n.Location_UnknownFishArea(locationName: L10n.LocationOverrides.LocationName(locationName), id: id));
            }
        }

        /// <summary>Get a human-readable representation of a value.</summary>
        /// <param name="value">The underlying value.</param>
        public static string Stringify(object value)
        {
            switch (value)
            {
                case null:
                    return null;

                // net types
                case NetBool net:
                    return L10n.Stringify(net.Value);
                case NetByte net:
                    return L10n.Stringify(net.Value);
                case NetColor net:
                    return L10n.Stringify(net.Value);
                case NetDancePartner net:
                    return L10n.Stringify(net.Value?.Name);
                case NetDouble net:
                    return L10n.Stringify(net.Value);
                case NetFloat net:
                    return L10n.Stringify(net.Value);
                case NetGuid net:
                    return L10n.Stringify(net.Value);
                case NetInt net:
                    return L10n.Stringify(net.Value);
                case NetLocationRef net:
                    return L10n.Stringify(net.Value?.NameOrUniqueName);
                case NetLong net:
                    return L10n.Stringify(net.Value);
                case NetPoint net:
                    return L10n.Stringify(net.Value);
                case NetPosition net:
                    return L10n.Stringify(net.Value);
                case NetRectangle net:
                    return L10n.Stringify(net.Value);
                case NetString net:
                    return L10n.Stringify(net.Value);
                case NetVector2 net:
                    return L10n.Stringify(net.Value);

                // core types
                case bool boolean:
                    return boolean ? L10n.Generic_Yes() : L10n.Generic_No();
                case Color color:
                    return $"(r:{color.R} g:{color.G} b:{color.B} a:{color.A})";
                case SDate date:
                    return date.ToLocaleString(withYear: date.Year != Game1.year);
                case TimeSpan span:
                    {
                        List<string> parts = new List<string>();
                        if (span.Days > 0)
                            parts.Add(L10n.Generic_Days(span.Days));
                        if (span.Hours > 0)
                            parts.Add(L10n.Generic_Hours(span.Hours));
                        if (span.Minutes > 0)
                            parts.Add(L10n.Generic_Minutes(span.Minutes));
                        return string.Join(", ", parts);
                    }
                case Vector2 vector:
                    return $"({vector.X}, {vector.Y})";
                case Rectangle rect:
                    return $"(x:{rect.X}, y:{rect.Y}, width:{rect.Width}, height:{rect.Height})";

                // game types
                case AnimatedSprite sprite:
                    return $"(textureName: {sprite.textureName.Value}, currentFrame:{sprite.currentFrame}, loop:{sprite.loop}, sourceRect:{L10n.Stringify(sprite.sourceRect)})";
                case MarriageDialogueReference dialogue:
                    return $"(file: {dialogue.DialogueFile}, key: {dialogue.DialogueKey}, gendered: {dialogue.IsGendered}, substitutions: {L10n.Stringify(dialogue.Substitutions)})";
                case Stats stats:
                    {
                        StringBuilder str = new StringBuilder();
                        foreach (FieldInfo field in stats.GetType().GetFields())
                            str.AppendLine($"- {field.Name}: {L10n.Stringify(field.GetValue(stats))}");
                        return str.ToString();
                    }

                // enumerable
                case IEnumerable array when !(value is string):
                    {
                        string[] values = (from val in array.Cast<object>() select L10n.Stringify(val)).ToArray();
                        return "(" + string.Join(", ", values) + ")";
                    }

                default:
                    // key/value pair
                    {
                        Type type = value.GetType();
                        if (type.IsGenericType)
                        {
                            Type genericType = type.GetGenericTypeDefinition();
                            if (genericType == typeof(NetDictionary<,,,,>))
                            {
                                object dict = type.GetProperty("FieldDict").GetValue(value);
                                return L10n.Stringify(dict);
                            }
                            if (genericType == typeof(KeyValuePair<,>))
                            {
                                string k = L10n.Stringify(type.GetProperty(nameof(KeyValuePair<byte, byte>.Key)).GetValue(value));
                                string v = L10n.Stringify(type.GetProperty(nameof(KeyValuePair<byte, byte>.Value)).GetValue(value));
                                return $"({k}: {v})";
                            }
                        }
                    }

                    // anything else
                    return value.ToString();
            }
        }
    }
}
