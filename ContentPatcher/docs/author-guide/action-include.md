← [author guide](../author-guide.md)

A patch with **`"Action": "Include"`** lets you load patches from another JSON file.

**🌐 In other languages: [zh (中文)](../zh/author-guide/action-include.md)**.

## Contents
* [Usage](#usage)
  * [Overview](#overview)
  * [Format](#format)
  * [Examples](#examples)
* [FAQs](#faqs)
  * [Are there limits to the files I can include?](#are-there-limits-to-the-files-i-can-include)
  * [Can I load non-patches using `Include`?](#can-i-load-non-patches-using-include)
* [See also](#see-also)

## Usage
### Overview
Instead of defining all your patches in one `content.json` file, `Include` lets you define them
in another file. The included patches work exactly as if you'd pasted them into the `Include`
patch's position yourself. For example, they can use all of the same features  available in your
`content.json` (like [tokens and conditions](../author-guide.md#tokens)), and any local file paths
are still relative from your `content.json`.

The included file must be a `.json` file which only contains a `Changes` field:
```js
{
    "Changes": [
        /* patches defined here like usual */
    ]
}
```

### Format
An `Include` patch consists of a model under `Changes` (see examples below) with these fields:

<dl>
<dt>Required fields:</dt>
<dd>

field     | purpose
--------- | -------
`Action`  | The kind of change to make. Set to `Include` for this action type.
`FromFile` | The relative path to the `.json` file containing patches in your content pack folder, or multiple comma-delimited paths to load. This path is always relative from your `content.json` (even when an included file includes another file).

</td>
</tr>

</dd>
<dt>Optional fields:</dt>
<dd>

field     | purpose
--------- | -------
`When`    | _(optional)_ Only apply the patch if the given [conditions](../author-guide.md#conditions) match.
`LogName` | _(optional)_ A name for this patch to show in log messages. This is useful for understanding errors; if not specified, it'll default to a name like `Include patches/data.json`.
`Update`  | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.
`LocalTokens` | _(Optional)_ A set of [local tokens](../author-guide/tokens.md#local-tokens) which can be used within this patch's field. These are inherited by all the patches loaded through the `Include` patch.

</dd>
</dl>

### Examples
In the simplest case, you can use this to organize your patches into subfiles:

```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "Include",
         "FromFile": "assets/John NPC.json, assets/Jane NPC.json"
      },
   ]
}
```

You can combine this with tokens and conditions to load files dynamically:

```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "Include",
         "FromFile": "assets/John_{{season}}.json",
         "When": {
            "EnableJohn": true
         }
      }
   ]
}
```

## FAQs
### Are there limits to the files I can include?
Nope. You can `Include` patches from any number of files, those files can use `Include` to load
_other_ files, and you can even include the same file multiple times. In each case it works exactly
as if you'd pasted all the patches into that position in `content.json`.

The only restriction is that you can't have a circular loop (e.g. file A loads B which loads A).

### Can I load non-patches using `Include`?
No. The included file can only contain a `Changes` field. Trying to add a different field like
`ConfigSchema`, `CustomLocations`, or `DynamicTokens` will result in an error message.

## See also
* [Author guide](../author-guide.md) for other actions and options
