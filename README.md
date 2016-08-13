**AccessChestAnywhere** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you access
your chests from anywhere. Transfer items without having to run around, from the comfort of your
bed to the deepest mine level.

**This is an experimental build. Use at your own risk.**

## Installation
1. Install [SMAPI](https://github.com/ClxS/SMAPI) (0.39.5+).
2. Install [Chest Label System](http://www.nexusmods.com/stardewvalley/mods/242/) to name your
   chests. (Recommended but not required.)
3. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/257/).
3. Launch the game using SMAPI.

## Usage
### Accessing your chests
Press `B` to open the menu:

![](screenshots/menu.png)

This will show the chests on your farm and in your buildings.
You can click the top-right menu to change location, and the top-left menu to change chest:

![](screenshots/menu-chest-list.png)

### Sorting
Your chests are sorted alphabetically by default. Want a different order? Just add a number between
pipes (like `|1|`) somewhere in the chest name:

![](screenshots/tags-order-name.png)

The chests will be sorted by that number, _then_ alphabetically. Chests with a specific order will
appear before those without.

### Hidden chests
Hide a chest by adding `|ignore|` to its name.

## Configuration
### Change hot key
Don't want to summon your chests with `B`? Change the hot key in the `config.json` to [any valid key](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.keys.aspx).

## Changelog
* 1.2.0 (not released yet)
  * Chests are now sorted alphabetically
  * Chests can now be sorted manually
  * Hot key is now configurable
  * Item tooltips now work
  * Location tab is now hidden if all your chests are in one place
* 1.1.0
  * Reworked UI
  * Added tabs for chests and locations
  * Added scrollable list for the two tabs
  * Chests can now be ignored
* 1.0.0
  * Initial release