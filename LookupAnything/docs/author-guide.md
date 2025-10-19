← [README](README.md)

This page helps mod authors integrate with Lookup Anything and describes how to add new UI themes via Content Patcher. **See the [main README](README.md) for other
info.**

## Contents
* [Show your mod in the 'Added by mod' field](#added-by-mod)
* [Add a UI theme via Content Patcher](#new-ui-theme)


## Show your mod in the 'Added by mod' field <a name="added-by-mod"></a>

Lookup Anything has a field 'Added by mod' which displays which mod added a given item/NPC/farm animal/building/tree/fruit tree/movie snack.
The mod name is determined by inspecting the (unqualified) internal ID of a subject for a `<ModId>_` prefix, then checking that against the loaded mods.

For example, if your mod is named "The Great Pufferfish Escapade" with mod ID "YourName.PufferfishEscapade" and you had these objects added via `Data/Objects`:

- `YourName.PufferfishEscapade_Thingy`: Lookup Anything will show "Added by mod: The Great Pufferfish Escapade".
- `YourName.PufferfishEscapade.Thingy`: Lookup Anything will NOT show "Added by mod".
- `Thingy`: Lookup Anything will NOT show "Added by mod".

## Add a UI theme via Content Patcher <a name="new-ui-theme"></a>

You can add new UI themes to Lookup Anything by editing the asset named `Pathoschild.LookupAnything/Themes`.
The player will be able to select your theme via config.

Example, using Content Patcher:

```js
{
    "Action": "EditData",
    "Target": "Pathoschild.LookupAnything/Themes",
    "Entries": {
        // The key should be unique
        "{{ModId}}_MenuBox_Purple": {
            // This name will appear in Lookup Anything's GMCM
            "DisplayName": "MenuBox: Purple",
            // There are 3 kinds of background display
            // - PlainColor: solid BackgroundPrimaryColor bordered by BackgroundSecondaryColor
            // - FixedSprite: a fixed texture whose aspect ratio is respected
            // - MenuBox: a texture that will be sliced and expanded while keeping the border as they are
            "BackgroundCategory": "MenuBox",
            // Background texture asset, can be vanilla texture or custom texture
            "BackgroundTexture": "Maps\\MenuTilesUncolored",
            // Area of background texture to use for this theme
            "BackgroundSourceRect": {
                "X": 0,
                "Y": 256,
                "Width": 60,
                "Height": 60
            },
            // Color used for the background draw, this can be named color or hex or rgba
            "BackgroundPrimaryColor": "Purple",
            // Color used for the border draw when BackgroundCategory=PlainColor, this can be named color or hex or rgba
            "BackgroundSecondaryColor": "Black",
            // Padding around the background's edges and the body 
            "BackgroundPadding": 4
        },
    }
}
```
