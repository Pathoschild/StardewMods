[← back to readme](README.md)

# Release notes
## Upcoming release
* Updated for Stardew Valley 1.6.9.
* Added two config options which apply when you press the 'toggle UI' key:
   - whether it reopens the last chest you selected;
   - a category name to open by default (if set).
* The default category for chests now uses the location's display name (if available), instead of its internal name.
* Improved dropdown positioning to reduce overlap with big chest UI.
* Improved translations. Thanks to celr00 (updated Spanish), ForgottenZ (updated Chinese), MakinDay (updated Italian), and moonggae (updated Korean)!

## 1.24.1
Released 01 July 2024 for SMAPI 4.0.7 or later.

* Support for sprinkler attachments is now optional and disabled by default.

## 1.24.0
Released 29 June 2024 for SMAPI 4.0.7 or later.

* Added support for sprinkler attachments (thanks to Mushymato!).

## 1.23.5
Released 08 June 2024 for SMAPI 4.0.7 or later.

* Fixed the community center fish tank being accessible through Chests Anywhere.
* Raised minimum versions to SMAPI 4.0.7 and Stardew Valley 1.6.4.  
  _This avoids errors due to breaking changes in earlier 1.6 patches._
* Internal refactoring.

## 1.23.4
Released 22 May 2024 for SMAPI 4.0.0 or later.

* Improved translations. Thanks to burunduk (updated Ukrainian) and mitekano23 (updated Japanese)!

## 1.23.3
Released 15 April 2024 for SMAPI 4.0.0 or later.

* Reduced chest dropdown UI overlap over big chest inventory (thanks to LeFauxMatt!).

## 1.23.2
Released 08 April 2024 for SMAPI 4.0.0 or later.

* Fixed chest options not updating Automate machines immediately when the chest is inside a farm building.

## 1.23.1
Released 21 March 2024 for SMAPI 4.0.0 or later.

* Fixed auto-grabbers no longer accessible remotely.

## 1.23.0
Released 19 March 2024 for SMAPI 4.0.0 or later.

* Updated for Stardew Valley 1.6.
* Removed migration for pre-1.20 chest options.  
* Fixed misaligned chest UI when the zoom level and UI scale don't match (thanks to SinZ163!).
* Fixed errors if some config fields are set to null.

**Breaking changes for players:**
* If you edited a chest's name/options before Chests Anywhere 1.20.0 (December 2020) and haven't loaded the save since
  then, it will no longer be migrated to the new format. You'll need to re-edit it to set the options you want. That's
  needed because the migration conflicted with newer Stardew Valley 1.6 features.

## 1.22.10
Released 01 December 2023 for SMAPI 3.14.0 or later.

* Reverted Android compatibility changes in 1.22.9.  
  _(The changes caused issues like dressers acting as regular chests and broke compatibility with some mods like Better Chests.)_

## 1.22.9
Released 01 November 2023 for SMAPI 3.14.0 or later.

* Improved Android compatibility after recent game updates.
* Disabled shipping bin feature on Android due to compatibility issues.

## 1.22.8
Released 03 October 2023 for SMAPI 3.14.0 or later.

* Improved Android compatibility after recent game updates.

## 1.22.7
Released 25 June 2023 for SMAPI 3.14.0 or later.

* Embedded `.pdb` data into the DLL, which fixes error line numbers in Linux/macOS logs.
* Improved translations. Thanks to Ritew (updated Portuguese!).

## 1.22.6
Released 30 March 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to Mysti57155 (updated French)!

## 1.22.5
Released 09 January 2023 for SMAPI 3.14.0 or later.

* Fixed error when another mod sets invalid chest options.

## 1.22.4
Released 30 October 2022 for SMAPI 3.14.0 or later.

* Updated integration with Generic Mod Config Menu.
* Improved translations. Thanks to watchakorn-18k (updated Thai)!

## 1.22.3
Released 18 August 2022 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to LeecanIt (updated Italian) and mc-kaishixiaxue (updated Chinese)!

