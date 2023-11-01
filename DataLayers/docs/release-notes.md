[← back to readme](README.md)

# Release notes
## 1.15.11
Released 01 November 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to MagoSupremo123 (updated Portuguese)!

## 1.15.10
Released 03 October 2023 for SMAPI 3.14.0 or later.

* Improved compatibility with recent game updates on Android.
* Improved translations. Thanks to Moredistant (updated Chinese)!

## 1.15.9
Released 27 August 2023 for SMAPI 3.14.0 or later.

* Added support for Better Sprinklers Plus' custom sprinkler range (thanks to jamescodesthings!).

## 1.15.8
Released 25 June 2023 for SMAPI 3.14.0 or later.

* Embedded `.pdb` data into the DLL, which fixes error line numbers in Linux/macOS logs.

## 1.15.7
Released 30 March 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to Mysti57155 (updated French)!

## 1.15.6
Released 18 August 2022 for SMAPI 3.14.0 or later.

* Internal changes to support Toolbar Icons.
* Improved translations. Thanks to LeecanIt (updated Italian)!

## 1.15.5
Released 09 May 2022 for SMAPI 3.14.0 or later.

* Updated for SMAPI 3.14.0.

## 1.15.4
Released 27 February 2022 for SMAPI 3.13.0 or later.

* Fixed missing translation in Portuguese.
* Improved translations. Thanks to martin66789 (updated Hungarian), Scartiana (updated German), and wally232 (updated Korean)!

## 1.15.3
Released 14 January 2022 for SMAPI 3.13.0 or later.

* Fixed _Coverage: Sprinklers_ layer not showing the coverage for a held sprinkler.
* Fixed _Crops: Ready to Harvest_ layer stopping to mourn if it finds a dead crop.
* Fixed _Crops: Water for Paddy Crops_ layer ignoring dirt that's tilled without a paddy crop.
* Improved translations. Thanks to ChulkyBow (added Ukrainian), ellipszist (updated Thai), Evexyron + Yllelder (updated Spanish), ruzgar01 (updated Turkish), and Zangorr (added Polish)!

## 1.15.2
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed load error in the previous update.

## 1.15.1
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed scarecrow radius being one tile too wide in Stardew Valley 1.5.5.
* Internal optimizations.

## 1.15.0
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0, including...
  * custom scarecrows using the new `obj.IsScarecrow()` and `obj.GetRadiusForScarecrow()` methods;
  * new `TouchAction Warp` tile property.

## 1.14.8
Released 27 November 2021 for SMAPI 3.12.5 or later.

* The _Crops: ready for harvest_ layer now highlights dead crops.
* Fixed overlay arrows not handling zoom / UI scale correctly.

## 1.14.7
Released 31 October 2021 for SMAPI 3.12.5 or later.

* Added support for MultiFertilizer in fertilizer layer.
* Refactored console command handling.

## 1.14.6
Released 18 September 2021 for SMAPI 3.12.5 or later.

