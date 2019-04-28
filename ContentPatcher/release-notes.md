[← back to readme](README.md)

# Release notes
## Upcoming release
* Updated for Stardew Valley 1.4.
* Added support for data model assets in Stardew Valley 1.4.
* Added support for tokens in field keys.
* Fixed incorrect error text when dynamic/config tokens conflict.

## 1.6.5
* Fixed `EditData` allowing field values containing `/` (which is the field delimiter).
* Fixed error with upcoming SMAPI 3.0 changes.
* Fixed some broken maps in Stardew Valley 1.3.36 not detected.
* Fixed typo in some errors.
* Internal rewriting to support upcoming features.

## 1.6.4
* Added detection for most custom maps broken by Stardew Valley 1.3.36 (they'll now be rejected instead of crashing the game).

## 1.6.3
* Fixed some conditions not available for multiplayer farmhands after 1.6.2.

## 1.6.2
* Conditions are now checked much sooner when loading a save, so early setup like map debris spawning can be affected conditionally.
* Fixed token subkey form not allowed in boolean fields.
* Updated for changes in the upcoming SMAPI 3.0.

## 1.6.1
* Fixed error when a content pack has a patch with no `Target` field.
* Fixed some conditions using subkeys marked invalid incorrectly.

## 1.6
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
* Added `patch summary` hint if `Target` value incorrectly includes a file extension.
* Migrated verbose logs to SMAPI's verbose logging feature.
* Fixed yet another error setting `EditData` entries to `null` since 1.5.

## 1.5.2
* Improved `patch summary` output a bit.
* Fixed another error setting `EditData` entries to `null` since 1.5.

## 1.5.1
* Added token support in `EditData` keys.
* Fixed error setting `EditData` entries to `null` since 1.5.
* Fixed error using tokens in `Enabled` field since 1.5.

## 1.5
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
* Updated for Stardew Valley 1.3.29.
* Fixed broken error message.

## 1.4
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
* Added more detailed info to `patch summary` command.
* Improved error handling for image edits.
* Fixed unnecessary warnings when a patch is disabled.
* Fixed error when a content pack's `config.json` has invalid keys.

## 1.3
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
* Added support for overlaying images.
* Added optional debug mode for modders.
* `FromFile`, `Target`, and map tilesheets are now case-insensitive.
* Fixed null fields not being ignored after warning.

## 1.1
* Added `Enabled` field to disable individual patches (thanks to f4iTh!).
* Added support for XNB files in `FromFile`.
* Added support for maps in `FromFile` which reference unpacked PNG tilesheets.

## 1.0
* Initial release.
* Added support for replacing assets, editing images, and editing data files.
* Added support for extending spritesheets.
* Added support for locale-specific changes.
