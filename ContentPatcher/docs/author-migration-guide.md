← [README](../README.md)

This document helps mod authors update their content packs for newer versions of Content Patcher.

**See the [main README](../README.md) for other info**.

## Contents
* [FAQs](#faqs)
* [Migration guides](#migration-guides)
  * [1.17](#117)
  * [1.15](#115)
  * [1.7](#17)
  * [1.6](#16)
* [See also](#see-also)

## FAQs
<dl>
<dt>Does this affect me as a player?</dt>
<dd>

No, this is only for content pack authors. Existing content packs should work fine.

</dd>

<dt>Are Content Patcher updates backwards-compatible?</dt>
<dd>

Yep; even content packs written for Content Patcher 1.0 still work in the latest versions.

Content Patcher updates rarely have breaking changes, but the [`Format` field in your
`content.json`](author-guide.md#format) says which version it was created for. When a future
version of Content Patcher loads your content pack, it internally migrates your `content.json` to
the latest format (without changing the actual files). Content Patcher itself no longer supports
older formats, but your content pack is just changed to use the latest one.

</dd>

<dt>So I never need to update my content packs?</dt>
<dd>

Technically you don't need to (aside from game changes), but **updating your `Format` version when
you update the content pack is strongly encouraged**.

Using an old `Format` version has major disadvantages:

* You can't use newer features.
* You may have undocumented behavior. (For example, the `Weather` token before `Format` version 1.6
  uses the same value for sunny and windy days, but that's not documented since it only does so for
  backwards compatibility.)
* There's more risk of bugs and startup time is impacted. (For example, a content pack which uses
  `"Format": "1.0"` has over a dozen automated migrations applied, which increases the chance that
  something will be migrated incorrectly and increases startup time.)

</dd>

<dt>How do I update my content pack?</dt>
<dd>

Just set the `Format` field to the latest version shown in the [author guide](author-guide.md),
then review the sections below for any changes you need to make. If a version isn't listed on this
page, there's nothing else to change for that version.

Feel free to [ask on Discord](https://smapi.io/community#Discord) if you need help!

</dd>
</dl>

## Migration guides
These changes only apply when you set the `Format` version in your `content.json` to the listed
version or higher. See [release notes](../release-notes.md) for a full list of changes.

### 1.17
Released 16 August 2020.

* **Patch updates on location change:** using `LocationName` or `IsOutdoors` as a condition/token
  no longer automatically updates the patch when the player changes location. You can add this
  patch field to enable that:

  ```js
  "Update": "OnLocationChange"
  ```

  (This is part of the migration to realtime content updates, since all tokens will soon update
  live.)

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
* [README](../README.md) for other info
* [Ask for help](https://stardewvalleywiki.com/Modding:Help)
