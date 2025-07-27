[← back to readme](README.md)

# Release notes
<!--


STOP RIGHT THERE.
When releasing a format change, don't forget to update the smapi.io/json schema!


-->
## 2.7.5
Released 27 July 2025 for SMAPI 4.1.10 or later.

* Fixed errors in split-screen mode in some cases if `Random` is initialized too early.
* Improved [Chinese documentation](zh/README.md) (thanks to SummerFleur2997 with review by mushymato!).

## 2.7.4
Released 19 July 2025 for SMAPI 4.1.10 or later.

* Fixed `{{Random}}` being less random than intended.

## 2.7.3
Released 13 July 2025 for SMAPI 4.1.10 or later.

* Added optional integration with the Profiler mod for performance monitoring (thanks to SinZ!).

## 2.7.2
Released 06 June 2025 for SMAPI 4.1.10 or later.

* Added [Chinese documentation](zh/README.md) (thanks to mushymato with help from Luo7710!).
* Fixed error migrating pre-Stardew-Valley-1.6 location edits in some cases.

## 2.7.1
Released 28 May 2025 for SMAPI 4.1.10 or later.

* Fixed content packs which incorrectly set `EndSlideShow` in `Data/Characters` to a true/false value.  
  _Content Patcher 2.7.0 fixed a bug that allowed setting enum fields to a boolean value; 2.7.1 adds a format migration so older content packs will work like before._