## 1.22.2
Released 27 May 2022 for SMAPI 3.14.0 or later.

* Fixed world area data and some config values not loaded correctly in Chests Anywhere 1.22.0+.

## 1.22.1
Released 22 May 2022 for SMAPI 3.14.0 or later.

* Internal changes to support Content Patcher.

## 1.22.0
Released 09 May 2022 for SMAPI 3.14.0 or later.

* Updated for SMAPI 3.14.0.
* Added [mod-provided API to get overlay info](README.md#mod-integrations).

## 1.21.3
Released 27 February 2022 for SMAPI 3.13.0 or later.

* Fixed dropdown options not selectable when gamepad mode is enabled.
* Optimized legacy save migration a bit (thanks to Michael Kuklinski / Ameisen!).
* Improved translations. Thanks to EmWhyKay (updated Turkish), wally232 (updated Korean), and ZijieFeng (updated Chinese)!

## 1.21.2
Released 14 January 2022 for SMAPI 3.13.0 or later.

* Improved translations. Thanks to ChulkyBow (added Ukrainian), Evexyron (updated Spanish), mezen (updated German), and Zangorr (added Polish)!

## 1.21.1
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed load error in the previous update.

## 1.21.0
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Added in-game config UI through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).

## 1.20.18
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0.

## 1.20.17
Released 31 October 2021 for SMAPI 3.12.5 or later.

* Fixed compatibility with recent Expanded Storage versions.
* Improved translations. Thanks to Lumina (updated French)!

## 1.20.16
Released 18 September 2021 for SMAPI 3.12.5 or later.

