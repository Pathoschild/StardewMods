using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using InnerPatch = Pathoschild.Stardew.Common.Utilities.InvariantDictionary<Newtonsoft.Json.Linq.JToken?>;

namespace ContentPatcher.Framework.Migrations
{
    internal record DataLocationsState(JArray forage, JArray fish, JArray artifacts);
    internal class ObjectsState
    {
        // Assuming food if not present
        public string Type { get; set; } = "food";
    }
    internal enum DataLocationType
    {
        Fish,
        Forage,
        Artifacts
    }

    /// <summary>Migrates patches to format version 2.0.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_2_0 : BaseMigration
    {
        /// <summary>Handles parsing raw strings into tokens.</summary>
        private readonly Lexer Lexer = Lexer.Instance;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_2_0()
            : base(new SemanticVersion(2, 0, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.ModId)
            );
            this.MigrationWarnings = [
                "Some content packs haven't been updated for Stardew Valley 1.6.0. Content Patcher will try to auto-migrate them, but compatibility isn't guaranteed."
            ];
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            // 2.0 adds Priority
            foreach (PatchConfig patch in patches)
            {
                if (patch.Priority != null)
                {
                    error = this.GetNounPhraseError($"using {nameof(patch.Priority)}");
                    return false;
                }
            }

            Dictionary<PatchConfig, (bool Replace, List<PatchConfig> Patches)> replacementMap = new();

            foreach (PatchConfig? patch in patches)
            {
                if (this.HasAction(patch, PatchType.EditData))
                {
                    if (PathUtilities.NormalizeAssetName(patch.Target) == "Data/NPCDispositions")
                    {
                        patch.Target = "Data/Characters";
                        if (patch.TargetField.Count > 0)
                        {
                            error = "Unable to convert NPCDispositions when TargetField is used";
                            return false;
                        }
                        // Data/Dispositions -> Data/Characters doesn't need state management, passing in object? null instead
                        if (!this.ConvertDataModel<object?>(null, patch,
                            mapping: new()
                            {
                                ["0"] = this.MapSingle("Age", val => val.ToLowerInvariant() switch
                                {
                                    "child" => "Child",
                                    "teen" => "Teen",
                                    _ => "Adult"
                                }),
                                ["1"] = this.MapSingle("Manner", val => val.ToLowerInvariant() switch
                                {
                                    "rude" => "Rude",
                                    "polite" => "Polite",
                                    _ => "Neutral"
                                }),
                                ["2"] = this.MapSingle("SocialAnxiety", val => val.ToLowerInvariant() switch
                                {
                                    "shy" => "Shy",
                                    "outgoing" => "Outgoing",
                                    _ => "Neutral"
                                }),
                                ["3"] = this.MapSingle("Optimism", val => val.ToLowerInvariant() switch
                                {
                                    "negative" => "Negative",
                                    "positive" => "Positive",
                                    _ => "Neutral"
                                }),
                                ["4"] = this.MapSingle("Gender", val => val.ToLowerInvariant() switch
                                {
                                    "male" => "Male",
                                    "female" => "Female",
                                    _ => "Undefined"
                                }),
                                // in pre 1.6 it was "datable", "not-datable" or "secret". It is now a boolean.
                                ["5"] = this.MapSingle("CanBeRomanced", val => val.ToLowerInvariant() == "datable"),
                                ["6"] = this.MapSingle("LoveInterest"),
                                ["7"] = this.MapSingle("HomeRegion"),
                                ["8"] = (patch, outerKey, _, val) =>
                                {
                                    if (this.Lexer.MightContainTokens(val))
                                    {
                                        return $"Cannot convert the npc birthday information in {outerKey} when it is contains tokens ({val})";
                                    }
                                    // from 1.5.6's NPCDisposition parsing, with added checks for conventional "null"
                                    if (val.Length > 0 && val != "null")
                                    {
                                        string[] birthInfo = val.Split(' ');
                                        patch["BirthSeason"] = birthInfo[0];
                                        patch["BirthDay"] = Convert.ToInt32(birthInfo[1]);
                                    }
                                    return null;
                                },
                                ["9"] = this.MapSingle("FriendsAndFamily", val =>
                                {
                                    var friendsAndFamily = new JObject();
                                    if (val.Length > 0)
                                    {
                                        string[] relationComponents = val.Split(" ");
                                        if (relationComponents.Length >= 2)
                                        {
                                            for (int i = 0; i < Math.Floor((double)relationComponents.Length / 2); i++)
                                            {
                                                // TODO: Replace common occurrences with the vanilla Translatable strings instead
                                                friendsAndFamily[relationComponents[2 * i]] = relationComponents[2 * i + 1].Replace("'", "");
                                            }
                                        }
                                    }
                                    return friendsAndFamily;
                                }),
                                ["10"] = (patch, outerKey, _, val) =>
                                {
                                    this.ConvertDefaultLocation(patch, outerKey, val, out string? innerError);
                                    return innerError;
                                },
                                ["11"] = this.MapSingle("DisplayName")
                            },
                            out error)
                        )
                        {
                            return false;
                        }
                    }
                    else if (PathUtilities.NormalizeAssetName(patch.Target) == "Data/Locations")
                    {
                        if (!this.ConvertDataModel(new DataLocationsState(new(), new(), new()), patch,
                            mapping: new()
                            {
                                ["0"] = this.MapDataLocations("spring", DataLocationType.Forage, true),
                                ["1"] = this.MapDataLocations("summer", DataLocationType.Forage, true),
                                ["2"] = this.MapDataLocations("fall", DataLocationType.Forage, true),
                                ["3"] = this.MapDataLocations("winter", DataLocationType.Forage, true),
                                ["4"] = this.MapDataLocations("spring", DataLocationType.Fish, false),
                                ["5"] = this.MapDataLocations("summer", DataLocationType.Fish, false),
                                ["6"] = this.MapDataLocations("fall", DataLocationType.Fish, false),
                                ["7"] = this.MapDataLocations("winter", DataLocationType.Fish, false),
                                ["8"] = this.MapDataLocations(null, DataLocationType.Artifacts, true),
                            },
                            postMapping: (patch, data) =>
                            {
                                if (data == null)
                                {
                                    throw new NullReferenceException("Missing DataLocationState in MapDataLocations");
                                }

                                if (data.forage.HasValues)
                                {
                                    patch["Forage"] = new JArray(data.forage);
                                }
                                if (data.fish.HasValues)
                                {
                                    patch["Fish"] = new JArray(data.fish);
                                }
                                if (data.artifacts.HasValues)
                                {
                                    patch["ArtifactSpots"] = new JArray(data.artifacts);
                                }
                                // the data state is used for the entire EditData patch, which may contain data for multiple locations. This should be per location
                                data.forage.Clear();
                                data.fish.Clear();
                                data.artifacts.Clear();
                                return null;
                            },
                            error: out error)
                        )
                        {
                            return false;
                        }
                        if (!this.ConvertTextOperations(patch,
                            replacementMap,
                            textOperationFilter: operation =>
                            {
                                return
                                (
                                    operation.Delimiter == " " &&
                                    operation.Operation == "Append" &&
                                    operation.Target.Length == 3 &&
                                    operation.Target[0] == "Fields" &&
                                    operation.Target[1] != null &&
                                    operation.Value != null &&
                                    int.TryParse(operation.Target[2], out int field3) &&
                                    field3 >= 0 && field3 <= 8
                                );
                            },
                            textOperationMapper: (patch, textOperation) =>
                            {
                                string outerKey = textOperation.Target[1]!;
                                string index = textOperation.Target[2]!;
                                DataLocationsState state = new(new(), new(), new());
                                string value = textOperation.Value!;
                                string? error = index switch
                                {
                                    // This.MapDataLocations does not look at the patch, so passing null in for simplicity
                                    "0" => this.MapDataLocations("spring", DataLocationType.Forage, true)(null!, outerKey, state, value),
                                    "1" => this.MapDataLocations("summer", DataLocationType.Forage, true)(null!, outerKey, state, value),
                                    "2" => this.MapDataLocations("fall", DataLocationType.Forage, true)(null!, outerKey, state, value),
                                    "3" => this.MapDataLocations("winter", DataLocationType.Forage, true)(null!, outerKey, state, value),
                                    "4" => this.MapDataLocations("spring", DataLocationType.Fish, false)(null!, outerKey, state, value),
                                    "5" => this.MapDataLocations("summer", DataLocationType.Fish, false)(null!, outerKey, state, value),
                                    "6" => this.MapDataLocations("fall", DataLocationType.Fish, false)(null!, outerKey, state, value),
                                    "7" => this.MapDataLocations("winter", DataLocationType.Forage, false)(null!, outerKey, state, value),
                                    "8" => this.MapDataLocations(null, DataLocationType.Artifacts, true)(null!, outerKey, state, value),
                                    _ => "Unknown index"
                                };
                                if (error != null)
                                    return error;

                                patch.TargetField.Add(textOperation.Target[1]!);
                                JArray collection;
                                if (state.fish.HasValues)
                                {
                                    patch.TargetField.Add("Fish");
                                    collection = state.fish;
                                }
                                else if (state.forage.HasValues)
                                {
                                    patch.TargetField.Add("Forage");
                                    collection = state.forage;
                                }
                                else if (state.artifacts.HasValues)
                                {
                                    patch.TargetField.Add("ArtifactSpots");
                                    collection = state.artifacts;
                                }
                                else
                                {
                                    return "TextOperation patch did not contain anything";
                                }

                                int id = 0;
                                foreach (var item in collection)
                                {
                                    string namedId = patch.LogName + id++;
                                    item["Id"] = namedId;
                                    patch.Entries[namedId] = item;
                                }

                                // no error
                                return null;
                            },
                            out error
                        ))
                        {
                            return false;
                        }
                    }
                    else if (PathUtilities.NormalizeAssetName(patch.Target) == "Data/ObjectInformation")
                    {
                        patch.Target = "Data/Objects";
                        if (patch.TargetField.Count > 0)
                        {
                            error = "Unable to convert ObjectInformation when TargetField is used";
                            return false;
                        }

                        // Misc fields relate to a different field, tracking it in state
                        if (!this.ConvertDataModel<ObjectsState>(new(), patch,
                            mapping: new()
                            {
                                ["0"] = this.MapSingle("Name"),
                                ["1"] = this.MapSingle("Price"),
                                ["2"] = this.MapSingle("Edibility"),
                                // Note 1.5.6 behavior for this field was... weird, so this may need more love
                                ["3"] = (patch, _, _, val) =>
                                {
                                    if (val.Length > 0 && val != "null")
                                    {
                                        string[] typeAndCategory = val.Split(' ');
                                        patch["Type"] = typeAndCategory[0];
                                        if (typeAndCategory.Length > 1)
                                        {
                                            patch["Category"] = Convert.ToInt32(typeAndCategory[1]);
                                        }
                                    }
                                    return null;
                                },
                                ["4"] = this.MapSingle("DisplayName"),
                                ["5"] = this.MapSingle("Description"),
                                ["6"] = (patch, _, state, val) =>
                                {
                                    state.Type = val;
                                    if (val == "drink")
                                    {
                                        patch["IsDrink"] = true;
                                    }
                                    return null;
                                },
                                ["7"] = (patch, _, state, val) =>
                                {
                                    if (state.Type == "food" || state.Type == "drink")
                                    {
                                        string[] foodEffects = val.Split(" ");
                                        for (int i = 0; i <= foodEffects.Length - 1; i++)
                                        {
                                            if (int.TryParse(foodEffects[i], out int foodEffect))
                                            {
                                                string? key = i switch
                                                {
                                                    00 => "FarmingLevel",
                                                    01 => "FishingLevel",
                                                    02 => "MiningLevel",
                                                    03 => null,
                                                    04 => "LuckLevel",
                                                    05 => "ForagingLevel",
                                                    06 => null,
                                                    07 => "MaxStamina",
                                                    08 => "MagneticRadius",
                                                    09 => "Speed",
                                                    10 => "Defense",
                                                    11 => "Attack",
                                                    _ => null
                                                };
                                                if (key == null) continue;
                                                if (!patch.ContainsKey("Buff"))
                                                {
                                                    patch["Buff"] = new JObject();
                                                }
                                                if (!(patch["Buff"] as JObject)!.ContainsKey("CustomAttributes"))
                                                {
                                                    patch["Buff"]!["CustomAttributes"] = new JObject();
                                                }
                                                patch["Buff"]!["CustomAttributes"]![key] = foodEffect;
                                            }
                                        }
                                    }
                                    // TODO: implement other formats
                                    else
                                    {
                                        return state.Type;
                                    }

                                    return null;
                                },
                                ["8"] = (patch, _, _, val) =>
                                {
                                    if (!patch.ContainsKey("Buff"))
                                    {
                                        patch["Buff"] = new JObject();
                                    }
                                    patch["Buff"]!["Duration"] = val;
                                    return null;
                                },
                            },
                            out error)
                        )
                        {
                            return false;
                        }
                    }
                    else if (PathUtilities.NormalizeAssetName(patch.Target) == "Data/BigCraftableInformation")
                    {
                        patch.Target = "Data/BigCraftables";
                        if (patch.TargetField.Count > 0)
                        {
                            error = "Unable to convert BigCraftableInformation when TargetField is used";
                            return false;
                        }

                        if (!this.ConvertDataModel<object?>(null, patch,
                            mapping: new()
                            {
                                ["0"] = this.MapSingle("Name"),
                                ["1"] = this.MapSingle("Price"),
                                ["2"] = (patch, _, _, val) =>
                                {
                                    // This is 1.5.6 Edibility which doesn't exist anymore
                                    return null;
                                },
                                ["3"] = (patch, _, _, val) =>
                                {
                                    // This is 1.5.6 type+category which doesn't exist anymore
                                    return null;
                                },
                                ["4"] = this.MapSingle("DisplayName"),
                                ["5"] = this.MapSingle("CanBePlacedOutdoors"),
                                ["6"] = this.MapSingle("CanBePlacedIndoors"),
                                ["7"] = this.MapSingle("Fragility"),
                                ["8"] = this.MapSingle("IsLamp", val => val == "true"),
                                ["9"] = this.MapSingle("DisplayName")
                            },
                            out error)
                        )
                        {
                            return false;
                        }
                    }
                    else if (PathUtilities.NormalizeAssetName(patch.Target) == "Data/Crops")
                    {
                        if (!this.ConvertDataModel<object?>(null, patch,
                            mapping: new()
                            {
                                ["0"] = this.MapSingle("DaysInPhase", val => JArray.FromObject(val.Split(" "))),
                                ["1"] = this.MapSingle("Seasons", val => JArray.FromObject(val.Split(" "))),
                                ["2"] = this.MapSingle("SpriteIndex"),
                                ["3"] = this.MapSingle("HarvestItemId"),
                                ["4"] = this.MapSingle("RegrowDays"),
                                ["5"] = this.MapSingle("HarvestMethod", val => val == "1" ? "Scythe" : "Grab"),
                                ["6"] = (patch, _, _, val) =>
                                {
                                    /*
                                     * Format in 1.5.6 was one of the following
                                     * false
                                     * true minHarvest maxHarvest maxHarvestIncreasePerFarmingLevel chanceForExtraCrops
                                     */
                                    string[] split = val.Split(" ");
                                    if (split.Length > 0 && split[0] == "true")
                                    {
                                        patch["HarvestMinStack"] = split[1];
                                        patch["HarvestMaxStack"] = split[2];
                                        patch["HarvestMaxIncreasePerFarmingLevel"] = split[3];
                                        patch["ExtraHarvestChance"] = split[4];
                                    }
                                    return null;
                                },
                                ["7"] = this.MapSingle("IsRaised"),
                                ["8"] = (patch, _, _, val) =>
                                {
                                    /*
                                     * Format in 1.5.6 was one of the following
                                     * false
                                     * true [R G B]
                                     * where [] indicates repeating pairs of three values
                                     */
                                    string[] split = val.Split(" ");
                                    if (split.Length > 0 && split[0] == "true")
                                    {
                                        string[] segments = split.Skip(1).Chunk(3).Select(row => string.Join(' ', row)).ToArray();
                                        patch["TintColors"] = JArray.FromObject(segments);
                                    }
                                    return null;
                                }
                            },
                            out error)
                        )
                        {
                            return false;
                        }
                    }
                }
            }
            if (replacementMap.Count > 0)
            {
                // TODO: work out if these in place expansions can be done easier than just making it a list and back to an array
                var patchList = patches.ToList();
                foreach (var (patch, replacementEntry) in replacementMap)
                {
                    int index = patchList.IndexOf(patch);
                    if (replacementEntry.Replace)
                    {
                        patchList.RemoveAt(index);
                    }
                    // if we are not replacing, then the index needs to be incremented by 1, as EditDataPatch evaluates TextOperations after Entries/Records and Fields
                    patchList.InsertRange(index + (replacementEntry.Replace ? 0 : 1), replacementEntry.Patches);
                }
                patches = patchList.ToArray();
            }

            return true;
        }

