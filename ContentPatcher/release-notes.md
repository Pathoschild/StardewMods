[← back to readme](README.md)

# Release notes
## 1.5 (upcoming)
* Added `Year` condition.
* Added support for all conditions in `Action: Load` patches.
* Added support for multiple values in the `Target` field (like `"Target": "Festivals, Maps/Festivals"`).
* Added config tokens to `patch summary`.
* Added warning when a config field has `AllowValues` but a patch checks for an unlisted value.
* Removed patch prevalidation, which warned modders about issues like patch conflicts before they
  happened. That validation required a number of restrictions on how conditions and tokens could be
  used. Based on discussion with content pack modders on Discord, lifting those restrictions was
  more valuable than the prevalidation.
* Removed image preloading, which is no longer needed with SMAPI 2.8+.

## 1.4.1
* Updated for Stardew Valley 1.3.29.
* Fixed broken error message.

## 1.4
* Updated for Stardew Valley 1.3 (including multiplayer support).
* Added new conditions:
  * `DayEvent` (check if there's a festival or wedding today);
  * `HasFlag` (check if the player has received a letter by ID or has a mail flag set);
  * `HasMod` (check if another mod is installed, e.g. for automatic compatibility patches);
  * `HasSeenEvent` (check if the player has seen a given event ID);
  * `Hearts:<NPC>` (check the relationship type for a specific NPC);
  * `Relationship:<NPC>` (check the relationship type for a specific NPC);
  * `Spouse` (check the current player's spouse name).
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