* Improved translations. Thanks to ellipszist (added Thai)!  
  _Note: Thai requires Stardew Valley 1.5.5 and the [Thai mod](https://www.nexusmods.com/stardewvalley/mods/7052)._

## 1.20.15
Released 04 September 2021 for SMAPI 3.12.6 or later.

* Improved translations. Thanks to adnan-shoukfeh (updated Spanish) and pikulet (updated Chinese)!

## 1.20.14
Released 25 May 2021 for SMAPI 3.9.5 or later.

* Improved translations. Thanks to randomC0der (updated German)!

## 1.20.13
Released 17 April 2021 for SMAPI 3.9.5 or later.

* Cellars now inherit their cabin/farmhouse's world area in balanced mode.
* Fixed Automate chest options shown for mini-shipping bins.
* Improved translations. Thanks to Caco-o-sapo (updated Portuguese) and J3yEreN (updated Turkish!).

## 1.20.12
Released 27 March 2021 for SMAPI 3.9.5 or later.

* Added world areas for Ginger Island in balanced mode.
* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 1.20.11
Released 08 March 2021 for SMAPI 3.9.0 or later.

* Fixed shipping bin UI frozen if you have Expanded Storage installed.

## 1.20.10
Released 07 March 2021 for SMAPI 3.9.0 or later.

* Fixed error opening dressers in rare cases.
* Fixed lag in some cases when opening a chest menu that can't be linked to a chest.
* Improved translations. Thanks to derJuba007 (updated German), horizon98 (updated Chinese), and zNatural (updated Spanish)!

## 1.20.9
Released 06 February 2021 for SMAPI 3.9.0 or later.

* Fixed exit keys not working after editing a textbox option and closing the edit UI.
* Fixed textboxes not deselected when you click outside them.
* Improved translations. Thanks to carloshbcabral (updated Portuguese) and Treize-Chronos (updated French)!

## 1.20.8
Released 26 January 2021 for SMAPI 3.9.0 or later.

* Fixed pressing `e` always closing the menu after 1.20.6. It should now match the previous behavior.

## 1.20.7
Released 25 January 2021 for SMAPI 3.9.0 or later.

* Fixed controller issues in 1.20.6.
* Fixed toggle key closing the menu even if you're typing into another mod's textbox.
* Improved translations. Thanks to Kareolis (updated Russian)!

## 1.20.6
Released 23 January 2021 for SMAPI 3.9.0 or later.

* Updated for multi-key bindings in SMAPI 3.9.
* Improved Automate options UI.
* Fixed toggle key not closing the menu.

## 1.20.5
Released 18 January 2021 for SMAPI 3.8.0 or later.

* Fixed dresser options broken in 1.20.4.

## 1.20.4
Released 17 January 2021 for SMAPI 3.8.0 or later.

* Rewrote how older chest data is migrated for compatibility with Expanded Storage.

## 1.20.3
Released 16 January 2021 for SMAPI 3.8.0 or later.

* Updated chest options for Automate 1.20.
* Fixed fish tanks showing dresser UI when opened remotely.
* Improved translations. Thanks to LeecanIt (updated Italian)!

## 1.20.2
Released 10 January 2021 for SMAPI 3.8.0 or later.

* Fixed support for fridges in some custom farmhouse maps.
* Fixed default names for custom chest types.
* Fixed dressers placed outside cabins/farmhouses/sheds not listed in the menu.
* Fixed more cases where a Junimo chest wasn't matched correctly.

## 1.20.1
Released 05 January 2021 for SMAPI 3.8.0 or later.

* Fixed issues related to Junimo chests and main shipping bins (which have shared inventories):
  * Fixed which one you opened not always tracked correctly.
  * Fixed dropdown always selecting the first chest of that type when you click a different one of that type.
  * Fixed Junimo chests not always listed separately.
  * Fixed named Junimo chests sometimes showing the label from another Junimo chest.
* Fixed mini-fridge options sometimes reset on load.
* Fixed dropdown height not correctly accounting for UI scale.

## 1.20.0
Released 21 December 2020 for SMAPI 3.8.0 or later.

* Updated for Stardew Valley 1.5, including...
  * split-screen mode and UI scaling;
  * new chest types.
* Fixed error in 1.19.8 if a chest name/category has a very long sequence of numbers.
* Fixed shipping bin playing a chest _dwop_ sound instead of bin _ker-thunk_.

**Breaking changes for mod authors:**
* Chests Anywhere previously saved chest options in the chest name using tags like `|ignore|`.
  These are now stored in the new `modData` field added in 1.5; existing chests will be migrated
  automatically.

## 1.19.8
Released 21 November 2020 for SMAPI 3.7.3 or later.

* Improved dropdown sorting (e.g. _Chest #2_ is now before _Chest #10_).
* You can now scroll dropdowns by clicking or tapping the arrow icons.
* Fixed clicks on a scrolled dropdown selecting the wrong option.
* Improved translations. Thanks to PanPan-p (updated Turkish)!

## 1.19.7
Released 04 November 2020 for SMAPI 3.7.3 or later.

* Changed dropdown buttons to better fit UI.
* Fixed compatibility with recent ChestEx update.
* Improved translations. Thanks to Caco-o-sapo (updated Portuguese)!

## 1.19.6
Released 15 October 2020 for SMAPI 3.7.3 or later.

* Refactored translation handling to prepare for future updates and use game translations where possible.
* Improved translations. Thanks to Macskasajt05 (updated Hungarian) and zhxxn (updated Korean)!

## 1.19.5
Released 02 August 2020 for SMAPI 3.6.0 or later.

* Fixed string sorting/comparison for some special characters.

## 1.19.4
Released 03 July 2020 for SMAPI 3.6.1 or later.

* Fixed mouse scroll wheel navigation reversed.
* Improved translations. Thanks to stefanhahn80 (updated German)!

## 1.19.3
Released 14 May 2020 for SMAPI 3.5.0 or later.

* Fixed error opening chests in a different location in 1.19.2.
* Fixed color picker shown for non-chests like the fridge.

## 1.19.2
Released 14 May 2020 for SMAPI 3.6.0 or later.

* Fixed chest color picker hidden unless you opened the chest directly.
* Fixed being able to open the menu during the eat/drink animations and interrupting them.

## 1.19.1
Released 05 May 2020 for SMAPI 3.6.0 or later.

* Moved `data.json` into standard `assets` folder.
* Updated 'multiplayer limitations' message for 1.19.

## 1.19.0
Released 02 May 2020 for SMAPI 3.5.0 or later.

* Farmhands in multiplayer can now access chests in all synced locations (including the farm, farmhouse, and constructed farm buildings).
* The menu now defaults to chests in the current location, if any.
* Updated Android support (thanks to ZaneYork!).
* Improved translations. Thanks to D0n-A (updated Russian) and niniack (updated Chinese)!

## 1.18.0
Released 08 March 2020 for SMAPI 3.3.0 or later.

* Added support for multi-key bindings (like `LeftShift + B`).
* Fixed compatibility with recent Android versions (thanks to ZaneYork!).
* Updated translations. Thanks to Annosz (added Hungarian) and Hesper (updated Korean)!

## 1.17.4
Released 02 February 2020 for SMAPI 3.2.0 or later.

* Automate chest options are now applied immediately (if SMAPI 3.3 is installed).
* Fixed reversed Automate options in the last update.

**Breaking change:** if you already installed the previous update _and_ changed Automate options through Chests Anywhere after updating, this may reverse the ones you changed. If you didn't change any options after updating, your options will be back to normal.

## 1.17.3
Released 01 February 2020 for SMAPI 3.2.0 or later.

* Added support for Automate's new 'take items from this chest first' option (thanks to MadaraUchiha!).
* Simplified Automate options.
* Internal refactoring.
* Improved translations. Thanks to Avisend (updated French) and two anonymous users (updated Chinese and Japanese)!

## 1.17.2
Released 15 December 2019 for SMAPI 3.0.1 or later.

* Fixed being able to open the menu when using a tool.
* Improved translations. Thanks to LeecanIt (added Italian)!

## 1.17.1
Released 02 December 2019 for SMAPI 3.0.1 or later.

* Updated for Stardew Valley 1.4.0.1.

## 1.17.0
Released 26 November 2019 for SMAPI 3.0.0 or later.

* Updated for Stardew Valley 1.4.
* Added support for dressers.
* Fixed auto-grabber sprite not updated when emptied/filled through Chests Anywhere.
* Improved translations. Thanks to Hesperusrus (updated Russian)!

## 1.16.2
Released 25 July 2019 for SMAPI 2.11.2 or later.

* Improved translations. Thanks to cilekli-link (added Turkish), shirutan (updated Japanese), and SolidJade (updated Portuguese)!

## 1.16.1
Released 09 June 2019 for SMAPI 2.11.1 or later.

* Fixed config parsing errors for some players.
* Improved translations. Thanks to binxhlin (updated Chinese), Firevulture (updated German), Yllelder (updated Spanish), and YSRyeol (updated Korean)!

## 1.16.0
Released 06 April 2019 for SMAPI 2.11.0 or later.

* Added owner's name to cabin categories.
* Added Automate options to block input/output for a chest.
* Separated categories for locations with the same name.
* Improved translations. Thanks to kelvindules (updated Portuguese), kice (updated Chinese), and TheOzonO3 (updated Russian)!

## 1.15.1
Released 05 March 2019 for SMAPI 2.11.0 or later.

* Added option to disable player inventory organize button.
* Fixed UI flicker when moving items in the chest menu.
* Fixed shipping bin also listed in Farm Expansion's custom location.
* Improved translations. Thanks to S2SKY (added Korean) and VincentRoth (added French)!

## 1.15.0
Released 08 December 2018 for SMAPI 2.9.0 or later.

* Added button to reset a chest's options.
* Updated for the upcoming SMAPI 3.0.
* Fixed fridge listed as 'chest'.
* Fixed chest category defaulting to chest name in some cases.
* Fixed chest options not saved in rare cases.
* Improved translations. Thanks to Nanogamer7 (German)!

## 1.14.1
Released 17 September 2018 for SMAPI 2.8.0 or later.

* Disabled shipping bin editing for non-main players (only possible from the host).

## 1.14.0
Released 26 August 2018 for SMAPI 2.8.0-beta or later.

* Updated for Stardew Valley 1.3.29.
* Added support for editing the shipping bin.
* Fixed special item menus (e.g. Luau soup) rarely replaced by shipping bin menu.
* Improved translations. Thanks to pomepome (added Japanese) and Ria (Spanish)!

## 1.13.0
Released 01 August 2018 for SMAPI 2.6.0 or later.

* Updated for Stardew Valley 1.3 (including multiplayer support).
* Added support for auto-grabbers.
* Added Automate options to edit-chest screen if it's installed.
* Added support for scrolling chest/category dropdowns with the mouse wheel. (Thanks to mattfeldman!)
* Improved message when no chests are accessible.
* Fixed 'ok' button not closing menu in rare cases.
* Fixed issue where opening the chest UI from the inventory screen while holding an item destroys the item.
* Improved translations. Thanks to alca259 (Spanish), changbowen (Chinese), dezqo (German), fadedDexofan (Russian), heiwaon (Russian), and TaelFayre (Portuguese)!

## 1.12.4
Released 14 February 2018 for SMAPI 2.4.0 or later.

* Updated to SMAPI 2.4.
* Improved translations. Thanks to Husky110 (German)!

## 1.12.3
Released 11 January 2018 for SMAPI 2.3.0 or later.

* Added option to disable remote chest access from certain locations.
* Fixed shipping bin UI not allowing more than 36 items.
* Fixed shipping bin UI resetting gamepad cursor position on refresh.
* Fixed error when the range is set to `None` and a chest is opened directly.

## 1.12.2
Released 02 January 2018 for SMAPI 2.3.0 or later.

* Fixed broken shipping bin UI.
* Fixed deprecated API usage.

## 1.12.1
Released 26 December 2017 for SMAPI 2.3.0 or later.

* Updated to SMAPI 2.3.
* Added option to disable the shipping bin features.
* Fixed shipping bin losing filter on refresh.
* Fixed dropdown clicks transferring items underneath (via SMAPI 2.2).
* Fixed overlay not being drawn when menu backgrounds are enabled (via SMAPI 2.3).

## 1.12.0
Released 03 December 2017 for SMAPI 2.1.0 or later.

* Updated to SMAPI 2.1.
* Added support for Junimo huts and the shipping bin.
* Added option to limit range, for players who want a more balanced mod.
* Added warning when translation files are missing.
* Tooltips are no longer shown for unnamed chests.
* Switched to SMAPI unified controller/keyboard/mouse bindings in `config.json`.
* Switched to SMAPI update checks.
* Fixed being able to close a chest while an item is held, causing the item to disappear.
* Fixed not being able to reset a chest name to default after editing it.
* Improved translations. Thanks to d0x7 (German), vanja-san (Russian), and yuwenlan (Chinese)!

## 1.11.1
Released 13 June 2017 for SMAPI 1.15.0 or later.

* Fixed double cursor when using a controller.
* Improved translations. Thanks to Fabilows (added Portuguese) and ThomasGabrielDelavault (added Spanish)!

## 1.11.0
Released 04 June 2017 for SMAPI 1.14.0 or later.

* Updated to SMAPI 1.14.
* Added translation support.
* Improved translations. Thanks to Sasara (added German) and yuwenlan (added Chinese)!

## 1.10.0
Released 24 April 2017 for SMAPI 1.10.0 or later.

* Updated for Stardew Valley 1.2.

## 1.9.1
Released 06 April 2017 for SMAPI 1.9.0 or later.

* Updated to SMAPI 1.9.
* Backported to Stardew Valley 1.11 until 1.2 is released.

## 1.9.0
Released 24 February 2017 for SMAPI 1.9.0 or later.

* Updated for Stardew Valley 1.2.

## 1.8.2
Released 05 February 2017 for SMAPI 1.8.0 or later.

* Updated to SMAPI 1.8.
* Updated new-version-available check.

## 1.8.1
Released 04 December 2016 for SMAPI 1.3.0 or later.

* Updated to SMAPI 1.3.

## 1.8.0
Released 20 November 2016 for SMAPI 1.1.0 or later.

* Updated to SMAPI 1.1 for SMAPI 1.1.0 or later.

## 1.7.0
Released 24 October 2016 for SMAPI 1.1.0 or later.

* Added support for Linux and Mac.
* Added support for opening chest overlay from inventory (mostly to allow more controller bindings).
* Added hotkeys to navigate categories.
* Added hotkey to edit chest.
* Added default controller bindings for chest UI navigation.
* Added location tile on edit screen.
* Updated minimum game and SMAPI versions.
* Fixed `B` controller button not cancelling like `ESC` key.
* Fixed various controller issues in 1.6.
* Fixed list scrolling broken in 1.6.
* Fixed navigate-chest hotkeys ignoring category.
* Fixed group list being unsorted.
* Fixed update-check error on startup adding scary error text in console.

## 1.6.0
Released 17 October 2016 for SMAPI 0.40.1.1 or later.

* Added Chests Anywhere UI when opening a chest directly.
* Added compatibility with most inventory mods.
* Added an option to disable hover tooltips.
* Fixed controller toggle button not closing the menu.
* Fixed chest menu behaving unpredictably after closing edit form in some cases.
* Major rewrite under the hood.

## 1.5.0
Released 09 October 2016 for SMAPI 0.40.1.1 or later.

* Added name tooltip when your cursor is over a chest.
* Added edit button when you open a chest directly.
* Added cancel button when editing a chest.
* Fixed fridge being accessible before you obtain it.
* Fixed error when you click an unavailable inventory slot.
* Fixed error when you open the menu but don't have any chests.
* Fixed UI not being resized when game window is resized.
* Fixed Lewis' giftbox when you start a new game being usable as a chest.
* Fixed visual issues.

## 1.4.0
Released 04 October 2016 for SMAPI 0.40.1.1 or later.

* Updated for Stardew Valley 1.1.
* Added chest category which lets you override the location grouping.
* Added fields to edit sort order and hide chests.
* Fixed edited chest name not saving correctly.

## 1.3.0
Released 02 October 2016 for SMAPI 0.40.1.1 or later.

* Added feature to rename a chest from the menu.
* Added organize button for inventory.
* Added update check on load.
* Added error if game or SMAPI are out of date.
* Improved chest/location dropdowns:
  * They can now be closed by clicking away or pressing `ESC`.
  * They now show as many items as possible (instead of 10).
  * They now show up/down arrows when there are too many items to display at once.
* Improved error handling.
* Pressing `ESC` will now close the chest UI.
* The location tab is no longer enabled by default, and may be removed in a future version.

## 1.2.0
Released 17 August 2016 for SMAPI 0.40.0 or later.

* Chests are now sorted alphabetically.
* Chests can now be sorted manually.
* Added item tooltips.
* Added organize button.
* Added controller support.
* Added support for rebinding keyboard/controller keys in `config.json`.
* Added hotkeys to navigate between chests.
* Fixed chests in constructed buildings (like barns) not showing up.
* Fixed farmhouse fridge not showing up.
* Location tab is now hidden if all your chests are in one place.
* Simplified default chest names (like "Chest #1" instead of "Chest(77,12)").

## 1.1.0
Released 10 April 2016 for SMAPI 0.40.0 or later [by VIspReaderUS](https://www.nexusmods.com/stardewvalley/mods/257).

* Reworked UI.
* Added tabs for chests and locations.
* Added scrollable list for the two tabs.
* Chests can now be ignored.

## 1.0.0
Released 04 April 2016 for SMAPI 0.39.7 or later [by VIspReaderUS](https://www.nexusmods.com/stardewvalley/mods/257).

* Initial release.
