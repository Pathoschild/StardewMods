**Chests Anywhere** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you access
your chests, fridge, Junimo huts, and shipping bin from anywhere and organize them your way.
Transfer items without having to run around, from the comfort of your bed to the deepest mine level.

![](screenshots/animated-usage.gif)

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Compatibility](#compatibility)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
3. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/518).
4. Run the game using SMAPI.

## Use
**Access chests** by pressing `B`. This will show your chests grouped by category. Navigate by
clicking the top dropdowns, or by pressing the `left` or `right` key.

**Edit chests** by clicking the edit icon after opening a chest ([see example](screenshots/animated-edit.gif)).
This will show a form where you can...
* set the chest name;
* set a category (which defaults to its location);
* set the sort order in the chest list;
* or hide the chest from the chest list.

**Point at a chest** in the world to see its name in a tooltip.

**Open the shipping bin** to view the items in the shipping bin. This lets you retrieve items
before they're shipped overnight. (You can ship any number of items, but only the first 36 will be
visible in the UI.)

## Configure
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

These are the available settings:

setting             | what it affects
------------------- | -------------------
`Range`             | Default `Unlimited`. The range at which chests are accessible. The possible options are... <ul><li>Unlimited: all chests.</li><li>CurrentWorldArea: chests in the current world area, based on these areas: beach, bus stop, desert, farm, forest, mine, mountain, railroads, town, witch swamp.</li><li>CurrentLocation: chests in the current location.</li><li>None: can't remotely access any chest.</li></ul>
`Controls`          | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default keyboard bindings are `B` to toggle the chest UI, `Left`/`Right` (or `Left Control` + scroll mouse wheel) to switch chests, `Up`/`Down` (or `Left Alt` + scroll mouse wheel) to switch categories. The default controller bindings are the shoulder and trigger buttons to navigate chests while the menu is open.
`ShowHoverTooltips` | Default `true`. Whether to show the chest name in a tooltip when you point at a chest.
`EnableShippingBin` | Default `true`. Whether to allow access to the shipping bin through Chests Anywhere.
`DisableInLocations`| The locations in which to disable remote chest lookups. You can use the [Debug Mode mod](https://www.nexusmods.com/stardewvalley/mods/679) to see the name of any in-game location, or get the location name for a chest from its edit screen. You can use `"UndergroundMine"` to match any level of the mines or skull caverns (except the entrance itself).

## Compatibility
Chests Anywhere is compatible with Stardew Valley 1.4+ on Linux/Mac/Windows, both single-player and
multiplayer.

Multiplayer notes:
* There's no problem with some players not having the mod.
* If you're not the main player, you can only access chests in your current location. (This is due to limitations in how the game syncs data.)

## See also
* [Release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/518)
* [Discussion thread](http://community.playstarbound.com/threads/smapi-chests-anywhere.122603/)
* This is an [open-source fork](https://github.com/VIspReaderUS/AccessChestAnywhere/issues/1) of the inactive [AccessChestAnywhere](https://github.com/VIspReaderUS/AccessChestAnywhere) mod. Versions 1.0 and 1.1 are from that mod.
