**Lookup Anything** is a [Stardew Valley](https://stardewvalley.net/) mod that shows live info about
whatever's under your cursor when you press `F1`. Learn a villager's favorite gifts, when a crop
will be ready to harvest, how long a fence will last, why your farm animals are unhappy, and more.

For advanced users, the optional data mining mode also shows raw game values so you can see exactly
what the game is doing.

![](screenshots/animated.gif)

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Showcase](#showcase)
* [Compatibility](#compatibility)
* [Extensibility for modders](#extensibility-for-modders)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](https://www.nexusmods.com/stardewvalley/mods/541/).
3. Run the game using SMAPI.

## Use
Just point your cursor at something and press `F1`. The mod will show live info about that object.
You can do this in the world, your inventory, the calendar, a shop, the social menu, and more.

If there's no cursor (e.g. when playing with a controller or on mobile), the most relevant subject
is shown instead. That may be something in front of the player, the player on the skills menu, the
NPC on their profile page, etc.

You can also press `left shift` + `F1` to search for something by name.

## Configure
If you install [Generic Mod Config Menu][], you can click the cog button (⚙) on the title screen
or the "mod options" button at the bottom of the in-game menu to configure the mod. Hover the
cursor over a field for details.

> ![](screenshots/generic-config-menu.png)

## Showcase
### Progression mode
The optional 'progression mode' hides spoilers until you've discovered them in-game ([disabled by
default](#Configure)). This currently affects gift tastes and puzzle solutions. Hidden content is
indicated by grayed-out text like this:
> ![](screenshots/progression-mode.png)

The screenshots below are without progression mode, and may show spoilers.

### Where you can look things up
You can look things up by pointing at them...

where | example
----- | -------
in the world | ![](screenshots/target-world.png)
| on the toolbar | ![](screenshots/target-toolbar.png)
| in the calendar | ![](screenshots/target-calendar.png)
| in the social menu | ![](screenshots/target-social-menu.png)
| in your inventory or chests | ![](screenshots/target-inventory.png)
| in shops | ![](screenshots/target-shops.png)
| in bundles<br /><small>(any item shown)</small> | ![](screenshots/target-bundle.png)

And many other places.

### Sample lookups
* See a villager's social data, friendship with you, and the gifts they like. This will highlight
  the gifts you're carrying (green) or own (black).
  > ![](screenshots/villager.png)
  > ![](screenshots/child.png)

  The optional progression mode hides gift tastes until you've learned them in-game.

* See your farm animals' happiness, friendship, problems, and any produce ready for you.
  > ![](screenshots/farm-animal.png)

* See your own stats and skill progress. Each green bar represents your progress towards that level.
  This works from the load-game menu too.
  > ![](screenshots/player.png)

* See a monster's stats, your progress towards the Adventurer's Guild eradication goals, and what
  items the monster will drop when killed.

  The drop list will highlight which items will definitely drop (black), might drop because you have
  the [Burglar's Ring](https://stardewvalleywiki.com/Burglar%27s_Ring) (gray but not crossed out),
  or won't drop this time (crossed out). **Note:** this shows the normal monster drops, but the game
  may add special drops that are hardcoded and can't be detected by mods.
  > ![](screenshots/monster.png)

* See what an item is used for, who likes it as a gift, and what you can use it for. Look up a movie
  ticket to see what's playing and who would like the movie.
  > ![](screenshots/item.png)

* See where you can catch a fish and what it'll produce in fish ponds:
  > ![](screenshots/fish.png)

* See when a crop will be ready to harvest.
  > ![](screenshots/crop.png)

* See when a crafting station will be ready, and what recipes it can produce. This works with
  most custom machines too.
  > ![](screenshots/crafting.png)
  > ![](screenshots/cask.png)

* See useful info about your buildings (different for each building type). For example:
  > ![](screenshots/barn.png)
  > ![](screenshots/fish-pond.png)

* See when a tree will bear fruit, how long until its fruit quality increases, and any
  problems preventing it from growing.
  > ![](screenshots/fruit-tree2.png)
  > ![](screenshots/fruit-tree.png)

* See how long your fences will last.
  > ![](screenshots/fence.png)

* See what mine nodes contain.
  > ![](screenshots/mine-stone.png)
  > ![](screenshots/mine-ore.png)
  > ![](screenshots/mine-ice.png)

### Supported lookups
And much more! Here's a rough list of things you can look up (not necessarily complete):

* characters:
  * monsters;
  * players (and save slots);
  * children and villagers;
  * farm animals;
  * horses;
  * pets;
  * spirits like Gourmand and Trash Bear;
* objects and items, including...
  * crops and seeds;
  * fences;
  * fish;
  * furniture;
  * machines;
  * movie snacks;
  * music blocks;
  * recipes;
  * spawned objects in the world (e.g. mining nodes in the mines);
* buildings:
  * barns/coops;
  * cabins;
  * fish ponds;
  * silos;
  * slime hutches;
  * stables;
  * etc;
* terrain features, including...
  * bushes;
  * crops;
  * trees and fruit trees.
* in-world puzzles, including...
  * Fern Islands crystal cave puzzle;
  * Fern Islands field office donation screen;
  * Fern Islands mermaid puzzle;
  * Fern Islands shrine puzzle.
* map tiles (if [enabled](#configure)).

### Data mining fields (advanced)
Are you a data miner or trying to figure out the game mechanics? [Enable data mining fields](#configuration)
to see raw game data too. This will show 'pinned' data handpicked by Lookup Anything, along with a
full dynamic dump of the raw data. Best used with the 'force full-screen' option:
> ![](screenshots/debug-farm-animal.png)

Enable tile lookups to see information about map tiles:
> ![](screenshots/map-tile.png)

## Compatibility
Lookup Anything is compatible with Stardew Valley 1.6+ on Linux/macOS/Windows, both single-player and
multiplayer. There are no known issues in multiplayer (even if other players don't have it installed).
> When playing with splitscreen multiplayer, it can be helpful to edit `"ToggleLookup"` and `"ToggleSearch"` in the config.json in the mod folder to include multiple keys or gamepad buttons seperated by comma.
>
> For example, the follwing configuraton allows gamepad players to press down on their left thumb stick to look up the most relevant object on their screen, while the keyboard player can still use F1 key for lookups on their screen.
>```json
>{
>  "Controls": {
>    "ToggleLookup": "LeftStick, F1",
>    // other configurations...
>  },
>}
>```

## Extensibility for modders
### Nested items
Lookup Anything scans the world to detect items for the 'number owned' and gift taste fields. It
scans inside standard items recursively; for example, if you have an `Object` with the `heldObject`
field set to a chest, Lookup Anything will look inside the chest too.

If you have a custom non-`Object` item (e.g. a tool) which contains items, you can add a custom
`heldObject` field or property with any `Item` type. For example:

```c#
// store one item
public Object heldObject;

// store many items
public Chest heldObject = new Chest();
```

Lookup Anything will detect the field and search inside it too.

### Hovered items in custom menus
Lookup Anything detects when the cursor is over an item in standard menus. For custom menus, create
a `HoveredItem` field with any `Item` type and Lookup Anything will detect it:

```c#
public Object HoveredItem;
```

### Hovered NPCs in custom menus
Lookup Anything detects when the cursor is over an NPC in standard menus. For custom menus, create
a `HoveredNpc` field with an `NPC` type and Lookup Anything will detect it:

```c#
public NPC HoveredNpc;
```

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/518)

[Generic Mod Config Menu]: https://www.nexusmods.com/stardewvalley/mods/5098
