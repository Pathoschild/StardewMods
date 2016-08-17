**Chests Anywhere** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you access
your chests from anywhere. Transfer items without having to run around, from the comfort of your
bed to the deepest mine level.

## Installation
1. Install [SMAPI](https://github.com/ClxS/SMAPI) (0.39.5+).
2. Install [Chest Label System](http://www.nexusmods.com/stardewvalley/mods/242/) to name your
   chests. (Recommended but not required.)
3. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/518).
4. Run the game using SMAPI.

## Usage
### Accessing your chests
Press `B` to open the menu:

![](screenshots/menu.png)

This will show the chests on your farm and in your buildings. You can click the top-right menu
to change location, and the top-left menu to change chest:

![](screenshots/menu-chest-list.png)

You can also navigate between chests using the left and right arrow keys.

### Sorting
Your chests are sorted alphabetically by default. Want a different order? Just add a number between
pipes (like `|1|`) somewhere in the chest name:

![](screenshots/tags-order-name.png)

The chests will be sorted by that number, _then_ alphabetically. Chests with a specific order will
appear before those without. (The number won't be shown in the list.)

### Hidden chests
Hide a chest by adding `|ignore|` to its name.

## Configuration
### Change input
Don't want to summon your chests with `B`? You can change all of the key bindings in the
`config.json` (see [valid keys](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.keys.aspx)),
and add controller bindings if you have one (see [valid buttons](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.buttons.aspx)).

## Changelog
* 1.2
  * Chests are now sorted alphabetically
  * Chests can now be sorted manually
  * Added item tooltips
  * Added organize button
  * Added controller support
  * Added support for rebinding keyboard/controller keys in `config.json`
  * Added hotkeys to navigate between chests
  * Fixed chests in constructed buildings (like barns) not showing up
  * Fixed farmhouse fridge not showing up
  * Location tab is now hidden if all your chests are in one place
  * Simplified default chest names (like "Chest #1" instead of "Chest(77,12)")
* 1.1
  * Reworked UI
  * Added tabs for chests and locations
  * Added scrollable list for the two tabs
  * Chests can now be ignored
* 1.0
  * Initial release

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/518)
* This is an open-source fork of [AccessChestAnywhere](https://github.com/VIspReaderUS/AccessChestAnywhere) (which is currently inactive), with the [author's blessing](https://github.com/VIspReaderUS/AccessChestAnywhere/issues/1). Versions 1.0 and 1.1 are from that mod.
