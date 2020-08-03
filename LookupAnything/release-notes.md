[← back to readme](README.md)

# Release notes
## 1.27.4
Released 02 August 2020.

* Fixed string sorting/comparison for some special characters.

## 1.27.3
Released 21 July 2020.

* Fixed error looking up items when some item data is invalid.
* Fixed incorrect color for sturgeon roe image.

## 1.27.2
Released 03 July 2020.

* Fixed display for tailored hat recipes.
* Fixed the search key working during cutscenes or when a menu is already open.
* Fixed spawn rules not showing "mine level X" matched when you're on that mine level.
* Fixed error looking up custom NPCs with invalid birthday data.
* Improved translations. Thanks to AndyAllanPoe (updated Italian) and Rittsuka (updated Portuguese)!

## 1.27.1
Released 02 May 2020.

* Fixed compatibility issue with Mega Storage in 1.27.

## 1.27
Released 02 May 2020.

* The lookup menu is now centered again. (It will fallback to non-centered mode only if needed for compatibility.)
* Improved compatibility with custom NPC mods (including mods which replace non-social NPCs with social ones).
* Improved item scanning (used for 'number owned' and gift taste fields):
  * now includes nested items (e.g. chests in chests in chests);
  * now includes tool attachments;
  * now searches within some mod containers (e.g. bags);
  * fixed some spawned items incorrectly counted as owned (e.g. weeds and stones).
* Improved debug field formatting for `npc.currentMarriageDialogue`.
* Fixed 'number crafted' including some incorrect recipes.
* Fixed placed and held torches counted as different items.
* Fixed cursor incorrectly detected on Android in some cases.
* Fixed rare 'scissor rectangle is invalid' error with search menu.
* Fixed issue where closing the search menu with the default key bindings could trigger a lookup after the menu closes.
* Improved translations. Thanks to Andites (updated Russian) and niniack (updated Chinese)!

## 1.26
Released 09 March 2020.

