← [README](README.md)

This doc helps SMAPI mod authors use Content Patcher's condition system in their own mods.

**To add custom tokens for content packs to use, see the [extensibility API](extensibility.md). See
the [main README](README.md) for other info**.

**🌐 In other languages: [zh (中文)](zh/conditions-api.md)**.

## Contents
* [Overview](#overview)
* [Access the API](#access-the-api)
* [Parse conditions](#parse-conditions)
* [Manage conditions](#manage-conditions)
* [Caveats](#caveats)
* [See also](#see-also)

## Overview
Content Patcher has a [conditions system](author-guide/tokens.md) which lets content packs check
dozens of contextual values for conditional changes. For example:
```js
"When": {
   "PlayerGender": "male",             // player is male
   "Relationship: Abigail": "Married", // player is married to Abigail
   "HavingChild": "{{spouse}}",        // Abigail is having a child
   "Season": "Winter"                  // current season is winter
}
```

Other SMAPI mods can use this conditions system too. They essentially create a dictionary
representing the conditions they want to check (e.g. by parsing them from their own content packs),
call the API below to get a 'managed conditions' object, then use that to manage the conditions.

## Access the API
To access the API:

1. Add Content Patcher as [a **required** dependency in your mod's `manifest.json`](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Dependencies):
   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "MinimumVersion": "2.7.0" }
   ]
   ```
2. Add a reference to the Content Patcher DLL in your mod's `.csproj` file. Make sure you set
   `Private="False"`, so the DLL isn't added to your mod folder:
   ```xml
   <ItemGroup>
     <Reference Include="ContentPatcher" HintPath="$(GameModsPath)\ContentPatcher\ContentPatcher.dll" Private="False" />
   </ItemGroup>
   ```
3. Somewhere in your mod code (e.g. in the [`GameLaunched` event](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events#GameLoop.GameLaunched)),
   get a reference to Content Patcher's API:
   ```c#
   var api = this.Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```

## Parse conditions
**Note:** see [_caveats_](#caveats) before calling this API.

Now that you have access to the API, you can parse conditions.

1. Create a `Dictionary<string, string>` model of the conditions you want to check. This can use
   Content Patcher features like tokens. For this example, let's assume you have these hardcoded
   conditions (see the [conditions documentation](author-guide/tokens.md) for the format):
   ```c#
   var rawConditions = new Dictionary<string, string>
   {
      ["PlayerGender"] = "male",             // player is male
      ["Relationship: Abigail"] = "Married", // player is married to Abigail
      ["HavingChild"] = "{{spouse}}",        // Abigail is having a child
      ["Season"] = "Winter"                  // current season is winter
   };
   ```

2. Call the API to parse the conditions into an `IManagedConditions` wrapper. The `formatVersion`
   matches the [`Format` field described in the author guide](author-guide.md#overview) to
   enable forward compatibility with future versions of Content Patcher.

   ```c#
   var conditions = api.ParseConditions(
      manifest: this.ModManifest,
      raw: rawConditions,
      formatVersion: new SemanticVersion("1.20.0")
   );
   ```

3. Get the result from the `IsMatch` property. For example:
   ```cs
   conditions.UpdateContext();
   if (conditions.IsMatch)
      ...
   ```

If you want to allow custom tokens added by other SMAPI mods, you can specify a list of mod IDs
to assume are installed. You don't need to do this for your own mod ID, for mods listed as
required dependencies in your mod's `manifest.json`, or for mods listed via `HasMod` in the
conditions dictionary.
```c#
var conditions = api.ParseConditions(
   manifest: this.ModManifest,
   raw: rawConditions,
   formatVersion: new SemanticVersion("1.20.0"),
   assumeModIds: new[] { "spacechase0.JsonAssets" }
);
```

## Manage conditions
The `IManagedConditions` object you got above provides a number of properties and methods to manage
the parsed conditions. You can check IntelliSense in Visual Studio to see what's available, but
here are some of the most useful properties:

<table>
<tr>
<th>property</th>
<th>type</th>
<th>description</th>
</tr>

<tr>
<td><code>IsValid</code></th>
<td><code>bool</code></td>
<td>

Whether the conditions were parsed successfully (regardless of whether they're in scope currently).

</td>
</tr>
<tr>
<td><code>ValidationError</code></td>
<td><code>string</code></td>
<td>

When `IsValid` is false, an error phrase indicating why the conditions failed to parse, formatted
like this:
> 'seasonz' isn't a valid token name; must be one of &lt;token list&gt;

If the conditions are valid, this is `null`.

</td>
</tr>
<tr>
<td><code>IsReady</code></td>
<td><code>bool</code></td>
<td>

Whether the conditions' tokens are all valid in the current context. For example, this would be
false if the conditions use `Season` and a save isn't loaded yet.

</td>
</tr>
<tr>
<td><code>IsMatch</code></td>
<td><code>bool</code></td>
<td>

Whether `IsReady` is true, and the conditions all match in the current context.

If there are no conditions (i.e. you parsed an empty dictionary), this is always true.

</td>
</tr>
<tr>
<td><code>IsMutable</code></td>
<td><code>bool</code></td>
<td>

Whether `IsMatch` may change depending on the context. For example, `Season` is mutable since it
depends on the in-game season. `HasMod` is not mutable, since it can't change after the game is
launched.

</td>
</tr>
</table>

And methods:

<table>
<tr>
<th>method</th>
<th>type</th>
<th>description</th>
</tr>

<tr>
<td><code>GetReasonNotMatched</code></th>
<td><code>string</code></td>
<td>

If `IsMatch` is false, this analyzes the conditions/context and provides a human-readable reason
phrase explaining why the conditions don't match the context. For example:
> conditions don't match: season

If the conditions do match, this returns `null`.

</td>
</tr>
<tr>
<td><code>UpdateContext</code></th>
<td><code>bool</code></td>
<td>

Updates the conditions based on Content Patcher's current context, and returns whether `IsMatch`
changed. It's safe to call this as often as you want, but it has no effect if the Content Patcher
context hasn't changed since you last called it.

</td>
</tr>
</table>

## Caveats
<dl>
<dt>The conditions API isn't available immediately.</dt>
<dd>

The conditions API is available two ticks after the `GameLaunched` event (and anytime after that
point). That's due to the Content Patcher lifecycle:

1. `GameLaunched`: other mods can register custom tokens.
2. `GameLaunched + 1 tick`: Content Patcher initializes the token context (including custom tokens).
3. `GameLaunched + 2 ticks`: other mods can use the conditions API.

</dd>
<dt>Conditions should be cached.</dt>
<dd>

Parsing conditions through the API is a relatively expensive operation. If you'll recheck the same
conditions often, it's best to save and reuse the `IManagedConditions` instance.

</dd>
<dt>Conditions don't update automatically.</dt>
<dd>

When using a cached `IManagedConditions` object, make sure to update it using
`conditions.UpdateContext()` as needed.

Note that condition updates are limited to Content Patcher's [update
rate](author-guide.md#update-rate). When you call `conditions.UpdateContext()`, it will reflect the
tokens as of Content Patcher's last internal context update.

</dd>
<dt>Conditions handle split-screen automatically.</dt>
<dd>

For example, `IsMatch` returns whether it matches the _current screen's_ context. The exception is
`UpdateContext`, which updates the context for all active screens.

</dd>
</dl>

## See also
* [README](README.md) for other info
