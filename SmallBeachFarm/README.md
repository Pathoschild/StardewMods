**Small Beach Farm** is a [Stardew Valley](http://stardewvalley.net/) mod which replaces the
riverlands farm with a fertile pocket beach suitable for slower/challenge runs, and lets you
catch both river and ocean fish. You can optionally enable islands and beach sounds.

> ![](docs/farm.png)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [this mod from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/3750).
3. Run the game using SMAPI.

## Usage
Just load a save with the riverlands farm, and you'll see the new map!

A few highlights:

* The beach is fertile, so you can plant crops in the grass and sand too:  
  > ![](docs/tilled.png)
* The exit to Marnie's ranch is on the right:
  > ![](docs/exits.png)
* You can catch riverlands fish in the river and ocean fish in the ocean (for both fishing and
  crab pots):
  > ![](docs/fish-areas.png)
* If you use [Automate](https://www.nexusmods.com/stardewvalley/mods/1063), you can connect chests
  to crab pots all along the beach:
  > ![](docs/automate-crabpots.png)
* The beach includes a functional campfire you can light or extinguish (pairs well with the
  [Campfire Cooking](https://mods.smapi.io/#Campfire_Cooking) mod):  
  > ![](docs/campfire.gif)

If you need more space, the alternate _Small Beach With Islands_ adds ocean islands for extra land
area:
> ![](docs/farm-islands.png)

## Configure
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

Here's what you can change:

setting         | default | what it affects
:-------------- | :------ | :------------------
`EnableIslands` | `false` | Whether to add ocean islands with extra land area.
`UseBeachMusic` | `false` | Use the beach's background music (i.e. wave sounds) on the beach farm.

## FAQs
### Compatibility
This is compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, both single-player and
multiplayer. It can't be combined with other mods that replace the riverlands farm.

It should work with most map recolors (notably A Wittily Named Recolor, Eemie's Just Another Map
Recolor, and Starblue Valley). To add support for a new recolor, create a folder in
`assets/recolors` matching its ID (from its `manifest.json` folder) and drop the modified
`{season}_smallBeachFarm.png` files into it. (Consider sending me the files, so I can add
official support for the recolor!)

### Can I use this with an existing save?
Yep! If you have things in the water due to the smaller map, see
[Saves#Change farm type](https://stardewvalleywiki.com/Saves#Change_farm_type)
for some suggested fixes (skip the part about editing the save file).

## Compiling from source
Installing stable releases from Nexus Mods is recommended for most users. If you really want to
compile the mod yourself, see the repository readme for the main instructions.

Special instructions for Small Beach Farm:

1. [Unpack your game's `Content` folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files).
2. Copy `Maps/paths.png`, `Maps/spring_beach.png`, and `Maps/spring_town.png` directly into the `assets` folder (without the `Map` folder).
3. Compile as usual. See [Modding:Maps](https://stardewvalleywiki.com/Modding:Maps) for help editing the map file.

## See also
* Initial farm maps commissioned from [Opalie](https://www.nexusmods.com/stardewvalley/users/38947035)!
* [release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/3750)
