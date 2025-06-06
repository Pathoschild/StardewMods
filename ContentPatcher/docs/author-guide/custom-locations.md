← [author guide](../author-guide.md)

> [!WARNING]  
> **This feature is deprecated and shouldn't be used in newer content packs.**  
> See [Modding:Location data](https://stardewvalleywiki.com/Modding:Location_data) for help
adding or editing locations through the built-in feature in Stardew Valley 1.6 and later.

----

The `CustomLocations` feature lets you add new in-game locations, complete with their own maps and
warps. Content Patcher automatically handles NPC pathfinding, object persistence, etc.

**This is only needed to add a new location.** To edit an existing one, use
[`EditMap`](action-editmap.md) instead.

**🌐 In other languages: [zh (中文)](../zh/author-guide/custom-locations.md)**.

## Contents
* [Introduction](#introduction)
  * [Maps vs locations](#maps-vs-locations)
* [Usage](#usage)
  * [Format](#format)
  * [Examples](#examples)
* [FAQs](#faqs)
  * [How do I get to my location in-game?](#how-do-i-get-to-my-location-in-game)
  * [Can I make the location conditional?](#can-i-make-the-location-conditional)
  * [Can I rename a location?](#can-i-rename-a-location)
* [See also](#see-also)

## Introduction
### Maps vs locations
Although players use them interchangeably, at a code level _maps_ and _locations_ are two different
things. The distinction is crucial to understanding how this feature works:

* A [**map**](https://stardewvalleywiki.com/Modding:Maps) is an asset which describes the tile
  layout, tilesheets, and map/tile properties for the in-game area. The map is reloaded each time
  you load a save, and each time a mod changes the map.
* A [**location**](https://stardewvalleywiki.com/Modding:Modder_Guide/Game_Fundamentals#GameLocation_et_al)
  is part of the game code and manages the in-game area and everything inside it (including non-map
  entities like players). The location is read/written to the save file, and is only loaded when
  loading the save file.

In other words, a _location_ (part of the game code) contains the _map_ (loaded from the `Content`
folder):

```
┌─────────────────────────────────┐
│ Location                        │
│   - objects                     │
│   - furniture                   │
│   - crops                       │
│   - bushes and trees            │
│   - NPCs and players            │
│   - etc                         │
│                                 │
│   ┌─────────────────────────┐   │
│   │ Map asset               │   │
│   │   - tile layout         │   │
│   │   - map/tile properties │   │
│   │   - tilesheets          │   │
│   └─────────────────────────┘   │
└─────────────────────────────────┘
```

## Usage
### Format
Custom locations are added using a separate `CustomLocations` field in your `content.json` file
(outside the `Changes` field which contains your patches). This consists of a list of models with
these fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td><code>Name</code></td>
<td>

The location's unique internal name.

The name:
* Must only contain alphanumeric or underscore characters.
* Must begin with [your mod's manifest `UniqueId`](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest)
  (like `Your.ModId_`), to avoid name conflicts. For legacy reasons, you can also start it with
  `Custom_` instead but this isn't recommended.
* Must be **globally unique**, so prefixing it with your mod ID is strongly recommended. If
  two content packs add a location with the same name, both will be rejected with an error message.
  If the player ignores the warning and saves anyway at that point, anything in the location will be
  permanently lost.

This field can't contain [tokens](../author-guide.md#tokens).

</td>
</tr>
<tr>
<td><code>FromMapFile</code></td>
<td>

The relative path to the location's map file in your content pack folder (`.tmx`, `.tbin`, or `.xnb`).

This field can't contain [tokens](../author-guide.md#tokens), but you can make conditional changes
using [`EditMap`](action-editmap.md) after it's loaded (see examples below).

</td>
</tr>
<td><code>MigrateLegacyNames</code></td>
<td>

_(optional)_ A list of former location names that may appear in the save file instead of the one
given by `Name`. This is only meant to allow migrating locations added through a different mod, and
shouldn't be used in most cases. See [_Can I rename a location?_](#can-i-rename-a-location) for
more info.

This field can't contain [tokens](../author-guide.md#tokens).

</td>
</tr>
</table>

### Examples
Let's say you want to give Abigail a walk-in closet. This example makes three changes:

1. add the in-game location with the base map;
2. add a simple warp from Abigail's room;
3. add a conditional map edit (optional).

Here's how you'd do that:

```js
{
   "Format": "2.7.0",

   "CustomLocations": [
      // add the in-game location
      {
         "Name": "{{ModId}}_AbigailCloset",
         "FromMapFile": "assets/abigail-closet.tmx"
      }
   ],

   "Changes": [
      // add a warp to the new location from Abigail's room
      {
         "Action": "EditMap",
         "Target": "Maps/SeedShop",
         "AddWarps": [
            "8 10 {{ModId}}_AbigailCloset 7 20"
         ]
      },

      // conditionally edit the map if needed
      {
         "Action": "EditMap",
         "Target": "Maps/{{ModId}}_AbigailCloset",
         "FromFile": "assets/abigail-closet-clean.tmx",
         "When": {
            "HasFlag": "AbigailClosetClean" // example custom mail flag
         }
      }
   ]
}
```

## FAQs
### How do I get to my location in-game?
`CustomLocations` only adds the location to the game. Don't forget to give players some way to
reach it, usually by adding warps from another map using [`EditMap`](action-editmap.md) (like the
example above). For a quick test, you can also run the `debug warp <location name>` [console
command](https://stardewvalleywiki.com/Modding:Console_commands#Console_commands) to warp directly
into it.

### Can I make the location conditional?
No, since removing the location will permanently delete everything inside it. That's just like the
base game, which adds every location even if the player doesn't have access to them yet.

There's many ways you can decide when players have access. For example, you can use
[`EditMap`](action-editmap.md) to add warps conditionally or to add some form of roadblock that
must be cleared (e.g. a landslide).

### Can I rename a location?
**Renaming a location will permanently lose player changes made for the old name if you're not
careful.**

Content Patcher lets you define "legacy" names to avoid that. When loading a save file, if it
doesn't have a location for `Name` but it does have one with a legacy name, the legacy location's
data will be loaded into the custom location. When the player saves, the previous location will be
permanently renamed to match the `Name`.

For example:

```js
{
   "Format": "2.7.0",
   "CustomLocations": [
      {
         "Name": "{{ModId}}_AbigailCloset",
         "FromMapFile": "assets/abigail-closet.tmx",
         "MigrateLegacyNames": [ "Custom_AbbyRoom" ]
      }
   ]
}
```

Legacy names can have any format, but they're subject to two restrictions:

* They must be **globally** unique. They can't match the `Name` or `MigrateLegacyNames` for any
  other custom location, including those added by another mod installed by the player.
* They can't match a vanilla location name.

## See also
* [Author guide](../author-guide.md) for other actions and options
