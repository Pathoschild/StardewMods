[← back to readme](README.md)

# Release notes
## Upcoming release
* Added support for machine recipes in `Data/Buildings` and `Data/Machines`, including custom recipes from other mods (thanks to collaboration with zombifier!).
* Added support for machine changes from the Extra Machine Config mod (thanks to zombifier!).
* Added quality star icons in recipe lists where relevant.
* Added config options for the new collapsible fields, and raised maximum number of items shown before they collapse by default.
* Added hint message when some machine rules are too dynamic to display.
* Re-added support for Producer Framework Mod machines (thanks to zombifier!).
* The 'progression mode' option now hides recipes you haven't learned yet (thanks to b3nk3lly!).
* Fixed repeating warning when looking up caged monsters at Spirit's Eve.
* Fixed keg and preserve jar lookups showing incorrect recipe items like "Error Item Jelly" (thanks to b3nk3lly!).
* Fixed roe lookups showing incorrect caviar recipe (thanks to b3nk3lly!).
* Fixed roe lookups showing recipes like 'Aged Aged Roe Roe'.
* Improved translations. Thanks to burunduk (updated Ukrainian), Caranud (updated French), MakinDay (updated Italian), mitekano23 (updated Japanese), nimbXnumb (updated Russian), ShanderB (updated Portuguese), and ykinsoul (updated Chinese)!

## 1.42.0
Released 21 April 2024 for SMAPI 4.0.0 or later.

* Some fields now collapse by default when they have too much content. You can click a link to show the full list. This affects:
  * in item lookups, the recipes field with more than 10 recipes;
  * in NPC lookups, gift taste fields with more than 30 items.
* Added integration with Custom Bushes (thanks to LeFauxMatt!).
* In tree lookups, the growth chance is now rounded to avoid strange values like 15.000001% (thanks to b3nk3lly!).
* Fixed `no translation:trees.stages.4` message in some tree lookups.
* Improved translations. Thanks to iglnierod (updated Spanish)!

## 1.41.6
Released 15 April 2024 for SMAPI 4.0.0 or later.

* Fixed handling of tailoring recipes with gender-dependent output.
* Fixed support for custom adventurer's guild slayer goals, and updated for the goal changes in Stardew Valley 1.6.
* Fixed fish lookups not handling fish pond changes in Stardew Valley 1.6.
* Improved translations. Thanks to Jualko (updated German), Kaian-Campos (updated Portuguese), MakinDay (updated Italian), Timur13240 (updated Russian), and wally232 (updated Korean)!

## 1.41.5
Released 08 April 2024 for SMAPI 4.0.0 or later.

* Fixed movie ticket lookup showing broken movie title/description.
* Fixed fish area names no longer translated.
* Improved translations. Thanks to EngurRuzgar (updated Turkish), fjf010223 & mc-kaishixiaxue (updated Chinese), iglnierod (updated Spanish), and MakinDay (updated Italian)!

## 1.41.4
Released 04 April 2024 for SMAPI 4.0.0 or later.

* Improved fish spawn rules in lookups (thanks to gr3ger!).
* Fixed display names for new wild tree types.
* Fixed upgrade level check for barns and coops.
* Fixed errors showing lookups when a mod has broken textures.
* Fixed backwoods shown as a source for some fish (thanks to gr3ger!).
* Improved translations. Thanks to EngurRuzgar (updated Turkish) and Jualko (updated German)!

## 1.41.3
Released 28 March 2024 for SMAPI 4.0.0 or later.

* Fixed machine output recipes no longer shown.
* Fixed cooking recipes always shown as never cooked.
* Improved translations. Thanks to MakinDay (updated Italian), mc-kaishixiaxue (updated Chinese), and Shi974 (updated French)!

## 1.41.2
Released 26 March 2024 for SMAPI 4.0.0 or later.

* Fixed chest lookups counting fridges as owned chests.
* Fixed crop lookups no longer showing growth info when full-grown.
* Fixed recipe data no longer shown in some cases.
* Fixed support for recipes with randomized output items.
* Fixed error using search menu if other mods added items with no names.
* Fixed lookups for upgraded barns/coops not showing correct upgrade summary.

