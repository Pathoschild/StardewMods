← [README](README.md)

This page helps mod authors integrate with Lookup Anything. **See the [main README](README.md) for other info.**

## Contents
* [Support for mod content](#support-for-mod-content)
  * ['Added by mod' field](#added-by-mod-field)
  * [Nested items](#nested-items)
  * [Hovered content in custom menus](#hovered-content-in-custom-menus)
* [Add a UI theme](#add-a-ui-theme)

## Support for mod content
### 'Added by mod' field
When players look up content in-game (like an item or NPC), the _added by mod_ field shows which mod added it if
possible.

For this to work, you must apply the standard [unique string ID](https://stardewvalleywiki.com/Modding:Common_data_field_types#Unique_string_ID)
format (including the mod ID prefix).

### Nested items
Lookup Anything scans the world to detect items for the 'number owned' and gift taste fields. It
scans inside standard items recursively; for example, if you have an `Object` with the `heldObject`
field set to a chest, Lookup Anything will look inside the chest too.

If you have a custom non-`Object` item (e.g. a tool) which contains items, you can add a custom
`heldObject` field or property with any `Item` type. For example:

```c#
// store one item
public Object heldObject;

// store many items
public Chest heldObject = new Chest();
```

Lookup Anything will detect the field and search inside it too.

### Hovered content in custom menus
Lookup Anything detects when the cursor is over an item or NPC in standard menus.

For custom menus, you can add one or both of these fields:
* a `HoveredItem` field with any `Item` type:
  ```c#
  public Object HoveredItem;
  ```
* and/or a `HoveredNpc` field with any `NPC` type:
  ```c#
  public NPC HoveredNpc;
  ```

If present, Lookup Anything will handle them automatically.

## Add a UI theme
Lookup Anything uses visual themes to change its menu appearance.

You can add UI themes by editing the `Mods/Pathoschild.LookupAnything/Themes` asset. Any theme in
that asset will appear in the config UI for Lookup Anything.

The asset consists of a `string` → model lookup, where...
- The key is a unique ID for the theme.
- The value is a model with the fields listed below.

<table>
<tr>
<th>field</th>
<th>effect</th>
</tr>

<tr>
<td><code>DisplayName</code></td>
<td>A translated display name shown in UIs.</td>
</tr>


<tr>
<td><code>BackgroundType</code></td>
<td>

How the menu background should be drawn. Default `FixedSprite`.

The possible values are:
* `FixedSprite`: draw a texture background sprite over the entire area.
* `MenuBox`: draw a menu box by taking specific sprites from the texture background for the corners, edges, and center.
* `PlainColor`: draw a plain colored background with no texture.

</td>
</tr>

<tr>
<td><code>BackgroundTexture</code></td>
<td>The background texture to draw, if applicable based on the <code>BackgroundType</code>.</td>
</tr>

<tr>
<td><code>BackgroundSourceRect</code></td>
<td>The pixel area within the <code>BackgroundTexture</code> to draw.</td>
</tr>

<tr>
<td><code>BackgroundColor</code></td>
<td>

The [color code](https://stardewvalleywiki.com/Modding:Common_data_field_types#Color) for the background (default
`White`).

The effect depends on the `BackgroundType`:

* `FixedSprite` and `MenuBox`: applied as a tint to the background texture (where `White` means no tint).
* `PlainColor`: the color to use as the background.

</td>
</tr>

<tr>
<td><code>BackgroundPadding</code></td>
<td>The pixel spacing between the edge of the <code>BackgroundTexture</code> and the inner content. Default 0.</td>
</tr>

<tr>
<td><code>BorderColor</code></td>
<td>

The [color code](https://stardewvalleywiki.com/Modding:Common_data_field_types#Color) for the border drawn around the background
(default `Black`).

</td>
</tr>

</table>

For example, [using Content Patcher](https://stardewvalleywiki.com/Modding:Content_Patcher):

```js
{
    "Action": "EditData",
    "Target": "Mods/Pathoschild.LookupAnything/Themes",
    "Entries": {
        "{{ModId}}_MenuBox_Purple": {
            "DisplayName": "Menu box (purple)",
            "BackgroundCategory": "MenuBox",
            "BackgroundTexture": "Maps/MenuTilesUncolored",
            "BackgroundSourceRect": { "X": 0, "Y": 256, "Width": 60, "Height": 60 },
            "BackgroundColor": "Purple",
            "BackgroundPadding": 4
        }
    }
}
```
