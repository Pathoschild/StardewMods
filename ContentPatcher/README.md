**Content Patcher** is a [Stardew Valley](http://stardewvalley.net/) mod which loads content packs
that change the game's images and data without replacing XNB files.

**This documentation is for modders. If you're a player, see the [Nexus page](https://www.nexusmods.com/stardewvalley/mods/1915) instead.**

## Contents
* [Install](#install)
* [Intro](#intro)
* [Create a content pack](#create-a-content-pack)
  * [Overview](#overview)
  * [Common fields](#common-fields)
  * [Patch types](#patch-types)
* [Advanced: tokens & conditions](#advanced-tokens--conditions)
  * [Overview](#overview-1)
  * [Global tokens](#global-tokens)
  * [Dynamic tokens](#dynamic-tokens)
  * [Player config](#player-config)
* [Release a content pack](#release-a-content-pack)
* [Troubleshoot](#troubleshoot)
  * [Patch commands](#patch-commands)
  * [Debug mode](#debug-mode)
  * [Verbose log](#verbose-log)
* [FAQs](#faqs)
  * [Compatibility](#compatibility)
  * [Multiplayer](#multiplayer)
  * [How multiple patches interact](#how-multiple-patches-interact)
  * [Special cases](#special-cases)
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
framework. More specialised frameworks might be better for specific things. You should consider
whether one of these would work for you:

  * [Advanced Location Loader](https://community.playstarbound.com/resources/smapi-advanced-location-loader.3619/) to add and edit maps.
  * [Custom Farming Redux](https://www.nexusmods.com/stardewvalley/mods/991) to add machines.
  * [Custom Furniture](https://www.nexusmods.com/stardewvalley/mods/1254) to add furniture.
  * [CustomNPC](https://www.nexusmods.com/stardewvalley/mods/1607) to add NPCs.
  * [Custom Shirts](https://www.nexusmods.com/stardewvalley/mods/2416) to add shirts.
  * [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720) to add items and fruit trees.

## Create a content pack
### Overview
A content pack is a folder with these files:
* a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:Content_packs) on the wiki);
* a `content.json` which describes the changes you want to make;
* and any images or files you want to use.

The `content.json` file has three main fields:

field          | purpose
-------------- | -------
`Format`       | The format version (just use `1.6`).
`Changes`      | The changes you want to make. Each entry is called a **patch**, and describes a specific action to perform: replace this file, copy this image into the file, etc. You can list any number of patches.
`ConfigSchema` | _(optional)_ Defines the `config.json` format, to support more complex mods. See [_player configuration_](#player-config).

Here's a quick example of each possible patch type (explanations below):

```js
{
  "Format": "1.6",
  "Changes": [
       // replace an entire file
       {
          "Action": "Load",
          "Target": "Animals/Dinosaur",
          "FromFile": "assets/dinosaur.png"
       },

       // edit part of an image
       {
          "Action": "EditImage",
          "Target": "Maps/springobjects",
          "FromFile": "assets/fish-object.png",
          "FromArea": { "X": 0, "Y": 0, "Width": 16, "Height": 16 }, // optional, defaults to entire FromFile
          "ToArea": { "X": 256, "Y": 96, "Width": 16, "Height": 16 } // optional, defaults to source size from top-left
       },

       // edit fields for existing entries in a data file (zero-indexed)
       {
          "Action": "EditData",
          "Target": "Data/ObjectInformation",
          "Fields": {
             "70": {
                0: "Jade",
                5: "A pale green ornamental stone."
             }
          }
       },

       // add or replace entries in a data file
       {
          "Action": "EditData",
          "Target": "Data/ObjectInformation",
          "Entries": {
             "70": "Jade/200/-300/Minerals -2/Jade/A pale green ornamental stone.",
             "72": "Diamond/750/-300/Minerals -2/Diamond/A rare and valuable gem."
          }
       }
    ]
}
```

### Common fields
All patches support these common fields:

field      | purpose
---------- | -------
`Action`   | The kind of change to make (`Load`, `EditImage`, or `EditData`); explained in the next section.
`Target`   | The game asset you want to patch (or multiple comma-delimited assets). This is the file path inside your game's `Content` folder, without the file extension or language. For example: use `Animals/Dinosaur` to edit `Content/Animals/Dinosaur.xnb`. Capitalisation doesn't matter. Your changes are applied in all languages unless you specify a language [condition](#advanced-tokens--conditions).
`LogName`  | _(optional)_ A name for this patch shown in log messages. This is very useful for understanding errors; if not specified, will default to a name like `entry #14 (EditImage Animals/Dinosaurs)`.
`Enabled`  | _(optional)_ Whether to apply this patch. Default true.
`When`     | _(optional)_ Only apply the patch if the given conditions match (see [_conditions_](#advanced-tokens--conditions)).

### Patch types
* **Replace an entire file** (`"Action": "Load"`).  
  When the game loads the file, it'll receive your file instead. This is useful for mods which
  change everything (like pet replacement mods).

  Avoid this if you don't need to change the whole file though — each file can only be replaced
  once, so your content pack won't be compatible with other content packs that replace the same
  file. (It'll work fine with content packs that only edit the file, though.)

  field      | purpose
  ---------- | -------
  &nbsp;     | See _common fields_ above.
  `FromFile` | The relative file path in your content pack folder to load instead (like `assets/dinosaur.png`). This can be a `.json` (data), `.png` (image), `.tbin` (map), or `.xnb` file. Capitalisation doesn't matter. 

* **Edit an image** (`"Action": "EditImage"`).  
  Instead of replacing an entire spritesheet, you can replace just the part you need. For example,
  you can change an item image by changing only its sprite in the spritesheet. Any number of
  content packs can edit the same file.

  You can extend an image downwards by just patching past the bottom. Content Patcher will
  automatically expand the image to fit.

  field      | purpose
  ---------- | -------
  &nbsp;     | See _common fields_ above.
  `FromFile` | The relative path to the image in your content pack folder to patch into the target (like `assets/dinosaur.png`). This can be a `.png` or `.xnb` file. Capitalisation doesn't matter.
  `FromArea` | _(optional)_ The part of the source image to copy. Defaults to the whole source image. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. [See example in overview](#overview).
  `ToArea`   | _(optional)_ The part of the target image to replace. Defaults to the `FromArea` size starting from the top-left corner. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. [See example in overview](#overview).
  `PatchMode`| _(optional)_ How to apply `FromArea` to `ToArea`. Defaults to `Replace`. Possible values: <ul><li><code>Replace</code>: replace the target area with your source image.</li><li><code>Overlay</code>: draw your source image over the target, so the original image shows through transparent pixels. Note that semi-transparent pixels will replace the underlying pixels, they won't be combined.</li></ul>

* **Edit a data file** (`"Action": "EditData"`).  
  Instead of replacing an entire data file, you can edit the individual entries or fields you need.

  field      | purpose
  ---------- | -------
  &nbsp;     | See _common fields_ above.
  `Fields`   | _(optional)_ The individual fields you want to change for existing entries. [See example in overview](#overview).
  `Entries`  | _(optional)_ The entries in the data file you want to add, replace, or delete. If you only want to change a few fields, use `Fields` instead for best compatibility with other mods. [See example in overview](#overview).<br />To add an entry, just specify a key that doesn't exist.<br />To delete an entry, set the value to `null` (like `"some key": null`).<br />**Caution:** some XNB files have extra fields at the end for translations; when adding or replacing an entry for all locales, make sure you include the extra field(s) to avoid errors for non-English players.

## Advanced: tokens & conditions
### Overview
A **token** is a name which represents a predefined value. For example, `season` (the token) may
contain `spring`, `summer`, `fall`, or `winter` (the value). You can use [player config](#player-config),
[global token values](#global-tokens), and [dynamic token values](#dynamic-tokens) as tokens.

There are two ways to use tokens.

<dl>
<dt>Conditions</dt>
<dd>

You can make a patch conditional by adding a `When` field, which can list any number of conditions.
Each condition has a token name (like `Season`) and the values to match (like `spring, summer`).
Condition names and values are not case-sensitive.

For example, this changes the house texture only in Spring or Summer, if the player hasn't upgraded
their house:

```js
{
    "Action": "EditImage",
    "Target": "Building/houses",
    "FromFile": "assets/green_house.png",
    "When": {
        "Season": "spring, summer",
        "FarmhouseUpgrade": "0"
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
    "Target": "Building/houses",
    "FromFile": "assets/{{season}}_house.png" // assets/spring_house.png, assets/summer_house.png, etc
}
```

You can do this in the `FromFile`, `Target`, `Enabled`, `Entries` (keys and values), and `Fields`
(entry keys and field values) fields.

Only tokens which return a single value can be used as tokens. For example, `{{season}}` is allowed
but `{{hasProfession}}` is not. Most tokens have an optional `{{tokenName:value}}` form which
returns `true` or `false` (like `{{hasProfession:Gemologist}}`); that form can be used in tokens if
needed (though it may be of limited use).

</dd>
</dl>

### Global tokens
Global token values are defined by Content Patcher, so you can use them without doing anything else.

These token values can be used as conditions and token placeholders for any patch:

<table>
<tr>
<th>condition</th>
<th>purpose</th>

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

The day of week. Possible values: `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`, `Saturday`, and `Sunday`.

</td>
</tr>

<tr valign="top">
<td>FarmCave</td>
<td>

The [farm cave](https://stardewvalleywiki.com/The_Cave) type. Possible values: `None`, `Bats`, `Mushrooms`.

</td>
</tr>

<tr valign="top">
<td>FarmhouseUpgrade</td>
<td>

The [farmhouse upgrade level](https://stardewvalleywiki.com/Farmhouse#Upgrades). The normal values are 0 (initial farmhouse), 1 (adds kitchen), 2 (add children's bedroom), and 3 (adds cellar). Mods may add upgrade levels beyond that.

</td>
</tr>

<tr valign="top">
<td>FarmName</td>
<td>The name of the current farm.</td>
</tr>

<tr valign="top">
<td>FarmType</td>
<td>

The [farm type](https://stardewvalleywiki.com/The_Farm#Farm_Maps). Possible values: `Standard`, `Riverland`, `Forest`, `Hilltop`, `Wilderness`, `Custom`.

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
`ja` | Japanese
`ru` | Russian
`pt` | Portuguese
`zh` | Chinese

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

These token values can be used as conditions, and (in their `tokenName:value` form only) as token
placeholders:

<table>
<tr>
<th>condition</th>
<th>purpose</th>

<tr valign="top">
<td>HasFile</td>
<td>

Whether a file exists in the content pack folder. The file path must be specified as part of the key,
and may contain tokens. Returns `true` or `false`. For example:

```js
"When": {
  "HasFile:assets/{{season}}.png": "true"
}
```

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
<td>HasFlag</td>
<td>

The letter IDs received by the player. The game also uses this to store some useful flags. For
example:

flag | meaning
---- | -------
`artifactFound` | The player has found at least one artifact.
`Beat_PK` | The player has beaten the Prairie King arcade game.
`beenToWoods` | The player has entered the Secret Woods at least once.
`canReadJunimoText` | The player can read the language of Junimos (i.e. the plaques in the Community Center).
`ccIsComplete` | The player has completed the Community Center. See also flags for specific sections: `ccBoilerRoom`, `ccCraftsRoom`, `ccFishTank`, `ccPantry`, and `ccVault`.
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
<td>HasMod</td>
<td>

The installed mod IDs (matching the `UniqueID` field in their `manifest.json`).

</td>
</tr>

<tr valign="top">
<td>HasSeenEvent</td>
<td>

The event IDs the player has seen, matching IDs in the `Data\Events` files. (You can use
[Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679) to see event IDs in-game.)

</td>
</tr>

<tr valign="top">
<td>Hearts</td>
<td>

The player's heart level with a given NPC. You can specify the character name as part of the key
(using their English name regardless of translations), like this:

```js
"When": {
   "Hearts:Abigail": "10, 11, 12, 13"
}
```

Or you can match against multiple NPCs like this:

```js
"When": {
   "Hearts": "Abigail:10, Leah:10" // 10 hearts with Abigail or Leah
}
```

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

Or you can match against multiple NPCs like this:

```js
"When": {
   "Relationship": "Abigail:Married, Leah:Married" // married Abigail or Leah
}
```

The valid relationship types are...

value    | meaning
-------- | -------
Friendly | The default if no other applies.
Dating   | The player gave them a bouquet.
Engaged  | The player gave them a mermaid's pendant, but the marriage hasn't happened yet.
Married  | The player married them.
Divorced | The player married and then divorced them.

</td>
</tr>

<tr valign="top">
<td>SkillLevel</td>
<td>

The player's skill levels. You can specify the skill level as part of the key like this:

```js
"When": {
   "SkillLevel:Combat": "1, 2, 3" // combat level 1, 2, or 3
}
```

Or you can match against multiple skills like this:

```js
"When": {
   "SkillLevel": "Combat:1, Farming:2" // combat level 1 or farming level 2
}
```

The valid skills are `Combat`, `Farming`, `Fishing`, `Foraging`, `Luck` (unused in the base game), and `Mining`.

</td>
</tr>

<tr valign="top">
<td>Spouse</td>
<td>The player's spouse name (using their English name regardless of translations).</td>
</tr>
</table>

**Special note about `"Action": "Load"`:**  
Each file can only be loaded by one patch. You can have multiple load patches with different
conditions, and the correct one will be used when the conditions change. However if multiple
patches can be applied in a given context, Content Patcher will show an error in the SMAPI console
and apply none of them.

### Dynamic tokens
Dynamic tokens are defined in a `DynamicTokens` section of your `content.json` (see example below).
Each block in this section defines the value for a token using these fields:

field   | purpose
------- | -------
`Name`  | The name of the token to use for [tokens & condition](#advanced-tokens--conditions).
`Value` | The value(s) to set. This can be a comma-delimited value to give it multiple values. If _any_ block for a token name has multiple values, it will only be usable in conditions.
`When`  | _(optional)_ Only set the value if the given [conditions](#advanced-tokens--conditions) match. If not specified, always matches.

Some usage notes:
* You can list any number of dynamic token blocks.
* If you list multiple blocks for the same token name, the last one whose conditions match will be
  used.
* You can use tokens in the `Value` and `When` fields. That includes dynamic tokens if they're
  defined earlier in the list (in which case the last value _defined before the current block_ will
  be used).
* Dynamic tokens can't have the same name as an existing global token or player config field.

For example, this `content.json` defines a custom `{{style}}` token and uses it to load different
crop sprites depending on the weather:

```js
{
    "Format": "1.6",
    "DynamicTokens": [
        {
            "Name": "Style",
            "Value": "default"
        },
        {
            "Name": "Style",
            "Value": "drenched",
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

To do this, you add a `ConfigSchema` section which defines your config fields and how to validate them
(see below for an example).
Available fields for each field:

   field               | meaning
   ------------------- | -------
   `AllowValues`       | Required. The values the player can provide, as a comma-delimited string.<br />**Tip:** for a boolean flag, use `"true, false"`.
   `AllowBlank`        | _(optional)_ Whether the field can be left blank. Behaviour: <ul><li>If false (default): missing and blank fields are filled in with the default value.</li><li>If true: missing fields are filled in with the default value; blank fields are left as-is.</li></ul>
   `AllowMultiple`     | _(optional)_ Whether the player can specify multiple comma-delimited values. Default false.
   `Default`           | _(optional)_ The default values when the field is missing. Can contain multiple comma-delimited values if `AllowMultiple` is true. If not set, defaults to the first value in `AllowValues`.

For example: this `content.json` defines a `Material` config field and uses it to change which
patch is applied. See below for more details.

```js
{
    "Format": "1.6",
    "ConfigSchema": {
        "Material": {
            "AllowValues": "Wood, Metal"
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
### Patch commands
Content Patcher adds two patch commands for testing and troubleshooting.

* `patch summary` lists all the loaded patches, their current values, and (if applicable) the
  reasons they weren't applied.

  Example output:

  ```
  Current conditions:
     Day: 5
     DayOfWeek: friday
     Language: en
     Season: spring
     Weather: sun

  Patches by content pack ([X] means applied):
     Sample Content Pack:
        [X] Palm Trees | Load TerrainFeatures/tree_palm
        [X] Bushes | Load TileSheets/bushes
        [ ] Maple Trees | Load TerrainFeatures/tree2_{{season}} | failed conditions: Season (summer, fall, winter)
        [ ] Oak Trees | Load TerrainFeatures/tree1_{{season}} | failed conditions: Season (summer, fall, winter)
        [X] World Map | EditImage LooseSprites/map
  ```
* `patch update` immediately updates Content Patcher's condition context and rechecks all patches.
  This is mainly useful if you change conditions through the console (like the date), and want to
  update patches without going to bed.

### Debug mode
Content Patcher has a 'debug mode' which lets you view loaded textures directly in-game with any
current changes. To enable it, open the mod's `config.json` file in a text editor and enable
`EnableDebugFeatures`.

Once enabled, press `F3` to display textures and left/right `CTRL` to cycle textures. Close and
reopen the debug UI to refresh the texture list.
> ![](docs/screenshots/debug-mode.png)

### Verbose log
Content Patcher doesn't log much info. You can change that by opening SMAPI's `smapi-internal/StardewModdingAPI.config.json`
in a text editor and enabling `VerboseLogging`. **This may significantly slow down loading, and
should normally be left disabled unless you need it.**

Once enabled, it will log significantly more information at three points:
1. when loading patches (e.g. whether each patch was enabled and which files were preloaded);
2. when SMAPI checks if Content Patcher can load/edit an asset;
3. and when the context changes (anytime the conditions change: different day, season, weather, etc).

If your changes aren't appearing in game, make sure you set a `LogName` (see [common fields](#common-fields))
and then search the SMAPI log file for that name. Particular questions to ask:
* Did Content Patcher load the patch?  
  _If it doesn't appear, check that your `content.json` is correct. If it says 'skipped', check your
  `Enabled` value or `config.json`._
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

### Special cases
Some game assets have special logic. This isn't specific to Content Patcher, but they're documented
here for convenience.

asset | notes
----- | -----
`Characters/Farmer/accessories` | The number of accessories is hardcoded, so custom accessories need to replace an existing one.
`Characters/Farmer/skinColors` | The number of skin colors is hardcoded, so custom colors need to replace an existing one.

## See also
* [Release notes](release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1915)
* [Discussion thread](https://community.playstarbound.com/threads/content-patcher.141420/)
