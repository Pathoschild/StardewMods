**Content Patcher** is a [Stardew Valley](http://stardewvalley.net/) mod which loads content packs
that change the game's images and data without replacing XNB files.

## Contents
* [Installation](#installation)
* [For modders](#for-modders)
* [Versions](#versions)
* [See also](#see-also)

## Installation
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install ~~this mod from Nexus mods~~ (not released yet).
3. Unzip any Content Patcher content packs into `Mods` to install them.
4. Run the game using SMAPI.

## For modders
### Why use this?
There are two main alternatives for changing the game assets: editing XNB files (which have many
limitations and problems) or creating a SMAPI mod (which requires programming). For any change
possible by only editing XNB files, content packs for Content Patcher combine the best parts of
both:

&nbsp;               | XNB mod                         | SMAPI mod               | Content Patcher
-------------------- | ------------------------------- | ----------------------- | ---------------
easy to create       | ✘ need to unpack/repack files  | ✘ programming needed   | ✓ edit JSON files
easy to install      | ✘ different for every mod      | ✓ drop into `Mods`     | ✓ drop into `Mods`
update checks        | ✘ no                           | ✓ via SMAPI            | ✓ via SMAPI
compatibility checks | ✘ no                           | ✓ automated + SMAPI DB | ✓ SMAPI DB
safe to update game  | ✘ high impact<br /><small>(any update to edited files breaks mod)</small> | ✓ SMAPI smooths impact        | ✓ reduced impact<br /><small>(only affected by changes to edited portions of file)</small>
easy to troubleshoot | ✘ no record of changes         | ✓ SMAPI log, compile checks | ✓ SMAPI log, files validates on load

### Why not use this?
If you were going to create an XNB mod, Content Patcher is a much better choice! Otherwise it isn't
the best option if...

* **If you're comfortable programming.** You can create a SMAPI mod and use SMAPI's content API
  directly.
* **If there's a higher-level mod available.** Content Patcher supports changes to all XNB files,
  but it's a low-level API. You'll need to learn how the game files you want to edit are structured,
  and you can't make changes that aren't possible through XNB edits.

  These mods provide higher-level APIs for certain content, which can be easier to learn and may
  support features not possible through pure XNB edits:
  * [Custom Farming Redux](https://www.nexusmods.com/stardewvalley/mods/991) for custom machines.
  * [Custom Furniture](https://www.nexusmods.com/stardewvalley/mods/1254) for custom furniture.
  * [CustomNPC](https://www.nexusmods.com/stardewvalley/mods/1607) for custom NPCs.
  * [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720) for custom items.

### Creating a content pack
A content pack consists of a folder with these files:
* a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:Content_packs) on the wiki);
* a `content.json` which describes the changes you want to make;
* and any images or maps you want to use.

The `content.json` file contains a list of changes you want to make. Here's a quick example of each
possible change (explanations below):

```js
[
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
  `Target`   | The game asset you want to change. This is the filename without the `Content` path, file extension, or language (like `Animals/Dinosaur` to edit `Content/Animals/Dinosaur.xnb`).
  `FromFile` | The relative path to the file in your content pack folder to replace it with (like `assets/dinosaur.png`).

* **Edit an image.**  
  Instead of replacing an entire spritesheet, you can replace just the part you need. For example,
  you can edit an object image by changing only its sprite in the spritesheet. Any number of
  content packs can edit the same file.

  field      | purpose
  ---------- | -------
  `Action`   | The kind of change to make. Must be `EditImage`.
  `Target`   | The game's image asset you want to change. This is the filename without the `Content` path, file extension, or language (like `Animals/Dinosaur` to edit `Content/Animals/Dinosaur.xnb`).
  `FromFile` | The relative path to the image in your content pack folder to patch into the target (like `assets/dinosaur.png`).
  `FromArea` | _(optional)_ The part of the source image to copy. This is specified as an object with the X and Y coordinates of the top-left corner, and the width and height of the area. For example, `{ "X": 256, "Y": 96, "Width": 16, "Height": 16 }`.
  `ToArea`   | _(optional)_ The part of the target image to replace. This is specified as an object with the X and Y coordinates of the top-left corner, and the width and height of the area. For example, `{ "X": 256, "Y": 96, "Width": 16, "Height": 16 }`.

* **Edit a data file.**  
  Instead of replacing an entire data file, you can edit the individual entries or even fields you
  need.

  field      | purpose
  ---------- | -------
  `Action`   | The kind of change to make. Must be `EditData`.
  `Target`   | The game's data asset you want to change. This is the filename without the `Content` path, file extension, or language (like `Data/ObjectInformation` to edit `Content/Data/ObjectInformation.xnb`).
  `Entries`  | _(optional)_ The entries in the data file you want to change. If you only want to change a few fields, use `Fields` instead for best compatibility with other mods. See example above.
  `Fields`   | _(optional)_ The individual fields you want to change for existing entries. See example above.

## Versions
See [release notes](release-notes.md).

## See also
* ~~Nexus mod~~ (not released yet)
* ~~Discussion thread~~
