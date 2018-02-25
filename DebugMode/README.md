**Debug Mode** is an open-source [Stardew Valley](http://stardewvalley.net/) mod which lets you
press `~` to view debug information and unlock the game's debug commands (including teleportation
and time manipulation).

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Versions](#versions)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/679/).
3. Run the game using SMAPI.

## Use
Press the `~` key (configurable) to enable or disable debug mode. This will...

1. Show cursor crosshairs (with the current map name and tile position), and the game's built-in debug info:

   ![screenshot](screenshots/debug-mode.png)

2. Unlock the game's built-in debug commands:

   hotkey | action
   :----- | :-----
   `T`    | Add one hour to the clock.
   `SHIFT` + `Y` | Subtract 10 minutes from the clock.
   `Y`    | Add 10 minutes to the clock.
   `1`    | Warp to the mountain (facing Robin's house).
   `2`    | Warp to the town (on the path between the town and community center).
   `3`    | Warp to the farm (at your farmhouse door).
   `4`    | Warp to the forest (near the traveling cart).
   `5`    | Warp to the beach (left of Elliott's house).
   `6`    | Warp to the mine (at the inside entrance).
   `7`    | Warp to the desert (in Sandy's shop).
   `K`    | Move down one mine level. If not currently in the mine, warp to it.
   `F5`   | Toggle the player.
   `F7`   | Draw a tile grid.
   `F8`   | Show a debug command input window (not currently documented).
   `B`    | Shift the toolbar to show the next higher inventory row.
   `N`    | Shift the toolbar to show the next lower inventory row.

3. If you set `AllowDangerousCommands: true` in the [configuration](#configuration) (disabled by
   default), also unlock these debug commands:

   hotkey | action
   :----- | :-----
   `P`    | Immediately go to bed and start the next day.
   `M`    | Immediately go to bed and start the next season.
   `H`    | Randomise the player's hat.
   `I`    | Randomise the player's hair.
   `J`    | Randomise the player's shirt and pants.
   `L`    | Randomise the player.
   `U`    | Randomise the farmhouse wallpaper and floors.
   `F10`  | **Broken.** Tries to launch a multiplayer server, and crashes.

## Configure
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want. These are the available settings:

setting           | what it affects
:---------------- | :------------------
`Controls`        | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default value is `~` to toggle debug mode.
`AllowDangerousCommands` | Default `false`. This allows debug commands which end the current day/season & save, randomise your player or farmhouse decorations, or crash the game. Only change this if you're aware of the consequences.

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/679)
* [Discussion thread](http://www.nexusmods.com/stardewvalley/mods/679)
