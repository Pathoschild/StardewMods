← [README](README.md)

This doc helps SMAPI mod authors use Content Patcher's token strings in their own mods.

**To add custom tokens for content packs to use, see the [extensibility API](extensibility.md). See
the [main README](README.md) for other info**.

**🌐 In other languages: [zh (中文)](zh/token-strings-api.md)**.

## Contents
* [Overview](#overview)
* [Access the API](#access-the-api)
* [Parse token strings](#parse-token-strings)
* [Manage token strings](#manage-token-strings)
* [Caveats](#caveats)
* [See also](#see-also)

## Overview
Content Patcher has a [token system](author-guide/tokens.md) which lets content packs make complex
strings using contextual values. For example:
```js
"My favorite season is {{Season}}." // If a save is loaded, {{Season}} will be replaced with the current season.
```

Other SMAPI mods can use this token system too. They essentially create a string with the tokens
they want to have evaluated and call the API below to get a 'managed token string' object, then use
that to manage the token string.

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

## Parse token strings
**Note:** see [_caveats_](#caveats) before calling this API.

Now that you have access to the API, you can parse token strings.

1. Create a string to evaluate. This can use Content Patcher features like [tokens](author-guide/tokens.md).
   For example:
   ```c#
   string rawTokenString = "The current time is {{Time}} on {{Season}} {{Day}}, year {{Year}}.";
   ```
2. Call the API to parse the string into an `IManagedTokenString` wrapper. The `formatVersion`
   matches the [`Format` field described in the author guide](author-guide.md#overview) to enable
   forward compatibility with future versions of Content Patcher.

   ```c#
   var tokenString = api.ParseTokenString(
      manifest: this.ModManifest,
      rawValue: rawTokenString,
      formatVersion: new SemanticVersion("2.7.0")
   );
   ```
3. Get the parsed string from the `Value` property. For example:
   ```cs
   tokenString.UpdateContext();
   string value = tokenString.Value; // The current time is 1430 on Spring 5, year 2.
   ```

If you want to allow custom tokens added by other SMAPI mods, you can specify a list of mod IDs
to assume are installed. You don't need to do this for your own mod ID or for mods listed as
required dependencies in your mod's `manifest.json`.
```c#
var tokenString = api.ParseTokenString(
   manifest: this.ModManifest,
   rawValue: rawTokenString,
   formatVersion: new SemanticVersion("2.7.0"),
   assumeModIds: new[] { "spacechase0.JsonAssets" }
);
```

## Manage token strings
The `IManagedTokenString` object you got above provides a number of properties and methods to
manage the parsed token string. You can check IntelliSense in Visual Studio to see what's available,
but here are some of the most useful properties:

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

Whether the token string was parsed successfully (regardless of whether its tokens are in scope currently).

</td>
</tr>
<tr>
<td><code>ValidationError</code></td>
<td><code>string</code></td>
<td>

When `IsValid` is false, an error phrase indicating why the token string failed to parse, formatted
like this:
> 'seasonz' isn't a valid token name; must be one of &lt;token list&gt;

If the token string is valid, this is `null`.

</td>
</tr>
<tr>
<td><code>IsReady</code></td>
<td><code>bool</code></td>
<td>

Whether the token string's tokens are all valid in the current context. For example, this would be
false if the token string uses `{{Season}}` and a save isn't loaded yet.

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td><code>string?</code></td>
<td>

If `IsReady` is true, the evaluated string.

If `IsReady` is false, `Value` will be null.

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
<td><code>UpdateContext</code></th>
<td><code>bool</code></td>
<td>

Updates the token string based on Content Patcher's current context, and returns whether `Value`
changed. It's safe to call this as often as you want, but it has no effect if the Content Patcher
context hasn't changed since you last called it.

</td>
</tr>
</table>

## Caveats
<dl>
<dt>The token string API isn't available immediately.</dt>
<dd>

The token string API is available two ticks after the `GameLaunched` event (and anytime after that
point). That's due to the Content Patcher lifecycle:

1. `GameLaunched`: other mods can register custom tokens.
2. `GameLaunched + 1 tick`: Content Patcher initializes the token context (including custom tokens).
3. `GameLaunched + 2 ticks`: other mods can use the token string API.

</dd>
<dt>Token strings should be cached.</dt>
<dd>

Parsing token strings through the API is a relatively expensive operation. If you'll recheck the
same token string often, it's best to save and reuse the `IManagedTokenString` instance.

</dd>
<dt>Token strings don't update automatically.</dt>
<dd>

When using a cached `IManagedTokenString` object, make sure to update it using
`tokenString.UpdateContext()` as needed.

Note that token string updates are limited to Content Patcher's [update
rate](author-guide.md#update-rate). When you call `tokenString.UpdateContext()`, it will reflect
the tokens as of Content Patcher's last internal context update.

</dd>
<dt>Token strings handle split-screen automatically.</dt>
<dd>

For example, `Value` returns the value for the _current screen's_ context. The exception is
`UpdateContext`, which updates the context for all active screens.

</dd>
</dl>

## See also
* [README](README.md) for other info
