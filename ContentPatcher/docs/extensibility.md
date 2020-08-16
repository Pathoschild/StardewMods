← [README](../README.md)

This document helps SMAPI mod authors extend Content Patcher with custom functionality.

**See the [main README](../README.md) for other info**.

## Contents
* [Overview](#overview)
  * [Introduction](#introduction)
  * [Access the API](#access-the-api)
* [Basic API](#basic-api)
* [Advanced API](#advanced-api)
  * [Caveats](#caveats)
  * [Token concepts](#token-concepts)
  * [Add a token](#add-a-token)
* [See also](#see-also)

## Overview
### Introduction
Content Patcher has a [mod-provided API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Mod-provided_APIs)
you can use from your own SMAPI mod to add custom tokens. Custom tokens are always prefixed with
the ID of the mod that created them, like `your-mod-id/SomeTokenName`.

There are two parts of the API you can use:

* The **basic API** is strongly recommended for most mods. This lets you create custom tokens
  with minimal knowledge of how Content Patcher works internally; Content Patcher will
  automatically handle the gritty details for you, and your tokens are highly compatible with
  future versions of SMAPI.

* The **advanced API** gives you much more control over how the token works. The disadvantages are
  that your token code will be more complex, you need a grasp of how Content Patcher works
  internally, and your token may break in future versions of Content Patcher when its internal
  implementation changes. Using the advanced API is strongly discouraged unless you can't use the
  basic API.

Note that you can use both at once from your mod code.

### Access the API
To access the API:

1. Add Content Patcher as [a dependency in your mod's `manifest.json`](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Dependencies):

   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "IsRequired": false }
   ]
   ```

2. Copy [`IContentPatcherAPI`](../IContentPatcherAPI.cs)
   into your mod code, and **delete any methods you won't need for future compatibility**.
3. Hook into [SMAPI's `GameLoop.GameLaunched` event](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events#GameLoop.GameLaunched)
   and get a copy of the API:
   ```c#
   var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```
4. Use the API to extend Content Patcher (see below).

## Basic API
You can add a simple token by calling `RegisterToken` from SMAPI's `GameLaunched` event (see
_Access the API_ above). For example, this creates a `{{your-mod-id/PlayerName}}` token for the
current player's name:
```c#
api.RegisterToken(this.ModManifest, "PlayerName", () =>
{
    if (Context.IsWorldReady)
        return new[] { Game1.player.Name };
    if (SaveGame.loaded?.player != null)
        return new[] { SaveGame.loaded.player.Name }; // lets token be used before save is fully loaded
    return null;
});
```

`RegisterToken` in this case has three arguments:

argument   | type | purpose
---------- | ---- | -------
`mod`      | `IManifest` | The manifest of the mod defining the token. You can just pass in `this.ModManifest` from your entry class.
`name`     | `string` | The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, so `PlayerName` in the above example will become `your-mod-id/PlayerName`.
`getValue` | `Func<IEnumerable<string>>` | A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context and any patches or dynamic tokens using it are disabled.

That's it! Now any content pack which lists your mod as a dependency can use the token in its fields:
```js
{
   "Format": "1.17.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Characters/Dialogue/Abigail",
         "Entries": {
            "Mon": "Oh hey {{your-mod-id/PlayerName}}! Taking a break from work?"
         }
      }
   ]
}
```

## Advanced API
### Caveats
The _basic API_ section above is strongly recommended for most tokens, since Content
Patcher will handle details like context updates and change tracking for you, it's easier to
troubleshoot, and it's guaranteed not to break without a major-version update.

If you really need it, the advanced API gives you full control (almost equivalent to a token in the
Content Patcher core). However:

* <strong>This is experimental. There's no guarantee that future versions will be backwards
  compatible, or that you'll get any warning before it changes.</strong>
* <strong>This is low-level. You must account for the token design considerations documented below,
  unlike the basic API above which handles them for you.</strong>

### Token concepts
When registering a token through the advanced API, here are some design considerations to avoid
problems.

<dl>
<dt>Context updates</dt>
<dd>

Token values are a cached view of the game state, updated at specific points (e.g. on day start).
The combination of all tokens is called the 'context'; a 'context update' is when Content Patcher
refreshes all tokens, rebuilds caches, rechecks patch conditions, reloads assets if needed, etc.

**Tokens must not change value outside of the `UpdateContext` method**. Doing so may have severe
and undocumented effects, from graphical glitches to outright game crashes.

That doesn't preclude tokens that calculate their value dynamically (e.g. `FileExists`), so long
as this calculation does not change. If a token may change dynamically between context updates
(e.g. `Random`), it must implement caching to ensure it does not.

</dd>

<dt>Bounded values</dt>
<dd>

A token is _bounded_ if its values are guaranteed to match a set of known values; otherwise it's
_unrestricted_.

This affects two things:
* Where the token can be used. For example, a token not guaranteed to return integer values can't
  be used in a number field, even if it _currently_ returns a number.
* Validation when the token is used as part of a `When` condition. For example, this will show a
  warning since it's guaranteed to always be false:
  ```js
  "When": {
     "Season": "totally not a valid season"
  }
   ```

Note that boundedness is _per-input_. For example, your token might be bounded if it receives input
arguments, but unrestricted without one:
```js
"When": {
   "Relationship": "Abigail:Married", // unrestricted: may return any value (e.g. for custom NPCs)
   "Relationship:Abigail": "Married"  // bounded: returns predefined values like 'married' or 'dating'
}
```

When registering a token, a token is bounded if you implement `HasBoundedValues` or
`HasBoundedRangeValues`. Implementing `TryValidateValues` lets you add custom validation, but does
_not_ make the token bounded since Content Patcher can't get a list of possible values.

</dd>
<dt>Immutable values</dt>
<dd>

A token is _immutable_ if its value for a given input will never change for the entire lifetime of
the current game instance (from game launch to full exit). Most tokens are _mutable_, meaning their
value may change.

Immutability enables several optimizations. For example, since Content Patcher doesn't need to
update their value, it also doesn't need to update dependent tokens/patches (and theirs dependents,
etc).

Immutable tokens may also be used in certain fields like `Enabled`, where tokens are otherwise
prohibited.

</dd>

<dt>Input arguments</dt>
<dd>

See [_input arguments_ in the tokens guide](author-tokens-guide.md#input-arguments) for more info.

Due to limitations in SMAPI's API proxying, your mod will receive a normalised input string
identical to the format shown in the tokens guide instead of a parsed object. Any tokens in the
input will be replaced by their value. Note that if no input arguments were given, the token will
receive `null`.

</dd>
</dl>

### Add a token
To register a custom token using the advanced API:

<ol>
<li>

Create a token class with any combination of [the methods listed in this file](../Framework/Tokens/ValueProviders/ModConvention/ConventionDelegates.cs).
Note that the methods in your class must exactly match the names, return values, and arguments. If
Content Patcher files a non-matching or unrecognized public method, it'll show an error and reject
the token.

For example, let's say we want a token which returns the initials for the given name (like
`{{Initials:John Smith}}` → `JS`), or the player's name if called with no input. Here's a token
class to do that:
```c#
/// <summary>A token which returns the player's initials, or the initials of the input name.</summary>
internal class InitialsToken
{
    /*********
    ** Fields
    *********/
    /// <summary>The player name as of the last context update.</summary>
    private string PlayerName;


    /*********
    ** Public methods
    *********/
    /****
    ** Metadata
    ****/
    /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
    public bool AllowsInput()
    {
        return true;
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public bool CanHaveMultipleValues(string input = null)
    {
        return false;
    }

    /****
    ** State
    ****/
    /// <summary>Update the values when the context changes.</summary>
    /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
    public bool UpdateContext()
    {
        string oldName = this.PlayerName;
        this.PlayerName = Game1.player?.Name ?? SaveGame.loaded?.player?.Name; // tokens may update while the save is still being loaded
        return this.PlayerName != oldName;
    }

    /// <summary>Get whether the token is available for use.</summary>
    public bool IsReady()
    {
        return this.PlayerName != null;
    }

    /// <summary>Get the current values.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public IEnumerable<string> GetValues(string input)
    {
        // get name
        string name = input ?? this.PlayerName;
        if (string.IsNullOrWhiteSpace(name))
            yield break;

        // get initials
        yield return string.Join("", name.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(p => p[0]));
    }
}
```

</li>
<li>

Next let's register it with Content Patcher in the `GameLaunched` event (see [_Access the API_](#access-the-api)
above):

```cs
api.RegisterToken(this.ModManifest, "Initials", new InitialsToken());
```

</li>
</ul>

That's it! Now any content pack which lists your mod as a dependency can use the token in its fields:
```js
{
   "Format": "1.17.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Characters/Dialogue/Abigail",
         "Entries": {
            "Mon": "Oh hey {{your-mod-id/Initials}}! Taking a break from work?"
         }
      }
   ]
}
```

## See also
* [README](../README.md) for other info