* Added search feature (thanks to collaboration with mattfeldman!).
* Added support for multi-key bindings (like `LeftShift + F1`).
* Added contextual lookups for better controller/mobile support:
  * When there's no cursor (e.g. when playing with a controller or mobile), Lookup Anything automatically finds the most relevant match instead of looking under the cursor. For example, that may be what's in front of the player, the item picked up in a menu, etc.
  * Added item lookup when holding an item in a chest/inventory menu.
  * Added NPC lookup from their profile page (when the cursor isn't over an item).
  * Added player lookup from the skills tab.
  * Removed `ToggleLookupInFrontOfPlayer` option in `config.json`. If you edited it, your value will be merged into `ToggleLookup` automatically next time you launch the game.
* Fixed some vanilla recipes not shown in lookups when Producer Framework Mod packs are installed.
* Improved translations. Thanks to Jeardius (updated German), Hesper (updated Korean), and mael-belval (updated French)!

## 1.25.2
Released 03 February 2020.

* Fixed 'needed for' shown for incorrect item types in some cases.
* Fixed some Producer Framework Mod recipes not shown correctly.

## 1.25.1
Released 02 February 2020.

* Errors in Producer Framework Mod integration can no longer break lookups.
* Fixed errors reading some Producer Framework Mod recipes.

## 1.25
Released 01 February 2020.

* Added support for most custom machine recipes from Producer Framework Mod (thanks to Digus!).
* Added number owned to tool/weapon lookups.
* When a fish pond drop is guaranteed, further drops are now crossed out instead of hidden.
* Fixed bushes & fruit trees showing next fruit tomorrow on the last day of their season.
* Fixed items owned count not including child/horse hats, items in dressers, and equipped items.
* Fixed a missing translation for non-English players.
* Fixed fish spawn location names not being translated.
* Improved translations. Thanks to asqwedcxz741 (updated Chinese) and corrinr (updated Spanish)!

## 1.24
Released 27 December 2019.

* Added fish spawn rules to fish lookups.
* Added fish pond drops to fish lookups.
* Added option to highlight item gift tastes that haven't been revealed in the NPC profile yet.
* Revamped fish pond lookups to show locked drops and make the selection process more clear.
* Fixed pet lookups showing untranslated 'cat' or 'dog' type.
* Fixed hay in silos not counted towards number owned or when highlighting owned gifts.
* Fixed lookup on Caroline's tea bush showing wrong "days ago" value for date planted.
* Fixed item lookups sometimes showing wrong tailoring recipes.
* Fixed missing/partial shirt icons in some cases.
* Improved translations. Thanks to jahangmar (updated German), L30Bola (updated Portuguese), and PlussRolf (updated Spanish)!

## 1.23.1
Released 15 December 2019.

* Updated for recent versions of Json Assets.
* Fixed lookup on Haunted Skulls.
* Fixed lookup on Caroline's tea bush.
* Fixed rare issue where the HUD isn't restored when the lookup menu is force-replaced by another menu.
* Improved translations. Thanks to LeecanIt (added Italian)!

## 1.23
Released 26 November 2019.

* Updated for Stardew Valley 1.4, including...
  * per-player shipping bins;
  * movie theater;
  * new mechanics (clothing, dyeing, tailoring, and tree fertilizer);
  * new recipes (aged roe, caviar, dinosaur mayonnaise, and green tea);
  * new content (fish ponds, tea bushes, and trash bear);
  * new farm map;
  * new 14-heart spouse events;
  * new social NPC profiles;
  * new Krobus relationship;
  * new Adventurer's Guild goals;
  * pet water bowl and petting changes;
  * chance of double Loom output with higher-quality input.
* Added optional progression mode (only shows gift tastes for gifts you've already given).
* Added save format version to save slot lookups.
* Updated for compatibility with Json Assets 1.3.8.
* Fixed player luck in multiplayer showing your own luck instead of theirs.
* Fixed 'scissor rectangle is invalid' error in rare cases.
* Fixed game freeze if you open a lookup on the load screen and then close it by pressing `F1` again.
* Fixed invalid crafting recipe data causing lookups to fail.
* Fixed some flowers never shown as needed for a community center bundle.
* Improved translations. Thanks to Hesperusrus (updated Russian), pomepome (updated Japanese), and shiro2579 (updated Portuguese)!

## 1.22.2
Released 25 July 2019.

* Improved translations. Thanks to FixThisPlz (updated Russian) and jahangmar (updated German)!

## 1.22.1
Released 10 June 2019.

* Fixed HUD left hidden if you close lookup menu by pressing lookup key.

## 1.22
Released 09 June 2019.

* Added recipes to machine lookups (except for custom machines).
* Added bush lookups.
* Added 'kissed today' to spouse lookups.
* Added farm type description to player lookup.
* Increased size of lookup UI.
* Fixed HUD being drawn over lookup UI in small resolutions.
* Fixed config parsing errors for some players.
* Fixed planted coffee beans showing seed fields instead of crop fields.
* Fixed seed growth time predictions not accounting for Agriculturist profession.
* Fixed "you made -1 of these" field for some crafted items.
* Fixed some missing/incorrect recipes.
* Fixed incorrect 'needed for' entries when looking up some furniture/craftable items.
* Fixed incorrect subject image when looking up a bigcraftable item in your inventory.
* Fixed date years not shown when needed.
* Fixed unable to lookup inventory items from kitchen cooking menu.

## 1.21.2
Released 06 April 2019.

* Fixed debug fields that only differ by name capitalisation not being merged correctly.
* Improved translations. Thanks to binxhlin (updated Chinese), kelvindules (updated Portuguese), and TheOzonO3 (updated Russian)!

## 1.21.1
Released 05 March 2019.

* Added readable debug fields for more types.
* Improved debug fields to only show one value if a field/property differ only by the capitalisation of their name.
* Fixed cooking achievement check incorrectly shown for items like rarecrows.
* Fixed invalid stack prices when looking up shop inventory items.
* Improved translations. Thanks to Nanogamer7 (improved German), S2SKY (added Korean), and VincentRoth (added French)!

## 1.21
Released 04 January 2019.

* Added building lookups. That includes general info (like name and description) and info specific to barns, coops, cabins, Junimo huts, mills, silos, slime hutches, and stables.
* Added support for lookups from the cooking, crafting, and collection menus.
* Added times cooked/crafted to item lookups. (Thanks to watson81!)
* Added 'needed for' support for Gourmet Chef and Craft Master achivements. (Thanks to watson81!)
* After clicking a link in a lookup menu, closing the new lookup now returns you to the previous one.
* Fixed previous menu not restored when `HideOnKeyUp` option is enabled.
* Fixed visual bug on social tab after lookup when zoom is exactly 100%.
* Fixed debug fields showing wrong values in rare cases when an item was customized after it was spawned.
* Improved translations. Thanks to Nanogamer7 (German)!

## 1.20.1
Released 07 December 2018.

* Updated for the upcoming SMAPI 3.0.
* Improved translations. Thanks to Nanogamer7 (German)!

## 1.20
Released 08 November 2018.

* Added support for looking up a load-game slot.
* Added farm type to player lookup.
* Added neutral gifts to NPC lookup.
* Data mining fields are now listed in alphabetical order.
* Data mining mode now shows property values too.
* Fixed display issues when returning to the previous menu after a lookup in some cases.

## 1.19.2
Released 17 September 2018.

* Improved translations. Thanks to pomepome (Japanese)!

## 1.19.1
Released 26 August 2018.

* Updated for Stardew Valley 1.3.29.
* Improved translations. Thanks to pomepome (added Japanese) and Yllelder (Spanish)!

## 1.19
Released 01 August 2018.

* Updated for Stardew Valley 1.3 (including multiplayer support).
* Added support for...
  * auto-grabbers;
  * garden pots and their crops;
  * other players;
  * new friendship data;
  * crab pot bait.
* Added number of item needed in bundle list. (Thanks to StefanOssendorf!)
* Added support for custom greenhouse locations.
* Fixed issue where a bundle that needs two stacks of an item won't be listed on the item lookup if one stack is filled. (Thanks to StefanOssendorf!)
* Fixed Custom Farming Redux machines not drawn correctly in some cases.
* Fixed excessively precise luck field in some cases.
* Fixed broken translation.
* Improved translations. Thanks to alca259 (Spanish), fadedDexofan (Russian), and TaelFayre (Portuguese)!

## 1.18.1
Released 09 March 2018.

* Fixed error when looking up something before the save is loaded (thanks to f4iTh!).

## 1.18
Released 14 February 2018.

* Updated to SMAPI 2.4.
* Added support for furniture.
* Added support for custom machines and objects from Custom Farming Redux.
* Fixed debug key working when a menu is open.
* Fixed typo in debug interface.
* Improved translations. Thanks to Husky110 (German) and yuwenlan (Chinese)!

## 1.17
Released 03 December 2017.

* Updated to SMAPI 2.0.
* Switched to SMAPI unified controller/keyboard/mouse bindings in `config.json`.
* Switched to SMAPI update checks.
* Fixed errors in modded object data causing all lookups to fail.
* Fixed basic bat kills not counted towards Adventure Quest goal.
* Fixed `HideOnKeyUp` mode not returning to previous menu on close.
* Improved translations. Thanks to d0x7 (German) and TaelFayre (Portuguese)!

## 1.16
Released 04 September 2017.

* NPC gift tastes now list inventory and owned items first.
* Added warning when translation files are missing.
* Fixed items inside buildings constructed in custom locations not being found for gift taste info.
* Fixed lookup errors with some custom NPCs.

## 1.15.1
Released 06 August 2017.

* Fixed missing translation in child 'age' field.
* Fixed incorrect child age calculation.
* Improved translations. Thanks to SteaNN (added Russian)!

## 1.15
Released 14 June 2017.

* You can now look up your children.
* Improved lookup matching — if there's no sprite under the cursor, it now tries to look up the tile contents.
* Fixed animal 'complaint' field text when an animal was attacked overnight.
* Fixed item 'needed for' field incorrectly matching non-fish items for fishing bundles.
* Fixed item 'needed for' field not showing bundle area names in English.
* Improved translations. Thanks to Fabilows (added Portuguese) and ThomasGabrielDelavault (added Spanish)!

## 1.14
Released 04 June 2017.

* Updated to SMAPI 1.14.
* Added translation support.
* You can now look up items from the Junimo bundle menu.
* Fixed a few lookup errors when playing in a language other than English.
* Improved translations. Thansk to Sasara (added German) and yuwenlan (added Chinese)!

## 1.13
Released 24 April 2017.

* Updated for Stardew Valley 1.2.

## 1.12.1
Released 22 April 2017.

* Fixed calendar lookup not working in Stardew Valley 1.2 beta.

## 1.12
Released 06 April 2017.

* Updated to SMAPI 1.9.
* Backported to Stardew Valley 1.11 until 1.2 is released.
* Fixed incorrect sell price shown for equipment.
* Fixed incorrect fruit tree quality info.
* Fixed rare error caused by duplicate NPC names.
* Fixed furniture/wallpaper being shown as potential recipe ingredients.

## 1.11
Released 24 February 2017.

* Updated for Stardew Valley 1.2.

## 1.10.1
Released 06 February 2017.

* Fixed tile lookups always enabled regardless of `config.json`.

## 1.10
Released 04 February 2017.

* You can now look up an item from the kitchen cooking menu.
* You can now look up map tile info (disabled by default).
* Updated to SMAPI 1.8.
* Updated new-version-available check.

## 1.9
Released 17 December 2016.

* Villager lookups now highlight gifts you carry or own.
* Added optional data mining fields which show raw game data.
* You can now click on the up/down arrows to scroll content.
* Improved controller support:
  * You can now look up what's directly in front of you using a separate hotkey. (Not bound by default.)
  * Fixed controller thumbsticks scrolling content too slowly.
  * Fixed controller button conventions not respected by lookup menu.
* Fixed a rare error caused by the game duplicating an NPC.
* Fixed fruit tree quality schedule being wrong in some cases.
* Fixed input bindings in `config.json` being case-sensitive.
* Fixed input bindings in `config.json` being discarded silently if invalid.

## 1.8
Released 04 December 2016.

* Added museum donations to item 'needed for' field.
* You can now look up things behind trees when you're behind them.
* You can now close the lookup UI by clicking outside it.
* Updated to SMAPI 1.3.
* Fixed incorrect farmer luck message when the spirits are feeling neutral.
* Fixed social menu lookup sometimes showing the wrong villager.

## 1.7
Released 18 November 2016.

* You can now look up a villager from the social page.
* You can now look up an item from the toolbar.
* Console logs are now less verbose.
* Updated to SMAPI 1.1.
* Fixed some cases where the item 'number owned' field was inacurate.
* Fixed iridium prices being shown for items that can't have iridium quality.
* `F2` debug mode is no longer suppressed (removed in latest version of SMAPI).

## 1.6
Released 25 October 2016.

* Added support for Linux and Mac.
* Added item 'needed for' field for community center bundles, full shipment achievement, and polyculture achievement.
* Added item 'sells to' field.
* Added item number owned field.
* Added fruit tree quality schedule.
* Added support for looking up shop items.
* Added `data.json` validation on startup.
* Disabled lookups when game rendering mode breaks Lookup Anything (only known to happen in the Stardew Valley Fair).
* Fixed sale price shown for unsellable items.
* Fixed update-check error on startup adding scary error text in console.
* Fixed incorrect gift tastes by deferring more to the game code (slower but more accurate).
* Fixed error when looking up a villager you haven't met.
* Fixed error when looking up certain NPCs with no social data.

## 1.5
Released 11 October 2016.

* You can now look up a villager from the calendar.
* You can now look up items from an open chest.
* Added cask aging schedule.
* Added better NPC friendship fields which account for dating and marriage.
* Added marriage stardrop to heart meter.
* Added support for new iridium quality.
* Added debug log.
* Added option to suppress SMAPI's `F2` debug hotkey, which can have unintended consequences like skipping an entire season or teleporting into walls.
* Fixed gift tastes not handling precedence when NPCs are conflicted about how they feel.
* Fixed error when screen resolution is too small to display lookup UI.
* Fixed error when calculating a day offset that wraps into the next year.
* Fixed errors crashing the game in rare cases.

## 1.4
Released 04 October 2016.

* Updated for Stardew Valley 1.1:
  * added new fertile weeds (forest farm) and geode stones (hilltop farm);
  * added new recipes for coffee, mead, sugar, void mayonnaise, and wheat flour;
  * updated for Gold Clock preventing fence decay;
  * updated to latest binaries & increased minimum versions.
* Fixed a few missing stones & weeds.

## 1.3
Released 25 September 2016.

* Added possible drops and their probability to monster lookup.
* Added item icons to crafting output, farm animal produce, and monster drops.
* Fixed item gift tastes being wrong in some cases.
* Fixed monster drops showing 'error item' in rare cases.
* Fixed fields being shown for dead crops.
* Internal refactoring.

## 1.2
Released 21 September 2016.

* On item lookup:
  * added crop info for seeds;
  * added recipes for the charcoal kiln, cheese press, keg, loom, mayonnaise machine, oil maker,
    preserves jar, recycling machine, and slime egg-press;
  * merged recipe fields;
  * fixed an error when displaying certain recipes.
* Added optional mode which hides the lookup UI when you release the button.
* `F1` now toggles the lookup UI (i.e. will close the lookup if it's already open).

## 1.1
Released 19 September 2016.

* On item lookup:
  * removed crafting recipe;
  * added crafting, cooking, and furnace recipes which use this item as an ingredient.
* Added error if game or SMAPI are out of date.

## 1.0
Released 18 September 2016.

* Initial version.
* Added support for NPCs (villagers, pets, farm animals, monsters, and players), items (crops and
   inventory), and map objects (crafting objects, fences, trees, and mine objects).
* Added controller support and configurable bindings.
* Added hidden debug mode.
* Added version check on load.
* Let players look up a target from any visible part of its sprite.