## 2.7.0
Released 27 May 2025 for SMAPI 4.1.10 or later. See [release highlights](https://www.patreon.com/posts/130042997).

* The `Priority` patch field is now sent to SMAPI to influence patch order relative to non-Content Patcher mods too (thanks to ichortower!).
* The `patch export` command now auto-detects the type for all vanilla top-level data assets (thanks to SinZ!).
* Fixed `EditData` patches appending to a list/dictionary field instead of replacing it in some cases.
* Fixed `EditData`'s handling of certain Windows-only data formats when playing on Android/Linux/macOS.
* Fixed `EditData`'s handling of data model fields with tokens in both the name and value (thanks to SinZ!).
* Fixed `EditData` incorrectly allowing true/false values for enum fields and mapping them transitively (like `true` → `1` → `"MainGroup"`).
* Clarified in [token docs](author-guide/tokens.md) that tokens use NPCs' internal names (thanks to DenisSilent!).

**Update notes for players:**  
If you see new errors after updating Content Patcher, make sure you update your other mods to their
latest versions. If the errors still happen, feel free to report them on the Content Patcher mod
page.

**Update notes for mod authors:**  
* Previously all patches were sent to SMAPI with a priority of `Exclusive` (load) or `Default` (edit). Content Patcher
  now sends the `Priority` value instead if set, so the priority is relative to non-Content Patcher mods too. This may
  change the order of some edits.
* `EditData` no longer appends to a list/dictionary in some cases instead of replacing.

## 2.6.1
Released 27 March 2025 for SMAPI 4.1.10 or later.

* Fixed 'unsupported format version 2.6.0' error when mods set `"Format": "2.6.0"`.

## 2.6.0
Released 26 March 2025 for SMAPI 4.1.10 or later. See [release highlights](https://www.patreon.com/posts/125098195).

* Added `AddNpcWarps` field in [`EditMap` patches](author-guide.md#editmap).
* Added `defaultKeys` option for [the `i18n` token](author-guide/translations.md).
* Fixed some cases where a local token on an `Include` patch wasn't available in the included patches.

## 2.5.3
Released 08 February 2025 for SMAPI 4.1.10 or later.

* Fixed backwards compatibility with Content Patcher Animations.

## 2.5.2
Released 07 February 2025 for SMAPI 4.1.10 or later.

* Internal changes to Generic Mod Config Menu integration.

## 2.5.1
Released 13 January 2025 for SMAPI 4.1.10 or later.

* Improved the new [local tokens feature](author-guide/tokens.md#local-tokens):
  * Local tokens can now use field tokens like `{{FromFile}}`, but can no longer be used in the `FromFile` and `Target` fields (except when inherited).
  * Fixed local tokens not working in some patch fields.
  * Fixed error loading patches with non-ready local tokens.
  * Fixed patches not updated correctly when a local token uses another token whose value changed.
* Fixed dynamic tokens not applied immediately on load if their initial value is ready.

## 2.5.0
Released 11 January 2025 for SMAPI 4.1.10 or later. See [release highlights](https://www.patreon.com/posts/119812841).

* Added [local tokens feature](author-guide/tokens.md#local-tokens) (thanks to collaboration with spacechase0!).
* When a list entry can't be moved via `MoveEntries` because the given ID doesn't exist, Content Patcher now logs a `TRACE` message instead of `WARN`.
* Fixed warnings for mods using `Hearts` or `Relationship` conditions with an NPC who's in the data but not added to the world yet.
* Fixed `patch_summary` error if a patch has a null target.

## 2.4.4
Released 08 November 2024 for SMAPI 4.1.6 or later.

* In SMAPI 4.1._x_, [gender-switch blocks](https://stardewvalleywiki.com/Modding:Dialogue#Gender_switch) are preprocessed automatically by default. That no longer applies to the `{{i18n}}` token.  
  _That didn't work with [patch update rates](author-guide.md#update-rate), and gender-switch blocks still work fine for translations passed to the game since they'll be parsed by the game._
* Fixed `{{i18n}}` making text lowercase if the entire text matches a special case like `true` or `false`.

## 2.4.3
Released 07 November 2024 for SMAPI 4.1.5 or later.

* Fixed `i18n` tokens containing gender-switch blocks not updating when the gender changes.

## 2.4.2
Released 05 November 2024 for SMAPI 4.1.4 or later.

* Fixed `CustomLocations` locations not working in Stardew Valley 1.6.9+.

## 2.4.1
Released 04 November 2024 for SMAPI 4.1.2 or later.

* Fixed the previous update being broken on Linux/macOS.

## 2.4.0
Released 04 November 2024 for SMAPI 4.1.0 or later. See the [release highlights](https://www.patreon.com/posts/115260984).

* Updated for Stardew Valley 1.6.9.
* Added `ReplaceDelimited` [text operation](author-guide/text-operations.md) (thanks to dmorsecode!).
* The [`patch reload` command](author-guide/troubleshooting.md#reload) can now reload patches from a specific `Include` patch (thanks to spacechase0!).
* Fixed `EditMap` patches with multiple `Target` or `FromFile` values not applying `SetProperties`.

## 2.3.0
Released 29 June 2024 for SMAPI 4.0.7 or later. See the [release highlights](https://www.patreon.com/posts/107147109).

* Added support for editing list-of-value fields.
* The [`patch summary` console command](author-guide/troubleshooting.md#summary) can now filter by asset name(s) using a new `asset` argument.

## 2.2.0
Released 08 June 2024 for SMAPI 4.0.7 or later. See the [release highlights](https://www.patreon.com/posts/105792446).

* Added new tokens:
  * [`FarmMapAsset`](author-guide/tokens.md#FarmMapAsset) for the farm type's map [asset name](https://stardewvalleywiki.com/Modding:Common_data_field_types#Asset_name) without the `Maps/` prefix (like `Farm_Combat` for the wilderness farm).
  * [`HasVisitedLocation`](author-guide/tokens.md#HasVisitedLocation) for the locations a player has visited before.
* Re-enabled Content Patcher 2.1.0's locales change for all content packs.  
  _This was temporarily disabled for older content packs in 2.1.1 so affected content packs could be updated._
* Raised minimum versions to SMAPI 4.0.7 and Stardew Valley 1.6.4.  
  _This avoids errors due to breaking changes in earlier 1.6 patches._
* Internal refactoring.
* Fixed the `HavingChild` and `Pregnant` tokens not accounting for the new `SpouseAdopts` field in `Data/Characters`.

## 2.1.2
Released 23 May 2024 for SMAPI 4.0.0 or later.

* Fixed some older content packs broken in Content Patcher 2.1.0+.

## 2.1.1
Released 23 May 2024 for SMAPI 4.0.0 or later.

* The localization changes in Content Patcher 2.1 now only apply to newer content packs, to avoid breaking content packs which depend on the old behavior.

## 2.1.0
Released 22 May 2024 for SMAPI 4.0.0 or later. See the [release highlights](https://www.patreon.com/posts/104747064).

* Added `TargetLocale` field to patch a specific localized variant (like `Data/Bundles.fr-FR` but not `Data/Bundles`, or vice versa).
* Added [token string API for C# mods](token-strings-api.md) (thanks to spacechase0!).
* Editing content pack settings through Generic Mod Config Menu now force-updates all patches to apply the config changes.
* Fixed some mod edits not applying correctly for non-English players.
* Fixed `MigrateIds` action not applied correctly for some items (thanks to Digus!).

**Update notes for mod authors:**  
* Content Patcher 2.1 changes how `"Action": "Load"` patches are applied for non-English players. See the
  [2.1 migration guide](author-migration-guide.md#21) for more info.

## 2.0.6
Released 15 April 2024 for SMAPI 4.0.0 or later.

* Fixed various issues caused by pre-1.6 content packs replacing data assets which changed format in 1.6.
* Fixed runtime migrations for pre-1.6 content packs letting them remove 1.6 content.

## 2.0.5
Released 07 April 2024 for SMAPI 4.0.0 or later.

* Fixed furniture from pre-Stardew-Valley-1.6 content packs shown in-game as "Unnamed Item".

## 2.0.4
Released 05 April 2024 for SMAPI 4.0.0 or later.

* Fixed `MigrateIds` setting incorrect sprite for former Json Assets items when Json Assets is still installed.

## 2.0.3
Released 04 April 2024 for SMAPI 4.0.0 or later.

* Content packs can now migrate Json Assets items to Content Patcher using the [`MigrateIds` trigger action](author-guide/trigger-actions.md#migrateids). This works even if the Json Assets content pack is no longer installed.

## 2.0.2
Released 26 March 2024 for SMAPI 4.0.0 or later.

* Fixed runtime migrations for...
  * pre-1.6 `Data/Locations` patches when the new asset has invalid spawn conditions;
  * pre-1.6 `Data/ObjectInformation` patches which edit context tags (thanks to SinZ!), which fixes issues like mayonnaise machines not working when some older content packs are installed.

## 2.0.1
Released 20 March 2024 for SMAPI 4.0.0 or later.

* Fixed error migrating some content packs to Stardew Valley 1.6.

## 2.0.0
Released 19 March 2024 for SMAPI 4.0.0 or later. See the [release highlights](https://www.patreon.com/posts/100496385).

* Updated for Stardew Valley 1.6.
* Added load & edit priorities (see updated [action docs](author-guide.md#actions)).
* Added [`ModId`](author-guide/tokens.md#ModId) token to get the unique ID of the current content pack.
* Added runtime migrations for content assets which changed in Stardew Valley 1.6. (Thanks to SinZ for the help creating some of the main migrations!)
* Added trigger action to change content IDs automatically (see [new docs](author-guide/trigger-actions.md)). 
* Deprecated `CustomLocations`. This is now a shortcut for editing the new [`Data/Locations` asset](https://stardewvalleywiki.com/Modding:Location_data), and now allows the [new location name format](https://stardewvalleywiki.com/Modding:Common_data_field_types#Unique_string_ID).
* Removed `GroupEditsByMod` config option. Edits are now grouped automatically based on mod and priority.
* Fixed off-by-one position with `MoveEntries` when the target entry is already before the anchor entry.
* Fixed some tokens being briefly unavailable during part of the save load process (notably `Hearts` and `Relationship`). This affected some specific cases like editing island resort dialogue.
* Fixed some cases where a `HasMod` condition didn't properly prevent warnings when using a token provided by that mod if it's not installed (thanks to SinZ!).

**Update notes for mod authors:**  
* Stardew Valley 1.6 has major content changes. Your content packs may still work due to the new runtime migrations,
  but these can have a significant performance impact. [Updating your code to the latest format](author-migration-guide.md#20)
  is strongly recommended.

## 1.30.4
Released 01 December 2023 for SMAPI 3.18.1 or later.

* When a patch fails, Content Patcher now lists all issues in `patch summary` instead of the first one.
* Fixed edge cases where validation fails due to commented-out code in the content pack (thanks to atravita!).

## 1.30.3
Released 01 November 2023 for SMAPI 3.18.1 or later.

* Re-enabled optimizations disabled in 1.30.2.
* Fixed unhandled exception when loading patches with immutable-but-broken tokens.
* Fixed `Format` not recognizing version 1.30.0.

## 1.30.2
Released 04 October 2023 for SMAPI 3.18.1 or later.

* Temporarily disabled 1.30.0 optimizations to fix "_Can't get values from a non-ready token string_" warnings.

## 1.30.1
Released 03 October 2023 for SMAPI 3.18.1 or later.

* Fixed some edits no longer applied in 1.30.0.

## 1.30.0
Released 03 October 2023 for SMAPI 3.18.1 or later. See the [release highlights](https://www.patreon.com/posts/90281255).

* Optimized deterministic token input (thanks to SinZ!). This significantly improves performance for content packs which use `Range` for time checks or other large ranges.
* Fixed `EditMap`'s `MapTiles` removing current tile properties on the tiles being edited.
* Fixed error setting a property name to a token value when that token isn't ready. The patch will now be correctly marked non-ready instead.
* Fixed debug overlay on Android.

## 1.29.4
Released 27 August 2023 for SMAPI 3.18.1 or later.

* The `patch export` command is now better at guessing the export type for assets that aren't loaded yet (thanks to atravita!).
* Fixed `EditData` replacing fields explicitly set to null with empty strings.
* Fixed `IsJojaMartComplete` token not working consistently.
* Fixed `Render` token incorrectly including named arguments.

## 1.29.3
Released 26 June 2023 for SMAPI 3.18.1 or later.

* Fixed error migrating some older content packs after 1.29.2.

## 1.29.2
Released 25 June 2023 for SMAPI 3.18.1 or later.

* Using `TargetField` to add an entry to a null list or dictionary now auto-creates it.
* Improved errors when an `EditData` patch can't convert the data to the asset type or a target field doesn't exist.
* Embedded `.pdb` data into the DLL, which fixes error line numbers in Linux/macOS logs.
* Fixed setting map/tile properties to null no longer removing them.
* Fixed some migrations not applied to files loaded via `Action: Include`.

## 1.29.1
Released 31 March 2023 for SMAPI 3.18.1 or later.

* Fixed `Format` version 1.29.0 not recognized.

## 1.29.0
Released 30 March 2023 for SMAPI 3.18.1 or later. See the [release highlights](https://www.patreon.com/posts/80797967).

* You can now edit more complex mod data models using `EditData`. This fixes many cases where you'd encounter an error like "_this asset has X values (but Y values were provided)_".
* Improved `patch export` command:
   * Added support for custom data types.
   * Added support for maps (thanks to atravita!).
   * Enum fields are now exported as their constant name instead of their numeric value.
* Added friendly error if an image can't be resized due to changes by Sprites in Detail.
* Raised min SMAPI version to 3.18.1 to prepare for the upcoming Stardew Valley 1.6.

## 1.28.4
Released 09 January 2023 for SMAPI 3.15.0 or later.

* Updated `EditData` ID detection for the upcoming Stardew Valley 1.6.
* Improved error handling for `MoveEntries`.

## 1.28.3
Released 30 October 2022 for SMAPI 3.15.0 or later.

* Updated integration with Generic Mod Config Menu.
* Fixed `Include` patches still trying to load when non-ready.
* Fixed error migrating locations from TMXL Map Toolkit when it has duplicate location data.

## 1.28.2
Released 10 October 2022 for SMAPI 3.15.0 or later.

* Improved performance and reduced memory usage when parsing tokenizable strings (thanks to atravita!).
* Reduced memory usage for loaded content packs a bit.
* Disabled the compatibility workaround for PyTK when running in SMAPI strict mode.

## 1.28.1
Released 29 August 2022 for SMAPI 3.15.0 or later.

* Updated compatibility workaround for the recent PyTK update.

## 1.28.0
Released 18 August 2022 for SMAPI 3.15.0 or later. See the [release highlights](https://www.patreon.com/posts/70722755).

* Added `RemoveDelimited` text operation (thanks to Shockah!).

## 1.27.2
Released 04 July 2022 for SMAPI 3.15.0 or later.

* Fixed patches with tokenized `Target` fields not correctly reapplied on token change after 1.26.6.
* Fixed typo in PyTK compatibility message (it applies to PyTK 1.23.0 or earlier, not 1.23.1).

## 1.27.1
Released 19 June 2022 for SMAPI 3.15.0 or later.

* Fixed compatibility with PyTK's scale-up feature.  
  _When PyTK 1.23.0 or earlier is installed, this will disable the main performance improvements in Content Patcher 1.27.0._

## 1.27.0
Released 17 June 2022 for SMAPI 3.15.0 or later. See the [release highlights](https://www.patreon.com/posts/67923889).

* Migrated image edits to SMAPI's new [`IRawTextureData` asset type](https://stardewvalleywiki.com/Modding:Migrate_to_SMAPI_4.0#Raw_texture_data) to reduce load times and improve performance.
* Optimized token updates to reduce in-game lag for some players.

## 1.26.6
Released 05 June 2022 for SMAPI 3.14.0 or later.

* Rewrote patch change tracking, which should significantly improve load times and performance in some cases.
* Improved error when adding an invalid entry to a data model asset.

## 1.26.5
Released 27 May 2022 for SMAPI 3.14.0 or later.

* Fixed `EditData` patches in older content packs not applied correctly if `FromFile` has an immutable value.

## 1.26.4
Released 22 May 2022 for SMAPI 3.14.0 or later.

* Optimized load times and in-game performance.
* Optimized redundant reindex on context updates (thanks to SinZ163!).
* Fixed config UI dropdown values no longer matching order listed in `ConfigSchema`.
* Fixed custom tokens sometimes failing in SMAPI 3.14 with '_rejected token … because it could not be mapped_' error.

## 1.26.3
Released 16 May 2022 for SMAPI 3.14.0 or later.

* Fixed token normalization not applied to conditions in 1.26.2.

## 1.26.2
Released 15 May 2022 for SMAPI 3.14.0 or later.

* Optimized performance and memory allocation:
  * Migrated to immutable sets internally, so Content Patcher can avoid copying values in many cases.
  * Added predefined sets for common values to reduce allocations.
  * Optimized token normalization, string splitting, `EditData` edits, and `{{time}}` formatting.
  * Removed unneeded array copies.
* Fixed error when passing a null input to the `LowerCase`/`UpperCase` or `Render` tokens.
* Fixed `EditData` patches in older content packs not updated if their `FromFile` changes.

## 1.26.1
Released 11 May 2022 for SMAPI 3.14.0 or later.

* Optimized performance and memory allocation:
  * Reduced time spent updating patches (they now stop updating at the first unready field).
  * Reduced allocations for immutable values and empty inputs.
  * Removed unneeded/recursive yields.
* Fixed content packs which include the `.xnb` extension in the `Target` field.
* Fixed error when a content pack uses `HasFile` or `FirstValidFile` with a path which contains only empty tokens before the first path separator.

## 1.26.0
Released 09 May 2022 for SMAPI 3.14.0 or later. See the [release highlights](https://www.patreon.com/posts/66203059).

* Updated for SMAPI 3.14.0.
* Added support for [config UI sections](author-guide/config.md#display-options) (thanks to Shockah!).
* The `patch export` console command's optional type argument can now be `image` or an unqualified type name (thanks to atravita!).
* Fixed content packs reloaded unnecessarily on startup.
* Fixed `patch summary` showing non-ready token values in some cases.

## 1.25.0
Released 27 February 2022 for SMAPI 3.13.0 or later. See the [release highlights](https://www.patreon.com/posts/63137910).

* `EditData` patches are now more powerful. You can now...
  * edit deeply nested fields using [`TargetField`](author-guide/action-editdata.md#target-field);
  * edit arbitrary data models;
  * edit list entries by their index position;
  * and patch past the end of a delimited string field using `Fields`.
* Added [token aliases](author-guide/tokens.md#aliases) (thanks to Shockah!).
* Added new tokens:
  * [`AbsoluteFilePath`](author-guide/tokens.md#AbsoluteFilePath) to get the full path for a file in the content pack folder.
  * [`FormatAssetName`](author-guide/tokens.md#FormatAssetName) to normalize an asset name into the form expected by the game.
  * [`InternalAssetKey`](author-guide/tokens.md#InternalAssetKey) to let the game load a file directly from your content pack without needing to `Load` it separately.
* Added [`AnyPlayer` option](author-guide/tokens.md#target-player) for player tokens.
* The `Format` field now ignores the third number, so `1.25.1` is equivalent to `1.25.0` instead of showing an error.
* Improved startup times:
  * Internal optimizations (thanks to Michael Kuklinski / Ameisen!).
  * Fixed content packs being loaded/validated twice per screen.
* Fixed error loading a patch which has some required fields missing.
* Fixed float rounding in `DailyLuck` token.
* Removed warning for null patches (they're now silently ignored instead).
* Removed `Enabled` field in newer `Format` versions.

**Update notes for mod authors:**  
The `Enabled` patch field is no longer supported when the `Format` field is `1.25.0` or later. See the
[migration guide](author-migration-guide.md) for more info.

## 1.24.8
Released 14 January 2022 for SMAPI 3.13.0 or later.

* Fixed patches added through `Action: Include` not applied immediately in some cases.
* Fixed `Random` values not in sync in multiplayer in Stardew Valley 1.5.5+.

## 1.24.7
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed load error in the previous update.

## 1.24.6
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed location tokens unready during part of the save loading process.
* Fixed minimum supported Generic Mod Config Menu version.
* Internal changes to support Content Patcher Animations.
* Internal optimizations.

## 1.24.5
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0 (including new game features like custom languages and farm types).

## 1.24.4
Released 27 November 2021 for SMAPI 3.12.6 or later.

* Fixed "failed in the Specialized.LoadStageChanged event" error in some cases when loading a save.

## 1.24.3
Released 12 November 2021 for SMAPI 3.12.6 or later.

* Fixed `DayOfWeek` token set incorrectly on some days after 1.24.0.
* Fixed error loading older saves in some cases.
* Fixed error getting weather for a non-standard location context.
* Fixed error using `Count`, `Lowercase`/`Uppercase`, or `Render` with empty tokens as input.
* Fixed tokens parsed incorrectly in edge case when nesting unready tokens which have named arguments.

## 1.24.2
Released 01 November 2021 for SMAPI 3.12.6 or later.

* Fixed player tokens not defaulting to the current player while save is loading.
* Fixed 'no translation' tooltips shown for config fields with no description.

## 1.24.1
Released 31 October 2021 for SMAPI 3.12.6 or later.

* Fixed error when `{{Merge}}` is called with all input tokens empty.

## 1.24.0
Released 31 October 2021 for SMAPI 3.12.6 or later. See the [release highlights](https://www.patreon.com/posts/58121270).

* Added new tokens:
  * [`HasCookingRecipe`](author-guide/tokens.md#HasCookingRecipe) and [`HasCraftingRecipe`](author-guide/tokens.md#HasCraftingRecipe) to get the crafting/cooking recipes known by a player.
  * [`LocationOwnerId`](author-guide/tokens.md#LocationOwnerId) to get the player who owns a cabin, cellar, building, etc.
  * [`Merge`](author-guide/tokens.md#Merge) to combine tokens in `When` conditions or perform value fallback.
  * [`PathPart`](author-guide/tokens.md#PathPart) to get part of a file/asset path (e.g. for patches with multiple `Target` or `FromFile` values).
  * [`Roommate`](author-guide/tokens.md#Roommate) to get a player's roommate NPC (similar to `Spouse` for a married NPC).
* Added support for [translating content pack config options in Generic Mod Config Menu](author-guide/config.md#config-ui).
* Improved tokens:
  * Tokens which let you specify a player type now accept player IDs too (like `{{PlayerName: 3864039824286870457}}`).
  * You can now get per-player values for more tokens (specifically `IsMainPlayer`, `IsOutdoors`, `LocationContext`, `LocationName`, `LocationUniqueName`, `PlayerGender`, `PlayerName`, and `Spouse`).
* Improved context updates:
  * Built-in tokens are now available immediately after the raw save file is read, before the game even starts loading it.
  * The `Hearts` and `Relationships` tokens are now available before the save is fully loaded.
  * Improved performance (especially for immutable patches and conditions).
* Improved console commands:
  * Added `unsorted` option for [`patch summary`](author-guide.md#patch-summary).
  * Added `compact` option for [`patch parse`](author-guide.md#patch-parse).
* Updated for Generic Mod Config Menu 1.5.0.
* Fixed error using `Target*` and `FromFile` tokens as a condition key.
* Fixed conditions parsed through the C# API unable to use custom tokens added by the same mod.
* Fixed error when a mod manifest has dependencies with no ID.
* Fixed patch conditions not updated in rare cases.

**Update notes for mod authors:**
* The `Spouse` token no longer includes roommates when the `Format` field is `1.24.0` or later.
* Some tokens now return values in a different order.

See the [migration guide](author-migration-guide.md) for more info.

## 1.23.5
Released 18 September 2021 for SMAPI 3.12.6 or later.

* Fixed patches which use the `i18n` token not always updated on language change.
* Fixed `HasMod` condition not enabling mod-provided tokens within the same `When` block.

## 1.23.4
Released 04 September 2021 for SMAPI 3.12.6 or later.

* Fixed compatibility with the upcoming Stardew Valley 1.5.5.

## 1.23.3
Released 24 July 2021 for SMAPI 3.9.5 or later.

* Improved patch validation to detect more common mistakes:
  * using an `Include` patch with a `Target` field;
  * using a field reference token when the patch doesn't have that field.
* Tweaked the naming for patches with multiple `FromFile`/`Target` values to reduce confusion.
* Fixed incorrect "file does not exist" warnings shown for `Include` patches that use tokens in their `FromFile` field.

## 1.23.2
Released 09 July 2021 for SMAPI 3.9.5 or later.

* Added [`patch invalidate` console command](author-guide.md#patch-invalidate).
* Fixed issue where editing a list field using `EditData` and `Fields` would append the values instead of replacing them.
* Fixed broken custom locations also preventing valid custom locations from loading.
* Fixed `Query` token not allowing queries that contain a comma.
* Fixed include-file-doesn't-exist message incorrectly logged as `TRACE` instead of `WARN`.
* Fixed error using `patch export` command with the `Data\RandomBundles` file.

## 1.23.1
Released 25 May 2021 for SMAPI 3.9.5 or later.

* `EditMap` patches with `FromFile` no longer require `ToArea`. (It now defaults defaults to an area starting from the the top-left.)
* Fixed 'unknown reserved argument' errors in Content Patcher 1.23.0 for content packs which use `inputSeparator`.

## 1.23.0
Released 25 May 2021 for SMAPI 3.9.5 or later. See the [release highlights](https://www.patreon.com/posts/51685726).

* Added [`valueAt` argument](author-guide/tokens.md#valueat) to get one value from any token.
* Added `TextOperations` support for `EditData` fields.
* Added new tokens:
  * [`ChildGenders`](author-guide/tokens.md#ChildGenders) and [`ChildNames`](author-guide/tokens.md#ChildNames) to get the genders/names of a given player's children.
  * [`Count`](author-guide/tokens.md#Count) to get the number of values in a token (e.g. `{{Count: {{HasActiveQuest}} }}` for the number of active quests).
  * [`HasCaughtFish`](author-guide/tokens.md#HasCaughtFish) to get fish caught by a player.
* Improved sort order used in token values, patch commands, and error messages.
* Fixed patches not applied correctly in some cases when added by a conditional `Include` patch.
* Fixed error if `EnableDebugFeatures` is enabled and a debug overlay navigation key is pressed without a debug overlay open.
* Fixed tokens which accept a [`PlayerType`](author-guide/tokens.md#playertype) argument always marked as returning multiple values for input arguments.

**Note for SMAPI mod authors:**  
If you use the [extensibility API](extensibility.md) to add custom tokens, the order of values
they return affects the `valueAt` argument.

## 1.22.0
Released 17 April 2021 for SMAPI 3.9.5 or later. See the [release highlights](https://www.patreon.com/posts/50144071).

* Added a [conditions API](conditions-api.md), which lets other mods parse and use Content Patcher conditions.
* Added new tokens:
  * [`FirstValidFile`](author-guide/tokens.md#FirstValidFile) to enable fallback files without duplicating patches.
  * [`HasActiveQuest`](author-guide/tokens.md#HasActiveQuest) to check a player's current quest list.
* Improved console commands:
  * Added `patch export` argument to optionally set the data type.
  * Tweaked console command handling.
  * Fixed `patch export` for an asset that's not already loaded causing the wrong data type to be cached.
* The latest `Format` version now always matches the main Content Patcher version. Previously it only changed if the format changed.
* Fixed default log names for patches with multiple `FromFile` or `Target` values.

## 1.21.2
Released 27 March 2021 for SMAPI 3.9.5 or later.

* Simplified 'unsupported format' message to avoid confusion when players need to update Content Patcher.
* When using [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098), you can now configure content packs in-game after loading the save.
* Fixed error when editing entries in `Data\RandomBundles`.
* Fixed misplaced warps when replacing some farm types.
* Fixed setting a map tile property to `null` not deleting it.
* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 1.21.1
Released 07 March 2021 for SMAPI 3.9.3 or later.

* Fixed 'changes the save serializer' warning in 1.21.

## 1.21.0
Released 07 March 2021 for SMAPI 3.9.3 or later. See the [release highlights](https://www.patreon.com/posts/48471994).

* Added support for [creating custom locations](author-guide.md#custom-locations).
* Added `AddWarps` field in [`EditMap` patches](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/author-guide.md#editmap).
* Added new tokens:
  * [`Render`](author-guide/tokens.md#string-manipulation) to allow string comparison in `When` blocks.
  * `DailyLuck` to get a player's daily luck (thanks to Thom1729!).
* The `FarmhouseUpgrade` token can now check either the current player (default) or the host player.
* The `Enabled` field no longer allows tokens (in format version 1.21.0+).
* Improved default `LogName` for patches with multiple `Target` or `FromFile` values.
* Improved split-screen support.
* Fixed changes through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) not correctly updating dynamic tokens and `Enabled` fields.
* Fixed `patch reload` command not reapplying format migrations to reloaded patches.
* Fixed error patching `Data\Concessions` using `EditData`.

**Update note for mod authors:**  
If you use tokens in the `Enabled` field, updating the `Format` field to `1.21.0` or later will
cause errors. See the [migration guide](author-migration-guide.md) for more info.

## 1.20.0
Released 06 February 2021 for SMAPI 3.9.0 or later. See the [release highlights](https://www.patreon.com/posts/47213526).

* Improved tokens:
  * Added `LocationContext` (the world area recognized by the game like `Island` or `Valley`).
  * Added `LocationUniqueName` (the unique name for constructed building and cabin interiors).
  * `Weather` now returns weather for the current location context by default, instead of always returning the valley's weather.
  * You can now use an optional argument like `{{Weather: Valley}}` to get the weather for a specific context.
* You can now set translation token values through `i18n` token arguments.
* Added console commands:
  * `patch dump applied` shows all active patches grouped by target in their apply order, including whether each patch is applied.
  * `patch dump order` shows the global definition order for all loaded patches.
* Fixed patch order not guaranteed when `Include` patches get reloaded.
* Improved performance for content packs using tokenized conditions in patches updated on time change.
* Config fields consisting of a numeric range are now formatted as a slider in [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).

**Update note for mod authors:**  
If you use the `Weather` token, updating the `Format` field to `1.20.0` or later changes its
behavior. See the [migration guide](author-migration-guide.md) for more info.

## 1.19.4
Released 23 January 2021 for SMAPI 3.9.0 or later.

* Updated for multi-key bindings in SMAPI 3.9.

## 1.19.3
Released 10 January 2021 for SMAPI 3.8.0 or later.

* Fixed `Include` patches skipped if they have multiple `FromFile` values.
* Fixed `FarmType` token returning `Custom` for the beach farm; it now returns `Beach` instead.
* Fixed patches not applied for farmhands in some highly specific cases resulting in an _invalid input arguments_ error.

## 1.19.2
Released 04 January 2021 for SMAPI 3.8.0 or later.

* Improved `patch summary` command:
  * Added optional arguments to filter by content packs IDs.
  * Long token values are now truncated to 200 characters by default to improve readability. You can use `patch summary full` to see the full summary.

## 1.19.1
Released 21 December 2020 for SMAPI 3.8.0 or later.

* Updated for Stardew Valley 1.5, including...
  * split-screen mode and UI scaling;
  * added `KeyToTheTown` value to `HasWalletItem` token.
* Fixed patch not applied correctly if `FromFile` or `Target` contains a single value with a trailing comma.

## 1.19.0
Released 05 December 2020 for SMAPI 3.7.3 or later. See the [release highlights](https://www.patreon.com/posts/44708077).

* Added [query expressions](author-guide/tokens.md#query-expressions).
* Added support for updating patches [on in-game time change](author-guide.md#update-rate).
* Added support for patches with multiple `FromFile` values.
* Added map patch modes for `"Action": "EditMap"`.
* Added `Time` token.
* Custom mod tokens can now normalize raw values before comparison.
* Fixed `{{DayEvent}}` translating festival names when not playing in English.
* Fixed error when `FromFile` has tokens containing comma-delimited input arguments.

## 1.18.6
Released 21 November 2020 for SMAPI 3.7.3 or later.

* Fixed validation for `Include` patches in 1.18.5.

## 1.18.5
Released 21 November 2020 for SMAPI 3.7.3 or later.

* Improved error-handling for some content packs with invalid formats.
* Fixed `EditData` patches with multiple targets sometimes applied incorrectly to some targets.

## 1.18.4
Released 04 November 2020 for SMAPI 3.7.3 or later.

* Fixed tokens which use input arguments failing to update silently in rare cases.
* Fixed 'collection was modified' error in some cases when patching a data model asset.

## 1.18.3
Released 15 October 2020 for SMAPI 3.7.3 or later.

* Added support for setting the default value for an `i18n` token.
* Fixed `i18n` token not accepting named arguments.
* Fixed error-handling for invalid `Include` patches.
* Fixed errors using a dynamic token in some cases when it's set to the value of an immutable token like `{{HasMod |contains=X}}`.

## 1.18.2
Released 13 September 2020 for SMAPI 3.7.2 or later.

* `ConfigSchema` options can now have an optional `Description` field, which is shown in UIs like [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
* Fixed `TextOperations` incorrectly adding delimiters when there's no previous value.
* Fixed errors sometimes showing "ContentPatcher.Framework.Conditions.TokenString" instead of the intended value.
* Fixed error when using a field reference token as the only input to a token which requires input.

## 1.18.1
Released 13 September 2020 for SMAPI 3.7.2 or later.

* Fixed format issue when applying field edits to `Data\Achievements`.

## 1.18.0
Released 12 September 2020 for SMAPI 3.7.2 or later. See the [release highlights](https://www.patreon.com/posts/41527845).

* Added [content pack translation](author-guide.md#translations) support using `i18n` files.
* Added [text operations](author-guide.md#text-operations), which let you change a value instead of replacing it (e.g. append to a map's `Warp` property).
* You can now [configure content packs in-game](README.md#configure-content-packs) if you have [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed (thanks to a collaboration with spacechase0!). This works automatically for any content pack that has configuration options, no changes needed by mod authors.
* You can now edit fields via `EditData` for `Data\Achievements` too.
* Patches now update immediately when you change language.
* Fixed `EditData` patches not always updated if they use `FromFile` to load a file containing tokens.
* Fixed patches not always updated for a `Random` token reroll.
* Fixed error text when an `EditData` patch uses an invalid field index.
* Removed support for `FromFile` in `EditData` patches in newer format versions.

**Update note for mod authors:**  
If you use the `FromFile` field with `EditData` patches, updating the `Format` field to `1.18.0` or
later requires changes to your `content.json`. See the
[migration guide](author-migration-guide.md) for more info.

## 1.17.2
Released 28 August 2020 for SMAPI 3.6.0 or later.

* Fixed patches not always updated if they depend on mod-provided tokens that incorrectly change outside a context update.

## 1.17.1
Released 19 August 2020 for SMAPI 3.6.0 or later.

* Made 'multiple patches want to load asset' errors more user-friendly.
* Fixed error in some cases when warping to a new location as a farmhand in multiplayer.
* Fixed error editing an image previously loaded through the Scale Up mod.

## 1.17.0
Released 16 August 2020 for SMAPI 3.6.0 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/40495753).

* Patches can now optionally [update on location change](author-guide.md#update-rate), including all tokens (not only location-specific tokens).
* Patches can now resize maps automatically using `Action: EditMap` (just patch past the bottom or right edges).
* Added `TargetPathOnly` token (the target field value for the current patch, without the filename).
* Added [`patch reload`](author-guide.md#patch-reload) console command (thanks to spacechase0!).
* Added troubleshooting hints related to update rate in `patch summary` console command.
* Removed legacy token API obsolete since Content Patcher 1.12.
* Fixed ambiguous-method detection in advanced API.
* Internal changes to prepare for realtime content updates.

**Update note for mod authors:**  
If you use the `LocationName` or `IsOutdoors` token/condition, updating the `Format` field to
`1.17.0` or later requires changes to your `content.json`. See the
[migration guide](author-migration-guide.md) for more info.

## 1.16.4
Released 12 August 2020 for SMAPI 3.6.0 or later.

* Fixed 'collection was modified' error when unloading `Action: Include` patches.

## 1.16.3
Released 08 August 2020 for SMAPI 3.6.0 or later.

* Fixed incorrect token input validation in some cases with 1.16.2.

## 1.16.2
Released 08 August 2020 for SMAPI 3.6.0 or later.

* Fixed patches not always unapplied when an `Include` patch changes.
* Fixed error using some tokens within the `contains` input argument.
* Fixed broken error message when multiple load patches apply in 1.16.

## 1.16.1
Released 03 August 2020 for SMAPI 3.6.0 or later.

* Fixed some patches not applied correctly in 1.16.

## 1.16.0
Released 02 August 2020 for SMAPI 3.6.0 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/40028155).

* Added [an `Include` action](author-guide.md#include) to load patches from another JSON file. That includes full token support, so you can load files dynamically or conditionally.
* Randomization is now consistent between players, regardless of installed content packs.
* Content packs containing `null` patches are no longer disabled; instead those patches are now skipped with a warning.
* Improved performance when updating very large content packs.
* Fixed boolean/numeric fields rejecting tokens with surrounding whitespace like `"  {{SomeToken}}  "`.
* Fixed auto-generated patch names not normalising path separators.
* Fixed `patch summary` showing duplicate target paths in some cases.
* Fixed string sorting/comparison for some special characters.
* Internal changes to prepare for realtime content updates.

**Update note for mod authors:**  
Using `"Action": "EditData"` with a `FromFile` field is now deprecated, though it still works.
Migrating to an `"Action": "Include"` patch is recommended; it's more flexible and works more
intuitively. (That doesn't apply to `"Action": "EditData"` patches without a `FromFile` field.)

## 1.15.2
Released 21 July 2020 for SMAPI 3.6.1 or later.

* Fixed error using `HasFile` with filenames containing commas.
* Fixed broken patches preventing other patches from being applied/updated in rare cases.
* Internal changes to prepare for 1.16.

## 1.15.1
Released 06 July 2020 for SMAPI 3.6.1 or later.

* Fixed error loading pre-1.15 content packs that use a token with empty input arguments like `{{token:}}`.

## 1.15.0
Released 04 July 2020 for SMAPI 3.6.1 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/38962480).

* Added [named token arguments](author-guide/tokens.md#global-input-arguments).
* Added a universal `|contains=` argument to search token values.
* Added a universal `|inputSeparator=` argument to allow commas in input values using a custom separator.
* Added a `key` argument to `{{Random}}`.
* Several [player tokens](author-guide/tokens.md#player) now let you choose whether to check the host player, current player, or both.
* Added `HasConversationTopic` token.
* Reduced trace logs when a mod adds many custom tokens.
* Fixed custom tokens added by other mods able to break Content Patcher in some cases.
* Fixed support for tokens in a `From`/`ToArea`'s `Width` and `Height` fields.
* Fixed support for tokens in a `.json` file loaded through `Action: EditData` with a `FromFile` path containing tokens.
* Fixed format migrations not applied to tokens within JSON objects.
* Fixed multiple input arguments allowed for tokens that only recognize one (like `{{HasFile: fileA.png, fileB.png}}`). Doing so now shows an error.

**Update note for mod authors:**  
Updating the `Format` field to `1.15.0` or later requires changes to your `content.json`. See the [migration guide](author-migration-guide.md) for more info.

## 1.14.1
Released 14 May 2020 for SMAPI 3.5.0 or later.

* Fixed patches not updating correctly in 1.14 when a changed token is only in their `FromFile` field.

## 1.14.0
Released 02 May 2020 for SMAPI 3.5.0 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/whats-new-in-1-36931803).

* Added `Round` token.
* Added `FromFile` patch token (e.g. so you can do `"HasFile:{{FromFile}}": true`).
* The `patch export` command can now export assets that haven't been loaded yet.
* Fixed `Range` token excluding its upper bound.
* Fixed validation for `Target` fields containing `{{Target}}` and `{{TargetWithoutPath}}` tokens.
* Fixed validation for `Target` fields not shown in `patch summary` in some cases.
* Fixed 'file does not exist' error when the `FromFile` path is ready and doesn't exist, but the patch is disabled by a patch-specific condition.
* Fixed error when removing a map tile without edits.
* Fixed token handling in map tile/property fields.
* Fixed format validation for 1.13 features not applied.

## 1.13.0
Released 09 March 2020 for SMAPI 3.3.0 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/whats-new-in-1-34749703).

* Added support for arithmetic expressions.
* Added support for editing map tiles.
* Added support for editing map tile properties.
* Added support for multi-key bindings (like `LeftShift + F3`).
* `EditMap` patches now also copy layers and layer properties from the source map (thanks to mouse!).
* Patches are now applied in the order listed more consistently.
* Improved logic for matching tilesheets when applying a map patch.
* Fixed incorrect warning when using `HasWalletItem` token in 1.12.

## 1.12.0
Released 01 February 2020 for SMAPI 3.2.0 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/whats-new-in-1-33691875).

* Added advanced API to let other mods add more flexible tokens.
* Added support for mod-provided tokens in `EditData` fields.
* Reduced trace logs when another mod adds a custom token.
* The `patch export` command now exports the asset cached by the game, instead of trying to load it.
* Fixed dialogue and marriage dialogue changes not applied until the next day (via SMAPI 3.2).
* Fixed error when a data model patch uses an invalid token in its fields.
* Fixed whitespace between tokens being ignored (e.g. `{{season}} {{day}}` now outputs `Summer 14` instead of `Summer14`).

## 1.11.1
Released 27 December 2019 for SMAPI 3.0.1 or later.

* Mitigated `OutOfMemoryException` issue for some players. (The underlying issue in SMAPI is still being investigated.)
* Reduced performance impact in some cases when warping with content packs which have a large number of seasonal changes.
* Fixed patches being reapplied unnecessarily in some cases.
* Fixed token validation not applied to the entire token string in some cases.
* Fixed `Random` tokens being rerolled when warping if the patch is location-dependent.
* Fixed error when married to an NPC that's not loaded.

## 1.11.0
Released 15 December 2019 for SMAPI 3.0.1 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/whats-new-in-1-1-32382030).

* Added `Lowercase` and `Uppercase` tokens.
* `Random` tokens can have 'pinned keys' to support many new scenarios (see readme).
* `Random` tokens are now bounded for immutable choices (e.g. you can use them in numeric fields if all their choices are numeric).
* `FromArea` and `ToArea` fields can now use tokens (thanks to spacechase0!).
* Optimized asset loading/editing a bit.
* Fixed warning when an `EditData` patch references a file that doesn't exist when that's checked with a `HasFile` condition.
* Fixed `HasFile` token being case-sensitive on Linux/Mac.
* Fixed error if a content pack has a null patch.

## 1.10.1
Released 02 December 2019 for SMAPI 3.0.1 or later.

* Updated for Stardew Valley 1.4.0.1.
* Fixed error when an `EditData` patch uses tokens in `FromFile` that aren't available.

## 1.10.0
Released 26 November 2019 for SMAPI 3.0.0 or later. See the [release highlights for mod authors](https://www.patreon.com/posts/whats-new-in-1-1-32382030).

* Updated for Stardew Valley 1.4, including new farm type.
* Added new tokens:
  * `HavingChild` and `Pregnant`: check if an NPC/player is having a child.
  * `HasDialogueAnswer`: the player's selected response IDs for question dialogues (thanks to mus-candidus!).
  * `IsJojaMartComplete`: whether the player bought a Joja membership and completed all Joja bundles.
  * `Random`: a random value from the given list.
  * `Range`: a list of integers between the specified min/max values.
* Added support for editing map properties with `EditMap` patches.
* Added support for using `FromFile` with `EditData` patches.
* Added `patch export` console command, which lets you see what an asset looks like with all changes applied.
* Added `patch parse` console command, which parses an arbitrary tokenizable string and shows the result.
* Added new 'current changes' list for each content pack to `patch summary` output.
* Added world state IDs to the `HasFlag` token.
* Added [`manifest.json` and `content.json` validator](author-guide.md#schema-validator) for content pack authors.
* Content packs can now use mod-provided tokens without a dependency if the patch has an appropriate `HasMod` condition.
* Improved error if a content pack sets a `FromFile` path with invalid characters.
* Fixed `Hearts` and `Relationship` tokens not working for unmet NPCs. They now return `0` and `Unmet` respectively.
* Fixed issue where dynamic tokens weren't correctly updated in some cases if they depend on another dynamic token whose conditions changed. (Thanks to kfahy!)
* Fixed `patch summary` display for mod-provided tokens which require an unbounded input.
* Fixed `patch summary` not showing token input validation errors in some cases.
* Fixed `NullReferenceException` in some cases with invalid `Entries` keys.

## 1.9.2
Released 25 July 2019 for SMAPI 2.11.2 or later.

* Fixed `Day` token not allowing zero values.
* Fixed dynamic tokens validated before they're ready.
* Fixed mod-provided tokens called with non-ready inputs in some cases.
* Fixed Linux/Mac players getting `HasFile`-related errors in some cases.

## 1.9.1
Released 12 June 2019 for SMAPI 2.11.1 or later.

* Fixed error loading local XNB files in some cases with Content Patcher 1.9.
* Fixed mod-provided tokens being asked for values when they're marked non-ready.

## 1.9.0
Released 09 June 2019 for SMAPI 2.11.1 or later.

* Added API to let other mods create custom tokens and conditions.
* Fixed config parsing errors for some players.
* Fixed tokens not being validated consistently in some cases.
* Fixed a broken warning message.

## 1.8.2
Released 27 May 2019 for SMAPI 2.11.1 or later.

* Fixed some patches broken in Content Patcher 1.8.1.
* Fixed `EditMap` working with older format versions.

## 1.8.1
Released 26 May 2019 for SMAPI 2.11.1 or later.

* Improved `patch summary`:
  * now tracks the reason a patch wasn't loaded (instead of showing a heuristic guess);
  * added more info for local tokens;
  * simplified some output.
* Improved errors when a local file doesn't exist.
* Fixed patch update bugs in Content Patcher 1.8.

## 1.8.0
Released 16 May 2019 for SMAPI 2.11.1 or later.

* Added new tokens:
  * `IsOutdoors`: whether the player is outdoors.
  * `LocationName`: the name of the player's current location.
  * `Target`: the target field value for the current patch.
  * `TargetWithoutPath`: the target field value for the current patch (only the part after the last path separator).
* Added map patching.
* Added support for list assets in the upcoming Stardew Valley 1.4.
* Improved errors when token parsing fails.
* Fixed patches not applied in some cases.
* Fixed incorrect error message when `Default` and `AllowValues` conflict.
* Fixed confusing errors when a content pack is broken and using an old format version.

Thanks to spacechase0 for contributions to support the new tokens!

## 1.7.0
Released 08 May 2019 for SMAPI 2.11.0 or later.

* Added new tokens:
  * `HasReadLetter`: whether the player has opened a given mail letter.
  * `HasValue`: whether the input argument is non-blank, like `HasValue:{{spouse}}`.
  * `IsCommunityCenterComplete`: whether all bundles in the community center are completed.
  * `IsMainPlayer`: whether the player is the main player.
* Tokens can now be nested (like `Hearts:{{spouse}}`).
* Tokens can now be used almost everywhere (including dynamic token values, condition values, and `Fields` keys).
* Tokens with multiple values can now be used as placeholders.
* Tokens from `config.json` can now be unrestricted (`AllowValues` is now optional).
* Improved input argument validation.
* Added support for new asset structures in the upcoming Stardew Valley 1.4.
* Fixed incorrect error text when dynamic/config tokens conflict.
* Fixed config schema issues logged as `Debug` instead of `Warning`.
* Removed support for the condition value subkey syntax (like `"Relationship": "Abigail:Married"` instead of `"Relationship:Abigail": "Married"`). This only affects one content pack on Nexus.

**Update note for mod authors:**  
Updating the `Format` field to `1.7.0` or later requires changes to your `content.json`. See the [migration guide](author-migration-guide.md) for more info.

## 1.6.5
Released 06 April 2019 for SMAPI 2.11.0 or later.

* Fixed `EditData` allowing field values containing `/` (which is the field delimiter).
* Fixed error with upcoming SMAPI 3.0 changes.
* Fixed some broken maps in Stardew Valley 1.3.36 not detected.
* Fixed typo in some errors.
* Internal rewriting to support upcoming features.

## 1.6.4
Released 05 March 2019 for SMAPI 2.11.0 or later.

* Added detection for most custom maps broken by Stardew Valley 1.3.36 (they'll now be rejected instead of crashing the game).

## 1.6.3
Released 15 January 2019 for SMAPI 2.10.1 or later.

* Fixed some conditions not available for multiplayer farmhands after 1.6.2.

## 1.6.2
Released 04 January 2019 for SMAPI 2.10.1 or later.

* Conditions are now checked much sooner when loading a save, so early setup like map debris spawning can be affected conditionally.
* Fixed token subkey form not allowed in boolean fields.
* Updated for changes in the upcoming SMAPI 3.0.

## 1.6.1
Released 08 December 2018 for SMAPI 2.9.0 or later.

* Fixed error when a content pack has a patch with no `Target` field.
* Fixed some conditions using subkeys marked invalid incorrectly.

## 1.6.0
Released 08 December 2018 for SMAPI 2.9.0 or later.

* Added new tokens:
  * `DaysPlayed`: the number of in-game days played for the current save.
  * `HasWalletItem`: the [special items in the player wallet](https://stardewvalleywiki.com/Wallet).
  * `SkillLevel`: the player's level for a given skill.
* Added `Wind` value for `Weather` token.
* Added support for matching subkey/value pairs like `"Relationship": "Abigail:Married, Marnie:Friend"`.
* Added support for conditional map edits (via SMAPI 2.9).
* Added support for editing `Data\NPCDispositions` after the NPC is already created (via SMAPI 2.9).
* Improved performance for most content packs.
* Improved `patch summary` format.
* Updated for the upcoming SMAPI 3.0.
* Fixed language token always marked 'not valid in this context'.
* Fixed token strings not validated for format version compatibility.
* Fixed some 1.5 tokens not validated for format version compatibility.

**Update note for mod authors:**  
Updating the `Format` field to `1.6.0` or later requires changes to your `content.json`. See the [migration guide](author-migration-guide.md) for more info.

## 1.5.3
Released 08 November 2018 for SMAPI 2.8.0 or later.

* Added `patch summary` hint if `Target` value incorrectly includes a file extension.
* Migrated verbose logs to SMAPI's verbose logging feature.
* Fixed yet another error setting `EditData` entries to `null` since 1.5.

## 1.5.2
Released 29 September 2018 for SMAPI 2.8.0 or later.

* Improved `patch summary` output a bit.
* Fixed another error setting `EditData` entries to `null` since 1.5.

## 1.5.1
Released 23 September 2018 for SMAPI 2.8.0 or later.

* Added token support in `EditData` keys.
* Fixed error setting `EditData` entries to `null` since 1.5.
* Fixed error using tokens in `Enabled` field since 1.5.

## 1.5.0
Released 17 September 2018 for SMAPI 2.8.0 or later.
* Added support for dynamic tokens defined by the modder.
* Added new tokens:
  * `FarmCave` (the current farm cave type);
  * `FarmhouseUpgrade` (the upgrade level for the main farmhouse);
  * `FarmName` (the farm name);
  * `FarmType` (the farm type like `Standard` or `Wilderness`);
  * `HasFile` (whether a given file path exists in the content pack);
  * `HasProfession` (whether the player has a given profession);
  * `PlayerGender` (the player's gender);
  * `PlayerName` (the player's name);
  * `PreferredPet` (whether the player is a cat or dog person);
  * `Year` (the year number).
* Added subkey form for all tokens, which can be used to enable AND logic and condition negation (see readme).
* Added: you can now use any condition with `Action: Load` patches.
* Added: you can now use tokens in `EditData` entries and fields.
* Added: you can now list multiple values in the `Target` field.
* Added config tokens to `patch summary`.
* Added warning when a config field has `AllowValues` but a patch checks for an unlisted value.
* Removed some early warnings for issues like patch conflicts. That validation required a number of
  restrictions on how conditions and tokens could be used. Based on discussion with content pack
  modders, lifting those restrictions was more valuable than the early validation.
* Removed image preloading, which is no longer needed with SMAPI 2.8+.
* Fixed `patch summary` showing tokens that aren't valid in the current context.

## 1.4.1
Released 26 August 2018 for SMAPI 2.8.0 or later.

* Updated for Stardew Valley 1.3.29.
* Fixed broken error message.

## 1.4.0
Released 01 August 2018 for SMAPI 2.6.0 or later.

* Updated for Stardew Valley 1.3 (including multiplayer support).
* Added new tokens:
  * `DayEvent` (the festival name or wedding today);
  * `HasFlag` (the letters or flags set for the current player);
  * `HasMod` (the installed mods and content packs);
  * `HasSeenEvent` (the events seen by the current player);
  * `Hearts:<NPC>` (the relationship type for a given NPC);
  * `Relationship:<NPC>` (the relationship type for a given NPC);
  * `Spouse` (the player's spouse name);
* Added support for deleting entries via `EditData`.
* Added warnings for common mistakes in `patch summary` result.
* Fixed case sensitivity issues in some cases.

## 1.3.1
Released 08 April 2018 for SMAPI 2.5.4 or later.

* Added more detailed info to `patch summary` command.
* Improved error handling for image edits.
* Fixed unnecessary warnings when a patch is disabled.
* Fixed error when a content pack's `config.json` has invalid keys.

## 1.3.0
Released 26 March 2018 for SMAPI 2.5.4 or later.

* Added support for patch conditions (with initial support for season, day of month, day of week, and language).
* Added support for content packs having `config.json`.
* Added support for condition/config tokens in `content.json`.
* Added `patch summary` and `patch update` commands to simplify troubleshooting.
* Added trace logs when a content pack loads/edits an asset.
* Added optional verbose logs.
* Added unique patch names (editable via `LogName` field) to simplify troubleshooting.
* Improved error when a patch specifies an invalid source/target area.
* Fixed issue where an exception in one patch prevented other patches from being applied.
* Fixed `Target` not being normalized.
* Fixed errors using debug overlay on Linux/Mac.

## 1.2.0
Released 09 March 2018 for SMAPI 2.5.2 or later.

* Added support for overlaying images.
* Added optional debug mode for modders.
* `FromFile`, `Target`, and map tilesheets are now case-insensitive.
* Fixed null fields not being ignored after warning.

## 1.1.0
Released 02 March 2018 for SMAPI 2.5.2 or later.

* Added `Enabled` field to disable individual patches (thanks to f4iTh!).
* Added support for XNB files in `FromFile`.
* Added support for maps in `FromFile` which reference unpacked PNG tilesheets.

## 1.0.0
Released 25 February 2018 for SMAPI 2.5.2 or later.

* Initial release.
* Added support for replacing assets, editing images, and editing data files.
* Added support for extending spritesheets.
* Added support for locale-specific changes.
