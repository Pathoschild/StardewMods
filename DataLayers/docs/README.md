**Data Layers** is a [Stardew Valley](http://stardewvalley.net/) mod that overlays the world with
visual data like accessibility, bee/Junimo/scarecrow/sprinkler coverage, etc. It automatically
includes data from other mods if applicable.

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Compatibility](#compatibility)
* [Advanced](#advanced)
  * [Export to JSON](#export-to-json)
  * [For mod authors](#for-mod-authors)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](https://www.nexusmods.com/stardewvalley/mods/1691).
3. Run the game using SMAPI.

## Use
Press `F2` to show the overlay, and then `left CTRL` and `right CTRL` to cycle between layers
(buttons configurable).

For coverage layers (e.g. scarecrows or sprinklers), point at one with the cursor to see a blue
border around that one's range and a green border around the coverage without it.

Below are the current data layers.

### Auto
Automatically chooses an overlay based on context like the item you're holding, with live updates
when that changes. You can configure which layers can be shown that way.
> ![](screenshots/auto-layer.png)

### Accessible
Shows where you can walk and highlights warp tiles. Useful for finding hidden paths and nooks, or
testing custom maps.
> ![](screenshots/accessible.png)

### Buildable
Shows where you can construct buildings on the farm. Useful for spotting issues before you try to
build at Robin's, or testing custom maps.
> ![](screenshots/buildable.png)

### Coverage: bee houses
Shows bee houses' flower search range.
> ![](screenshots/bee-houses.png)

### Coverage: bombs
Shows bombs' explosion radius, including the three different effects (where they'll dig, destroy
objects, and hurt monsters/players).
> ![](screenshots/bombs.png)

### Coverage: Junimo huts
Shows Junimo hut coverage, and highlights crops they won't reach. Also works when placing a Junimo
hut from the Wizard's build menu.
> ![](screenshots/junimo-huts.png)

### Coverage: scarecrows
Shows scarecrow coverage, and highlights unprotected crops. Also works on scarecrows being placed.
> ![](screenshots/scarecrows.png)

### Coverage: sprinklers
Shows sprinkler coverage, and highlights unsprinkled crops. Also works on sprinklers being placed.
> ![](screenshots/sprinklers.png)

### Crops: fertilized
Shows which crops have fertilizer applied.
> ![](screenshots/crops-fertilized.png)

### Crops: watered
Shows which crops have been watered today.
> ![](screenshots/crops-watered.png)

### Crops: ready for harvest
Shows which crops are ready to harvest, or which won't be ready before they die due to a season
change.
> ![](screenshots/crops-harvest.png)

### Crops: water for paddy crops
Shows which tiles are close enough to water for paddy crops to get auto-watered and bonus growth.
> ![](screenshots/crops-paddy-water.png)

### Machine processing
Shows whether your machines are empty, processing, or finished. (You need to install
[Automate](https://www.nexusmods.com/stardewvalley/mods/1063) for this layer to appear, but it'll
work for machines that aren't being automated too.)
> ![](screenshots/machines.png)

### Tillable
Shows where you can till dirt with your hoe. Useful for planning crop layouts.
> ![](screenshots/tillable.png)

### Grid
The grid layer shows [tile borders](https://stardewvalleywiki.com/Modding:Modder_Guide/Game_Fundamentals#Tiles)
useful for planning layouts, calculating fishing distance, etc:
> ![](screenshots/grid-layer.png)

You can optionally [edit the settings](#configure) to enable the grid for all layers instead:
> ![](screenshots/grid-option.png)

## Configure
If you install [Generic Mod Config Menu][], you can click the cog button (⚙) on the title screen
or the "mod options" button at the bottom of the in-game menu to configure the mod. Hover the
cursor over a field for details.

> ![](screenshots/generic-config-menu.png)

## Compatibility
Data Layers is compatible with Stardew Valley 1.6+ on Linux/macOS/Windows, both single-player and
multiplayer. There are no known issues in multiplayer (even if other players don't have it
installed).

Data Layers will automatically integrate with these mods if you they're installed:

mod installed     | effects
----------------- | ----------
Automate          | Shows whether your machines are empty, processing, or ready to harvest.
Better Junimos    | Shows custom Junimo hut coverage.
Better Sprinklers | Shows custom sprinkler coverage.
Line Sprinklers   | Shows line sprinkler coverage.
MultiFertilizer   | Show crops covered with multi-fertilizers.
Pelican Fiber     | Shows coverage when building a Junimo hut or sprinkler through Pelican Fiber's menu.
Prismatic Tools   | Shows prismatic scarecrow coverage.
Simple Sprinkler  | Shows custom sprinkler coverage.
_custom sprinkers_ | Shows custom sprinkler coverage for any mod using the game's built-in sprinkler methods (including Prismatic Tools).


## Advanced
### Export to JSON
You can export a data layer to a JSON file (e.g. to use in another tool). Just go to the location
in-game you want to export, open a data layer, and enter `data-layers export` in the SMAPI console.
The layer data will be exported for the entire current location (not just the visible area).

### For mod authors
See the [author guide](author-guide.md) for more info!

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1691)

[Generic Mod Config Menu]: https://www.nexusmods.com/stardewvalley/mods/5098
