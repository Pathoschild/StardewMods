[← back to readme](README.md)

# Release notes
## 1.3
* Added support for patch conditions, with initial support for season, day of month, day of week, and language.
* Added support for player settings via `config.json`.
* Added support for condition and config tokens in `content.json`.
* Added `patch summary` command to simplify troubleshooting.
* Added trace logs when a content pack loads or edits an asset.
* Added optional verbose logs.
* Added unique patch names (editable via `LogName`) to simplify troubleshooting.
* Fixed unhelpful error when a patch specifies an invalid source/target area.
* Fixed an exception in one patch preventing other patches from being applied.
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
