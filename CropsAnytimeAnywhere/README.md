**Crops Anytime Anywhere** is a [Stardew Valley](http://stardewvalley.net/) mod that lets you grow
crops in any season and location, including on grass/dirt tiles you normally couldn't till. You can
optionally configure the seasons, locations, and tillable tile types.

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

setting              | default     | what it affects
:------------------- | :---------- | :------------------
`EnableInSeasons`    | all seasons | The seasons in which any crops should grow. Crops will revert to normal in disabled seasons.
`FarmAnyLocation`    | `true`      | Whether you can plant crops in non-farm locations, as long as there's tillable dirt there. This only works in seasons enabled via `EnableInSeasons`.
`ForceTillable`      | dirt, grass | The tile types to make tillable beyond those that would normally be (`other` includes indoor flooring). The default values let you plant on dirt and grass tiles.

## Compatibility
Compatible with Stardew Valley 1.4+ on Linux/Mac/Windows, both single-player and multiplayer. In
multiplayer mode, it must be installed by the main player to work correctly; farmhands only need it
if they want to enable tilling more tile types (but it won't cause any issues if they don't have
it).

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/3000)
