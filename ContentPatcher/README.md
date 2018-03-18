**Content Patcher** is a [Stardew Valley](http://stardewvalley.net/) mod which loads content packs
that change the game's images and data without replacing XNB files.

## Contents
* [Installation](#installation)
* [For modders](#for-modders)
* [Versions](#versions)
* [See also](#see-also)

## Installation
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [this mod from Nexus mods](https://www.nexusmods.com/stardewvalley/mods/1915).
3. Unzip any Content Patcher content packs into `Mods` to install them.
4. Run the game using SMAPI.

## For modders
### Why use this?
Content Patcher is one of three main approaches to editing the game assets:
1. create a SMAPI mod (requires programming);
2. _or_ create an XNB mod (many limitations and issues);
3. _or_ create a content pack for Content Patcher.

Content Patcher combines the advantages of SMAPI and XNB mods:

&nbsp;               | XNB mod                         | SMAPI mod               | Content Patcher
-------------------- | ------------------------------- | ----------------------- | ---------------
easy to create       | ✘ need to unpack/repack files  | ✘ programming needed   | ✓ edit JSON files
easy to install      | ✘ different for every mod      | ✓ drop into `Mods`     | ✓ drop into `Mods`
update checks        | ✘ no                           | ✓ via SMAPI            | ✓ via SMAPI
compatibility checks | ✘ no                           | ✓ automated + SMAPI DB | ✓ SMAPI DB
mod compatibility    | ✘ poor<br /><small>(each file can only be changed by one mod)</small> | ✓ almost universal | ✓ pretty good<br /><small>(mods only conflict if they edit the same part of a file)</small>
safe to update game  | ✘ high impact<br /><small>(any update to edited files breaks mod)</small> | ✓ SMAPI smooths impact        | ✓ reduced impact<br /><small>(only affected by changes to edited portions of file)</small>
easy to troubleshoot | ✘ no record of changes         | ✓ SMAPI log, compile checks | ✓ SMAPI log, files validated on load

In some cases there may be a better option:

* **If you're comfortable programming.**  
  You can create a SMAPI mod and use SMAPI's content API directly instead.
* **If there's a higher-level mod available.**  
  Content Patcher is much better than editing XNB files directly, but you still need to learn how
  they're structured and you can't make changes that aren't possible through XNB edits. Some mods
  provide higher-level APIs for specific cases, which can be easier to learn and may support
  features that aren't possible through pure XNB changes. For example:
  * [Advanced Location Loader](https://community.playstarbound.com/resources/smapi-advanced-location-loader.3619/) for map edits and custom maps.
  * [Custom Farming Redux](https://www.nexusmods.com/stardewvalley/mods/991) for custom machines.
  * [Custom Furniture](https://www.nexusmods.com/stardewvalley/mods/1254) for custom furniture.
  * [CustomNPC](https://www.nexusmods.com/stardewvalley/mods/1607) for custom NPCs.
  * [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720) for custom items.

### Creating a content pack
#### Overview
A content pack consists of a folder with these files:
* a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:Content_packs) on the wiki);
* a `content.json` which describes the changes you want to make;
* and any images or other files you want to use.

The `content.json` file contains a format version (just use `1.0`) and a list of changes you
want to make. Each change (technically called a _patch_) describes a specific action: replace one
file, copy this image into the file, etc. You can list any number of changes.

Known limitations:
* Content Patcher can't change festival textures yet (fixed in Stardew Valley 1.3).

#### Example
Here's a quick example of each possible change type (explanations below):

```js
{
  "Format": "1.0",
  "Changes": [
       // replace entire file
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

       // replace entries in a data file
       {
          "Action": "EditData",
          "Target": "Data/ObjectInformation",
          "Entries": {
             70: "Jade/200/-300/Minerals -2/Jade/A pale green ornamental stone.",
             72: "Diamond/750/-300/Minerals -2/Diamond/A rare and valuable gem."
          }
       },

       // edit fields for existing entries in a data file (zero-indexed)
       {
          "Action": "EditData",
          "Target": "Data/ObjectInformation",
          "Fields": {
             70: {
                0: "Jade",
                5: "A pale green ornamental stone."
             }
          }
       }
    ]
}
```

### Common fields
All changes support these common fields:

field      | purpose
---------- | -------
`Action`   | The kind of change to make (`Load`, `EditImage`, or `EditData`). See below.
`Target`   | The game asset you want to change. This is the path relative to your game's `Content` folder, without the `Content` part, file extension, or language (like `Animals/Dinosaur` to edit `Content/Animals/Dinosaur.xnb`). Capitalisation doesn't matter.
`Enabled`  | _(optional)_ Whether to apply this patch. Default true.
`When`     | _(optional)_ Only apply the patch if the given conditions match (see [_conditions_ below](#conditions)).

### Supported changes
* **Replace an entire file** (`"Action": "Load"`).  
  When the game loads the file, it'll receive your file instead. This is useful for mods which
  change everything (like pet replacement mods).

  Avoid this if you don't need to change the whole file though — each file can only be replaced
  once, so your content pack won't be compatible with other content packs that replace the same
  file. (It'll work fine with content packs that only edit the file, though.)

  field      | purpose
  ---------- | -------
  &nbsp;     | See _common fields_ above.
  `FromFile` | The relative file path in your content pack folder to load instead (like `assets/dinosaur.png`). This can be a `.png`, `.tbin`, or `.xnb` file. Capitalisation doesn't matter.

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
  `FromArea` | _(optional)_ The part of the source image to copy. Defaults to the whole source image. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. [See example](#example).
  `ToArea`   | _(optional)_ The part of the target image to replace. Defaults to the `FromArea` size starting from the top-left corner. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. [See example](#example).
  `PatchMode`| _(optional)_ How to apply `FromArea` to `ToArea`. Defaults to `Replace`. Possible values: <ul><li><code>Replace</code>: replace the target area with your source image.</li><li><code>Overlay</code>: draw your source image over the target, so the original image shows through transparent pixels. Note that semi-transparent pixels will replace the underlying pixels, they won't be combined.</li></ul>

* **Edit a data file** (`"Action": "EditData"`).  
  Instead of replacing an entire data file, you can edit the individual entries or even fields you
  need.

  field      | purpose
  ---------- | -------
  &nbsp;     | See _common fields_ above.
  `Entries`  | _(optional)_ The entries in the data file you want to change. If you only want to change a few fields, use `Fields` instead for best compatibility with other mods. [See example](#example).
  `Fields`   | _(optional)_ The individual fields you want to change for existing entries. [See example](#example).

### Conditions
**(Content Patcher 1.3+)**
You can make a patch conditional by adding a `When` field. The patch will be applied when all
conditions match, and removed when they no longer match. Conditions are not case-sensitive, and you
can specify multiple values as a comma-delimited list. You don't need to specify all conditions.

For example:

```js
{
    "Action": "EditImage",
    "Target": "Building/houses",
    "FromFile": "assets/green_house.png",
    "When": {
        "Season": "spring, summer"
    }
}
```

The possible conditions are:

condition   | description
----------- | -----------
`Day`       | The day of month. Possible values: any integer from 1 through 28.
`DayOfWeek` | The day of week. Possible values: `monday`, `tuesday`, `wednesday`, `thursday`, `friday`, `saturday`, and `sunday`.
`Language`  | The game's current language. Possible values: <table><tr><th>code</th><th>meaning</th></tr><tr><td>`de`</td><td>German</td></tr><tr><td>`en`</td><td>English</td></tr><tr><td>`es`</td><td>Spanish</td></tr><tr><td>`ja`</td><td>Japanese</td></tr><tr><td>`ru`</td><td>Russian</td></tr><tr><td>`pt`</td><td>Portuguese</td></tr><tr><td>`zh`</td><td>Chinese</td></tr></table></ul>
`Season`    | The season name. Possible values: `spring`, `summer`, `fall`, and `winter`.
`Weather`   | The weather name. Possible values: `sun`, `rain`, `snow`, and `storm`.

Special note about `"Action": "Load"`:
* Each file can only be loaded by one content pack. Content Patcher will allow multiple loaders, so
  long as their conditions can never overlap. If they can overlap, it will refuse to add the second
  one. For example:

  loader A           | loader B               | result
  ------------------ | ---------------------- | ------
  `season: "Spring"` | `season: "Summer"`     | both are loaded correctly (seasons never overlap).
  `day: 1`           | `dayOfWeek: "Tuesday"` | both are loaded correctly (1st day of month is never Tuesday).
  `day: 1`           | `weather: "Sun"`       | error: sun could happen on the first of a month.

### Releasing a content pack
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

### Debug mode
Content Patcher has a debug mode for modders. This lets you view loaded textures directly with any
changes applied, to help troubleshoot content packs. To enable it:

1. Run the game once with Content Patcher installed.
2. Edit the mod's `config.json` file in a text editor.
3. Set `EnableDebugFeatures` to `true`.

Once enabled, press `F3` to display textures and left/right `CTRL` to cycle textures. Close and
reopen the debug UI to refresh the texture list.
> ![](docs/screenshots/debug-mode.png)

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1915)
* [Discussion thread](https://community.playstarbound.com/threads/content-patcher.141420/)
