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

You can change these settings:

setting                | default | what it affects
:--------------------- | :------ | :------------------
`GrowCrops`            | true    | Per-location, whether crops can grow here.
`GrowCropsOutOfSeason` | true    | Per-location, whether out-of-season crops grow here too.
`ForceTillable`        | dirt, grass | The tile types to make tillable beyond those that would normally be. The available types are `dirt`, `grass`, `stone`, and `other`.

The location keys can be one of `*` (all locations), `Indoors`, `Outdoors`, or an internal location
name. (You can use [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679) to see location
names.)

By default all locations can grow any crop, but you can change that. For example, this enables
in-season crops anywhere _and_ out-of-season crops in the secret woods:

```js
{
   "InLocations": {
      "*": {
         "GrowCrops": true,
         "GrowCropsOutOfSeason": false
      },
      "SecretWoods": {
         "GrowCrops": true,
         "GrowCropsOutOfSeason": true
      },
   }
}
```

If multiple configs apply to a location, the last one specified applies. This is always additive,
so you can't _disable_ crops in a location that normally does allow them.

## Compatibility
Compatible with Stardew Valley 1.4+ on Linux/Mac/Windows, both single-player and multiplayer. In
multiplayer mode, it must be installed by the main player to work correctly.

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/3000)
