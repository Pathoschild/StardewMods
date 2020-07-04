**Content Patcher** is a [Stardew Valley](http://stardewvalley.net/) mod which loads content packs
to change the game's data, images, and maps without replacing game files.

## Contents
* [For players](#for-players)
  * [Install](#install)
  * [Compatibility](#compatibility)
  * [Multiplayer](#multiplayer)
* [For mod authors](#for-mod-authors)
* [See also](#see-also)

## For players
### Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [this mod from Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/1915).
3. Unzip any Content Patcher content packs into `Mods` to install them.
4. Run the game using SMAPI.

That's it! Content packs unzipped into `Mods` will be loaded and applied automatically.

### Compatibility
Content Patcher is compatible with Stardew Valley 1.4+ on Linux/Mac/Windows, both single-player and
multiplayer.

### Multiplayer
Content Patcher works fine in multiplayer. It's best if all players have the same content packs,
but not required. Here are the effects if some players don't have a content pack installed:

patch type | effect
---------- | ------
visual     | Only visible to players that have it installed.
maps       | Only visible to players that have it installed. Players without the custom map will see the normal map and will be subject to the normal bounds (e.g. they may see other players walk through walls, but they won't be able to follow).
data       | Only directly affects players that have it installed, but can indirectly affect other players. For example, if a content pack changes `Data/ObjectInformation` and you create a new object, other player will see that object's custom values even if their `Data/ObjectInformation` doesn't have those changes.

## For mod authors
* To create content packs, see the [author guide](docs/author-guide.md) and its [tokens subpage](docs/author-tokens-guide.md).
* To add custom Content Patcher tokens from a SMAPI mod, see the [extensibility guide](docs/extensibility.md).

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1915)
* [Ask for help](https://stardewvalleywiki.com/Modding:Help)
