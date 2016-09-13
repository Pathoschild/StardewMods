**Lookup Anything** is a [Stardew Valley](http://stardewvalley.net/) mod that shows live info about
whatever's under your cursor when you press `F1`:
![](screenshots/animated.gif)

This is a prerelease beta version; use at your own risk.

## Installation
1. Install [SMAPI](https://github.com/ClxS/SMAPI) (0.39.5+).
2. <s>Install this mod from Nexus mods.</s> See the [thread on the Stardew Valley forums for instructions](http://community.playstarbound.com/threads/smapi-lookup-anything.122929/#post-3019451).
3. Run the game using SMAPI.

## Usage
Just point your cursor at something and press `F1`. The mod will show live info about that object.

## Examples
Here are some representative screenshots (layout and values will change dynamically as needed).

### Items
| item        | screenshots |
| ----------- | ----------- |
| crop        | ![](screenshots/crop.png) |
| inventory   | ![](screenshots/item.png) |

### Characters
| character   | screenshots |
| ----------- | ----------- |
| villager    | ![](screenshots/villager.png) |
| pet         | ![](screenshots/pet.png) |
| farm animal | ![](screenshots/farm-animal.png) |
| monster     | ![](screenshots/monster.png) |
| player      | ![](screenshots/player.png) |

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
  * Added support for NPCs (villagers, pets, farm animals, monsters, and players), items (crops and
    inventory), and map objects (crafting objects, fences, trees, and mine objects).
  * Added controller support and configurable bindings.
  * Added hidden debug mode.
  * Let players lookup a target from any visible part of its sprite.