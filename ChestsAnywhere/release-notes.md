[← back to readme](README.md)

# Release notes
## Upcoming release
* Updated for Stardew Valley 1.4.
* Improved translations. Thanks to binxhlin (updated Chinese) and YSRyeol (updated Korean)!

## 1.16
Released 06 April 2019.

* Added owner's name to cabin categories.
* Added Automate options to block input/output for a chest.
* Separated categories for locations with the same name.
* Improved translations. Thanks to kelvindules (updated Portuguese), kice (updated Chinese), and TheOzonO3 (updated Russian)!

## 1.15.1
Released 05 March 2019.

* Added option to disable player inventory organise button.
* Fixed UI flicker when moving items in the chest menu.
* Fixed shipping bin also listed in Farm Expansion's custom location.
* Improved translations. Thanks to S2SKY (added Korean) and VincentRoth (added French)!

## 1.15
Released 08 December 2018.

* Added button to reset a chest's options.
* Updated for the upcoming SMAPI 3.0.
* Fixed fridge listed as 'chest'.
* Fixed chest category defaulting to chest name in some cases.
* Fixed chest options not saved in rare cases.
* Improved translations. Thanks to Nanogamer7 (German)!

## 1.14.1
Released 17 September 2018.

* Disabled shipping bin editing for non-main players (only possible from the host).

## 1.14
Released 26 August 2018.

* Updated for Stardew Valley 1.3.29.
* Added support for editing the shipping bin.
* Fixed special item menus (e.g. Luau soup) rarely replaced by shipping bin menu.
* Improved translations. Thanks to pomepome (added Japanese) and Ria (Spanish)!

## 1.13
Released 01 August 2018.

* Updated for Stardew Valley 1.3 (including multiplayer support).
* Added support for auto-grabbers.
* Added Automate options to edit-chest screen if it's installed.
* Added support for scrolling chest/category dropdowns with the mouse wheel. (Thanks to mattfeldman!)
* Improved message when no chests are accessible.
* Fixed 'ok' button not closing menu in rare cases.
* Fixed issue where opening the chest UI from the inventory screen while holding an item destroys the item.
* Improved translations. Thanks to alca259 (Spanish), changbowen (Chinese), dezqo (German), fadedDexofan (Russian), heiwaon (Russian), and TaelFayre (Portuguese)!

## 1.12.4
Released 14 February 2018.

* Updated to SMAPI 2.4.
* Improved translations. Thanks to Husky110 (German)!

## 1.12.3
Released 11 January 2018.

* Added option to disable remote chest access from certain locations.
* Fixed shipping bin UI not allowing more than 36 items.
* Fixed shipping bin UI resetting gamepad cursor position on refresh.
* Fixed error when the range is set to `None` and a chest is opened directly.

## 1.12.2
Released 02 January 2018.

* Fixed broken shipping bin UI.
* Fixed deprecated API usage.

## 1.12.1
Released 26 December 2017.

* Updated to SMAPI 2.3.
* Added option to disable the shipping bin features.
* Fixed shipping bin losing filter on refresh.
* Fixed dropdown clicks transferring items underneath (via SMAPI 2.2).
* Fixed overlay not being drawn when menu backgrounds are enabled (via SMAPI 2.3).

## 1.12
Released 03 December 2017.

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
Released 13 June 2017.

* Fixed double cursor when using a controller.
* Improved translations. Thanks to Fabilows (added Portuguese) and ThomasGabrielDelavault (added Spanish)!

## 1.11
Released 04 June 2017.

* Updated to SMAPI 1.14.
* Added translation support.
* Improved translations. Thanks to Sasara (added German) and yuwenlan (added Chinese)!

## 1.10
Released 24 April 2017.

* Updated for Stardew Valley 1.2.

## 1.9.1
Released 06 April 2017.

* Updated to SMAPI 1.9.
* Backported to Stardew Valley 1.11 until 1.2 is released.

## 1.9
Released 24 February 2017.

* Updated for Stardew Valley 1.2.

## 1.8.2
Released 05 February 2017.

* Updated to SMAPI 1.8.
* Updated new-version-available check.

## 1.8.1
Released 04 December 2016.

* Updated to SMAPI 1.3.

## 1.8
Released 20 November 2016.

* Updated to SMAPI 1.1.

## 1.7
Released 24 October 2016.

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

## 1.6
Released 17 October 2016.

* Added Chests Anywhere UI when opening a chest directly.
* Added compatibility with most inventory mods.
* Added an option to disable hover tooltips.
* Fixed controller toggle button not closing the menu.
* Fixed chest menu behaving unpredictably after closing edit form in some cases.
* Major rewrite under the hood.

## 1.5
Released 09 October 2016.

* Added name tooltip when your cursor is over a chest.
* Added edit button when you open a chest directly.
* Added cancel button when editing a chest.
* Fixed fridge being accessible before you obtain it.
* Fixed error when you click an unavailable inventory slot.
* Fixed error when you open the menu but don't have any chests.
* Fixed UI not being resized when game window is resized.
* Fixed Lewis' giftbox when you start a new game being usable as a chest.
* Fixed visual issues.

## 1.4
Released 04 October 2016.

* Updated for Stardew Valley 1.1.
* Added chest category which lets you override the location grouping.
* Added fields to edit sort order and hide chests.
* Fixed edited chest name not saving correctly.

## 1.3
Released 02 October 2016.

* Added feature to rename a chest from the menu.
* Added organise button for inventory.
* Added update check on load.
* Added error if game or SMAPI are out of date.
* Improved chest/location dropdowns:
  * They can now be closed by clicking away or pressing `ESC`.
  * They now show as many items as possible (instead of 10).
  * They now show up/down arrows when there are too many items to display at once.
* Improved error handling.
* Pressing `ESC` will now close the chest UI.
* The location tab is no longer enabled by default, and may be removed in a future version.

## 1.2
Released 17 August 2016.

* Chests are now sorted alphabetically.
* Chests can now be sorted manually.
* Added item tooltips.
* Added organise button.
* Added controller support.
* Added support for rebinding keyboard/controller keys in `config.json`.
* Added hotkeys to navigate between chests.
* Fixed chests in constructed buildings (like barns) not showing up.
* Fixed farmhouse fridge not showing up.
* Location tab is now hidden if all your chests are in one place.
* Simplified default chest names (like "Chest #1" instead of "Chest(77,12)").

## 1.1
Released 10 April 2016 [by VIspReaderUS](https://www.nexusmods.com/stardewvalley/mods/257).

* Reworked UI.
* Added tabs for chests and locations.
* Added scrollable list for the two tabs.
* Chests can now be ignored.

## 1.0
Released 04 April 2016 [by VIspReaderUS](https://www.nexusmods.com/stardewvalley/mods/257).

* Initial release.
