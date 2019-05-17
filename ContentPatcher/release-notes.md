[← back to readme](README.md)

# Release notes
## Upcoming release
* Improved `patch summary` display for local tokens.

## 1.8
Released 16 May 2019.

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

## 1.7
Released 08 May 2019.

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
* The `ConfigSchema` field changes when you update your format to 1.7:
  * `AllowValues` is no longer required. If you omit it, the config field will allow _any_ value.
  * If you omit `Default`, the default is now blank instead of the first `AllowValues` value.

## 1.6.5
Released 06 April 2019.

* Fixed `EditData` allowing field values containing `/` (which is the field delimiter).
* Fixed error with upcoming SMAPI 3.0 changes.
* Fixed some broken maps in Stardew Valley 1.3.36 not detected.
* Fixed typo in some errors.
* Internal rewriting to support upcoming features.

## 1.6.4
Released 05 March 2019.

* Added detection for most custom maps broken by Stardew Valley 1.3.36 (they'll now be rejected instead of crashing the game).

## 1.6.3
Released 15 January 2019.

* Fixed some conditions not available for multiplayer farmhands after 1.6.2.

## 1.6.2
Released 04 January 2019.

* Conditions are now checked much sooner when loading a save, so early setup like map debris spawning can be affected conditionally.
* Fixed token subkey form not allowed in boolean fields.
* Updated for changes in the upcoming SMAPI 3.0.

## 1.6.1
Released 08 December 2018.

* Fixed error when a content pack has a patch with no `Target` field.
* Fixed some conditions using subkeys marked invalid incorrectly.

## 1.6
Released 08 December 2018.

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
* The `Weather` token now returns `Wind` on windy days instead of `Sun`. Existing content packs with `"Format": "1.5"` or earlier should work fine, since Content Patcher will adjust their conditions. Content packs which target version 1.6 or later should handle the new weather value.

## 1.5.3
Released 08 November 2018.

* Added `patch summary` hint if `Target` value incorrectly includes a file extension.
* Migrated verbose logs to SMAPI's verbose logging feature.
* Fixed yet another error setting `EditData` entries to `null` since 1.5.

## 1.5.2
Released 29 September 2018.

* Improved `patch summary` output a bit.
* Fixed another error setting `EditData` entries to `null` since 1.5.

## 1.5.1
Released 23 September 2018.

* Added token support in `EditData` keys.
* Fixed error setting `EditData` entries to `null` since 1.5.
* Fixed error using tokens in `Enabled` field since 1.5.

## 1.5
Released 17 September 2018.
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
Released 26 August 2018.

* Updated for Stardew Valley 1.3.29.
* Fixed broken error message.

## 1.4
Released 01 August 2018.

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
Released 08 April 2018.

* Added more detailed info to `patch summary` command.
* Improved error handling for image edits.
* Fixed unnecessary warnings when a patch is disabled.
* Fixed error when a content pack's `config.json` has invalid keys.

## 1.3
Released 26 March 2018.

* Added support for patch conditions (with initial support for season, day of month, day of week, and language).
* Added support for content packs having `config.json`.
* Added support for condition/config tokens in `content.json`.
* Added `patch summary` and `patch update` commands to simplify troubleshooting.
* Added trace logs when a content pack loads/edits an asset.
* Added optional verbose logs.
* Added unique patch names (editable via `LogName` field) to simplify troubleshooting.
* Improved error when a patch specifies an invalid source/target area.
* Fixed issue where an exception in one patch prevented other patches from being applied.
* Fixed `Target` not being normalised.
* Fixed errors using debug overlay on Linux/Mac.

## 1.2
Released 09 March 2018.

* Added support for overlaying images.
* Added optional debug mode for modders.
* `FromFile`, `Target`, and map tilesheets are now case-insensitive.
* Fixed null fields not being ignored after warning.

## 1.1
Released 02 March 2018.

* Added `Enabled` field to disable individual patches (thanks to f4iTh!).
* Added support for XNB files in `FromFile`.
* Added support for maps in `FromFile` which reference unpacked PNG tilesheets.

## 1.0
Released 25 February 2018.

* Initial release.
* Added support for replacing assets, editing images, and editing data files.
* Added support for extending spritesheets.
* Added support for locale-specific changes.