        /// <summary>This method converts previously <c>Dictionary&lt;string, string&gt;</c> data model patches into <c>Dictionary&lt;string, T&gt;</c></summary>
        /// <remarks>
        /// This method is intended to be operated on EditData patches where the target is 
        /// where values were / separated values.
        /// 
        /// This method will convert the Fields and Entries of the patch into a form that can be run on  instead
        /// mapping the keys from the old integer indexes into new string values represented in the new data model,
        /// while doing transformation of values in the process.
        /// </remarks>
        /// <typeparam name="T">The Datatype for state management in <paramref name="data"/></typeparam>
        /// <param name="data">State management which will be passed into every call in <paramref name="mapping"/> and <paramref name="postMapping"/></param>
        /// <param name="patch">The patch that is being transformed</param>
        /// <param name="mapping">
        /// A dictionary of old keys and values are transformer methods to be operated on the inner patch contents.
        /// The methods arguments are as follows:
        /// <list type="number">
        ///     <item>the inner patch contents the original dictionary key being referenced</item>
        ///     <item>the key from the dictionary being accessed (i.e npc name in NPCDispositions)</item>
        ///     <item>the state object from <paramref name="data"/></item>
        ///     <item>The raw string value that is being mapped and potentially transformed on</item>
        /// </list>
        /// and would return an optional error message if things went wrong
        /// </param>
        /// <param name="error">An error message if things went wrong.</param>
        /// <param name="postMapping">A method that is run after the all the Fields of a given entry key, or after each value in Entries to do any final transformations in bulk</param>
        /// <returns>whether it was successful, if unsuccessful then <paramref name="error"/> will explain why</returns>
        private bool ConvertDataModel<T>(
            T data,
            PatchConfig patch,
            Dictionary<string, Func<InnerPatch, string, T, string, string?>> mapping,
            [NotNullWhen(false)] out string? error,
            Func<InnerPatch, T, string?>? postMapping = null
        )
        {
            bool InnerConversion(InnerPatch patch, string outerKey, [NotNullWhen(false)] out string? innerError)
            {
                bool hasRun = false;
                foreach (string key in patch.Keys.ToArray())
                {
                    var value = patch[key];
                    if (value?.Type != JTokenType.String) continue;
                    // This is invoking JToken's explicit string operator and tests if it was a string token, if so spit out the value.
                    string? strValue = (string?)value;
                    if (strValue == null) continue;
                    if (mapping.TryGetValue(key, out var mapper))
                    {
                        innerError = mapper(patch, outerKey, data, strValue);
                        if (innerError != null) return false;
                        patch.Remove(key);
                        hasRun = true;
                    }
                }
                if (hasRun && postMapping != null)
                {
                    string? postError = postMapping(patch, data);
                    if (postError != null)
                    {
                        innerError = postError;
                        return false;
                    }
                }
                innerError = null;
                return true;
            }

            if (patch.Fields.Count > 0)
            {
                foreach (var (key, _) in patch.Fields)
                {
                    var field = patch.Fields[key];
                    if (field == null) continue;
                    if (!InnerConversion(field, key, out error))
                    {
                        return false;
                    }
                }
            }
            foreach (var (key, value) in patch.Entries)
            {
                if (value?.Type == JTokenType.String)
                {
                    string? strValue = (string?)value;
                    if (strValue == null) continue;
                    var components = this.SplitTokenAware(strValue, '/');
                    // Converting the patch to look closer to a fully populated Fields patch so can reuse code
                    var newPatch = new InnerPatch(components.Select((v, i) => new { Key = i, Value = v }).ToDictionary(item => item.Key.ToString(), item => (JToken?)new JValue(item.Value)));
                    var newValue = new JObject();
                    if (!InnerConversion(newPatch, key, out error))
                    {
                        return false;
                    }
                    patch.Entries[key] = JObject.FromObject(newPatch);
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Converts TextOperations in complex patches into new patches using list insertion instead
        /// </summary>
        /// <param name="patch">The patch that may contain TextOperation entries</param>
        /// <param name="replacementMap">A working collection of patches and the mapping back to the patch that causes them for upstream insertion in <see cref="TryMigrate(ref PatchConfig[], out string?)"/></param>
        /// <param name="textOperationFilter">Whether this text operation is valid for the transformer.</param>
        /// <param name="textOperationMapper">Transforms the empty patch with the text operation into a patch with content.</param>
        /// <param name="error">The error message if anything went wrong</param>
        /// <returns>This returns whether the conversion was successful. If it wasn't then <paramref name="error"/> will explain why.</returns>
        private bool ConvertTextOperations(
            PatchConfig patch,
            Dictionary<PatchConfig, (bool, List<PatchConfig>)> replacementMap,
            Func<TextOperationConfig, bool> textOperationFilter,
            Func<PatchConfig, TextOperationConfig, string?> textOperationMapper,
            [NotNullWhen(false)] out string? error)
        {
            int textOperationId = 0;
            var replacementPatches = new List<PatchConfig>();
            foreach (var textOperation in patch.TextOperations.WhereNotNull())
            {
                if (!textOperationFilter(textOperation))
                {
                    // TODO: Provide reason why
                    error = $"Text Operation is invalid.";
                    return false;
                }

                var newPatch = new PatchConfig()
                {
                    Action = patch.Action,
                    Update = patch.Update,
                    // This is post mutations from preValidation if target redirection is required
                    Target = patch.Target,
                    // TODO: Work out if this should be cloned when its technically a dead field and not supported since 1.25 (but still functions if format is <1.25)
                    Enabled = patch.Enabled,
                    // This migration runs after PatchLoader.UniquelyNamePatches so LogName will be present
                    LogName = (patch.LogName ?? "") + "TextOperation" + textOperationId++,
                };
                // When isn't assignable, so just iterate over the old to make the new
                foreach (var when in patch.When)
                {
                    newPatch.When.Add(when.Key, when.Value);
                }
                error = textOperationMapper(newPatch, textOperation);
                if (error != null)
                {
                    return false;
                }
                replacementPatches.Add(newPatch);
            }
            if (replacementPatches.Count > 0)
            {
                // bool is if it should remove the original patch or not, which in this case is if the patch was exclusively textOperations or not
                replacementMap[patch] = (patch.Entries.Count == 0 && patch.Fields.Count == 0, replacementPatches);
            }
            error = null;
            return true;
        }

        /// <summary>
        /// This is an abstraction for the simple cases where you are just mapping an old key (like 4) to a new key (like LoveInterest) with an optional transformation
        /// </summary>
        /// <param name="newKey">The key you are mapping to</param>
        /// <param name="transformer">An optional transformer that takes in the raw string value and spits out the <c>JToken</c> it should now be</param>
        /// <returns>The return value can be directly inserted into the mapping table of <see cref="ConvertDataModel{T}" /></returns>
        private Func<InnerPatch, string, object?, string, string?> MapSingle(string newKey, Func<string, JToken>? transformer = null)
        {
            return (patch, _, _, val) =>
            {
                JToken output = val;
                if (transformer != null)
                {
                    output = transformer(val);
                }
                patch[newKey] = output;
                return null;
            };
        }

        /// <summary>
        /// Converts NPCDisposition index 10 into the data model in use in 1.6
        /// </summary>
        /// <param name="patch">The patch that is being operated on.</param>
        /// <param name="outerKey">The npc name we are operating on. Only used for error messaging and is informational only.</param>
        /// <param name="defaultLocationInfo">the previous value in NPCDisposition that needs transforming</param>
        /// <param name="error">The error message if something went wrong.</param>
        /// <returns>This returns a boolean as to whether the conversion was successful or not. If not, then <paramref name="error"/> will explain why.</returns>
        private bool ConvertDefaultLocation(InnerPatch patch, string outerKey, string defaultLocationInfo, [NotNullWhen(false)] out string? error)
        {
            if (this.Lexer.MightContainTokens(defaultLocationInfo))
            {
                error = $"Cannot convert the npc default location information in {outerKey} when it is contains tokens ({defaultLocationInfo})";
                return false;
            }

            string[] locationParts = defaultLocationInfo.Split(' ');
            // Vanilla parsing splits and looks at 0,1,2 indexes, some mods put in a 4th section which is unread
            if (locationParts.Length < 3)
            {
                error = $"Cannot convert the npc default location information in {outerKey} when it isn't 'LocationName X Y' ({defaultLocationInfo})";
                return false;
            }

            patch["Home"] = new JArray
            {
                new JObject()
                {
                    ["Id"] = "Default",
                    ["Location"] = locationParts[0],
                    ["Tile"] = new JObject()
                    {
                        ["X"] = Convert.ToInt32(locationParts[1]),
                        ["Y"] = Convert.ToInt32(locationParts[2])
                    }
                }
            };
            error = null;
            return true;
        }

        /// <summary>
        /// This is an abstraction for all the cases in Data/Locations where they are all space separated strings working in pairs.
        /// </summary>
        /// <param name="season">The season this index of the patch is operating on, or null if it doesn't operate on a season.</param>
        /// <param name="dataset">Which dataset is this index associated with</param>
        /// <param name="hasChance">Whether the second value in the string pairs should be interpreted as chance or not.</param>
        /// <returns>An error message if anything went wrong, otherwise null</returns>
        /// <exception cref="MissingFieldException">If dataset wasn't a valid value.</exception>
        private Func<InnerPatch, string, DataLocationsState, string, string?> MapDataLocations(string? season, DataLocationType dataset, bool hasChance)
        {
            return (patch, _, data, val) =>
            {
                var args = this.SplitTokenAware(val, ' ');
                if (args.Count > 1)
                {
                    for (int i = 0; i < args.Count; i += 2)
                    {
                        // If this is the last argument and there is an odd number of arguments just ignore it :(
                        // Fishing and Artifact code would error in this condition normally, but forage code silently handled it
                        if (i == args.Count - 1 && args.Count % 2 == 1)
                        {
                            continue;
                        }
                        var newObject = new JObject()
                        {
                            ["ItemId"] = "(O)" + args[i],
                        };
                        // TODO: Attempt if its worth trying to port legacy fish area id's over
                        if (hasChance)
                        {
                            newObject["Chance"] = args[i + 1];
                        }
                        if (season != null)
                        {
                            // On the event of multiple seasons, have unique ID's
                            newObject["ID"] = $"{args[i]}_${season}";
                            newObject["Season"] = season;
                        }
                        (dataset switch
                        {
                            DataLocationType.Forage => data.forage,
                            DataLocationType.Fish => data.fish,
                            DataLocationType.Artifacts => data.artifacts,
                            _ => throw new MissingFieldException(dataset.ToString()),
                        }).Add(newObject);
                    }
                }
                return null;
            };
        }

        // TODO: Look into seeing if this should be a utility method, as its not strictly related to migration
        /// <summary>
        /// This utility method splits <paramref name="rawText"/> by <paramref name="separator"/> but will not cut inside a Token.
        /// </summary>
        /// <param name="rawText">The string that may contain tokens that needs to be split</param>
        /// <param name="separator">The character that will be splitting by</param>
        /// <returns>A list of strings split by <paramref name="separator"/> This will include empty values.</returns>
        /// <exception cref="Exception">If an unknown LexerTokenType is hit, this will throw</exception>
        private List<string> SplitTokenAware(string rawText, char separator)
        {

            /**
             * This code block is being very generous and is assuming a token will not contain spaces that impacts how Data/Locations will be mapped.
             * Examples that this code block is intended to handle is JsonAssets ObjectId tokens that contained spaces in the item names
             */
            var tokens = this.Lexer.ParseBits(rawText, false).ToArray();
            List<string> args = new();

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token.Type == Lexing.LexTokens.LexTokenType.Literal)
                {
                    string tokenStr = token.ToString();
                    List<string> literalSplit = tokenStr.Split(separator).ToList();
                    // If the previous token was a token, and we start with a separator, it would be an empty string arg so we append
                    // Otherwise if it didn't start with a separator, we need to append anyway
                    if (i > 0)
                    {
                        args[args.Count - 1] += literalSplit[0];
                        literalSplit.RemoveAt(0);
                    }
                    args.AddRange(literalSplit);
                }
                else if (token.Type == Lexing.LexTokens.LexTokenType.Token)
                {
                    // If the previous token ended without a separator, we need to append to the previous
                    // If the previous token ended with a separator, there would be an empty string arg, append to previous anyway
                    if (i > 0)
                    {
                        args[args.Count - 1] += token.ToString();
                    }
                    else
                    {
                        args.Add(token.ToString());
                    }
                }
                else
                {
                    throw new Exception($"Unknown token type {token.Type} while parsing Data/Locations string");
                }
            }
            return args;
        }
    }
}
