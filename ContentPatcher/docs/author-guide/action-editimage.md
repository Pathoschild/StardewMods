← [author guide](../author-guide.md)

A patch with **`"Action": "EditImage"`** changes part of an image loaded by the game. Any number of
content packs can edit the same asset. You can extend an image downwards by just patching past the
bottom (Content Patcher will expand the image to fit).

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

field     | purpose
--------- | -------
`FromArea` | The part of the source image to copy. Defaults to the whole source image. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. Its fields may contain tokens.
`ToArea`   | The part of the target image to replace. Defaults to the `FromArea` size starting from the top-left corner. This is specified as an object with the X and Y pixel coordinates of the top-left corner, and the pixel width and height of the area. If you specify an area past the bottom or right edges of the image, the image will be resized automatically to fit. Its fields may contain tokens.
`PatchMode`| How to apply `FromArea` to `ToArea`. Defaults to `Replace`. Possible values: <ul><li><code>Replace</code>: replace every pixel in the target area with your source image. If the source image has transparent pixels, the target image will become transparent there.</li><li><code>Overlay</code>: draw your source image over the target area. If the source image has transparent pixels, the target image will 'show through' those pixels. Semi-transparent or opaque pixels will replace the target pixels.</li></ul>For example, let's say your source image is a pufferchick with a transparent background, and the target image is a solid green square. Here's how they'll be combined with different `PatchMode` values:<br />![](../screenshots/patch-mode-examples.png)
`When`    | _(optional)_ Only apply the patch if the given [conditions](../author-guide.md#conditions) match.
`LogName` | _(optional)_ A name for this patch to show in log messages. This is useful for understanding errors; if not specified, it'll default to a name like `entry #14 (EditImage Animals/Dinosaurs)`.
`Update`  | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.

</dd>
</dl>

### Examples
This changes the in-game sprite for one item:
```js
{
   "Format": "1.25.0",
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
