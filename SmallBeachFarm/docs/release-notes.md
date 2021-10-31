[← back to readme](README.md)

# Release notes
## 1.9.4
Released 31 October 2021 for SMAPI 3.12.6 or later.

* Improved integration with [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098):
  * All config options are now translatable.
  * Updated for Generic Mod Config Menu 1.5.0.

## 1.9.3
Released 04 September 2021 for SMAPI 3.12.6 or later.

* Internal changes for compatibility with the upcoming Stardew Valley 1.5.5.
* Fixed error replacing the vanilla beach farm when married.
* Fixed wrong island warp arrival tile when replacing the vanilla beach farm.

## 1.9.2
Released 01 August 2021 for SMAPI 3.12.0 or later.

* Updated for Harmony upgrade in SMAPI 3.12.0.

## 1.9.1
Released 24 July 2021 for SMAPI 3.9.5 or later.

* Internal changes to prepare for the upcoming [Harmony 2.x migration](https://stardewvalleywiki.com/Modding:Migrate_to_Harmony_2.0).

## 1.9.0
Released 17 April 2021 for SMAPI 3.9.5 or later.

* Added support for replacing the new beach farm in Stardew Valley 1.5.
* The default replaced farm is now the beach farm. (This only affects new installs that don't already have a `config.json`.)
* Improved compatibility with recolor mods. All tiles now use vanilla tilesheets (except the river/sea transition which is still custom).
* You can now walk closer to the water.
* Fixed two impassable tiles near bridge between the islands.
* Fixed issue where entering from the forest while riding a horse clipped you into the fence in 1.8.2.
* Fixed farm warp obelisk on Ginger Island warping you into the ocean.

## 1.8.2
Released 27 March 2021 for SMAPI 3.9.5 or later.

* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 1.8.1
Released 04 January 2021 for SMAPI 3.8.0 or later.

* Fixed issues when moving greenhouse or default shipping bin in Stardew Valley 1.5.
* Fixed fishing in fish ponds on the small beach farm.

## 1.8.0
Released 12 September 2020 for SMAPI 3.7.2 or later.

* You can now choose a different farm to replace in the mod settings.
* You can now configure the mod in-game if you have [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed.

## 1.7.1
Released 05 May 2020 for SMAPI 3.5.0 or later.

* Fixed error in upcoming versions of SMAPI.

## 1.7.0
Released 02 May 2020 for SMAPI 3.5.0 or later.

* You can now catch both river and ocean fish where the river/ocean meet. The river is also wide enough there to catch high-quality river fish.
* Migrated to tile flip/rotation added in SMAPI 3.4. This improves compatibility with recolors, since there are fewer custom tiles.
* Fixed disabling the campfire not removing the driftwood pile next to it.
* Fixed islands always added in 1.6.

## 1.6.0
Released 08 March 2020 for SMAPI 3.3.0 or later.

* Added config option for the campfire.
* Improved compatibility with Better Water (thanks to laulajatar!).
* Improved compatibility with Eemie's Map Recolor (now handled by that mod directly).
* Fixed issue where another mod's compatibility patch for Small Beach Farm wouldn't be applied correctly.
* Internal changes (migrated to `.tmx` maps, campfire is no longer part of the map itself, islands are now a map patch).

## 1.5.0
Released 01 February 2020 for SMAPI 3.2.0 or later.

* Added support for other mods patching the custom tilesheet.
* Added support for overlay compatibility files.
* Added compatibility with Better Water (thanks to laulajatar!).
* Fixed missing warp tile.

## 1.4.3
Released 27 December 2019 for SMAPI 3.0.0 or later.

* Updated compatibility with Eeemie's Just A New Recolor (thanks to laulajatar!).

## 1.4.2
Released 15 December 2019 for SMAPI 3.0.0 or later.

* Fixed ocean fishing on the farm only finding trash in 1.4.1.
* Fixed fishing in fish ponds.

## 1.4.1
Released 26 November 2019 for SMAPI 3.0.0 or later.

* Updated for Stardew Valley 1.4.

## 1.4.0
Released 25 July 2019 for SMAPI 2.11.2 or later.

* Added optional beach sounds on the farm (disabled by default).
* Added functional campfire (replaces previous decorative one).
* Fixed island building exits leading to the farm exit area.

## 1.3.0
Released 09 June 2019 for SMAPI 2.11.1 or later.

* Added optional version for players who want more space. Thanks to [Opalie](https://www.nexusmods.com/stardewvalley/users/38947035) for the new map!

## 1.2.0
Released 12 April 2019 for SMAPI 2.11.0 or later.

* Added support for map recolors (with support for A Wittily Named Recolor, Eeemie's Just A New Recolor, and Starblue Valley thanks to Opalie!).
* Reduced foliage overhang along left edge.
* Changed river mouth and simplified tilesheets to improve recolor compatibility.
* Fixed position after entering from Marnie's ranch while riding a horse/tractor.
* Fixed farm warp totem behavior.
* Fixed support for prebuilding co-op cabins.
* Fixed mailbox not using its default farm sprite.

## 1.1.0
Released 09 April 2019 for SMAPI 2.11.0 or later.

* The river/ocean are now treated as their respective types for fishing and crabpots.
* Fixed warp change also applied to non-riverland farms.
* Fixed some dirt tiles not marked tillable.
* Fixed being able to walk through riverside bushes.

## 1.0.0
Released 06 April 2019 for SMAPI 2.11.0 or later.

* Initial release. Farm map commissioned from [Opalie](https://www.nexusmods.com/stardewvalley/users/38947035)!
