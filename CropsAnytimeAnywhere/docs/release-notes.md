﻿[← back to readme](README.md)

# Release notes
## 1.4.3
Released 09 May 2022 for SMAPI 3.14.0 or later.

* Updated for SMAPI 3.14.0.
* Fixed mod preventing tilled dirt decay in affected locations.

## 1.4.2
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0.

## 1.4.1
Released 01 November 2021 for SMAPI 3.12.5 or later.

* Fixed error when the `config.json` file has null values.
* Fixed some `config.json` files not migrated correctly in 1.4.0.

**Update note:**  
This will reset your `config.json` to apply the correct defaults. If you have a lot of custom
configuration, you may want to back it up so you can reapply it after it's reset.

## 1.4.0
Released 31 October 2021 for SMAPI 3.12.5 or later.

* You can now configure `ForceTillable` per-location.  
  _If you previously edited the option, you'll need to reapply your changes for the `*` location._
* Internal performance optimizations.

## 1.3.5
Released 04 September 2021 for SMAPI 3.12.6 or later.

* Fixed fruit trees not plantable in town.

## 1.3.4
Released 01 August 2021 for SMAPI 3.12.0 or later.

* Updated for Harmony upgrade in SMAPI 3.12.0.

## 1.3.3
Released 24 July 2021 for SMAPI 3.9.5 or later.

* Internal changes to prepare for the upcoming [Harmony 2.x migration](https://stardewvalleywiki.com/Modding:Migrate_to_Harmony_2.0).

## 1.3.2
Released 09 July 2021 for SMAPI 3.9.5 or later.

* Fixed some tiles not diggable. (Thanks to KMFrench for a list of affected tiles!)

## 1.3.1
Released 27 March 2021 for SMAPI 3.9.5 or later.

* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 1.3.0
Released 21 December 2020 for SMAPI 3.8.0 or later.

* Updated for Stardew Valley 1.5.
* Overhauled mod configuration to be more flexible (e.g. allow crops anywhere without disabling season checks).
* Improved compatibility with custom-location mods.

## 1.2.1
Released 02 August 2020 for SMAPI 3.6.0 or later.

* Fixed string sorting/comparison for some special characters.

## 1.2.0
Released 02 May 2020 for SMAPI 3.5.0 or later.

* The 'force tillable' feature now works with tool charging.
* Some grass/dirt tiles don't have a type specified by the game. These are now marked as grass/dirt via `assets/data.json`.

## 1.1.0
Released 04 January 2019 for SMAPI 2.10.1 or later.

* Added support for tilling any dirt/grass by default (other tile types optional).
* Fixed fruit trees not producing fruit outside their normal seasons.
* Fixed fruit tree textures not updated correctly.

## 1.0.0
Released 07 December 2018 for SMAPI 2.9.0 or later.

* Initial version.
* Added option to configure seasons and locations.