## 1.41.1
Released 21 March 2024 for SMAPI 4.0.0 or later.

* Fixed error looking up wild forage.
* Fixed error looking up players on the Meadowlands farm or a custom `Data/AdditionalFarms` farm type.

## 1.41.0
Released 19 March 2024 for SMAPI 4.0.0 or later.

* Updated for Stardew Valley 1.6.  
  _Thanks to SinZ163 for contributing some of the fixes!_
* Item lookups now show which mod added the item, if the mod follows the [unique string ID](https://stardewvalleywiki.com/Modding:Modder_Guide/Game_Fundamentals#Unique_string_IDs) format.
* Added more seed info in tree lookups.
* Added support for looking up NPCs in some custom mod menus (thanks to BinaryLip!).
* Improved debug field format for player stats and schedules.
* Improved translations. Thanks to EmWhyKay (updated Turkish) and MakinDay (updated Italian)!
* Fixed errors if some config fields are set to null.

## 1.40.4
Released 01 December 2023 for SMAPI 3.14.0 or later.

* Datamining mode now shows properties instead of fields where equivalent.
* Fixed datamining mode showing internal backing fields for properties.

## 1.40.3
Released 01 November 2023 for SMAPI 3.14.0 or later.

* Simplified the player lookup skill progression bars (thanks to TehCupcakes!).
* Fixed Android-only error loading data.
* Improved translations. Thanks to wally232 (updated Korean)!

## 1.40.2
Released 03 October 2023 for SMAPI 3.14.0 or later.

* Fixed item search not matching Pickled Ginger and Wild Honey.
* Improved translations. Thanks to CoolRabbit123 (updated German) and Moredistant (updated Chinese)!

## 1.40.1
Released 27 August 2023 for SMAPI 3.14.0 or later.

* Fixed item lookups marking some fish as always out-of-season.
* Improved translations. Thanks to DanielleTlumach (updated Ukrainian), MakinDay (updated Italian), and MyEclipseyang (updated Chinese)!

## 1.40.0
Released 25 June 2023 for SMAPI 3.14.0 or later.

* Added specific options for which gift tastes to show, like 'show neutral gifts'. These replace the 'show all gift tastes' option.
* The 'progression mode' and 'show unrevealed gift tastes' options no longer override which gift taste levels are shown.
* Embedded `.pdb` data into the DLL, which fixes error line numbers in Linux/macOS logs.
* Fixed untranslated NPC names in some lookups.
* Improved translations. Thanks to BenG-07 (updated German!) and MakinDay (updated Italian!).

## 1.39.0
Released 30 March 2023 for SMAPI 3.14.0 or later.

* Added option to hide gift tastes for items you don't own in NPC lookups (thanks to SinZ163!).
* Fixed ingredient names in recipes that need any egg or any milk.
* Improved controller support in the search menu.
* Improved translations. Thanks to MakinDay (updated Italian)!

## 1.38.2
Released 09 January 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to Mysti57155 (updated French) and wally232 (updated Korean)!

## 1.38.1
Released 30 October 2022 for SMAPI 3.14.0 or later.

* Updated integration with Generic Mod Config Menu.
* Improved translations. Thanks to ChulkyBow (updated Ukrainian) and watchakorn-18k (updated Thai)!

## 1.38.0
Released 10 October 2022 for SMAPI 3.14.0 or later.

* Added melee weapon stats to item lookup.
* Fixed 'hide on key up' option closing lookup while the key is held.
* Fixed weapon item description showing the game's auto-generated stat values.
* Improved translations. Thanks to Becks723 (updated Chinese)!

## 1.37.5
Released 29 August 2022 for SMAPI 3.14.0 or later.

* Fixed item lookup 'needed for' not listing bundles which use categories.
* Fixed item lookup error if a mod added broken building data.
* Internal optimizations.

## 1.37.4
Released 18 August 2022 for SMAPI 3.14.0 or later.

* Fixed child birthday calculation (thanks to iBug!).
* Improved translations. Thanks to LeecanIt (updated Italian)!

## 1.37.3
Released 04 July 2022 for SMAPI 3.14.0 or later.

* Fixed lookup error when a child's `daysOld` field indicates they were born before the game started.

## 1.37.2
Released 27 May 2022 for SMAPI 3.14.0 or later.

* Fixed support for hovered items in custom game menu pages (thanks to KhloeLeclair!).

## 1.37.1
Released 09 May 2022 for SMAPI 3.14.0 or later.

* Updated for SMAPI 3.14.0.
* Fixed support for custom NPC roommates.
* Fixed error in crop lookup when it has invalid season data.
* Improved translations. Thanks to ChulkyBow (updated Ukrainian)!

## 1.37.0
Released 27 February 2022 for SMAPI 3.13.0 or later.

* Improved movie ticket lookups:
  * Added list of villagers who'd refuse to watch it.
  * Fixed error if some NPCs will reject the invitation.
  * Fixed gift tastes shown (a movie ticket isn't giftable).
* Fixed item lookups no longer showing construction recipes.
* Fixed error when the community center bundle data is invalid.
* Fixed a few missing translations in some languages.
* Improved translations. Thanks to ChulkyBow (updated Ukrainian), EmWhyKay (updated Turkish), Evexyron (updated Spanish), Scartiana (updated German), and wally232 (added Korean)!

## 1.36.2
Released 14 January 2022 for SMAPI 3.13.0 or later.

* Fixed scroll up/down arrows and keybinds not working in the search UI.
* Fixed error clicking 'see also' crop link from a seed lookup.
* Fixed support for custom locations with farm animals.
* Fixed 'tile lookup' option in Generic Mod Config Menu UI not working correctly.
* Fixed lookup error if another mod added invalid recipe data.
* Fixed transparency issue with faded icons in the lookup menu (thanks to TehPers!).
* Improved translations. Thanks to ChulkyBow (added Ukrainian), Evexyron (updated Spanish), mezen (updated German), and Zangorr (updated Polish)!

## 1.36.1
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed load error in the previous update.

## 1.36.0
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Added option to show all gift tastes in item/NPC lookups.
* Added in-game config UI through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
* Added support for animal lookups from [Animal Social Menu](https://www.nexusmods.com/stardewvalley/mods/3283)'s UI.
* Fixed error getting harvest dates for potted tea bushes.
* Internal optimizations.

## 1.35.7
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0.

## 1.35.6
Released 27 November 2021 for SMAPI 3.12.5 or later.

* Fixed some untranslated text (building names in recipes, farm animal and horse types).
* Improved translations. Thanks to TwelveYO (updated Japanese)!

## 1.35.5
Released 12 November 2021 for SMAPI 3.12.6 or later.

* Fixed lookup for Stardew Valley Expanded's Marlon not showing social info when looking him up through the calendar.

## 1.35.4
Released 31 October 2021 for SMAPI 3.12.5 or later.

* Added support for MultiFertilizer.
* Fixed error if a mod adds invalid tailoring recipes.
* Improved translations. Thanks to Lumina (updated French) and Zangorr (added Polish)!

## 1.35.3
Released 18 September 2021 for SMAPI 3.12.5 or later.

* Improved translations. Thanks to ellipszist (added Thai)!  
  _Note: Thai requires Stardew Valley 1.5.5 and the [Thai mod](https://www.nexusmods.com/stardewvalley/mods/7052)._

## 1.35.2
Released 04 September 2021 for SMAPI 3.12.6 or later.

* Opening a search/lookup with a keybind no longer triggers vanilla actions bound to the same key too (thanks to Deflaktor!).
* Fixed seed fields shown for non-seed items in some cases.

## 1.35.1
Released 01 August 2021 for SMAPI 3.12.0 or later.

* Added support for looking up bushes planted in indoor pot.
* Fixed error if `assets/data.json` file is missing.

## 1.35.0
Released 24 July 2021 for SMAPI 3.9.5 or later.

* Added support for scrolling up/down by a full page with `PageUp` and `PageDown` (configurable).
* Improved formatting for `modData` fields in datamining mode.
* Fixed social menu lookups not working from lower half of character slots.
* Fixed social menu lookup for an offline player showing the wrong player.
* Fixed datamining showing both `modData` and `modDataForSerialization` fields.
* Improved translations. Thanks to tonhodamoon (updated Portuguese)!

## 1.34.3
Released 09 July 2021 for SMAPI 3.9.5 or later.

* You can now open the search UI when another menu is open, similar to the lookup UI.
* Fixed lookups for custom seeds added via Json Assets not showing crop info.
* Fixed some fish pond drops shown as requiring zero fish instead of one.
* Fixed monster drop list showing 'Error Item' in some special cases.

## 1.34.2
Released 25 May 2021 for SMAPI 3.9.5 or later.

* Improved the drop list in monster lookups:
  * Fixed every possible drop for an item marked guaranteed if at least one of them is.
  * Fixed guaranteed drops not shown if they're not in the monster's possible drop list.
* Fixed some items with duplicate names (like the many `Shirt` items) not shown in the search UI.

## 1.34.1
Released 17 April 2021 for SMAPI 3.9.5 or later.

* Improved translations. Thanks to J3yEreN (added Turkish)!

## 1.34.0
Released 27 March 2021 for SMAPI 3.9.5 or later.

* Improved tailoring recipes in item lookups:
  * Added reverse lookup (i.e. see tailoring recipes which produce the current item).
  * Added full ingredient list for tailoring recipes.
  * Tailoring recipes are now faded if you haven't discovered them yet.
  * The produced item's icon now reflects the dye effect of the spool item.
  * Fixed tailoring recipes shown that aren't actually accessible in-game due to higher-priority recipes.
* You can now look up...
  * items in the tailoring UI's spool and output slots;
  * disabled items in the tailoring UI's inventory box.
* Fixed torches on fences counted as owned 93 times due to a game quirk.
* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 1.33.0
Released 07 March 2021 for SMAPI 3.9.0 or later.

* Improved item recipes field:
  * Overhauled recipe layout (in collaboration with cofiem) to be more consistent and better handle long recipes.
  * Added recipes which create the item (thanks to cofiem!).
  * Added building construction recipes (thanks to cofiem!).
  * Fixed the pickled ginger recipe shown as 'pickles'.
  * Fixed some cooking/crafting recipes not shown in some cases.
  * Fixed error when showing recipes from a broken machine.
* You can now look up...
  * unknown recipes from the cooking UI.
  * player inventory from the shop UI.
* Added quests and special orders to the item 'needed for' field (thanks to cofiem!).
* Fixed monster lookup drop probabilities under 1% chance shown as 0%.
* Fixed island shrine puzzle lookup showing untranslated text for some languages.
* Improved translations. Thanks to horizon98 and mcBegins2Snow (updated Chinese), psychochicken80 (updated German), wally232 (updated Korean), and zNatural (updated Spanish)!

## 1.32.4
Released 06 February 2021 for SMAPI 3.9.0 or later.

* Fixed NPC lookup not working from their profile menu.
* Fixed NPC lookup showing gift tastes for flavored variants in 1.32.3.
* Fixed item lookup showing fish pond info for legendary fish, and not showing it for coral and sea urchins.
* Fixed item lookup failing when the game has invalid bundle data.
* Fixed tree lookup incorrectly saying that a fertilized tree won't grow in winter.
* Fixed error when trying to lookup on the 'help wanted' screen.
* Fixed untranslated text in child lookup.
* Fixed 'number owned' field not counting items in the island farmhouse fridge.
* Improved translations. Thanks to carloshbcabral (updated Portuguese) and Kareolis (updated Russian)!

## 1.32.3
Released 23 January 2021 for SMAPI 3.9.0 or later.

* Updated for multi-key bindings in SMAPI 3.9.
* Fixed NPC lookups taking a long time to open when using mods that add a large number of custom items.
* Fixed support for mods which add gift tastes by context tag.
* Fixed coffee crops not showing next harvest date.
* Fixed lookup background too short when 'force full-screen' is enabled in split-screen mode.

## 1.32.2
Released 17 January 2021 for SMAPI 3.8.0 or later.

* Fixed some crop fields not shown for growing mixed seeds.
* Improved translations. Thanks to norges (updated German)!

## 1.32.1
Released 16 January 2021 for SMAPI 3.8.0 or later.

* The search menu now shows unlimited results.
* Fixed search menu letting you click on off-screen results.
* Fixed swapped labels for deluxe/quality fertilizers.
* Fixed tea bush lookups not showing winter growth dates on the Ginger Island farm.
* Fixed datamining mode not showing non-datamining fields in 1.32.
* Improved translations. Thanks to LeecanIt (updated Italian)!

## 1.32.0
Released 11 January 2021 for SMAPI 3.8.0 or later.

* Added 'force full-screen' option (disabled by default).
* Added missing mill recipe for rice.
* Improved data mining info:
  * Added `GameLocation` fields to tile lookups.
  * Fields are now sorted by reverse inheritance. <small>(For example, for a pet lookup you'll see the fields/properties declared directly on `Pet` first, then those on `NPC`, then those on `Character`.)</small>
  * Fixed duplicate entries for inherited fields.
* Fixed new 'needed for' bundle logic in 1.31.1.
* Fixed search UI hiding some results if they had the same name.
* Improved translations. Thanks to LeecanIt (updated Italian)!

## 1.31.1
Released 10 January 2021 for SMAPI 3.8.0 or later.

* Added names for Ginger Island fishing locations.
* Fixed rare issue where item lookup fails due to mismatched bundle game data.

## 1.31.0
Released 08 January 2021 for SMAPI 3.8.0 or later.

* Added support for NPC lookups at festivals.
* Added target redirection, which fixes an issue where looking up Abigail in the mines wouldn't show her real data.
* Added lookup links in item icon fields.
* Updated for new machine recipes in Stardew Valley 1.5.
* Tree 'is fertilized' field now links to the tree fertilizer lookup if applicable.
* Fixed furnace not showing wilted bouquet recipe.
* Fixed furniture placed outside cabins/farmhouses/sheds not counted for 'owned' stats.
* Fixed items counted more than once for 'owned' stats if they're in a Junimo chest.
* Fixed mahogany tree lookups showing wrong percentage growth chance.

## 1.30.0
Released 04 January 2021 for SMAPI 3.8.0 or later.

* Looking up a planted crop now shows its water and fertilizer state.

## 1.29.0
Released 21 December 2020 for SMAPI 3.8.0 or later.

* Updated for Stardew Valley 1.5, including support for...
  * split-screen mode and UI scaling;
  * new content;
  * island puzzle lookups (crystal cave, field office, Gourmand, mermaid puzzle, and shrine puzzle);
  * new chest types.
* Fixed some untranslated text.
* Fixed NPC lookups not showing social info for some custom mod NPCs.
* Improved translations. Thanks to spindensity (updated Chinese)!

## 1.28.3
Released 05 December 2020 for SMAPI 3.7.3 or later.

* Moved `data.json` into standard `assets` folder.
* Fixed crop fields shown for forage crops.
* Fixed item lookups no longer showing iridium or stack prices.

## 1.28.2
Released 21 November 2020 for SMAPI 3.7.3 or later.

* Fixed search UI lookups for certain item types causing an error.

## 1.28.1
Released 04 November 2020 for SMAPI 3.7.3 or later.

* Fixed 'number owned' field sometimes counting two of the same tool/weapon as different items.
* Fixed lookups on forage items.
* Fixed search UI lookups on flavored items like wines showing incorrect prices.
* Fixed search UI not showing Clam Roe, Sea Urchin Roe, and custom roe from mods.
* Fixed some translation issues in 1.28.
* Improved translations. Thanks to Caco-o-sapo (updated Portuguese) and therealmate (updated Hungarian)!

## 1.28.0
Released 15 October 2020 for SMAPI 3.7.3 or later.

* Added music block lookups.
* Improved Adventurer's Guild eradication goal info in monster lookups.
* Fixed spawned stones sometimes showing info for a different item.
* Fixed description for some generic spawned stones.
* Fixed Scorpion Carp showing 'contents: no' field.
* Refactored to prepare for future game updates.
* Refactored translation handling.
* Improved translations. Thanks to Enaium (updated Chinese), Macskasajt05 (added Hungarian), and zhxxn (updated Korean)!

## 1.27.5
Released 28 August 2020 for SMAPI 3.6.0 or later.

* Improved translations. Thanks to wally232 on Nexus (updated Korean)!

## 1.27.4
Released 02 August 2020 for SMAPI 3.6.0 or later.

* Fixed string sorting/comparison for some special characters.

## 1.27.3
Released 21 July 2020 for SMAPI 3.6.1 or later.

* Fixed error looking up items when some item data is invalid.
* Fixed incorrect color for sturgeon roe image.

## 1.27.2
Released 03 July 2020 for SMAPI 3.6.1 or later.

* Fixed display for tailored hat recipes.
* Fixed the search key working during cutscenes or when a menu is already open.
* Fixed spawn rules not showing "mine level X" matched when you're on that mine level.
* Fixed error looking up custom NPCs with invalid birthday data.
* Improved translations. Thanks to AndyAllanPoe (updated Italian) and Rittsuka (updated Portuguese)!

## 1.27.1
Released 02 May 2020 for SMAPI 3.5.0 or later.

* Fixed compatibility issue with Mega Storage in 1.27.

## 1.27.0
Released 02 May 2020 for SMAPI 3.5.0 or later.

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

## 1.26.0
Released 09 March 2020 for SMAPI 3.3.0 or later.

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
Released 03 February 2020 for SMAPI 3.2.0 or later.

* Fixed 'needed for' shown for incorrect item types in some cases.
* Fixed some Producer Framework Mod recipes not shown correctly.

## 1.25.1
Released 02 February 2020 for SMAPI 3.2.0 or later.

* Errors in Producer Framework Mod integration can no longer break lookups.
* Fixed errors reading some Producer Framework Mod recipes.

## 1.25.0
Released 01 February 2020 for SMAPI 3.2.0 or later.

* Added support for most custom machine recipes from Producer Framework Mod (thanks to Digus!).
* Added number owned to tool/weapon lookups.
* When a fish pond drop is guaranteed, further drops are now crossed out instead of hidden.
* Fixed bushes & fruit trees showing next fruit tomorrow on the last day of their season.
* Fixed items owned count not including child/horse hats, items in dressers, and equipped items.
* Fixed a missing translation for non-English players.
* Fixed fish spawn location names not being translated.
* Improved translations. Thanks to asqwedcxz741 (updated Chinese) and corrinr (updated Spanish)!

## 1.24.0
Released 27 December 2019 for SMAPI 3.0.0 or later.

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
Released 15 December 2019 for SMAPI 3.0.0 or later.

* Updated for recent versions of Json Assets.
* Fixed lookup on Haunted Skulls.
* Fixed lookup on Caroline's tea bush.
* Fixed rare issue where the HUD isn't restored when the lookup menu is force-replaced by another menu.
* Improved translations. Thanks to LeecanIt (added Italian)!

## 1.23.0
Released 26 November 2019 for SMAPI 3.0.0 or later.

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
Released 25 July 2019 for SMAPI 2.11.2 or later.

* Improved translations. Thanks to FixThisPlz (updated Russian) and jahangmar (updated German)!

## 1.22.1
Released 10 June 2019 for SMAPI 2.11.1 or later.

* Fixed HUD left hidden if you close lookup menu by pressing lookup key.

## 1.22.0
Released 09 June 2019 for SMAPI 2.11.1 or later.

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
Released 06 April 2019 for SMAPI 2.11.0 or later.

* Fixed debug fields that only differ by name capitalisation not being merged correctly.
* Improved translations. Thanks to binxhlin (updated Chinese), kelvindules (updated Portuguese), and TheOzonO3 (updated Russian)!

## 1.21.1
Released 05 March 2019 for SMAPI 2.11.0 or later.

* Added readable debug fields for more types.
* Improved debug fields to only show one value if a field/property differ only by the capitalisation of their name.
* Fixed cooking achievement check incorrectly shown for items like rarecrows.
* Fixed invalid stack prices when looking up shop inventory items.
* Improved translations. Thanks to Nanogamer7 (improved German), S2SKY (added Korean), and VincentRoth (added French)!

## 1.21.0
Released 04 January 2019 for SMAPI 2.10.1 or later.

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
Released 07 December 2018 for SMAPI 2.9.0 or later.

* Updated for the upcoming SMAPI 3.0.
* Improved translations. Thanks to Nanogamer7 (German)!

## 1.20.0
Released 08 November 2018 for SMAPI 2.8.0 or later.

* Added support for looking up a load-game slot.
* Added farm type to player lookup.
* Added neutral gifts to NPC lookup.
* Data mining fields are now listed in alphabetical order.
* Data mining mode now shows property values too.
* Fixed display issues when returning to the previous menu after a lookup in some cases.

## 1.19.2
Released 17 September 2018 for SMAPI 2.8.0 or later.

* Improved translations. Thanks to pomepome (Japanese)!

## 1.19.1
Released 26 August 2018 for SMAPI 2.8.0 or later.

* Updated for Stardew Valley 1.3.29.
* Improved translations. Thanks to pomepome (added Japanese) and Yllelder (Spanish)!

## 1.19.0
Released 01 August 2018 for SMAPI 2.6.0 or later.

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
Released 09 March 2018 for SMAPI 2.4.0 or later.

* Fixed error when looking up something before the save is loaded (thanks to f4iTh!).

## 1.18.0
Released 14 February 2018 for SMAPI 2.4.0 or later.

* Updated to SMAPI 2.4.
* Added support for furniture.
* Added support for custom machines and objects from Custom Farming Redux.
* Fixed debug key working when a menu is open.
* Fixed typo in debug interface.
* Improved translations. Thanks to Husky110 (German) and yuwenlan (Chinese)!

## 1.17.0
Released 03 December 2017 for SMAPI 1.17.0 or later.

* Updated to SMAPI 2.0.
* Switched to SMAPI unified controller/keyboard/mouse bindings in `config.json`.
* Switched to SMAPI update checks.
* Fixed errors in modded object data causing all lookups to fail.
* Fixed basic bat kills not counted towards Adventure Quest goal.
* Fixed `HideOnKeyUp` mode not returning to previous menu on close.
* Improved translations. Thanks to d0x7 (German) and TaelFayre (Portuguese)!

## 1.16.0
Released 04 September 2017 for SMAPI 1.15.0 or later.

* NPC gift tastes now list inventory and owned items first.
* Added warning when translation files are missing.
* Fixed items inside buildings constructed in custom locations not being found for gift taste info.
* Fixed lookup errors with some custom NPCs.

## 1.15.1
Released 06 August 2017 for SMAPI 1.15.0 or later.

* Fixed missing translation in child 'age' field.
* Fixed incorrect child age calculation.
* Improved translations. Thanks to SteaNN (added Russian)!

## 1.15.0
Released 14 June 2017 for SMAPI 1.15.0 or later.

* You can now look up your children.
* Improved lookup matching — if there's no sprite under the cursor, it now tries to look up the tile contents.
* Fixed animal 'complaint' field text when an animal was attacked overnight.
* Fixed item 'needed for' field incorrectly matching non-fish items for fishing bundles.
* Fixed item 'needed for' field not showing bundle area names in English.
* Improved translations. Thanks to Fabilows (added Portuguese) and ThomasGabrielDelavault (added Spanish)!

## 1.14.0
Released 04 June 2017 for SMAPI 1.14.0 or later.

* Updated to SMAPI 1.14.
* Added translation support.
* You can now look up items from the Junimo bundle menu.
* Fixed a few lookup errors when playing in a language other than English.
* Improved translations. Thansk to Sasara (added German) and yuwenlan (added Chinese)!

## 1.13.0
Released 24 April 2017 for SMAPI 1.10.0 or later.

* Updated for Stardew Valley 1.2.

## 1.12.1
Released 22 April 2017 for SMAPI 1.9.0 and 1.10.0 or later.

* Fixed calendar lookup not working in Stardew Valley 1.2 beta.

## 1.12.0
Released 06 April 2017 for SMAPI 1.9.0 and 1.10.0 or later.

* Updated to SMAPI 1.9.
* Backported to Stardew Valley 1.11 until 1.2 is released.
* Fixed incorrect sell price shown for equipment.
* Fixed incorrect fruit tree quality info.
* Fixed rare error caused by duplicate NPC names.
* Fixed furniture/wallpaper being shown as potential recipe ingredients.

## 1.11.0
Released 24 February 2017 for SMAPI 1.9.0 or later.

* Updated for Stardew Valley 1.2.

## 1.10.1
Released 06 February 2017 for SMAPI 1.8.0 or later.

* Fixed tile lookups always enabled regardless of `config.json`.

## 1.10.0
Released 04 February 2017 for SMAPI 1.8.0 or later.

* You can now look up an item from the kitchen cooking menu.
* You can now look up map tile info (disabled by default).
* Updated to SMAPI 1.8.
* Updated new-version-available check.

## 1.9.0
Released 17 December 2016 for SMAPI 1.4.0 or later.

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

## 1.8.0
Released 04 December 2016 for SMAPI 1.3.0 or later.

* Added museum donations to item 'needed for' field.
* You can now look up things behind trees when you're behind them.
* You can now close the lookup UI by clicking outside it.
* Updated to SMAPI 1.3.
* Fixed incorrect farmer luck message when the spirits are feeling neutral.
* Fixed social menu lookup sometimes showing the wrong villager.

## 1.7.0
Released 18 November 2016 for SMAPI 1.1.0 or later.

* You can now look up a villager from the social page.
* You can now look up an item from the toolbar.
* Console logs are now less verbose.
* Updated to SMAPI 1.1.
* Fixed some cases where the item 'number owned' field was inacurate.
* Fixed iridium prices being shown for items that can't have iridium quality.
* `F2` debug mode is no longer suppressed (removed in latest version of SMAPI).

## 1.6.0
Released 25 October 2016 for SMAPI 0.40.1.1 or later.

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

## 1.5.0
Released 11 October 2016 for SMAPI 0.40.1.1 or later.

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

## 1.4.0
Released 04 October 2016 for SMAPI 0.40.1.1 or later.

* Updated for Stardew Valley 1.1:
  * added new fertile weeds (forest farm) and geode stones (hilltop farm);
  * added new recipes for coffee, mead, sugar, void mayonnaise, and wheat flour;
  * updated for Gold Clock preventing fence decay;
  * updated to latest binaries & increased minimum versions.
* Fixed a few missing stones & weeds.

## 1.3.0
Released 25 September 2016 for SMAPI 0.40.0 or later.

* Added possible drops and their probability to monster lookup.
* Added item icons to crafting output, farm animal produce, and monster drops.
* Fixed item gift tastes being wrong in some cases.
* Fixed monster drops showing 'error item' in rare cases.
* Fixed fields being shown for dead crops.
* Internal refactoring.

## 1.2.0
Released 21 September 2016 for SMAPI 0.40.0 or later.

* On item lookup:
  * added crop info for seeds;
  * added recipes for the charcoal kiln, cheese press, keg, loom, mayonnaise machine, oil maker,
    preserves jar, recycling machine, and slime egg-press;
  * merged recipe fields;
  * fixed an error when displaying certain recipes.
* Added optional mode which hides the lookup UI when you release the button.
* `F1` now toggles the lookup UI (i.e. will close the lookup if it's already open).

## 1.1.0
Released 19 September 2016 for SMAPI 0.40.0 or later.

* On item lookup:
  * removed crafting recipe;
  * added crafting, cooking, and furnace recipes which use this item as an ingredient.
* Added error if game or SMAPI are out of date.

## 1.0.0
Released 18 September 2016 for SMAPI 0.40.0 or later.

* Initial version.
* Added support for NPCs (villagers, pets, farm animals, monsters, and players), items (crops and
   inventory), and map objects (crafting objects, fences, trees, and mine objects).
* Added controller support and configurable bindings.
* Added hidden debug mode.
* Added version check on load.
* Let players look up a target from any visible part of its sprite.
