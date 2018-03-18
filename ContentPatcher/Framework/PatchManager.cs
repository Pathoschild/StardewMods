using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework
{
    /// <summary>Manages loaded patches.</summary>
    internal class PatchManager : IAssetLoader, IAssetEditor
    {
        /*********
        ** Properties
        *********/
        /// <summary>The valid condition values.</summary>
        private readonly IDictionary<ConditionKey, HashSet<string>> ValidValues = new Dictionary<ConditionKey, HashSet<string>>
        {
            [ConditionKey.Day] = new HashSet<string>(Enumerable.Range(1, 28).Select(p => p.ToString())),
            [ConditionKey.DayOfWeek] = new HashSet<string>(Enum.GetNames(typeof(DayOfWeek)), StringComparer.InvariantCultureIgnoreCase),
            [ConditionKey.Language] = new HashSet<string>(Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)).Where(p => p != LocalizedContentManager.LanguageCode.th.ToString()), StringComparer.InvariantCultureIgnoreCase),
            [ConditionKey.Season] = new HashSet<string>(new[] { "Spring", "Summer", "Fall", "Winter" }, StringComparer.InvariantCultureIgnoreCase),
            [ConditionKey.Weather] = new HashSet<string>(Enum.GetNames(typeof(Weather)), StringComparer.InvariantCultureIgnoreCase)
        };

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The patches to apply indexed by asset name.</summary>
        private readonly IDictionary<string, IList<IPatch>> Patches = new Dictionary<string, IList<IPatch>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>The patches which were previously applied.</summary>
        private readonly HashSet<IPatch> AppliedPatches = new HashSet<IPatch>();

        /// <summary>The current condition context.</summary>
        private readonly ConditionContext ConditionContext;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="language">The current language.</param>
        public PatchManager(IMonitor monitor, LocalizedContentManager.LanguageCode language)
        {
            this.Monitor = monitor;
            this.ConditionContext = new ConditionContext(language);
        }

        /****
        ** Patching
        ****/
        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return this.GetCurrentLoaders(asset).Any();
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return this.GetCurrentEditors(asset).Any();
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            IPatch patch = this.GetCurrentLoaders(asset).FirstOrDefault();
            if (patch == null)
                throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because no patches currently apply. This should never happen.");

            this.AppliedPatches.Add(patch);
            this.Monitor.Log($"{patch.ContentPack.Manifest.Name} loaded {asset.AssetName}.", LogLevel.Trace);
            return patch.Load<T>(asset);
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            IPatch[] patches = this.GetCurrentEditors(asset).ToArray();
            if (!patches.Any())
                throw new InvalidOperationException($"Can't edit asset key '{asset.AssetName}' because no patches currently apply. This should never happen.");

            HashSet<string> loggedContentPacks = new HashSet<string>();
            foreach (IPatch patch in patches)
            {
                this.AppliedPatches.Add(patch);
                if(loggedContentPacks.Add(patch.ContentPack.Manifest.Name))
                    this.Monitor.Log($"{patch.ContentPack.Manifest.Name} edited {asset.AssetName}.", LogLevel.Trace);
                patch.Edit<T>(asset);
            }
        }

        /// <summary>Update the current context.</summary>
        /// <param name="contentHelper">The content helper through which to invalidate assets.</param>
        /// <param name="language">The current language.</param>
        /// <param name="date">The current in-game date (if applicable).</param>
        /// <param name="weather">The current in-game weather (if applicable).</param>
        public void UpdateContext(IContentHelper contentHelper, LocalizedContentManager.LanguageCode language, SDate date, Weather? weather)
        {
            // update context
            this.ConditionContext.Update(language, date, weather);

            // detect patches which changed conditional result
            HashSet<string> reloadAssetNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string assetName in this.Patches.Keys)
            {
                foreach (IPatch patch in this.Patches[assetName])
                {
                    bool shouldApply = patch.IsMatch(this.ConditionContext);
                    if (shouldApply != this.AppliedPatches.Contains(patch))
                    {
                        reloadAssetNames.Add(assetName);
                        break;
                    }
                }
            }

            // reload assets if needed
            if (reloadAssetNames.Any())
            {
                this.AppliedPatches.RemoveWhere(p => reloadAssetNames.Contains(p.AssetName));
                contentHelper.InvalidateCache(asset => reloadAssetNames.Contains(asset.AssetName));
            }
        }


        /****
        ** Patches
        ****/
        /// <summary>Add a patch.</summary>
        /// <param name="patch">The patch to add.</param>
        public void Add(IPatch patch)
        {
            // validate
            if (patch.Type == PatchType.Load && this.GetConflictingLoaders(patch).Any())
                throw new InvalidOperationException($"Can't add {patch.Type} patch because it conflicts with an already-registered loader.");

            // add
            if (this.Patches.TryGetValue(patch.AssetName, out IList<IPatch> patches))
                patches.Add(patch);
            else
                this.Patches[patch.AssetName] = new List<IPatch> { patch };
        }

        /// <summary>Get all patches regardless of context.</summary>
        /// <param name="assetName">The asset name for which to find patches.</param>
        public IEnumerable<IPatch> GetAll(string assetName)
        {
            return this.Patches.TryGetValue(assetName, out IList<IPatch> patches)
                ? patches
                : new IPatch[0];
        }

        /// <summary>Get patches which load the given asset in the current context.</summary>
        /// <param name="asset">The asset being intercepted.</param>
        public IEnumerable<IPatch> GetCurrentLoaders(IAssetInfo asset)
        {
            return this
                .GetAll(asset.AssetName)
                .Where(patch => patch.Type == PatchType.Load && patch.IsMatch(this.ConditionContext));
        }

        /// <summary>Get patches which edit the given asset in the current context.</summary>
        /// <param name="asset">The asset being intercepted.</param>
        public IEnumerable<IPatch> GetCurrentEditors(IAssetInfo asset)
        {
            PatchType? patchType = this.GetEditType(asset.DataType);
            if (patchType == null)
                return new IPatch[0];

            return this
                .GetAll(asset.AssetName)
                .Where(patch => patch.Type == patchType && patch.IsMatch(this.ConditionContext));
        }

        /// <summary>Find any <see cref="PatchType.Load"/> patches which conflict with the given patch, taking conditions into account.</summary>
        /// <param name="newPatch">The new patch to check.</param>
        public IEnumerable<IPatch> GetConflictingLoaders(IPatch newPatch)
        {
            return this
                .GetAll(newPatch.AssetName)
                .Where(patch => patch.Type == PatchType.Load && this.CanConditionsOverlap(newPatch.Conditions, patch.Conditions));
        }

        /****
        ** Condition parsing
        ****/
        /// <summary>Normalise and parse the given condition values.</summary>
        /// <param name="raw">The raw condition values to normalise.</param>
        /// <param name="conditions">The normalised conditions.</param>
        /// <param name="error">An error message indicating why normalisation failed.</param>
        public bool TryParseConditions(IDictionary<string, string> raw, out ConditionDictionary conditions, out string error)
        {
            // no conditions
            if (raw == null || !raw.Any())
            {
                conditions = new ConditionDictionary(this.ValidValues);
                error = null;
                return true;
            }

            // Split a comma-delimited set of condition values and normalise to lowercase.
            string[] SplitAndNormalise(string values)
            {
                if (string.IsNullOrWhiteSpace(values))
                    return new string[0];

                return (
                    from value in values.Split(',')
                    where !string.IsNullOrWhiteSpace(value)
                    select value.Trim().ToLower()
                ).ToArray();
            }

            // parse conditions
            conditions = new ConditionDictionary(this.ValidValues);
            foreach (KeyValuePair<string, string> pair in raw)
            {
                // parse condition key
                if (!Enum.TryParse(pair.Key, true, out ConditionKey key))
                {
                    error = $"'{pair.Key}' isn't a valid condition; must be one of {string.Join(", ", this.ValidValues.Keys)}";
                    conditions = null;
                    return false;
                }

                // parse values
                string[] values = SplitAndNormalise(pair.Value);
                if (!values.Any())
                {
                    error = $"{key} can't be empty";
                    conditions = null;
                    return false;
                }

                // restrict to allowed values
                {
                    string[] invalidValues = values.Except(this.ValidValues[key], StringComparer.InvariantCultureIgnoreCase).ToArray();
                    if (invalidValues.Any())
                    {
                        error = $"invalid {key} values ({string.Join(", ", invalidValues)}); expected one of {string.Join(", ", this.ValidValues[key])}";
                        conditions = null;
                        return false;
                    }
                }

                // create condition
                conditions[key] = new Condition(key, values);
            }

            // return parsed conditions
            error = null;
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the patch type which applies when editing a given asset type.</summary>
        /// <param name="assetType">The asset type.</param>
        private PatchType? GetEditType(Type assetType)
        {
            if (assetType == typeof(Texture2D))
                return PatchType.EditImage;
            if (assetType.IsGenericType && assetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return PatchType.EditData;

            return null;
        }

        /// <summary>Get whether two sets of conditions can potentially both match in some contexts.</summary>
        /// <param name="left">The first set of conditions to compare.</param>
        /// <param name="right">The second set of conditions to compare.</param>
        private bool CanConditionsOverlap(ConditionDictionary left, ConditionDictionary right)
        {
            // no conflict if they edit different locales
            if (!left.GetImpliedValues(ConditionKey.Language).Intersect(right.GetImpliedValues(ConditionKey.Language)).Any())
                return false;

            // check for potential conflicts
            return this.GetPotentialImpactsExcludingLocale(left).Intersect(this.GetPotentialImpactsExcludingLocale(right)).Any();
        }

        /// <summary>Get all dates which the given conditions can potentially affect. This does not account for locale (which is checked separately) and weather (which can affect any day anyway).</summary>
        /// <param name="conditions">The condition key.</param>
        private IEnumerable<SDate> GetPotentialImpactsExcludingLocale(ConditionDictionary conditions)
        {
            // get implied values
            HashSet<int> days = new HashSet<int>(conditions.GetImpliedValues(ConditionKey.Day).Select(int.Parse));
            HashSet<string> seasons = new HashSet<string>(conditions.GetImpliedValues(ConditionKey.Season), StringComparer.InvariantCultureIgnoreCase);
            HashSet<DayOfWeek> daysOfWeek = new HashSet<DayOfWeek>(conditions.GetImpliedValues(ConditionKey.DayOfWeek).Select(name => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), name, ignoreCase: true)));

            // get all potentially impacted dates
            foreach (string season in this.ValidValues[ConditionKey.Season])
            {
                foreach (int day in this.ValidValues[ConditionKey.Day].Select(int.Parse))
                {
                    SDate date = new SDate(day, season.ToLower());

                    // matches any date
                    if (!conditions.Any())
                        yield return date;

                    // matches date
                    else if (seasons.Contains(date.Season) && days.Contains(date.Day) && daysOfWeek.Contains(date.DayOfWeek))
                        yield return date;
                }
            }
        }
    }
}
