**Content Patcher** is a [Stardew Valley](http://stardewvalley.net/) mod which loads content packs
that change the game's images and data without replacing XNB files.

**This documentation is for modders. If you're a player, see the [Nexus page](https://www.nexusmods.com/stardewvalley/mods/1915) instead.**

## Contents
* [Install](#install)
* [Intro](#intro)
* [Create a content pack](#create-a-content-pack)
  * [Overview](#overview)
  * [Common fields](#common-fields)
  * [Replace an entire file](#replace-an-entire-file)
  * [Edit part of an image](#edit-part-of-an-image)
  * [Edit part of a data file](#edit-part-of-a-data-file)
  * [Edit part of a map](#edit-part-of-a-map)
* [Advanced: tokens & conditions](#advanced-tokens--conditions)
  * [Overview](#overview-1)
  * [Global tokens](#global-tokens)
  * [Randomization](#randomization)
  * [Dynamic tokens](#dynamic-tokens)
  * [Player config](#player-config)
  * [Mod-provided tokens](#mod-provided-tokens)
* [Release a content pack](#release-a-content-pack)
* [Troubleshoot](#troubleshoot)
  * [Schema validator](#schema-validator)
  * [Patch commands](#patch-commands)
  * [Debug mode](#debug-mode)
  * [Verbose log](#verbose-log)
* [FAQs](#faqs)
  * [Compatibility](#compatibility)
  * [Multiplayer](#multiplayer)
  * [How multiple patches interact](#how-multiple-patches-interact)
  * [Known limitations](#known-limitations)
* [Extensibility for modders](#extensibility-for-modders)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [this mod from Nexus mods](https://www.nexusmods.com/stardewvalley/mods/1915).
3. Unzip any Content Patcher content packs into `Mods` to install them.
4. Run the game using SMAPI.

## Intro
### What is Content Patcher?
Content Patcher lets you create a [standard content pack](https://stardewvalleywiki.com/Modding:Content_packs)
which changes the game's data and images, no programming needed. Players can install it by
unzipping it into `Mods`, just like a SMAPI mod.

Just by editing a JSON file, you can make very simple changes to the game (like replace one image
file), or more interesting changes (like things that look different in each season), or very
specific changes (like coffee is more expensive in winter when it's snowing on the weekend).

### Content Patcher vs XNB mods
If you're familiar with creating XNB mods, Content Patcher supports everything XNB mods supported.
Here's a quick comparison:

&nbsp;               | XNB mod                         | Content Patcher
-------------------- | ------------------------------- | ---------------
easy to create       | ✘ need to unpack/repack files  | ✓ edit JSON files
easy to install      | ✘ different for every mod      | ✓ drop into `Mods`
easy to uninstall    | ✘ manually restore files       | ✓ remove from `Mods`
update checks        | ✘ no                           | ✓ yes (via SMAPI)
compatibility checks | ✘ no                           | ✓ yes (via SMAPI DB)
mod compatibility    | ✘ very poor<br /><small>(each file can only be changed by one mod)</small> | ✓ high<br /><small>(mods only conflict if they edit the same part of a file)</small>
game compatibility   | ✘ break in most updates        | ✓ only affected if the part they edited changes
easy to troubleshoot | ✘ no record of changes         | ✓ SMAPI log + Content Patcher validation

### Content Patcher vs other mods
Content Patcher supports all game assets with some very powerful features, but it's a generalist
framework. More specialized frameworks might be better for specific things. You should consider
whether one of these would work for you:

  * [Advanced Location Loader](https://community.playstarbound.com/resources/smapi-advanced-location-loader.3619/) for complex changes to maps. (For simple changes, see _[edit part of a map](#edit-part-of-a-map)_ below.)
  * [Custom Farming Redux](https://www.nexusmods.com/stardewvalley/mods/991) to add machines.
  * [Custom Furniture](https://www.nexusmods.com/stardewvalley/mods/1254) to add furniture.
  * [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720) to add many things like items, crafting recipes, crops, fruit trees, hats, and weapons.

## Create a content pack
### Overview
A content pack is a folder with these files:
* a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:Content_packs) on the wiki);
* a `content.json` which describes the changes you want to make;
* and any images or files you want to use.

The `content.json` file has three main fields:

field          | purpose
-------------- | -------
`Format`       | The format version. You should always use the latest version (currently `1.11.0`) to use the latest features and avoid obsolete behavior.<br />(**Note:** this is not the Content Patcher version!)
`Changes`      | The changes you want to make. Each entry is called a **patch**, and describes a specific action to perform: replace this file, copy this image into the file, etc. You can list any number of patches.
`ConfigSchema` | _(optional)_ Defines the `config.json` format, to support more complex mods. See [_player configuration_](#player-config).

You can list any number of patches (surrounded by `{` and `}` in the `Changes` field). See the next
few sections for more info about the format. For example:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "Load",
         "Target": "Animals/Dinosaur",
         "FromFile": "assets/dinosaur.png"
      },

      {
         "Action": "EditImage",
         "Target": "Maps/springobjects",
         "FromFile": "assets/fish-object.png"
      },
   ]
}
```

### Common fields
All patches support these common fields:

field      | purpose
---------- | -------
`Action`   | The kind of change to make (`Load`, `EditImage`, `EditData`, `EditMap`); explained in the next four sections.
`Target`   | The game asset you want to patch (or multiple comma-delimited assets). This is the file path inside your game's `Content` folder, without the file extension or language (like `Animals/Dinosaur` to edit `Content/Animals/Dinosaur.xnb`). This field supports [tokens](#advanced-tokens--conditions) and capitalisation doesn't matter. Your changes are applied in all languages unless you specify a language [condition](#advanced-tokens--conditions).
`LogName`  | _(optional)_ A name for this patch shown in log messages. This is very useful for understanding errors; if not specified, will default to a name like `entry #14 (EditImage Animals/Dinosaurs)`.
`Enabled`  | _(optional)_ Whether to apply this patch. Default true. This fields supports immutable [tokens](#advanced-tokens--conditions) (e.g. config tokens) if they return true/false.
`When`     | _(optional)_ Only apply the patch if the given conditions match (see [_conditions_](#advanced-tokens--conditions)).

### Replace an entire file
`"Action": "Load"` replaces the entire file with your version. This is useful for mods which
change the whole file (like pet replacement mods).

Avoid this if you don't need to change the whole file though — each file can only be replaced once,
so your content pack won't be compatible with other content packs that replace the same file.
(It'll work fine with content packs that only edit the file, though.)

field      | purpose
---------- | -------
&nbsp;     | See _common fields_ above.
`FromFile` | The relative file path in your content pack folder to load instead (like `assets/dinosaur.png`). This can be a `.json` (data), `.png` (image), `.tbin` (map), or `.xnb` file. This field supports [tokens](#advanced-tokens--conditions) and capitalisation doesn't matter.

Required fields: `FromFile`.

For example, this replaces the dinosaur sprite with your own image:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "Load",
         "Target": "Animals/Dinosaur",
         "FromFile": "assets/dinosaur.png"
      },
   ]
}
```

### Edit part of an image
`"Action": "EditImage"` changes one part of an image. For example, you can change one area in a
spritesheet, or overlay an image onto the existing one.

Any number of content packs can edit the same file. You can extend an image downwards by just
patching past the bottom (Content Patcher will expand the image to fit).

field      | purpose
---------- | -------
&nbsp;     | See _common fields_ above.
`FromFile` | The relative path to the image in your content pack folder to patch into the target (like `assets/dinosaur.png`). This can be a `.png` or `.xnb` file. This field supports [tokens](#advanced-tokens--conditions) and capitalisation doesn't matter.
`FromArea` | The part of the source image to copy. Defaults to the whole source image. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. Its fields may contain tokens.
`ToArea`   | The part of the target image to replace. Defaults to the `FromArea` size starting from the top-left corner. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. Its fields may contain tokens.
`PatchMode`| How to apply `FromArea` to `ToArea`. Defaults to `Replace`. Possible values: <ul><li><code>Replace</code>: replace every pixel in the target area with your source image. If the source image has transparent pixels, the target image will become transparent there.</li><li><code>Overlay</code>: draw your source image over the target area. If the source image has transparent pixels, the target image will 'show through' those pixels. Semi-transparent or opaque pixels will replace the target pixels.</li></ul>For example, let's say your source image is a pufferchick with a transparent background, and the target image is a solid green square. Here's how they'll be combined with different `PatchMode` values:<br />![](docs/screenshots/patch-mode-examples.png)

Required fields: `FromFile`.

For example, this changes one object sprite:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditImage",
         "Target": "Maps/springobjects",
         "FromFile": "assets/fish-object.png",
         "FromArea": { "X": 0, "Y": 0, "Width": 16, "Height": 16 }, // optional, defaults to entire FromFile
         "ToArea": { "X": 256, "Y": 96, "Width": 16, "Height": 16 } // optional, defaults to source size from top-left
      },
   ]
}
```

### Edit part of a data file
`"Action": "EditData"` lets you edit fields or add/remove/edit entries inside a data file.

field      | purpose
---------- | -------
&nbsp;     | See _common fields_ above.
`Fields`   | The individual fields you want to change for existing entries. This field supports [tokens](#advanced-tokens--conditions) in field keys and values. The key for each field is the field index (starting at zero) for a slash-delimited string, or the field name for an object.
`Entries`  | The entries in the data file you want to add, replace, or delete. If you only want to change a few fields, use `Fields` instead for best compatibility with other mods. To add an entry, just specify a key that doesn't exist; to delete an entry, set the value to `null` (like `"some key": null`). This field supports [tokens](#advanced-tokens--conditions) in entry keys and values.<br />**Caution:** some XNB files have extra fields at the end for translations; when adding or replacing an entry for all locales, make sure you include the extra fields to avoid errors for non-English players.
`MoveEntries` | Change the entry order in a list asset like `Data/MoviesReactions`. (Using this with a non-list asset will cause an error, since those have no order.)
`FromFile` | The relative path to a JSON file in your content pack folder containing the `Fields`, `Entries`, and `MoveEntries`. The field and file contents can contain [tokens](#advanced-tokens--conditions). Mutually exclusive with `Fields`, `Entries`, and `MoveEntries`. See _load changes from a file_ below for an example.

Required fields: at least one of `Fields`, `Entries`, `MoveEntries`, or `FromFile`.

You can have any combination of those fields within one patch. They'll be applied in this order:
`Entries`, `FromFile`, `Fields`, `MoveEntries`.

<dl>
<dt>Definitions</dt>
<dd>

Consider this line in `Data/ObjectInformation`:
```js
   "70": "Jade/200/-300/Minerals -2/Jade/A pale green ornamental stone."
```

The whole line is one **entry** with a key (`"70"`) and value (`"Jade/200/-300/Minerals -2/Jade/A
pale green ornamental stone."`). The value contains six **fields** counting from zero, from field 0
(`Jade`) to field 5 (`A pale green ornamental stone.`).

</dd>

<dt>Basic changes</dt>
<dd>

This example patch creates a new in-game item (i.e. adds a new entry), and edits the name and
description fields for an existing entry (item #70):

```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Data/ObjectInformation",
         "Entries": {
            "900": "Crimson Jade/400/-300/Minerals -2/Crimson Jade/A pale green ornamental stone with a strange crimson sheen."
         },
         "Fields": {
            "70": {
               4: "Normal Jade",
               5: "A pale green ornamental stone with no sheen."
            }
         }
       }
   ]
}
```

You can also delete entries entirely by setting their value to `null`. For example, that can be
used to change event conditions:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Data/Events/Beach",
         "Entries": {
            "733330/f Sam 750/w sunny/t 700 1500/z winter/y 1": null,
            "733330/f Sam 750/w sunny/t 700 1500/z winter": "[snipped: long event script here]"
         }
      }
   ]
}
```

</dd>

<dt>Load changes from a file</dt>
<dd>

You can optionally load changes from a separate JSON file in your content pack. The file can contain
`Entries`, `Fields`, and `MoveEntries`. It can use any tokens that would work if used directly in
the patch.

For example, this patch in `content.json`:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Data/ObjectInformation",
         "FromFile": "assets/jade.json"
      }
   ]
}
```

Loads changes from this `assets/jade.json` file:
```js
{
   "Entries": {
      "900": "Crimson Jade/400/-300/Minerals -2/Crimson Jade/A pale green ornamental stone with a strange crimson sheen."
   },
   "Fields": {
      "70": {
         4: "Normal Jade",
         5: "A pale green ornamental stone with no sheen."
      }
   }
}
```

The `FromFile` field can contain tokens, so you can dynamically load a different file. For example,
this single patch loads a dialogue file for multiple NPCs:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Characters/Dialogue/Abigail, Characters/Dialogue/Alex, Characters/Dialogue/Caroline",
         "FromFile": "assets/dialogue/{{TargetWithoutPath}}.json"
      }
   ]
}
```

</dd>

<dt>Edit data model assets</dt>
<dd>

A few assets like `Data/Movies` contain data models, not strings like above. You can edit those
the same way, with two differences: fields have names instead of indexes, and entry values are
structures instead of strings.

For example, this renames a movie to _The Brave Little Pikmin_ and adds a new movie:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Data/Movies",
         "Fields": {
            "spring_movie_0": {
               "Title": "The Brave Little Pikmin"
            }
         },
         "Entries": {
            "spring_movie_3": {
               "ID": "spring_movie_3",
               "Title": "The Brave Little Pikmin II: Now I'm a Piktree",
               "Description": "Follow the continuing adventures of our pikmin hero as he absorbs nutrients from the soil!",
               "SheetIndex": 6,
               "Tags": [ "documentary", "family" ],
               "Scenes": [
                  {
                     "Image": 0,
                     "MessageDelay": 500,
                     "Music": "movie_nature",
                     "Text": "'The Brave Little Pikmin II: Now I'm a Piktree', sponsored by Joja Cola"
                  }
               ]
            }
         }
      }
   ]
}
```

</dd>

<dt>Edit list assets</dt>
<dd>

A few assets like `Data/MoviesReactions` contain a list of entries (meaning they don't have a key).
Content Patcher will automatically select one field to treat as the key, so you can edit them the
same way as usual:

asset | field used
----- | ----------
_default_ | `ID` if it exists.
`Data/ConcessionTastes` | `Name`
`Data/FishPondData` | The `RequiredTags` field with comma-separated tags (like `fish_ocean,fish_crab_pot`). The key is space-sensitive.
`Data/MoviesReactions` | `NPCName`
`Data/TailoringRecipes` | `FirstItemTags` and `SecondItemTags`, with comma-separated tags and a pipe between them (like `item_cloth|category_fish,fish_semi_rare`).  The key is space-sensitive.

List assets also have an order which can affect game logic (e.g. the first entry in
`Data\MoviesReactions` matching the NPC is used). You can move an entry within that order using the
`MoveEntries` field.

Here's an example showing all possible reorder options. (If you specify a `BeforeID` or `AfterID`
that doesn't match any entry, a warning will be shown.)
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Data/MoviesReactions",
         "MoveEntries": [
            { "ID": "Abigail", "BeforeID": "Leah" }, // move entry so it's right before Leah
            { "ID": "Abigail", "AfterID": "Leah" }, // move entry so it's right after Leah
            { "ID": "Abigail", "ToPosition": "Top" }, // move entry to the top of the list
            { "ID": "Abigail", "ToPosition": "Bottom" }, // move entry to the bottom of the list
         ]
      }
   ]
}
```

New entries are added at the bottom of the list by default.

</dd>
</dl>

### Edit part of a map
`"Action": "EditMap"` changes part of an in-game map by copying tiles, properties, and tilesheets
from a source map. This is essentially a copy & paste from one map into another, replacing whatever
was in the target area before.

Any number of content packs can edit the same map. If two patches overlap, whichever one is applied
last will take effect for the overlapping tiles.

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

See _common fields_ above.

</td>
</tr>
<tr>
<td>

`FromFile`

</td>
<td>

The relative path to the map in your content pack folder from which to copy (like
`assets/town.tbin`). This can be a `.tbin` or `.xnb` file. This field supports [tokens](#advanced-tokens--conditions)
and capitalisation doesn't matter.

Content Patcher will handle tilesheets referenced by the `FromFile` map for you if it's a `.tbin`
file:
* If a tilesheet isn't referenced by the target map, Content Patcher will add it for you (with a
  `z_` ID prefix to avoid conflicts with hardcoded game logic). If the source map has a custom
  version of a tilesheet that's already referenced, it'll be added as a separate tilesheet only
  used by your tiles.
* If you include the tilesheet file in your mod folder, Content Patcher will use that one
  automatically; otherwise it will be loaded from the game's `Content/Maps` folder.

</td>
</tr>
<tr>
<td>

`FromArea`

</td>
<td>

The part of the source map to copy. Defaults to the whole source map. This is specified as an
object with the X and Y tile coordinates of the top-left corner, and the tile width and height of
the area. Its fields may contain tokens.

</td>
</tr>
<tr>
<td>

`ToArea`

</td>
<td>

The part of the target map to replace. This is specified as an object with the X and Y tile
coordinates of the top-left corner, and the tile width and height of the area. Its fields may contain tokens.

</td>
</tr>

<tr>
<td>

`MapProperties`

</td>
<td>

The map properties (not tile properties) to add, replace, or delete. To add an property, just
specify a key that doesn't exist; to delete an entry, set the value to `null` (like
`"some key": null`). This field supports [tokens](#advanced-tokens--conditions) in property keys
and values.

</td>
</tr>

</table>

Required fields: at least one of (`FromFile` and `ToArea`) or (`MapProperties`).

For example, this replaces the town square with the one in another map:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditMap",
         "Target": "Maps/Town",
         "FromFile": "assets/town.tbin",
         "FromArea": { "X": 22, "Y": 61, "Width": 16, "Height": 13 },
         "ToArea": { "X": 22, "Y": 61, "Width": 16, "Height": 13 }
      },
   ]
}
```

This changes the warp map property for the farm cave:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditMap",
         "Target": "Maps/FarmCave",
         "MapProperties": {
            "Warp": "8 12 Farm 34 6"
         }
      },
   ]
}
```

(You can patch a map area and change map properties in the same patch.)

Known limitations:
* Patching non-farmhouse-floor tiles into the farmhouse's `Back` layer may cause strange effects,
  due to the game's floor decorating logic.
* Conditional map patches may reset the map's seasonal tilesheets to spring. This is a SMAPI bug
  that will be fixed in SMAPI 3.0.

## Advanced: tokens & conditions
### Overview
A **token** is a placeholder for a predefined value. For example, `season` (the token) may contain
`spring`, `summer`, `fall`, or `winter` (the value). You can use [player config](#player-config),
[global token values](#global-tokens), and [dynamic token values](#dynamic-tokens) as tokens.

Tokens are indicated by two curly braces (except in `When` condition keys, where the braces are
implied). For example, here's a simple dialogue text which includes the current season name:
```js
"Entries": {
   "fri": "It's a beautiful {{season}} day!"
}
```

Most tokens contain values directly, like `{{Season}}` = `spring` or `{{HasProfession}}` =
`Blacksmith, Forester, Miner`. Some tokens also have an _input argument_, which can be a literal
value (like `{{Relationship:Abigail}}` = `Married`) or contain tokens too (like
`{{HasFile:assets/{{spouse}}.png}}` = `true`).

There are two ways to use tokens.

<dl>
<dt>Conditions</dt>
<dd>

You can make a patch conditional by adding a `When` field, which can list any number of conditions.
Each condition has...
* A key (before `:`) containing a [token](#advanced-tokens--conditions) without the outer curly
  braces, like `Season` or `HasValue:{{spouse}}`. The key is not case-sensitive.
* A value (after `:`) containing the comma-separated values to match, like `spring, summer`. If the
  key token returns any of these values, the condition matches. This field supports
  [tokens](#advanced-tokens--conditions) and is not case-sensitive.

For example: this changes the house texture only in spring or summer, if the player is married, and
the number of hearts with their spouse matches the number in the `{{minHearts}}` config field:

```js
{
    "Action": "EditImage",
    "Target": "Buildings/houses",
    "FromFile": "assets/{{season}}_house.png",
    "When": {
        "Season": "spring, summer",
        "HasValue:{{spouse}}": "true",
        "Hearts:{{spouse}}": "{{minHearts}}"
    }
}
```

Each condition is true if _any_ of its values match, and the patch is applied if _all_ of its
conditions match.

Most tokens have an optional `{{tokenName:value}}` form which returns `true` or `false`. This can be
used to perform AND logic:

```js
// player has blacksmith OR gemologist
"When": {
   "HasProfession": "Blacksmith, Gemologist"
}

// player has blacksmith AND gemologist
"When": {
   "HasProfession:Blacksmith": "true",
   "HasProfession:Gemologist": "true"
}
```

This can also be used for negative conditions:

```js
// only year one
"When": {
   "Year": "1"
}

// NOT year 1
"When": {
   "Year:1": "false"
}
```
</dd>

<dt>Token placeholders</dt>
<dd>

You can use tokens in text by putting two curly brackets around the token name, which will be
replaced with the actual value automatically. Token placeholders are not case-sensitive. Patches
will be disabled automatically if a token they use isn't currently available.

For example, this gives the farmhouse a different appearance in each season:

```js
{
    "Action": "EditImage",
    "Target": "Buildings/houses",
    "FromFile": "assets/{{season}}_house.png" // assets/spring_house.png, assets/summer_house.png, etc
}
```

You can use placeholders in most fields (the documentation for each field will mention if it does).

Tokens which return a single value (like `{{season}}`) are most useful in placeholders. You can
use multi-value tokens as placeholders too, which will return a comma-delimited list. Most tokens
also have an optional `{{tokenName:value}}` form which returns `true` or `false` (like
`{{hasProfession:Gemologist}}`).

</dd>
</dl>

### Global tokens
Global token values are defined by Content Patcher, so you can use them without doing anything else.

<dl>
<dt>Date and weather:</dt>
<dd>

<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>Day</td>
<td>The day of month. Possible values: any integer from 1 through 28.</td>
</tr>

<tr valign="top">
<td>DayEvent</td>
<td>

The festival or wedding happening today. Possible values:
* `wedding` (current player is getting married);
* `dance of the moonlight jellies`;
* `egg festival`;
* `feast of the winter star`;
* `festival of ice`;
* `flower dance`;
* `luau`;
* `stardew valley fair`;
* `spirit's eve`;
* a custom festival name.

</td>
</tr>

<tr valign="top">
<td>DayOfWeek</td>
<td>

The day of week. Possible values: `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`,
`Saturday`, and `Sunday`.

</td>
</tr>

<tr valign="top">
<td>DaysPlayed</td>
<td>The total number of in-game days played for the current save (starting from one when the first day starts).</td>
</tr>

<tr valign="top">
<td>Season</td>
<td>

The season name. Possible values: `Spring`, `Summer`, `Fall`, and `Winter`.

</td>
</tr>

<tr valign="top">
<td>Weather</td>
<td>

The weather type. Possible values:

value   | meaning
------- | -------
`Sun`   | The weather is sunny (including festival/wedding days). This is the default weather if no other value applies.
`Rain`  | Rain is falling, but without lightning.
`Storm` | Rain is falling with lightning.
`Snow`  | Snow is falling.
`Wind`  | The wind is blowing with visible debris (e.g. flower petals in spring and leaves in fall).

</td>
</tr>

<tr valign="top">
<td>Year</td>
<td>

The year number (like `1` or `2`).

</td>
</tr>
</table>
</dd>

<dt>Player:</dt>
<dd>
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>HasDialogueAnswer</td>
<td>

The [response IDs](https://stardewvalleywiki.com/Modding:Dialogue#Response_IDs) for the player's
answers to question dialogues.

</td>
</tr>

<tr valign="top">
<td>HasFlag</td>
<td>

The flags set for the current player, including letters received and world state IDs. Some useful
flags:

flag | meaning
---- | -------
`artifactFound` | The player has found at least one artifact.
`Beat_PK` | The player has beaten the Prairie King arcade game.
`beenToWoods` | The player has entered the Secret Woods at least once.
`canReadJunimoText` | The player can read the language of Junimos (i.e. the plaques in the Community Center).
`ccIsComplete` | The player has completed the Community Center. Note that this isn't set reliably; see the `IsCommunityCenterComplete` and `IsJojaMartComplete` tokens instead.  See also flags for specific sections: `ccBoilerRoom`, `ccBulletin`, `ccCraftsRoom`, `ccFishTank`, `ccPantry`, and `ccVault`. The equivalent section flags for the Joja warehouse are `jojaBoilerRoom`, `jojaCraftsRoom`, `jojaFishTank`, `jojaPantry`, and `jojaVault`.
`doorUnlockAbigail` | The player has unlocked access to Abigail's room. See also flags for other NPCs: `doorUnlockAlex`, `doorUnlockCaroline`, `doorUnlockEmily`, `doorUnlockHaley`, `doorUnlockHarvey`, `doorUnlockJas`, `doorUnlockJodi`, `doorUnlockMarnie`, `doorUnlockMaru`, `doorUnlockPenny`, `doorUnlockPierre`, `doorUnlockRobin`, `doorUnlockSam`, `doorUnlockSebastian`, `doorUnlockVincent`.
`galaxySword` | The player has acquired the Galaxy Sword.
`geodeFound` | The player has found at least one geode.
`guildMember` | The player is a member of the Adventurer's Guild.
`jojaMember` | The player bought a Joja membership.
`JunimoKart` | The player has beaten the Junimo Kart arcade game.
`landslideDone` | The landside blocking access to the mines has been cleared.
`museumComplete` | The player has completed the Museum artifact collection.
`openedSewer` | The player has unlocked the sewers.
`qiChallengeComplete` | The player completed the Qi's Challenge quest by reaching level 25 in the Skull Cavern.

</td>
</tr>

<tr valign="top">
<td>HasProfession</td>
<td>

The [professions](https://stardewvalleywiki.com/Skills) learned by the player. Possible values:

* Combat skill: `Acrobat`, `Brute`, `Defender`, `Desperado`, `Fighter`, `Scout`.
* Farming skill: `Agriculturist`, `Artisan`, `Coopmaster`, `Rancher`, `Shepherd`, `Tiller`.
* Fishing skill: `Angler`, `Fisher`, `Mariner`, `Pirate`, `Luremaster`, `Trapper`.
* Foraging skill: `Botanist`, `Forester`, `Gatherer`, `Lumberjack`, `Tapper`, `Tracker`.
* Mining skill: `Blacksmith`, `Excavator`, `Gemologist`, `Geologist`, `Miner`, `Prospector`.

Custom professions added by a mod are represented by their integer profession ID.

</td>
</tr>

<tr valign="top">
<td>HasReadLetter</td>
<td>The letter IDs opened by the player (i.e. a letter UI was displayed).</td>
</tr>

<tr valign="top">
<td>HasSeenEvent</td>
<td>

The event IDs the player has seen, matching IDs in the `Data/Events` files. (You can use
[Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679) to see event IDs in-game.)

</td>
</tr>

<tr valign="top">
<td>HasWalletItem</td>
<td>

The [special items in the player wallet](https://stardewvalleywiki.com/Wallet). Possible values:

flag                       | meaning
-------------------------- | -------
`DwarvishTranslationGuide` | Unlocks speaking to the Dwarf.
`RustyKey`                 | Unlocks the sewers.
`ClubCard`                 | Unlocks the desert casino.
`SpecialCharm`             | Permanently increases daily luck.
`SkullKey`                 | Unlocks the Skull Cavern in the desert, and the Junimo Kart machine in the Stardrop Saloon.
`MagnifyingGlass`          | Unlocks the ability to find secret notes.
`DarkTalisman`             | Unlocks the Witch's Swamp.
`MagicInk`                 | Unlocks magical buildings through the Wizard, and the dark shrines in the Witch's Swamp.
`BearsKnowledge`           | Increases sell price of blackberries and salmonberries.
`SpringOnionMastery`       | Increases sell price of spring onions.

</td>
</tr>

<tr valign="top">
<td>IsMainPlayer</td>
<td>

Whether the player is the main player. Possible values: `true`, `false`.

</td>
</tr>

<tr valign="top">
<td>IsOutdoors</td>
<td>

Whether the player is outdoors. Possible values: `true`, `false`. This [does not affect dialogue](#known-limitations).

</td>
</tr>

<tr valign="top">
<td>LocationName</td>
<td>

The internal name of the player's current location (visible using [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679)).
This [does not affect dialogue](#known-limitations).

</td>
</tr>

<tr valign="top">
<td>PlayerGender</td>
<td>

The player's gender. Possible values: `Female`, `Male`.

</td>
</tr>

<tr valign="top">
<td>PlayerName</td>
<td>The player's name.</td>
</tr>

<tr valign="top">
<td>PreferredPet</td>
<td>

The player's preferred pet. Possible values: `Cat`, `Dog`.

</td>
</tr>

<tr valign="top">
<td>SkillLevel</td>
<td>

The player's skill levels. You can specify the skill level as an input argument like this:

```js
"When": {
   "SkillLevel:Combat": "1, 2, 3" // combat level 1, 2, or 3
}
```

The valid skills are `Combat`, `Farming`, `Fishing`, `Foraging`, `Luck` (unused in the base game),
and `Mining`.

</td>
</tr>
</table>
</dd>

<dt>Relationships:</dt>

<dd>
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>Hearts</td>
<td>

The player's heart level with a given NPC. You can specify the character name as an input argument
(using their English name regardless of translations), like this:

```js
"When": {
   "Hearts:Abigail": "10, 11, 12, 13"
}
```

**Note:** this is only available once the save is fully loaded, so it may not reliably affect
conditional map spawn logic.

</td>
</tr>

<tr valign="top">
<td>Relationship</td>
<td>

The player's relationship with a given NPC or player. You can specify the character name as part
of the key (using their English name regardless of translations), like this:

```js
"When": {
   "Relationship:Abigail": "Married"
}
```

The valid relationship types are...

value    | meaning
-------- | -------
Unmet    | The player hasn't talked to the NPC yet.
Friendly | The player talked to the NPC at least once, but hasn't reached one of the other stages yet.
Dating   | The player gave them a bouquet.
Engaged  | The player gave them a mermaid's pendant, but the marriage hasn't happened yet.
Married  | The player married them.
Divorced | The player married and then divorced them.

**Note:** this is only available once the save is fully loaded, so it may not reliably affect
conditional map spawn logic.

</td>
</tr>


<tr valign="top">
<td>Spouse</td>
<td>The player's spouse name (using their English name regardless of translations).</td>
</tr>

</table>
</dd>

<dt>World:</dt>

<dd>
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>FarmCave</td>
<td>

The [farm cave](https://stardewvalleywiki.com/The_Cave) type. Possible values: `None`, `Bats`,
`Mushrooms`.

</td>
</tr>

<tr valign="top">
<td>FarmhouseUpgrade</td>
<td>

The [farmhouse upgrade level](https://stardewvalleywiki.com/Farmhouse#Upgrades). The normal values
are 0 (initial farmhouse), 1 (adds kitchen), 2 (add children's bedroom), and 3 (adds cellar). Mods
may add upgrade levels beyond that.

</td>
</tr>

<tr valign="top">
<td>FarmName</td>
<td>The name of the current farm.</td>
</tr>

<tr valign="top">
<td>FarmType</td>
<td>

The [farm type](https://stardewvalleywiki.com/The_Farm#Farm_Maps). Possible values: `Standard`,
`FourCorners`, `Forest`, `Hilltop`, `Riverland`, `Wilderness`, `Custom`.

</td>
</tr>

<tr valign="top">
<td>IsCommunityCenterComplete</td>
<td>

Whether all bundles in the community center are completed. Possible values: `true`, `false`.

</td>
</tr>

<tr valign="top">
<td>IsJojaMartComplete</td>
<td>

Whether the player bought a Joja membership and completed all Joja bundles. Possible values: `true`
 `false`.

</td>
</tr>

<tr valign="top">
<td>HavingChild</td>
<td>

The names of players and NPCs whose relationship has an active pregnancy or adoption. Player names
are prefixed with `@` to avoid ambiguity with NPC names. For example, to check if the current
player is having a child:

```js
"When": {
    "HavingChild": "{{spouse}}"
}
```

Usage notes:
* `HavingChild:@{{playerName}}` and `HavingChild:{{spouse}}` are equivalent for this token.
* See also the `Pregnant` token.

</td>
</td>
</tr>

<tr valign="top">
<td>Pregnant</td>
<td>

The players or NPCs who are currently pregnant. This is a subset of `HavingChild` that only applies
to the female partner in heterosexual relationships. (Same-sex partners adopt a child instead.)

</td>
</tr>

</table>
</dd>

<dt>Metadata:</dt>

<dd>
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>HasMod</td>
<td>

The installed mod IDs (matching the `UniqueID` field in their `manifest.json`).

</td>
</tr>

<tr valign="top">
<td>HasFile</td>
<td>

Whether a file exists in the content pack folder. The file path must be specified as an input
argument. Returns `true` or `false`. For example:

```js
"When": {
  "HasFile:assets/{{season}}.png": "true"
}
```

</td>
</tr>

<tr valign="top">
<td>HasValue</td>
<td>

Whether the input argument is non-blank. For example, to check if the player is married to anyone:

```js
"When": {
  "HasValue:{{spouse}}": "true"
}
```

This isn't limited to a single token. You can pass in any tokenized string, and `HasValue` will
return true if the resulting string is non-blank:

```js
"When": {
  "HasValue:{{spouse}}{{LocationName}}": "true"
}
```

</td>
</tr>

<tr valign="top">
<td>Language</td>
<td>

The game's current language. Possible values:

code | meaning
---- | -------
`de` | German
`en` | English
`es` | Spanish
`fr` | French
`hu` | Hungarian
`it` | Italian
`ja` | Japanese
`ko` | Korean
`pt` | Portuguese
`ru` | Russian
`tr` | Turkish
`zh` | Chinese

</td>
</tr>
</table>
</dd>

<dt>String manipulation tokens:</dt>
<dd>
<table>
<tr>
<th>condition</th>
<th>purpose</th>

<tr valign="top">
<td>Lowercase<br />Uppercase</td>
<td>

Convert the input text to a different letter case:

<dl>
<dt>Lowercase</dt>
<dd>

Change to all small letters.<br />Example: `{{Lowercase:It's a warm {{Season}} day!}}` &rarr; `it's a warm summer day!`

</dd>
<dt>Uppercase</dt>
<dd>

Change to all capital letters.<br />Example: `{{Uppercase:It's a warm {{Season}} day!}}` &rarr; `IT'S A WARM SUMMER DAY!`

</dd>
</dl>
</td>
</tr>

<tr valign="top">
<td>Range</td>
<td>

A list of integers between the specified min/max integers (inclusive). This is mainly meant for
comparing values; for example:

```js
"When": {
   "Hearts:Abigail": "{{Range: 6, 14}}" // equivalent to "6, 7, 8, 9, 10, 11, 12, 13, 14"
}
```

You can use tokens for the individual numbers (like `{{Range:6, {{MaxHearts}}}}`) or both (like
`{{Range:{{FriendshipRange}}}})`, as long as the final parsed input has the form `min, max`.

To minimise the possible performance impact, the range can't exceed 5000 numbers and should be much
smaller if possible.

</td>
</tr>
</table>
</dd>

<dt>Patch-specific tokens:</dt>

<dd>
These tokens provide a value specific to the current patch. They can't be used in dynamic tokens or
any other field outside a patch block.

<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>Target</td>
<td>

The patch's `Target` field value for the current asset. Path separators are normalized for the OS.
This is mainly useful for patches which specify multiple targets:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail, Characters/Sam",
   "FromFile": "assets/{{Target}}.png" // assets/Characters/Abigail.png *or* assets/Characters/Sam.png
}
```

</td>
</tr>

<tr valign="top">
<td>TargetWithoutPath</td>
<td>

Equivalent to `Target`, but only the part after the last path separator:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail, Characters/Sam",
   "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png *or* assets/Sam.png
}
```

</td>
</tr>
</table>
</dd>
</dl>

**Special note about `"Action": "Load"`:**  
Each file can only be loaded by one patch. You can have multiple load patches with different
conditions, and the correct one will be used when the conditions change. However if multiple
patches can be applied in a given context, Content Patcher will show an error in the SMAPI console
and apply none of them.

### Randomization
You can randomize values using the `Random` token:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:hood, jacket, raincoat}}.png"
}
```

And you can optionally use pinned keys to keep multiple `Random` tokens in sync (see below for more
info):
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:hood, jacket, raincoat | outfit}}.png",
   "When": {
      "HasFile": "assets/abigail-{{Random:hood, jacket, raincoat | outfit}}.png"
   }
}
```

This token is dynamic and may behave in unexpected ways; see below to avoid surprises.

<dl>
<dt>Unique properties:</dt>
<dd>

`Random` tokens are...

<ol>
<li>

**Dynamic.** Random tokens rechoose each time they're evaluated, specifically...

* When a new day starts.
* When you change location, if used in a patch or [dynamic token](#dynamic-tokens) linked to a
  location token like `LocationName`. This is true even if the location token value doesn't change
  (e.g. using `IsOutdoors` while warping between two outdoor locations). You can prevent
  location-linked changes by using pinned keys (see below), or by storing the `Random` token in a
  [dynamic token](#dynamic-tokens) which doesn't depend on a location token.

The randomness is seeded with the save ID and in-game date, so reloading the save won't change
which choices were made.

</li>
<li>

**Independent**. Each `Random` token changes separately. In particular:

* If a patch has multiple `Target` values, `Random` may have a different value for each target.
* If a `FromFile` field has a `Random` token, you can't just copy its value into a `HasFile` field
  to check if the file exists, since `Random` may return a different choice in each field.

To keep multiple `Random` tokens in sync, see _pinned keys_ below.
</li>
<li>

**Fair**. Each option has an equal chance of being chosen. To load the dice, just specify a value multiple
times. For example, 'red' is twice as likely as 'blue' in this patch:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:red, red, blue}}.png"
}
```

</li>
<li>

**Bounded** if the choices don't contain tokens. For example, you can use it in true/false contexts
if all the choices are 'true' or 'false', or numeric contexts if all the choices are numbers.

</li>
</ul>
</dd>

<dt>Basic pinned keys:</dt>
<dd>

If you need multiple `Random` tokens to make the same choices (e.g. to keep an NPC's portrait and
sprite in sync), you can specify a 'pinned key'. This is like a name for the random; every `Random`
token with the same pinned key will make the same choice. (Note that list order does matter.)

For example, this keeps Abigail's sprite and portrait in sync using `abigail-outfit` as the pinned
key:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat | abigail-outfit}}.png"
}
```

You can use tokens in a pinned key. For example, this synchronizes values separately for each NPC:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail, Characters/Haley, Portraits/Haley",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat | {{TargetWithoutPath}}-outfit}}.png"
}
```

<dt>Advanced pinned keys:</dt>
<dd>

The pinned key affects the internal random number used to make a choice, not the choice itself. You
can use it with `Random` tokens containing different values (even different numbers of values) for
more interesting features.

For example, this gives Abigail and Haley random outfits but ensures they never wear the same one:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat | outfit}}.png"
},
{
   "Action": "Load",
   "Target": "Characters/Haley, Portraits/Haley",
   "FromFile": "assets/{{Target}}-{{Random:jacket, raincoat, hood | outfit}}.png"
}
```

</dd>

<dt>Okay, I'm confused. What the heck are pinned keys?</dt>
<dd>

Without pinned keys, each token will randomly choose its own value:
```txt
{{Random: hood, jacket, raincoat}} = raincoat
{{Random: hood, jacket, raincoat}} = hood
{{Random: hood, jacket, raincoat}} = jacket
```

If they have the same pinned key, they'll always be in sync:
```txt
{{Random: hood, jacket, raincoat | outfit}} = hood
{{Random: hood, jacket, raincoat | outfit}} = hood
{{Random: hood, jacket, raincoat | outfit}} = hood
```

For basic cases, you just need to know that same options + same key = same value.

If you want to get fancy, then the way it works under the hood comes into play. Setting a pinned
key doesn't sync the choice, it syncs the _internal number_ used to make that choice:
```txt
{{Random: hood, jacket, raincoat | outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: hood, jacket, raincoat | outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: hood, jacket, raincoat | outfit}} = 217437 modulo 3 choices = index 0 = hood
```

You can use that in interesting ways. For example, shifting the values guarantees they'll never
choose the same value (since same index = different value):
```txt
{{Random: hood, jacket, raincoat | outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: jacket, raincoat, hood | outfit}} = 217437 modulo 3 choices = index 0 = jacket
```
</dd>
</dl>

### Dynamic tokens
Dynamic tokens are defined in a `DynamicTokens` section of your `content.json` (see example below).
Each block in this section defines the value for a token using these fields:

field   | purpose
------- | -------
`Name`  | The name of the token to use for [tokens & condition](#advanced-tokens--conditions).
`Value` | The value(s) to set. This can be a comma-delimited value to give it multiple values. If _any_ block for a token name has multiple values, it will only be usable in conditions. This field supports [tokens](#advanced-tokens--conditions), including dynamic tokens defined before this entry.
`When`  | _(optional)_ Only set the value if the given [conditions](#advanced-tokens--conditions) match. If not specified, always matches.

Some usage notes:
* You can list any number of dynamic token blocks.
* If you list multiple blocks for the same token name, the last one whose conditions match will be
  used.
* You can use tokens in the `Value` and `When` fields. That includes dynamic tokens if they're
  defined earlier in the list (in which case the last applicable value _defined before this block_
  will be used). Using a token in the value implicitly adds a `When` condition (so the block is
  skipped if the token is unavailable, like `{{season}}` when a save isn't loaded).
* Dynamic tokens can't have the same name as an existing global token or player config field.

For example, this `content.json` defines a custom `{{style}}` token and uses it to load different
crop sprites depending on the weather:

```js
{
   "Format": "1.11.0",
   "DynamicTokens": [
      {
         "Name": "Style",
         "Value": "dry"
      },
      {
         "Name": "Style",
         "Value": "wet",
         "When": {
            "Weather": "rain, storm"
         }
      }
   ],
   "Changes": [
      {
         "Action": "Load",
         "Target": "TileSheets/crops",
         "FromFile": "assets/crop-{{style}}.png"
      }
   ]
}
```

### Player config
You can let players configure your mod using a `config.json` file. Content Patcher will
automatically create and load the file, and you can use the config values as
[tokens & conditions](#advanced-tokens--conditions). Config fields are not case-sensitive.

To do this, you add a `ConfigSchema` section which defines your config fields and how to validate
them (see below for an example).
Available fields for each field:

   field               | meaning
   ------------------- | -------
   `AllowValues`       | _(optional.)_ The values the player can provide, as a comma-delimited string. If omitted, any value is allowed.<br />**Tip:** for a boolean flag, use `"true, false"`.
   `AllowBlank`        | _(optional.)_ Whether the field can be left blank. If false or omitted, blank fields will be replaced with the default value.
   `AllowMultiple`     | _(optional.)_ Whether the player can specify multiple comma-delimited values. Default false.
   `Default`           | _(optional unless `AllowBlank` is false.)_ The default values when the field is missing. Can contain multiple comma-delimited values if `AllowMultiple` is true. If omitted, blank fields are left blank.

For example: this `content.json` defines a `Material` config field and uses it to change which
patch is applied. See below for more details.

```js
{
   "Format": "1.11.0",
   "ConfigSchema": {
      "Material": {
         "AllowValues": "Wood, Metal",
         "Default": "Wood"
      }
   },
   "Changes": [
      // as a token
      {
         "Action": "Load",
         "Target": "LooseSprites/Billboard",
         "FromFile": "assets/material_{{material}}.png"
      },

      // as a condition
      {
         "Action": "Load",
         "Target": "LooseSprites/Billboard",
         "FromFile": "assets/material_wood.png",
         "When": {
            "Material": "Wood"
         }
      }
   ]
}
```

When you run the game, a `config.json` file will appear automatically with text like this:

```js
{
  "Material": "Wood"
}
```

Players can edit it to configure your content pack.

### Mod-provided tokens
SMAPI mods can add new tokens for content packs to use (see [_extensibility for modders_](#extensibility-for-modders)),
which work just like normal Content Patcher tokens. For example, this patch uses a token from Json
Assets:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Data/NpcGiftTastes",
         "Entries": {
            "Universal_Love": "74 446 797 373 {{spacechase0.jsonAssets/ObjectId:item name}}",
         }
      }
   ]
}
```

To use a mod-provided token, at least one of these must be true:
* The mod which provides the token is a [required dependency](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Dependencies)
  of your content pack.
* Or the patch using the token has an immutable (i.e. not using any tokens) `HasMod` condition
  which lists the mod:
  ```js
  {
     "Format": "1.11.0",
     "Changes": [
        {
           "Action": "EditData",
           "Target": "Data/NpcGiftTastes",
           "Entries": {
              "Universal_Love": "74 446 797 373 {{spacechase0.jsonAssets/ObjectId:item name}}",
           },
           "When": {
              "HasMod": "spacechase0.jsonAssets"
           }
        }
     ]
  }
  ```

## Release a content pack
See [content packs](https://stardewvalleywiki.com/Modding:Content_packs) on the wiki for general
info. Suggestions:

1. Add specific install steps in your mod description to help players:
   ```
   [size=5]Install[/size]
   [list=1]
   [*][url=https://smapi.io]Install the latest version of SMAPI[/url].
   [*][url=https://www.nexusmods.com/stardewvalley/mods/1915]Install Content Patcher[/url].
   [*]Download this mod and unzip it into [font=Courier New]Stardew Valley/Mods[/font].
   [*]Run the game using SMAPI.
   [/list]
   ```
2. When editing the Nexus page, add Content Patcher under 'Requirements'. Besides reminding players
   to install it first, it'll also add your content pack to the list on the Content Patcher page.
3. Including `config.json` (if created) in your release download is not recommended. That will
   cause players to lose their settings every time they update. Instead leave it out and it'll
   generate when the game is launched, just like a SMAPI mod's `content.json`.

## Troubleshoot
### Schema validator
You can validate your `content.json` and `manifest.json` automatically to detect some common issues.
(You should still test your content pack in-game before releasing it, since the validator won't
detect all issues.)

To validate online:
1. Go to [json.smapi.io](https://json.smapi.io/).
2. Set the format to 'Manifest' (for `manifest.json`) or 'Content Patcher' (for `content.json`).
3. Drag & drop the JSON file onto the textbox, or paste in the text.
4. Click the button to view the validation summary. You can optionally share the URL to let someone
   else see the result.

To validate in a text editor that supports JSON Schema, see
[_Using a schema file directly_](https://github.com/Pathoschild/SMAPI/blob/develop/docs/technical/web.md#using-a-schema-file-directly)
in the JSON validator documentation.

Tips:
* You should update your content pack to the latest format version whenever you update it, for best
  futureproofing.
* If you get an error like `Unexpected character`, your JSON syntax is invalid. Try checking the
  line mentioned (or the one above it) for a missing comma, bracket, etc.
* If you need help figuring out an error, see [_see also_](#see-also) for some links to places you
  can ask.

### Patch commands
Content Patcher adds several console commands for testing and troubleshooting. Enter `patch help`
directly into the SMAPI console for more info.

#### patch summary
`patch summary` lists all the loaded patches, their current values, what they changed, and (if
applicable) the reasons they weren't applied.

For example:

```
=====================
==  Global tokens  ==
=====================
   Content Patcher:

      token name       | value
      ---------------- | -----
      Day              | [X] 5
      DayEvent         | [X]
      DayOfWeek        | [X] Friday
      DaysPlayed       | [X] 305
      FarmCave         | [X] Mushrooms
      FarmhouseUpgrade | [X] 1
      FarmName         | [X] River Coop
      FarmType         | [X] Riverland

      [snipped for simplicity]

=====================
== Content patches ==
=====================
The following patches were loaded. For each patch:
  - 'loaded' shows whether the patch is loaded and enabled (see details for the reason if not).
  - 'conditions' shows whether the patch matches with the current conditions (see details for the reason if not). If this is unexpectedly false, check (a) the conditions above and (b) your Where field.
  - 'applied' shows whether the target asset was loaded and patched. If you expected it to be loaded by this point but it's false, double-check (a) that the game has actually loaded the asset yet, and (b) your Targets field is correct.


Example Content Pack:
------------------------------

   Local tokens:
      token name        | value
      ----------------- | -----
      WeatherVariant    | [X] Wet

   Patches:
      loaded  | conditions | applied | name + details
      ------- | ---------- | ------- | --------------
      [X]     | [ ]        | [ ]     | Dry Palm Trees // conditions don't match: WeatherVariant
      [X]     | [X]        | [X]     | Wet Palm Trees

   Current changes:
      asset name                | changes
      ------------------------- | -------
      TerrainFeatures/tree_palm | edited image
```

#### patch update
`patch update` immediately updates Content Patcher's condition context and rechecks all patches.
This is mainly useful if you change conditions through the console (like the date), and want to
update patches without going to bed.

#### patch export
`patch export` saves a copy of a given asset to your game folder, which lets you see what it looks
like with all changes applied. This currently works for image and data assets.

For example:

```
> patch export "Maps/springobjects"
Exported asset 'Maps/springobjects' to 'C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\patch export\Maps_springobjects.png'.
```

#### patch parse
`patch parse` parses a tokenizable string and shows the resulting metadata, using the current
Content Patcher context (the same values used when applying patches).

This recognizes global tokens by default. You can use tokens for a specific content pack (including
dynamic tokens and config values) by giving the content pack ID from its `manifest.json` in the
optional second argument.

For example:

```
> patch parse "assets/{{Variant}}.{{Language}}.png" "Pathoschild.ExampleContentPack"

Metadata
----------------
   raw value:   assets/{{Variant}}.{{Language}}.png
   ready:       True
   mutable:     True
   has tokens:  True
   tokens used: Language, Variant

Diagnostic state
----------------
   valid:    True
   in scope: True
   ready:    True

Result
----------------
   The token string is valid and ready. Parsed value: "assets/wood.en.png"
```

This can also be used to troubleshoot token syntax:

```
> patch parse "assets/{{Season}.png"
[ERROR] Can't parse that token value: Reached end of input, expected end of token ('}}').

```

### Debug mode
Content Patcher has a 'debug mode' which lets you view loaded textures directly in-game with any
current changes. To enable it, open the mod's `config.json` file in a text editor and enable
`EnableDebugFeatures`.

Once enabled, press `F3` to display textures and left/right `CTRL` to cycle textures. Close and
reopen the debug UI to refresh the texture list.
> ![](docs/screenshots/debug-mode.png)

### Verbose log
Content Patcher doesn't log much info. You can change that by opening SMAPI's
`smapi-internal/StardewModdingAPI.config.json` in a text editor and enabling `VerboseLogging`.
**This may significantly slow down loading, and should normally be left disabled unless you need it.**

Once enabled, it will log significantly more information at three points:
1. when loading patches (e.g. whether each patch was enabled and which files were preloaded);
2. when SMAPI checks if Content Patcher can load/edit an asset;
3. and when the context changes (anytime the conditions change: different day, season, weather, etc).

If your changes aren't appearing in game, make sure you set a `LogName` (see [common fields](#common-fields))
and then search the SMAPI log file for that name. Particular questions to ask:
* Did Content Patcher load the patch?  
  _If it doesn't appear, check that your `content.json` is correct. If it says 'skipped', check
  your `Enabled` value or `config.json`._
* When the context is updated, is the box ticked next to the patch name?  
  _If not, checked your `When` field._
* When SMAPI checks if it can load/edit the asset name, is the box ticked?  
  _If not, check your `When` and `Target` fields._

## FAQs
### Compatibility
Content Patcher is compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, both single-player and
multiplayer.

### Multiplayer
Content Patcher works fine in multiplayer. It's best if all players have the same content packs,
but not required. Here are the effects if some players don't have a content pack installed:

patch type | effect
---------- | ------
visual     | Only visible to players that have it installed.
maps       | Only visible to players that have it installed. Players without the custom map will see the normal map and will be subject to the normal bounds (e.g. they may see other players walk through walls, but they won't be able to follow).
data       | Only directly affects players that have it installed, but can indirectly affect other players. For example, if a content pack changes `Data/ObjectInformation` and you create a new object, other player will see that object's custom values even if their `Data/ObjectInformation` doesn't have those changes.

### How multiple patches interact
Any number of patches can be applied to the same file. `Action: Load` always happens before other
action types, but otherwise each patch is applied sequentially. After each patch is done, the next
patch will see the combined asset as the input.

Within one content pack, patches are applied in the order they're listed in `content.json`. When
you have multiple content packs, each one is applied in the order they're loaded by SMAPI; if you
need to explicitly patch after another content pack, see [manifest dependencies](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Dependencies).

### Known limitations
* Dialogue is set when the day starts, so conditions that update during the day (like `IsOutdoors`)
  won't affect dialogue.
* Some game assets have special logic. This isn't specific to Content Patcher, but they're
  documented here for convenience.

  asset | notes
  ----- | -----
  `Characters/Farmer/accessories` | The number of accessories is hardcoded, so custom accessories need to replace an existing one.
  `Characters/Farmer/skinColors` | The number of skin colors is hardcoded, so custom colors need to replace an existing one.
  `Maps/*` | See [Modding:Maps#Potential issues](https://stardewvalleywiki.com/Modding:Maps#Potential_issues) on the wiki.

## Extensibility for modders
Content Patcher has a [mod-provided API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Mod-provided_APIs)
you can use to add custom tokens. Custom tokens are always prefixed with the ID of the mod that
created them, like `your-mod-id/SomeTokenName`.


<big><strong>The Content Patcher API is experimental and may change at any time.</strong></big>


### Access the API
To access the API:

1. Add Content Patcher as [a dependency in your mod's `manifest.json`](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Dependencies):

   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "IsRequired": false }
   ]
   ```

2. Copy [`IContentPatcherAPI`](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/IContentPatcherAPI.cs)
   into your mod code, and delete any methods you won't need for best future compatibility.
3. Hook into [SMAPI's `GameLoop.GameLaunched` event](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events#GameLoop.GameLaunched)
   and get a copy of the API:
   ```c#
   var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```
4. Use the API to extend Content Patcher (see below).

### Add a simple custom token
You can add a simple token by calling `RegisterToken` from SMAPI's `GameLaunched` event (see
_Access the API_ above). For example, this creates a `{{your-mod-id/PlayerName}}` token for the
current player's name:
```c#
api.RegisterToken(this.ModManifest, "PlayerName", () =>
{
    if (Context.IsWorldReady)
        return new[] { Game1.player.Name };
    if (SaveGame.loaded?.player != null)
        return new[] { SaveGame.loaded.player.Name }; // lets token be used before save is fully loaded
    return null;
});
```

`RegisterToken` in this case has three arguments:

argument   | type | purpose
---------- | ---- | -------
`mod`      | `IManifest` | The manifest of the mod defining the token. You can just pass in `this.ModManifest` from your entry class.
`name`     | `string` | The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, so `PlayerName` in the above example will become `your-mod-id/PlayerName`.
`getValue` | `Func<IEnumerable<string>>` | A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context and any patches or dynamic tokens using it are disabled.

That's it! Now any content pack which lists your mod as a dependency can use the token in its fields:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Characters/Dialogue/Abigail",
         "Entries": {
            "Mon": "Oh hey {{your-mod-id/PlayerName}}! Taking a break from work?"
         }
      }
   ]
}
```

### Add a complex custom token
The previous section is recommended for most tokens, since Content Patcher will handle details like
context updates and change tracking for you. With a bit more work though, you can add more complex
tokens with features like input arguments.

Let's say we want an 'initials' token with this behavior:

* If called with no input, it returns the player's initials (like `JS` if the player is John Smith).
* If called with an input argument, it returns the initials of that input (like `A` if the player
  is married to Abigail and you pass in `{{spouse}}`).

First, let's define our token logic. You don't actually need a class here, that's just a convenient
way to encapsulate the functions we'll be sending to Content Patcher below. For example:

```c#
/// <summary>An arbitrary class to handle token logic.</summary>
internal class InitialsToken
{
    /*********
    ** Fields
    *********/
    /// <summary>The player name as of the last context update.</summary>
    private string PlayerName;


    /*********
    ** Public methods
    *********/
    /// <summary>Get whether the token is ready to use.</summary>
    public bool IsReady()
    {
        return this.PlayerName != null;
    }

    /// <summary>Update the token value.</summary>
    /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
    public bool UpdateContext()
    {
        string oldName = this.PlayerName;
        this.PlayerName = this.GetPlayerName();
        return this.PlayerName != oldName;
    }

    /// <summary>Get the token value.</summary>
    /// <param name="input">The input argument passed to the token, if any.</param>
    public IEnumerable<string> GetValue(string input)
    {
        // get initials of input argument (if any), else initials of player name
        yield return this.GetInitials(input ?? this.PlayerName);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the current player name.</summary>
    private string GetPlayerName()
    {
        // Tokens may update while the save is still being loaded; we can make our token available
        // by checking SaveGame.loaded here.
        if (Context.IsWorldReady)
            return Game1.player.Name;

        if (SaveGame.loaded?.player != null)
            return SaveGame.loaded.player.Name;

        return null;
    }

    /// <summary>Recalculate the current token value.</summary>
    /// <param name="name">The name for which to get initials.</param>
    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        return string.Join("", name.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(p => p[0]));
    }
}
```

Next let's register it with Content Patcher in the `GameLaunched` event (see _Access the API_ above).
Note that we're not passing the actual `InitialsToken` instance to Content Patcher, that's
just a class we created to contain our token logic.

```c#
InitialsToken token = new InitialsToken();
api.RegisterToken(
    mod: this.ModManifest,
    name: "Initials",
    updateContext: token.UpdateContext,
    isReady: token.IsReady,
    getValue: token.GetValue,
    allowsInput: false,
    requiresInput: false
);
```

Content Patcher tokens are flexible, so there's a lot to unpack in that method call. Here's a
summary of each field:

field | type | purpose
----- | ---- | -------
`mod` | `IManifest` | The manifest of the mod defining the token. You can just pass in `this.ModManifest` from your entry class.
`name` | `string` | The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, so `Initials` in the above example will become `your-mod-id/Initials`.
`updateContext` | `Func<bool>` | A function which updates the token value (if needed), and returns whether the token changed. Content Patcher will call this method once when it's updating the context (e.g. when a new day starts). The token is 'changed' if it may return a different value _for the same inputs_ than before; it's important to report a change correctly, since Content Patcher will use this to decide whether patches need to be rechecked.
`isReady` | `Func<bool>` | A function which returns whether the token is available for use. This is always called after `updateContext`. If this returns false, any patches or dynamic tokens using this token will be disabled. (A token may return true and still have no value, in which case the token value is simply blank.)
`getValue` | `Func<string, IEnumerable<string>>` | A function which returns the current value for a given input argument (if any). For example, `{{your-mod-id/Initials}}` would result in a null input argument; `{{your-mod-id/Initials:{{spouse}}}}` would pass in the parsed string after token substitution, like `"Abigail"`. If the token doesn't use input arguments, you can simply ignore the input.
`allowsInput` | `bool` | Whether the player can provide an input argument (see `getValue`).
`requiresInput` | `bool` | Whether the token can _only_ be used with an input argument (see `getValue`).

That's it! Now any content pack which lists your mod as a dependency can use the token in its fields:
```js
{
   "Format": "1.11.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Characters/Dialogue/Abigail",
         "Entries": {
            "Mon": "Oh hey {{your-mod-id/Initials}}! Taking a break from work?"
         }
      }
   ]
}
```

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1915)
* [Discussion thread](https://community.playstarbound.com/threads/content-patcher.141420/)
* [Ask for help in #modding on Discord](https://stardewvalleywiki.com/Modding:Community#Discord)
