← [README](README.md)

This document helps mod authors update their content packs for newer versions of Content Patcher.

**See the [main README](README.md) for other info**.

**🌐 In other languages: [zh (中文)](zh/author-migration-guide.md)**.

## Contents
* [FAQs](#faqs)
* [Migration guides](#migration-guides)
  * [2.1](#21)
  * [2.0](#20)
  * [1.25](#125)
  * [1.24](#124)
  * [1.21](#121)
  * [1.20](#120)
  * [1.18](#118)
  * [1.17](#117)
  * [1.15](#115)
  * [1.7](#17)
  * [1.6](#16)
* [See also](#see-also)

## FAQs
<dl>
<dt>Does this page affect me as a player?</dt>
<dd>

No, this is only for content pack authors. Existing content packs should work fine.

</dd>

<dt>Are Content Patcher updates backwards-compatible?</dt>
<dd>

Yes, even content packs written for Content Patcher 1.0.0 should still work. Content Patcher uses
your [`Format` field](author-guide.md#format) to convert the content pack to the latest version if
needed (without changing the actual files).

</dd>

<dt>Do I need to update my content packs?</dt>
<dd>

Usually your content packs will work indefinitely without manual updates. Game changes may
sometimes break content packs (though Content Patcher will try to rewrite those too).

However, using an old `Format` version has some major disadvantages. Your content pack...

* Won't have access to newer features.
* May have legacy behavior that doesn't match the current docs.
* May increase startup time or cause in-game lag. Rewriting code for compatibility is sometimes
  complicated and inefficient, so it's much faster if the code is already updated instead.
* May have more bugs. For example, a content pack for `Format` version 1.0 has dozens of automated
  migrations applied, which increases the chance that something will be migrated incorrectly.

Migrating to the latest format when you update the content pack is strongly encouraged.

</dd>

<dt>How do I update my content pack?</dt>
<dd>

Just set the `Format` field to the latest version shown in the [author guide](author-guide.md),
then review the sections below for any changes you need to make. If a version isn't listed on this
page, there's nothing else to change for that version.

</dd>

<dt>
  Why does my content pack show "reported warnings when applying runtime migration 2.0.0" in the
  SMAPI console?
</dt>
<dd>

Your content pack has a `Format` version from before Stardew Valley 1.6, so Content Patcher tried
to migrate your content pack to the new asset format and failed.

You can fix it by:
1. Setting `"Format": "2.0.0"` in your `content.json`.
2. Updating your content pack to the latest Content Patcher and Stardew Valley format (see below).

</dd>
</dl>

> [!TIP]
> Feel free to [ask on Discord](https://smapi.io/community#Discord) if you need help!

## Migration guides
These changes only apply when you set the `Format` version in your `content.json` to the listed
version or higher. See [release notes](release-notes.md) for a full list of changes.

### 2.1
Released 22 May 2024.

* `"Action": "Load"` patches now apply to localized asset names _only_ if they have localized forms in the base game
  folder.

  For example, this patch will now always load `Characters/Toddler`, and _not_ a localized variant like
  `Characters/Toddler.fr-FR`:
  ```json
  {
      "Action": "Load",
      "Target": "Characters/Toddler",
      "FromFile": "assets/toddler.png"
  }
  ```

  This should have no effect on most content packs, besides fixing various issues with some edits for non-English
  players.

### 2.0
Released 19 March 2024.

<ul>
<li>

See _[migrate to Stardew Valley 1.6](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)_ for
  content changes in the game update.

</li>
<li>

[`Load` patches](author-guide/action-load.md) have a new `Priority` field. It's optional, but you can improve mod
compatibility by using it when relevant.

</li>
<li>

[`CustomLocations`](author-guide/custom-locations.md) is now deprecated. You should add custom locations to the
[new `Data/Locations` asset](https://stardewvalleywiki.com/Modding:Location_data) in Stardew Valley
1.6 instead.

For example, if you have a custom location like this:

```js
"CustomLocations": [
    {
        "Name": "Custom_ExampleMod_AbigailCloset",
        "FromMapFile": "assets/abigail-closet.tmx"
    }
]
```

You can now add it to the game directly like this:

```js
"Changes": [
    // add map
    {
        "Action": "Load",
        "Target": "Maps/{{ModId}}_AbigailCloset",
        "FromFile": "assets/abigail-closet.tmx"
    },

    // add location
    {
        "Action": "EditData",
        "Target": "Data/Locations",
        "Entries": {
            "{{ModId}}_AbigailCloset": {
                "CreateOnLoad": { "MapPath": "Maps/{{ModId}}_AbigailCloset" },
                "FormerLocationNames": [ "Custom_ExampleMod_AbigailCloset" ]
            }
        }
    }
]
```

The game uses a standard [unique string ID](https://stardewvalleywiki.com/Modding:Common_data_field_types#Unique_string_ID)
format for the location name. In the example above, we use the new name format (`{{ModId}}_AbigailCloset`) and add the
old name (`Custom_ExampleMod_AbigailCloset`) to the `FormerLocationNames` field so the location will be migrated
automatically for current players.

Content Patcher will replace `{{ModId}}` automatically with [your mod's manifest `UniqueId`](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest).

**Known limitations:**
* You can't migrate TMXL Map Toolkit locations directly to Data/Locations. If you need to support migrations from TMXL,
  you can continue using `CustomLocations` which still supports specifying TMXL locations. You can then edit
  `Data/Locations` to edit the data added for your location.

</li>
</ul>

### 1.25
Released 27 February 2022.

* **The `Enabled` field is no longer supported.** You can use `When` conditions instead.

### 1.24
Released 31 October 2021.

* **The `Spouse` token no longer includes roommates.** If you want to check for both roommate and
  spouse, you can use `{{Merge: {{Roommate}}, {{Spouse}}}}` to match the previous behavior.
* **Some tokens return values in a different order** to match the game order. This should have no
  effect on most content packs, unless they use `valueAt` with any of these tokens:
  `HasActiveQuest`, `HasCaughtFish`, `HasDialogueAnswer`, `HasFlag`, `HasProfession`, and
  `HasSeenEvent`.

### 1.21
Released 07 March 2021.

* **The `Enabled` field no longer allows tokens.** You should use `When` for conditional logic
  instead.

### 1.20
Released 06 February 2021.

* **The `Weather` token now returns weather for the _current location context_ (i.e. island or
  valley) by default**. You can use `{{Weather: Valley}}` to match the previous behavior.

### 1.18
Released 12 September 2020.

* **Using the `FromFile` field with an `EditData` patch is no longer supported.** This worked
  differently than `FromFile` on any other patch type and often caused confusion, so it's been
  deprecated since 1.16.

  This has no effect on using `FromFile` with a non-`EditData` patch, or on `EditData` patches
  which don't use `FromFile`.

  If you have a patch like this:

  ```js
  // in content.json
  {
     "Action": "EditData",
     "Target": "Characters/Dialogue/Abigail",
     "FromFile": "assets/abigail.json"
  }

  // assets/abigail.json
  {
     "Entries": {
        "4": "Oh, hi.",
        "Sun_17": "Hmm, interesting..."
     }
  }
  ```

  You can migrate it to this:

  ```js
  // in content.json
  {
     "Action": "Include",
     "FromFile": "assets/abigail.json"
  }

  // assets/abigail.json
  {
     "Changes": [
        {
           "Action": "EditData",
           "Target": "Characters/Dialogue/Abigail",
           "Entries": {
              "4": "Oh, hi.",
              "Sun_17": "Hmm, interesting..."
           }
        }
     ]
  }
  ```

### 1.17
Released 16 August 2020.

* **Patch updates on location change:** using `LocationName` or `IsOutdoors` as a condition/token
  no longer automatically updates the patch when the player changes location. You can add this
  patch field to enable that:

  ```js
  "Update": "OnLocationChange"
  ```

### 1.15
Released 04 July 2020.

* **Token search syntax:** you could previously search some tokens by passing the value as an input
  argument like `{{Season: Spring}}`. That should now be written like `{{Season |contains=Spring}}`,
  which works with all tokens.

  The change affects all tokens _except_ `HasFile`, `HasValue`, `Hearts`, `Lowercase`/`Uppercase`,
  `Query`, `Random`, `Range`, `Round` `Relationship`, `SkillLevel`, and mod-provided tokens (which
  all use input arguments for a different purpose).

  That also affects conditions:
  ```js
  "When": {
    "Season: Spring": "true" // should be "Season |contains=Spring": "true"
  }
  ```

  Note that conditions like this aren't affected:
  ```js
  // still okay!
  "When": {
    "Season": "Spring"
  }
  ```

* **Random pinned keys:** the `Random` token allows an optional pinned key. The previous format was
  `{{Random: choices | pinned-key}}`; that should be changed to `{{Random: choices |key=pinned-key}}`.

### 1.7
Released 08 May 2019.

* The `ConfigSchema` field changed:
  * `AllowValues` is no longer required. If you omit it, the config field will allow _any_ value.
  * If you omit `Default`, the default is now blank instead of the first `AllowValues` value.

### 1.6
Released 08 December 2018.

* The `Weather` token now returns `Wind` on windy days instead of `Sun`.

## See also
* [README](README.md) for other info
* [Ask for help](https://stardewvalleywiki.com/Modding:Help)
