**Lookup Anything** is a [Stardew Valley](http://stardewvalley.net/) mod that shows live info about
whatever's under your cursor when you press `F1`.

This is a very early prototype and isn't ready for general use.

## Installation
1. Install [SMAPI](https://github.com/ClxS/SMAPI) (0.39.5+).
2. <s>Install this mod from Nexus mods.</s> This mod isn't released yet. If you know what you're
   doing, you can compile the mod yourself to use it.
3. Run the game using SMAPI.

## Usage
Just point your cursor at something and press `F1`. The mod will show live info about that object.

## Examples
Here are some representative screenshots (layout and values will change dynamically as needed).

### Characters
| character   | screenshots |
| ----------- | ----------- |
| villager    | ![](screenshots/villager.png) |
| pet         | ![](screenshots/pet.png) |
| farm animal | ![](screenshots/farm-animal.png) |
| monster     | ![](screenshots/monster.png) |

### Items
| item        | screenshots |
| ----------- | ----------- |
| crop        | ![](screenshots/crop.png) |
| inventory   | ![](screenshots/item.png) |

### Map objects
| object          | screenshots |
| --------------- | ----------- |
| crafting object | ![](screenshots/crafting.png) |
| fence           | ![](screenshots/fence.png) |
| fruit tree      | ![](screenshots/fruit-tree.png) |
| wild tree       | ![](screenshots/wild-tree.png) |
| mine objects    | ![](screenshots/mine-gem.png) ![](screenshots/mine-ore.png) ![](screenshots/mine-stone.png) ![](screenshots/mine-ice.png) |
| ...             | ![](screenshots/artifact-spot.png) |

## Configuration
### Change input
Don't want to lookup things with `F1`? You can change all of the key bindings in the
`config.json` (see [valid keys](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.keys.aspx)),
and add controller bindings if you have one (see [valid buttons](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.buttons.aspx)).

## Changelog
* 1.0 (not yet released)
  * Initial version.
  * Added support for NPCs (villagers, pets, farm animals, and monsters), items (crops and inventory), and map objects (crafting objects, fruit trees, wild trees, and mine objects).
  * Added controller support and configurable bindings.