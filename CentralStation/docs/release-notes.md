[← back to readme](README.md)

# Release notes
## Upcoming release
* Improved translations. Thanks to Lexith (updated Turkish)!

## 1.5.1
Released 03 September 2025 for SMAPI 4.3.1 or later.

* Improved translations. Thanks to Fumorisz (updated Chinese), MakinDay (updated Italian), and OSHIKAWA (updated Japanese)!

## 1.5.0
Released 27 July 2025 for SMAPI 4.3.1 or later.

* Added various custom travel-related decor in the gift shop, which you can use to decorate your farmhouse or other locations.
* Adjusted tourist positions to avoid small tourists being hidden behind decor.
* Fixed tourists sometimes overlapping or spawning in the wrong area.

## 1.4.2
Released 19 July 2025 for SMAPI 4.3.1 or later.

* Improved translations. Thanks to JanUwU42 (updated German)!

## 1.4.1
Released 14 July 2025 for SMAPI 4.3.1 or later.

* Fixed a broken warp and layering issue in 1.4.0 (thanks to Kisaa!).

## 1.4.0
Released 13 July 2025 for SMAPI 4.3.1 or later.

* Revamped central station visuals (thanks to 6480 for the sprites and [Kisaa](https://next.nexusmods.com/profile/crystalinerose) for the map changes!).  
  _This includes a revamped gift shop area with a new clerk NPC, more tourist/travel clutter scattered around the station, floor shadows, and many new sprites that will be used in later versions._
* Added a new dog tourist (thanks to 6480 for the sprites and Kisaa for the map edit!).
* Added employee lounge area with a new resident.
* Added garbage cans you can rummage through.
* Added [C# mod API](author-guide.md#c-mod-api) to get the current available stops.
* The 'rare wood' in the gift shop now has a pedestal in the gift shop area.
* Updated to use the new ID parsing support in SMAPI 4.3.x.
* Fixed destinations from old Bus Locations and Train Station content packs not always hidden when they should be (e.g. when already in their location).
* Improved translations. Thanks to MakinDay (updated Italian)!

## 1.3.0
Released 06 June 2025 for SMAPI 4.1.10 or later.

* Central Station can now load old Bus Locations and Train Station content packs directly if [you reassign them](README.md#reassign-old-content-packs).

## 1.2.2
Released 27 May 2025 for SMAPI 4.1.10 or later.

* Improved translations. Thanks to Thukino (added Japanese)!
* Internal changes to simplify maintenance.

## 1.2.1
Released 26 March 2025 for SMAPI 4.1.10 or later.

* For players:
  * Improved error message when the content pack isn't installed to be clearer.
  * Fixed edge case where a ticket machine could disappear if a mod reloaded its location's map while you were there.

## 1.2.0
Released 26 February 2025 for SMAPI 4.1.10 or later.

* For players:
  * Added more tourist spots in the gift shop.
  * Added rare chance for the station to be dark when traveling late.
  * Rare interactions in the Central Station now only happen after you've visited it a certain number of times.
  * Improved translations. Thanks to rosearecute_52045 (added Korean)!

* For mod authors:
  * Added `OnlyInAreas` tourist field, which sets which part of the Central Station a tourist can appear in.
  * Added `Pathoschild.CentralStation_TimesVisited` stat to track the number of visits to the Central Station.
  * Fixed stops with `"Tile": null` ignoring ticket machines past tile position (64, 64) when choosing a default position.

## 1.1.0
Released 11 February 2025 for SMAPI 4.1.10 or later.

* For players:
  * Improved Central Station's exit door area and added a rare interaction for it.
  * If you see multiple rare messages in a play session, you now always see a different one.
  * Raised juice prices to prevent reselling them for a higher price with the artisan profession.
  * Fixed map layer issue with a gift shop basket.
  * Fixed ticket machine not added if you start the day in its location.
  * Improved translations. Thanks to Hayato2236 (added Spanish) and NARCOAZAZAL (updated Portuguese)!
* For mod authors:
  * Added warning if a bookshelf entry has no messages to simplify troubleshooting.
  * Fixed custom content refreshed for the day before Content Patcher's tokens are fully updated.

## 1.0.1
Released 08 February 2025 for SMAPI 4.1.10 or later.

* Added warning when a stop is hidden because its target location doesn't exist.
* Fixed Bus Locations mod overriding Central Station's ticket machine at the bus stop.
* Improved translations. Thanks to CapMita (added Chinese), creeperkatze (added German), Lexith (added Turkish), MakinDay (added Italian), MaxBladix (added French), and NARCOAZAZAL (added Portuguese)!

## 1.0.0
Released 07 February 2025 for SMAPI 4.1.10 or later.

- Initial release. This includes:
  - boat, bus, and train networks.
  - Central Station map and custom ticket machine sprite commissioned from [Kisaa](https://next.nexusmods.com/profile/crystalinerose) (thanks!).
  - food court, gift shop, tourists, interactive bookshelves, and rare interactions in the Central Station.
  - integrations with the Bus Locations, CJB Cheats Menu, and Train Station mods.
  - data assets to register stops, tourists, and bookshelf messages through Content Patcher.
  - C# mod API to register stops.
