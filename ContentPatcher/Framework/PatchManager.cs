using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Utilities;
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
        /// <summary>Handles constructing, permuting, and updating condition dictionaries.</summary>
        private readonly ConditionFactory ConditionFactory = new ConditionFactory();

        /// <summary>Whether to enable verbose logging.</summary>
        private readonly bool Verbose;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The patches to apply indexed by asset name.</summary>
        private readonly IDictionary<string, IList<IPatch>> Patches = new Dictionary<string, IList<IPatch>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>The current condition context.</summary>
        private readonly ConditionContext ConditionContext;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="language">The current language.</param>
        /// <param name="verboseLog">Whether to enable verbose logging.</param>
        public PatchManager(IMonitor monitor, LocalizedContentManager.LanguageCode language, bool verboseLog)
        {
            this.Monitor = monitor;
            this.ConditionContext = new ConditionContext(language);
            this.Verbose = verboseLog;
        }

        /****
        ** Patching
        ****/
        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            bool canLoad = this.GetCurrentLoaders(asset).Any();
            this.VerboseLog($"Can load: {canLoad} ({asset.AssetName})");
            return canLoad;
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            bool canEdit = this.GetCurrentEditors(asset).Any();
            this.VerboseLog($"Can edit: {canEdit} ({asset.AssetName})");
            return canEdit;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            IPatch patch = this.GetCurrentLoaders(asset).FirstOrDefault();
            if (patch == null)
                throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because no patches currently apply. This should never happen.");

            if (this.Verbose)
                this.VerboseLog($"Patch \"{patch.LogName}\" loaded {asset.AssetName}.");
            else
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

            InvariantHashSet loggedContentPacks = new InvariantHashSet();
            foreach (IPatch patch in patches)
            {
                if (this.Verbose)
                    this.VerboseLog($"Applied patch \"{patch.LogName}\" to {asset.AssetName}.");
                else if (loggedContentPacks.Add(patch.ContentPack.Manifest.Name))
                    this.Monitor.Log($"{patch.ContentPack.Manifest.Name} edited {asset.AssetName}.", LogLevel.Trace);

                try
                {
                    patch.Edit<T>(asset);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"unhandled exception applying patch: {patch.LogName}.\n{ex}", LogLevel.Error);
                }
            }
        }

        /// <summary>Update the current context.</summary>
        /// <param name="contentHelper">The content helper through which to invalidate assets.</param>
        /// <param name="language">The current language.</param>
        /// <param name="date">The current in-game date (if applicable).</param>
        /// <param name="weather">The current in-game weather (if applicable).</param>
        public void UpdateContext(IContentHelper contentHelper, LocalizedContentManager.LanguageCode language, SDate date, Weather? weather)
        {
            this.VerboseLog("Propagating context...");

            // update context
            this.ConditionContext.Update(language, date, weather);

            // update patches
            InvariantHashSet reloadAssetNames = new InvariantHashSet();
            foreach (string assetName in this.Patches.Keys)
            {
                foreach (IPatch patch in this.Patches[assetName])
                {
                    bool wasApplied = patch.MatchesContext;
                    bool changed = patch.UpdateContext(this.ConditionContext);
                    bool shouldApply = patch.MatchesContext;
                    bool reload = (wasApplied && changed) || (!wasApplied && shouldApply);

                    this.VerboseLog($"   {patch.LogName}: should apply {wasApplied} => {shouldApply}, changed={changed}, will reload={reload}");

                    if (reload)
                        reloadAssetNames.Add(assetName);
                }
            }

            // reload if needed
            if (reloadAssetNames.Any())
            {
                this.VerboseLog($"   reloading {reloadAssetNames.Count} assets...");
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
            this.VerboseLog($"      added {patch.Type} {patch.AssetName}.");
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
                .Where(patch => patch.Type == PatchType.Load && patch.MatchesContext);
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
                .Where(patch => patch.Type == patchType && patch.MatchesContext);
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
                conditions = this.ConditionFactory.BuildEmpty();
                error = null;
                return true;
            }

            // parse conditions
            conditions = this.ConditionFactory.BuildEmpty();
            foreach (KeyValuePair<string, string> pair in raw)
            {
                // parse condition key
                if (!Enum.TryParse(pair.Key, true, out ConditionKey key))
                {
                    error = $"'{pair.Key}' isn't a valid condition; must be one of {string.Join(", ", this.ConditionFactory.GetValidConditions())}";
                    conditions = null;
                    return false;
                }

                // parse values
                InvariantHashSet values = this.ParseCommaDelimitedField(pair.Value);
                if (!values.Any())
                {
                    error = $"{key} can't be empty";
                    conditions = null;
                    return false;
                }

                // restrict to allowed values
                InvariantHashSet validValues = new InvariantHashSet(this.ConditionFactory.GetValidValues(key));
                {
                    string[] invalidValues = values.Except(validValues, StringComparer.InvariantCultureIgnoreCase).ToArray();
                    if (invalidValues.Any())
                    {
                        error = $"invalid {key} values ({string.Join(", ", invalidValues)}); expected one of {string.Join(", ", validValues)}";
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

        /// <summary>Parse a comma-delimited set of case-insensitive condition values.</summary>
        /// <param name="field">The field value to parse.</param>
        public InvariantHashSet ParseCommaDelimitedField(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return new InvariantHashSet();

            IEnumerable<string> values = (
                from value in field.Split(',')
                where !string.IsNullOrWhiteSpace(value)
                select value.Trim().ToLower()
            );
            return new InvariantHashSet(values);
        }

        /// <summary>Get all possible values of a tokenable string.</summary>
        /// <param name="tokenable">The tokenable string.</param>
        /// <param name="conditions">The conditions for which to filter permutations.</param>
        public IEnumerable<string> GetPermutations(TokenString tokenable, ConditionDictionary conditions)
        {
            // no tokens: return original string
            if (!tokenable.ConditionTokens.Any())
            {
                yield return tokenable.Raw;
                yield break;
            }

            // yield token permutations
            foreach (IDictionary<ConditionKey, string> permutation in this.ConditionFactory.GetConditionPermutations(tokenable.ConditionTokens, conditions))
                yield return tokenable.GetStringWithTokens(permutation);
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
            InvariantHashSet seasons = new InvariantHashSet(conditions.GetImpliedValues(ConditionKey.Season));
            HashSet<DayOfWeek> daysOfWeek = new HashSet<DayOfWeek>(conditions.GetImpliedValues(ConditionKey.DayOfWeek).Select(name => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), name, ignoreCase: true)));

            // get all potentially impacted dates
            foreach (string season in this.ConditionFactory.GetValidValues(ConditionKey.Season))
            {
                foreach (int day in this.ConditionFactory.GetValidValues(ConditionKey.Day).Select(int.Parse))
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

        /// <summary>Log a message if <see cref="Verbose"/> is enabled.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The log level.</param>
        private void VerboseLog(string message, LogLevel level = LogLevel.Trace)
        {
            if (this.Verbose)
                this.Monitor.Log(message, level);
        }
    }
}
