← [author guide](../author-guide.md)

A patch with **`"Action": "EditImage"`** changes part of an image loaded by the game. Any number of
content packs can edit the same asset. You can extend an image downwards by just patching past the
bottom (Content Patcher will expand the image to fit).

**🌐 In other languages: [zh (中文)](../zh/author-guide/action-editimage.md)**.

## Contents
* [Usage](#usage)
  * [Format](#format)
  * [Examples](#examples)
* [See also](#see-also)

## Usage
### Format
An `EditImage` patch consists of a model under `Changes` (see examples below) with these fields:

<dl>
<dt>Required fields:</dt>
<dd>

field     | purpose
--------- | -------
`Action`  | The kind of change to make. Set to `EditImage` for this action type.
`Target`  | The [game asset name](../author-guide.md#what-is-an-asset) to replace (or multiple comma-delimited asset names), like `Portraits/Abigail`. This field supports [tokens](../author-guide.md#tokens), and capitalisation doesn't matter.
`FromFile` | The relative path to the image in your content pack folder to patch into the target (like `assets/dinosaur.png`), or multiple comma-delimited paths. This can be a `.png` or `.xnb` file. This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.

</dd>
<dt>Optional fields:</dt>
<dd>

field       | purpose
----------- | -------
`FromArea`  | <p>The part of the source image to copy. Defaults to the whole source image.</p><p>This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. Its fields may contain tokens.</p>
`ToArea`    | <p>The part of the target image to replace. Defaults to the same size as `FromArea`, positioned at the top-left corner of the spritesheet.</p><p>This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. Its fields may contain tokens.</p><p>If you specify an area past the bottom edge of the image, the image will be resized automatically to fit.</p>
`PatchMode` | <p>How to apply `FromArea` to `ToArea`. Defaults to `Replace`.</p> Possible values: <ul><li><code>Replace</code>: replace every pixel in the target area with your source image. If the source image has transparent pixels, the target image will become transparent there.</li><li><code>Overlay</code>: draw your source image over the target area. If the source image has transparent or semi-transparent pixels, the target image will 'show through' those pixels. Opaque pixels will replace the target pixels.</li></ul>For example, let's say your source image is a pufferchick with a transparent background, and the target image is a solid green square. Here's how they'll be combined with different `PatchMode` values:<br />![](../screenshots/patch-mode-examples.png)
`When`      | _(optional)_ Only apply the patch if the given [conditions](../author-guide.md#conditions) match.
`LogName`   | _(optional)_ A name for this patch to show in log messages. This can be useful for understanding errors. If omitted, it defaults to a name like `EditImage Animals/Dinosaur`.
`Update`    | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.
`LocalTokens` | _(Optional)_ A set of [local tokens](../author-guide/tokens.md#local-tokens) which can be used within this patch's field.

</dd>
<dt>Advanced fields:</dt>
<dd>

<table>
  <tr>
    <td>field</td>
    <td>purpose</td>
  </tr>
  <tr>
  <td><code>Priority</code></td>
  <td>

_(optional)_ When multiple patches or mods edit the same asset, the order in which they should be
applied. The possible values are `Early`, `Default`, and `Late`. The default value is `Default`.

The patches for an asset (across all mods) are applied in this order:

1. by earliest to latest priority;
2. then by mod load order (e.g. based on dependencies);
3. then by the order the patches are listed in your `content.json`.

If you need a more specific order, you can use a simple offset like `"Default + 2"` or `"Late - 10"`.
The default levels are -1000 (early), 0 (default), and 1000 (late).

This field does _not_ support tokens, and capitalization doesn't matter.

> [!TIP]  
> Priorities can make your changes harder to follow and troubleshoot. Suggested best practices:
> * Consider only using very general priorities when possible (like `Late` for a cosmetic overlay
>   meant to be applied over base edits from all mods).
> * There's no need to set priorities relative to _your own_ patches, since you can just list them
>   in the order they should be applied.

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

_(optional)_ The locale code to match in the asset name. For example, setting `"TargetLocale": "fr-FR"`
will only edit the French localized form of the asset (e.g. `Animals/Dinosaur.fr-FR`). This can be
an empty string to only edit the base unlocalized asset.

If omitted, it's applied to all localized and unlocalized variants of the asset.

</td>
</table>
</dd>
</dl>

### Examples
This changes the in-game sprite for one item:
```js
{
   "Format": "2.7.0",
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

## See also
* [Author guide](../author-guide.md) for other actions and options
