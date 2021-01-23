**Skip Intro** is a minimal [Stardew Valley](http://stardewvalley.net/) mod that skips straight to
the title screen when you start the game. You can optionally skip to the load screen, co-op join
screen, or co-op host screen instead. It also skips the screen transitions, so starting the game is
much faster.

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/533).
3. Run the game using SMAPI.

## Configure
### In-game settings
If you have [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed,
you can click the cog button (⚙) on the title screen to configure the mod. Hover the cursor over
a field for details, or see the next section.

![](screenshots/generic-config-menu.png)

### `config.json` file
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

These are the available settings:

| setting  | what it affects
| -------- | -------------------
| `SkipTo` | Default `Title`. Which screen to skip to; can be `Title`, `Load`, `JoinCoop`, `HostCoop`.

## Compatibility
Skip Intro is compatible with Stardew Valley 1.4+ on Linux/Mac/Windows, both single-player and
multiplayer. There are no known issues in multiplayer (even if other players don't have it installed).

## See also
* [release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/533)
