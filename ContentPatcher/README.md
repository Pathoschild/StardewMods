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
A content pack consists of a folder with these files:
* a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:Content_packs) on the wiki);
* a `content.json` which describes the changes you want to make;
* and any images or other files you want to use.

The `content.json` file contains a format version (just use `1.0`) and a list of changes you
want to make. Here's a quick example of each possible change (explanations below):

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

       // edit one part of a image
       {
          "Action": "EditImage",
          "Target": "Maps/springobjects",
          "FromFile": "assets/fish-object.png",
          "ToArea": { "X": 256, "Y": 96, "Width": 16, "Height": 16 }
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

The file consists of a JSON array (the `[` and `]` at the top and bottom) containing one JSON
object (the `{` and `}` blocks) for each change you want to make. You can list any number of
changes.

Here are the supported changes:

* **Replace an entire file.**  
  When the game loads the file, it'll receive your file instead. That's
  fine if you're changing the entire file, but avoid this if possible. Each file can only be
  replaced once — your content pack won't be compatible with any other content pack that replaces
  the same file. (It'll work fine with any content pack that only edits the file, though.)

  field      | purpose
  ---------- | -------
  `Action`   | The kind of change to make. Must be `Load`.
  `Target`   | The game asset you want to change. This is the file path without the `Content` part, file extension, or language (like `Animals/Dinosaur` to edit `Content/Animals/Dinosaur.xnb`).
  `Locale`   | _(optional)_ The language code of the game asset you want to replace. Omit to replace for any language.
  `FromFile` | The relative file path in your content pack folder to load instead (like `assets/dinosaur.png`).
  `Enabled`  | _(optional)_ Whether to apply this patch (default `true`).

* **Edit an image.**  
  Instead of replacing an entire spritesheet, you can replace just the part you need. For example,
  you can change an item image by changing only its sprite in the spritesheet. Any number of
  content packs can edit the same file. You can extend an image downwards by simply patching past
  the bottom.

  field      | purpose
  ---------- | -------
  `Action`   | The kind of change to make. Must be `EditImage`.
  `Target`   | The game asset you want to change. This is the file path without the `Content` part, file extension, or language (like `Animals/Dinosaur` to edit `Content/Animals/Dinosaur.xnb`).
  `Locale`   | _(optional)_ The language code of the game asset you want to edit. Omit to edit for any language.
  `FromFile` | The relative path to the image in your content pack folder to patch into the target (like `assets/dinosaur.png`).
  `FromArea` | _(optional)_ The part of the source image to copy. This is specified as an object with the X and Y coordinates of the top-left corner, and the width and height of the area. For example, `{ "X": 256, "Y": 96, "Width": 16, "Height": 16 }`.
  `ToArea`   | _(optional)_ The part of the target image to replace. This is specified as an object with the X and Y coordinates of the top-left corner, and the width and height of the area. For example, `{ "X": 256, "Y": 96, "Width": 16, "Height": 16 }`.
  `Enabled`  | _(optional)_ Whether to apply this patch (default `true`).

* **Edit a data file.**  
  Instead of replacing an entire data file, you can edit the individual entries or even fields you
  need.

  field      | purpose
  ---------- | -------
  `Action`   | The kind of change to make. Must be `EditData`.
  `Target`   | The game asset you want to change. This is the file path without the `Content` part, file extension, or language (like `Data/ObjectInformation` to edit `Content/Data/ObjectInformation.xnb`).
  `Locale`   | _(optional)_ The language code of the game asset you want to edit. Omit to edit for any language.
  `Entries`  | _(optional)_ The entries in the data file you want to change. If you only want to change a few fields, use `Fields` instead for best compatibility with other mods. See example above.
  `Fields`   | _(optional)_ The individual fields you want to change for existing entries. See example above.
  `Enabled`  | _(optional)_ Whether to apply this patch (default `true`).

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

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1915)
* [Discussion thread](https://community.playstarbound.com/threads/content-patcher.141420/)
