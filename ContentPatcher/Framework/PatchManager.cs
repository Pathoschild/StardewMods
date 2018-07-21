using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
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
        /****
        ** Constants
        ****/
        /// <summary>The condition types which require a <see cref="ConditionKey.ForID"/> value.</summary>
        private readonly HashSet<ConditionType> TypesRequireID = new HashSet<ConditionType>
        {
            ConditionType.Hearts,
            ConditionType.Relationship
        };

        /// <summary>The minimum format versions for newer condition types.</summary>
        private readonly IDictionary<string, HashSet<ConditionType>> MinimumVersions = new Dictionary<string, HashSet<ConditionType>>
        {
            ["1.4"] = new HashSet<ConditionType>
            {
                ConditionType.DayEvent,
                ConditionType.HasFlag,
                ConditionType.HasSeenEvent,
                ConditionType.Hearts,
                ConditionType.Relationship,
                ConditionType.Spouse
            }
        };

        /****
        ** State
        ****/
        /// <summary>Handles constructing, permuting, and updating conditions.</summary>
        private readonly ConditionFactory ConditionFactory;

        /// <summary>Whether to enable verbose logging.</summary>
        private readonly bool Verbose;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The patches which are permanently disabled for this session.</summary>
        private readonly IList<DisabledPatch> PermanentlyDisabledPatches = new List<DisabledPatch>();

        /// <summary>The patches to apply.</summary>
        private readonly HashSet<IPatch> Patches = new HashSet<IPatch>();

        /// <summary>The patches to apply, indexed by asset name.</summary>
        private InvariantDictionary<HashSet<IPatch>> PatchesByCurrentTarget = new InvariantDictionary<HashSet<IPatch>>();

        /// <summary>A loader lookup by asset name, used to detect potential conflicts when adding loaders.</summary>
        private readonly InvariantDictionary<ISet<IPatch>> LoaderCache = new InvariantDictionary<ISet<IPatch>>();

        /// <summary>Normalise an asset name.</summary>
        private readonly Func<string, string> NormaliseAssetName;


        /*********
        ** Accessors
        *********/
        /// <summary>The current condition context.</summary>
        public ConditionContext ConditionContext { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="conditionFactory">Handles constructing, permuting, and updating conditions.</param>
        /// <param name="verboseLog">Whether to enable verbose logging.</param>
        /// <param name="normaliseAssetName">Normalise an asset name.</param>
        /// <param name="language">The current language.</param>
        public PatchManager(IMonitor monitor, ConditionFactory conditionFactory, bool verboseLog, Func<string, string> normaliseAssetName, LocalizedContentManager.LanguageCode language)
        {
            this.Monitor = monitor;
            this.ConditionFactory = conditionFactory;
            this.ConditionContext = conditionFactory.BuildContext(language);
            this.Verbose = verboseLog;
            this.NormaliseAssetName = normaliseAssetName;
        }

        /****
        ** Patching
        ****/
        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            bool canLoad = this.GetCurrentLoaders(asset).Any();
            this.VerboseLog($"check: [{(canLoad ? "X" : " ")}] can load {asset.AssetName}");
            return canLoad;
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            bool canEdit = this.GetCurrentEditors(asset).Any();
            this.VerboseLog($"check: [{(canEdit ? "X" : " ")}] can edit {asset.AssetName}");
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

            T data = patch.Load<T>(asset);
            patch.IsApplied = true;
            return data;
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
                    patch.IsApplied = true;
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"unhandled exception applying patch: {patch.LogName}.\n{ex}", LogLevel.Error);
                    patch.IsApplied = false;
                }
            }
        }

        /// <summary>Set the initial context values that aren't expected to change.</summary>
        /// <param name="installedMods">The installed mod IDs.</param>
        public void SetInitialContext(string[] installedMods)
        {
            this.ConditionContext.Set(ConditionKey.HasMod, installedMods);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="contentHelper">The content helper which manages game assets.</param>
        public void UpdateContext(IContentHelper contentHelper)
        {
            if (Context.IsWorldReady)
            {
                this.UpdateContext(
                    contentHelper: contentHelper,
                    language: contentHelper.CurrentLocaleConstant,
                    date: SDate.Now(),
                    weather: this.GetCurrentWeather(),
                    dayEvent: this.GetDayEvent(contentHelper),
                    spouse: Game1.player?.spouse,
                    seenEvents: Game1.player?.eventsSeen.OrderBy(p => p).ToArray(),
                    mailFlags: this.GetMailFlags().OrderBy(p => p).ToArray(),
                    friendships: Game1.player?.friendshipData.Pairs
                );
            }
            else
            {
                this.UpdateContext(
                    contentHelper: contentHelper,
                    language: contentHelper.CurrentLocaleConstant,
                    date: null,
                    weather: null,
                    dayEvent: null,
                    spouse: null,
                    seenEvents: null,
                    mailFlags: null,
                    friendships: null
                );
            }
        }

        /// <summary>Update the current context.</summary>
        /// <param name="contentHelper">The content helper which manages game assets.</param>
        /// <param name="language">The current language.</param>
        /// <param name="date">The current in-game date (if applicable).</param>
        /// <param name="weather">The current in-game weather (if applicable).</param>
        /// <param name="spouse">The current player's internal spouse name (if applicable).</param>
        /// <param name="dayEvent">The day event (e.g. wedding or festival) occurring today (if applicable).</param>
        /// <param name="seenEvents">The event IDs which the player has seen.</param>
        /// <param name="mailFlags">The mail flags set for the player.</param>
        /// <param name="friendships">The current player's friendship details.</param>
        public void UpdateContext(IContentHelper contentHelper, LocalizedContentManager.LanguageCode language, SDate date, Weather? weather, string dayEvent, string spouse, int[] seenEvents, string[] mailFlags, IEnumerable<KeyValuePair<string, Friendship>> friendships)
        {
            this.VerboseLog("Propagating context...");

            // update context
            this.ConditionContext.Set(language: language, date: date, weather: weather, dayEvent: dayEvent, spouse: spouse, seenEvents: seenEvents, mailFlags: mailFlags, friendships: friendships);
            IDictionary<ConditionKey, string> tokenisableConditions = this.ConditionContext.GetSingleValueConditions();

            // update patches
            InvariantHashSet reloadAssetNames = new InvariantHashSet();
            string prevAssetName = null;
            foreach (IPatch patch in this.Patches.OrderBy(p => p.AssetName).ThenBy(p => p.LogName))
            {
                // log asset name
                if (this.Verbose && prevAssetName != patch.AssetName)
                {
                    this.VerboseLog($"   {patch.AssetName}:");
                    prevAssetName = patch.AssetName;
                }

                // track old values
                string wasAssetName = patch.AssetName;
                bool wasApplied = patch.MatchesContext;

                // update patch
                bool changed = patch.UpdateContext(this.ConditionContext, tokenisableConditions);
                bool shouldApply = patch.MatchesContext;

                // track patches to reload
                bool reload = (wasApplied && changed) || (!wasApplied && shouldApply);
                if (reload)
                {
                    patch.IsApplied = false;
                    if (wasApplied)
                        reloadAssetNames.Add(wasAssetName);
                    if (shouldApply)
                        reloadAssetNames.Add(patch.AssetName);
                }

                // log change
                if (this.Verbose)
                {
                    IList<string> changes = new List<string>();
                    if (wasApplied != shouldApply)
                        changes.Add(shouldApply ? "enabled" : "disabled");
                    if (wasAssetName != patch.AssetName)
                        changes.Add($"target: {wasAssetName} => {patch.AssetName}");
                    string changesStr = string.Join(", ", changes);

                    this.VerboseLog($"      [{(shouldApply ? "X" : " ")}] {patch.LogName}: {(changes.Any() ? changesStr : "OK")}");
                }
            }

            // rebuild asset name lookup
            this.PatchesByCurrentTarget = new InvariantDictionary<HashSet<IPatch>>(
                from patchGroup in this.Patches.GroupBy(p => p.AssetName, StringComparer.InvariantCultureIgnoreCase)
                let key = patchGroup.Key
                let value = new HashSet<IPatch>(patchGroup)
                select new KeyValuePair<string, HashSet<IPatch>>(key, value)
            );

            // reload assets if needed
            if (reloadAssetNames.Any())
            {
                this.VerboseLog($"   reloading {reloadAssetNames.Count} assets: {string.Join(", ", reloadAssetNames.OrderBy(p => p))}");
                contentHelper.InvalidateCache(asset =>
                {
                    this.VerboseLog($"      [{(reloadAssetNames.Contains(asset.AssetName) ? "X" : " ")}] reload {asset.AssetName}");
                    return reloadAssetNames.Contains(asset.AssetName);
                });
            }
        }

        /****
        ** Patches
        ****/
        /// <summary>Add a patch.</summary>
        /// <param name="patch">The patch to add.</param>
        public void Add(IPatch patch)
        {
            // set initial context
            patch.UpdateContext(this.ConditionContext, this.ConditionContext.GetSingleValueConditions());

            // validate loader
            if (patch.Type == PatchType.Load)
            {
                // detect conflicts
                if (this.GetConflictingLoaders(patch).Any())
                    throw new InvalidOperationException($"Can't add {patch.Type} patch because it conflicts with an already-registered loader.");

                // track loader
                foreach (string assetName in this.GetPossibleAssetNames(patch))
                {
                    if (!this.LoaderCache.ContainsKey(assetName))
                        this.LoaderCache.Add(assetName, new HashSet<IPatch>());
                    this.LoaderCache[assetName].Add(patch);
                }
            }

            // add to patch list
            this.VerboseLog($"      added {patch.Type} {patch.AssetName}.");
            this.Patches.Add(patch);

            // add to lookup cache
            if (this.PatchesByCurrentTarget.TryGetValue(patch.AssetName, out HashSet<IPatch> patches))
                patches.Add(patch);
            else
                this.PatchesByCurrentTarget[patch.AssetName] = new HashSet<IPatch> { patch };
        }

        /// <summary>Add a patch that's permanently disabled for this session.</summary>
        /// <param name="patch">The patch to add.</param>
        public void AddPermanentlyDisabled(DisabledPatch patch)
        {
            this.PermanentlyDisabledPatches.Add(patch);
        }

        /// <summary>Get valid patches regardless of context.</summary>
        public IEnumerable<IPatch> GetPatches()
        {
            return this.Patches;
        }

        /// <summary>Get valid patches regardless of context.</summary>
        /// <param name="assetName">The asset name for which to find patches.</param>
        public IEnumerable<IPatch> GetPatches(string assetName)
        {
            if (this.PatchesByCurrentTarget.TryGetValue(assetName, out HashSet<IPatch> patches))
                return patches;
            return new IPatch[0];
        }

        /// <summary>Get patches which are permanently disabled for this session, along with the reason they were.</summary>
        public IEnumerable<DisabledPatch> GetPermanentlyDisabledPatches()
        {
            return this.PermanentlyDisabledPatches;
        }

        /// <summary>Get patches which load the given asset in the current context.</summary>
        /// <param name="asset">The asset being intercepted.</param>
        public IEnumerable<IPatch> GetCurrentLoaders(IAssetInfo asset)
        {
            return this
                .GetPatches(asset.AssetName)
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
                .GetPatches(asset.AssetName)
                .Where(patch => patch.Type == patchType && patch.MatchesContext);
        }

        /// <summary>Find any <see cref="PatchType.Load"/> patches which conflict with the given patch, taking conditions into account.</summary>
        /// <param name="newPatch">The new patch to check.</param>
        public InvariantDictionary<IPatch> GetConflictingLoaders(IPatch newPatch)
        {
            InvariantDictionary<IPatch> conflicts = new InvariantDictionary<IPatch>();

            if (newPatch.Type == PatchType.Load)
            {
                foreach (string assetName in this.GetPossibleAssetNames(newPatch))
                {
                    if (!this.LoaderCache.TryGetValue(assetName, out ISet<IPatch> otherPatches))
                        continue;

                    foreach (IPatch otherPatch in otherPatches)
                    {
                        if (this.ConditionFactory.CanConditionsOverlap(newPatch.Conditions, otherPatch.Conditions))
                        {
                            conflicts[assetName] = otherPatch;
                            break;
                        }
                    }
                }
            }

            return conflicts;
        }

        /****
        ** Condition parsing
        ****/
        /// <summary>Normalise and parse the given condition values.</summary>
        /// <param name="raw">The raw condition values to normalise.</param>
        /// <param name="formatVersion">The format version specified by the content pack.</param>
        /// <param name="latestFormatVersion">The latest format version.</param>
        /// <param name="conditions">The normalised conditions.</param>
        /// <param name="error">An error message indicating why normalisation failed.</param>
        public bool TryParseConditions(InvariantDictionary<string> raw, ISemanticVersion formatVersion, ISemanticVersion latestFormatVersion, out ConditionDictionary conditions, out string error)
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
                if (!ConditionKey.TryParse(pair.Key, out ConditionKey key))
                {
                    error = $"'{pair.Key}' isn't a valid condition; must be one of {string.Join(", ", Enum.GetValues(typeof(ConditionType)))}";
                    conditions = null;
                    return false;
                }

                // validate types which require an ID
                if (this.TypesRequireID.Contains(key.Type) && string.IsNullOrWhiteSpace(key.ForID))
                {
                    error = $"{key.Type} conditions must specify a separate ID (see readme for usage)";
                    conditions = null;
                    return false;
                }

                // check compatibility
                foreach (var versionPair in this.MinimumVersions)
                {
                    if (formatVersion.IsOlderThan(versionPair.Key) && versionPair.Value.Contains(key.Type))
                    {
                        error = $"{key} isn't available with format version {formatVersion} (change the {nameof(ContentConfig.Format)} field to {latestFormatVersion} to use newer features)";
                        conditions = null;
                        return false;
                    }
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
                string[] rawValidValues = this.ConditionFactory.GetValidValues(key)?.ToArray();
                if (rawValidValues?.Any() == true)
                {
                    InvariantHashSet validValues = new InvariantHashSet(rawValidValues);
                    {
                        string[] invalidValues = values.Except(validValues, StringComparer.InvariantCultureIgnoreCase).ToArray();
                        if (invalidValues.Any())
                        {
                            error = $"invalid {key} values ({string.Join(", ", invalidValues)}); expected one of {string.Join(", ", validValues)}";
                            conditions = null;
                            return false;
                        }
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


        /*********
        ** Private methods
        *********/
        /// <summary>Get the current weather from the game state.</summary>
        private Weather GetCurrentWeather()
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || Game1.weddingToday)
                return Weather.Sun;

            if (Game1.isSnowing)
                return Weather.Snow;
            if (Game1.isRaining)
                return Game1.isLightning ? Weather.Storm : Weather.Rain;

            return Weather.Sun;
        }

        /// <summary>Get the letter IDs and mail flags set for the player.</summary>
        /// <remarks>See game logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
        private IEnumerable<string> GetMailFlags()
        {
            Farmer player = Game1.player;
            if (player == null)
                return new string[0];

            return player
                .mailReceived
                .Union(player.mailForTomorrow)
                .Union(player.mailbox);
        }

        /// <summary>Get the name for today's day event (e.g. wedding or festival) from the game data.</summary>
        /// <param name="contentHelper">The content helper from which to load festival data.</param>
        private string GetDayEvent(IContentHelper contentHelper)
        {
            // marriage
            if (Game1.weddingToday)
                return "wedding";

            // festival
            IDictionary<string, string> festivalDates = contentHelper.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates", ContentSource.GameContent);
            if (festivalDates.TryGetValue($"{Game1.currentSeason}{Game1.dayOfMonth}", out string festivalName))
                return festivalName;

            return null;
        }

        /// <summary>Get all possible normalised <see cref="IPatch.AssetName"/> values for a patch.</summary>
        /// <param name="patch">The patch to check.</param>
        private IEnumerable<string> GetPossibleAssetNames(IPatch patch)
        {
            foreach (string rawAssetName in this.ConditionFactory.GetPossibleStrings(patch.TokenableAssetName, patch.Conditions))
                yield return this.NormaliseAssetName(rawAssetName);
        }

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
