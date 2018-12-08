**Crops Anytime Anywhere** is a [Stardew Valley](http://stardewvalley.net/) mod that lets you grow
crops in any season (including winter) and location, both configurable. 

![](screenshot.gif)

## Contents
* [Install](#install)
* [Configure](#configure)
* [Compatibility](#compatibility)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](https://www.nexusmods.com/stardewvalley/mods/3000).
3. Run the game using SMAPI.

Note that the mod doesn't change store inventories, so you can only buy crop seeds during their
usual seasons.

## Configure
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

Here's what you can change:

setting              | default    | what it affects
:------------------- | :--------- | :------------------
`Seasons`            | all `true` | The seasons in which any crops should grow. Crops will revert to normal in disabled seasons (and die in the season transition if they're out of season).
`AllowCropsAnywhere` | `true`     | Whether you can plant crops in non-farm locations, as long as there's tillable dirt there. This only works in seasons enabled via `Seasons`.

## Compatibility
The mod is compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, both single-player and
multiplayer. In multiplayer mode, it must be installed by the main player to work correctly. (It
doesn't need to be installed by farmhands, but it won't cause any issues if they have it.)

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/3000)
* [Discussion thread](https://community.playstarbound.com/threads/crops-anytime-anywhere.150585/)