* Improved translations. Thanks to ellipszist (added Thai) and Tenebrosful (updated French)!  
  _Note: Thai requires Stardew Valley 1.5.5 and the [Thai mod](https://www.nexusmods.com/stardewvalley/mods/7052)._

## 1.14.5
Released 09 July 2021 for SMAPI 3.9.5 or later.

* Fixed _accessibility_ layer incorrectly marking some tiles adjacent to warps as warps.

## 1.14.4
Released 25 May 2021 for SMAPI 3.9.0 or later.

* Improved translations. Thanks to martin66789 (updated Hungarian)!

## 1.14.3
Released 27 March 2021 for SMAPI 3.9.5 or later.

* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 1.14.2
Released 06 February 2021 for SMAPI 3.9.0 or later.

* Fixed the top-left legend not scaling with the game's UI scale option.

## 1.14.1
Released 23 January 2021 for SMAPI 3.9.0 or later.

* Updated for multi-key bindings in SMAPI 3.9.

## 1.14.0
Released 16 January 2021 for SMAPI 3.8.0 or later.

* Reopening the menu now shows your last selected layer.
* Improved translations. Thanks to LeecanIt (updated Italian)!

## 1.13.3
Released 10 January 2021 for SMAPI 3.8.0 or later.

* Improved _water for paddy crops_ layer. (It's now updated for the latest game code, and no longer highlights untillable tiles.)

## 1.13.2
Released 04 January 2021 for SMAPI 3.8.0 or later.

* Fixed sprinkler layer not showing range for older sprinkler mods in Stardew Valley 1.5.
* Improved translations. Thanks to elCrimar (updated Spanish) and norges (updated German)!

## 1.13.1
Released 21 December 2020 for SMAPI 3.8.0 or later.

* Updated for Stardew Valley 1.5, including...
  * split-screen mode and UI scaling;
  * new fertilizer types;
  * new sprinkler upgrades.

## 1.13.0
Released 04 November 2020 for SMAPI 3.7.3 or later.

* Added arrow UI to navigate layers.
* Dropped support for abandoned Cobalt mod.

## 1.12.3
Released 15 October 2020 for SMAPI 3.7.3 or later.

* Refactored translation handling.
* Improved translations. Thanks to Enaium (updated Chinese) and zhxxn (updated Korean)!

## 1.12.2
Released 28 August 2020 for SMAPI 3.6.0 or later.

* Dead crops are now ignored in the crop fertiliser, harvest, and watered layers.

## 1.12.1
Released 03 July 2020 for SMAPI 3.6.1 or later.

* Improved translations. Thanks to Rittsuka (updated Portuguese)!

## 1.12.0
Released 02 May 2020 for SMAPI 3.5.0 or later.

* Added tile grid layer (when grid isn't enabled for all layers).
* Fixed translations not updated after changing language until you restart the game.
* Improved translations. Thanks to Annosz (added Hungarian), BURAKMESE (added Turkish), D0n-A (updated Russian), and misho104 (updated Japanese)!

## 1.11.0
Released 08 March 2020 for SMAPI 3.3.0 or later.

* Added support for multi-key bindings (like `LeftShift + F3`).
* Improved translations. Thanks to Enaium (updated Chinese), kchapelier (updated French), and xCarloC (updated Italian)!

## 1.10.0
Released 01 February 2020 for SMAPI 3.2.0 or later.

* Added shortcut keys to switch to a specific layer while the overlay is open (thanks to Drachenkaetzchen!).
* Added tilled group to tillable layer (thanks to Drachenkätzchen!).
* Improved translations. Thanks to jahangmar (updated German), kchapelier (updated French), shirutan (updated Japanese), and VengelmBjorn (updated Russian)!

## 1.9.1
Released 27 December 2019 for SMAPI 3.0.0 or later.

* Improved translations. Thanks to L30Bola (updated Portuguese) and PlussRolf (updated Spanish)!

## 1.9.0
Released 15 December 2019 for SMAPI 3.0.0 or later.

* Added grid feature (disabled by default).
* Fixed the `export` console command not exporting the full map.
* Improved translations. Thanks to LeecanIt (added Italian)!

## 1.8.0
Released 26 November 2019 for SMAPI 3.0.0 or later.

* Updated for Stardew Valley 1.4, including...
  * Deluxe Scarecrow;
  * paddy crops (which have a new layer to show tiles eligible for their growth bonus);
  * new areas and warps;
  * new bee house flower range.
* Added `data-layers export` console command to export the current data layer to a JSON file.
* Updated for compatibility with the latest version of Pelican Fiber.
* Improved translations. Thanks to shiro2579 (updated Portuguese)!

## 1.7.0
Released 25 July 2019 for SMAPI 2.11.2 or later.

* The accessibility layer now shows mine ladders/shafts as warp tiles.
* Fixed accessibility layer showing incorrect warp tiles near some buildings.
* Fixed accessibility layer errors when viewing some areas patched by TMX Loader.
* Dropped Data Maps migration code.
* Improved translations. Thanks to Shirutan (updated Japanese) and Skirmsiher (updated French)!

## 1.6.2
Released 09 June 2019 for SMAPI 2.11.1 or later.

* Fixed config parsing errors for some players.
* Improved translations. Thanks to Firevulture (updated German) and YSRyeol (updated Korean)!

## 1.6.1
Released 07 April 2019 for SMAPI 2.11.0 or later.

* Improved translations. Thanks to binxhlin (updated Chinese)!

## 1.6.0
Released 06 April 2019 for SMAPI 2.11.0 or later.

* Added _buildable_ and _tillable_ layers.
* Added support for Line Sprinklers mod.
* Improved translations. Thanks to binxhlin (updated Chinese), kelvindules (updated Portuguese), and TheOzonO3 (updated Russian)!

## 1.5.1
Released 05 March 2019 for SMAPI 2.11.0 or later.

* Improved translations. Thanks to S2SKY (added Korean) and VincentRoth (added French)!

## 1.5.0
Released 08 December 2018 for SMAPI 2.9.0 or later.

* Added _machine processing_ layer (requires Automate 1.11+).
* Updated for the upcoming SMAPI 3.0.
* Improved translations. Thanks to Nanogamer7 (German)!

## 1.4.2
Released 08 November 2018 for SMAPI 2.8.0 or later.

* Fixed error accessing Better Sprinklers in SMAPI 2.8+.

## 1.4.1
Released 17 September 2018 for SMAPI 2.8.0 or later.

* Updated for Stardew Valley 1.3.29.
* Improved translations. Thanks to pomepome (added Japanese), Ria (Spanish), and Yllelder (Spanish)!

## 1.4.0
Released 01 August 2018 for SMAPI 2.6.0 or later.

* Renamed to Data Layers due to common confusion about the name Data Maps.
* Updated for Stardew Valley 1.3 (including multiplayer).
* Added _crops: ready to harvest_ layer.
* Added support for Better Junimos and Prismatic Tools.
* Improved layers:
  * _accessibility_ now shows farm building door warps;
  * crop layers now show garden pots.
* Improved performance with more nuanced update rate.
* Improved translations. Thanks to alca259 (added Spanish), fadedDexofan (added Russian), and TaelFayre (added Portuguese)!

## 1.3.0
Released 14 February 2018 for SMAPI 2.4.0 or later.

* Updated to SMAPI 2.4.
* Added _crops: fertilized_ and _crops: watered_ maps. (Thanks to irecinius!)
* Added support for hiding individual maps in `config.json`.
* Improved consistency and sorted by name.
* Fixed error in the Cobalt integration.
* Improved translations. Thanks to Husky110 (added German) and yuwenlan (added Chinese)!

### 1.2.0
Released 13 January 2018 for SMAPI 2.3.0 or later.

* Added: point at a scarecrow/sprinkler/etc in its data map to highlight that one's range.
* Added: two overlapping groups of the same color will now share one border (configurable).
* Fixed error in Junimo hut map when Pelican Fiber isn't installed.

### 1.1.0
Released 11 January 2018 for SMAPI 2.3.0 or later.

* Added bee house coverage map.
* Added support for Cobalt's sprinkler.
* Added support for Simple Sprinkler's custom sprinkler range.
* Updated Better Sprinklers support.
* Fixed deprecated API usage.

### 1.0.0
Released 26 December 2017 for SMAPI 2.3.0 or later.

* Initial version.
* Added Junimo hut coverage, scarecrow coverage, sprinkler coverage, and accessibility maps.
* Added support for Better Sprinklers' custom sprinkler range.
* Added support for Pelican Fiber's custom build menu.
