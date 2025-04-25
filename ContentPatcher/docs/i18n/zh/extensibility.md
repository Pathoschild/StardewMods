← [README](README.md)

此文档帮助SMAPI模组作者延伸Content Patcher的功能。

**如果你想在你的模组中使用条件，详见[条件API](conditions-api.md)。其他信息请参见[主README](README.md)**

## 目录
* [入门](#introduction)
* [访问API](#access-the-api)
* [基本API](#basic-api)
  * [概念](#concepts)
  * [添加令牌](#add-a-token)
* [进阶API](#advanced-api)
  * [注意事项](#caveats)
  * [概念](#concepts-1)
  * [添加令牌](#add-a-token-1)
* [参见](#see-also)

## 入门<a name="introduction"></a>

你的SMAPI模组可以使用Content Patcher的[模组API](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Integrations#.E6.A8.A1.E7.BB.84API)来添加自定义令牌。自定义令牌的前缀为提供它们的模组，如`your-mod-id/SomeTokenName`。

你可以用这两种API：

* 大部分模组推荐使用 **基本API**。你可以在不了解Content Patcher内部结构的情况下创建令牌；Content Patcher会自动管理各种细节，而且你的令牌和未来版本更兼容。

* **进阶API**提供更多控制选项。但是你的令牌会更加复杂，你需要了解Content Patcher的内在运行逻辑，而且你的令牌可能会在未来Content Patcher内在运行逻辑更改时失效。进阶API强烈不推荐使用，除非基本API不可用。

你可以同时使用基本和进阶API。

## 访问API<a name="access-the-api"></a>

访问API的步骤为：

1. 将Content Patcher设为[`manifest.json`中的依赖](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Dependencies):

   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "IsRequired": false }
   ]
   ```

2. 把[`IContentPatcherAPI`](../IContentPatcherAPI.cs) 复制到你模组里并删除**任何你不需要用的方法，为了兼容未来更改**.
3. 在你的模组代码中（如[`GameLaunched`事件](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Events#GameLoop.GameLaunched)中，获取Content Patcher的API：
   ```c#
   var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```
4. 使用API延伸Content Patcher功能，（详见以下）。

## 基本API<a name="basic-api"></a>
### 概念<a name="concepts"></a>

基本API会替你处理大部分设计因素。你只需要考虑以下两点：

<dl>
<dt>范围</dt>
<dd>


Content Patcher只有在更新令牌时才会调用你的代码。这可以在存档加载前，加载中，和加载后。如果你的令牌还没准备好，你可以返回null或空列表 这三种情况在 _[添加令牌](#add-a-token)_ 中都照顾到。

</dd>

<dt>数据顺序</dt>
<dd>

The order you return values affects features like `valueAt`. You should use the order which makes
most sense for your token, since content pack authors can't change it. For most tokens,
alphanumeric order is fine (e.g. `.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)`).

你返回的顺序会影响`valueAt`。推荐使用对于你令牌来说最有意义的顺序，因为内容包作者无法改变此顺序。大部分令牌使用字母数字顺序即可（如 `.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)`）。

</dd>
</dl>

### 添加令牌<a name="add-a-token"></a>
You can add a simple token by calling `RegisterToken` from SMAPI's `GameLaunched` event (see
_[Access the API](#access-the-api)_ above). For example, this creates a `{{your-mod-id/PlayerName}}` token for the
current player's name:
```c#
api.RegisterToken(this.ModManifest, "PlayerName", () =>
{
    // save is loaded
    if (Context.IsWorldReady)
        return new[] { Game1.player.Name };

    // or save is currently loading
    if (SaveGame.loaded?.player != null)
        return new[] { SaveGame.loaded.player.Name };

    // no save loaded (e.g. on the title screen)
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
   "Format": "2.6.0",
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

## 添加令牌<a name="advanced-api"></a>
### 注意事项<a name="caveats"></a>
The _basic API_ section above is strongly recommended for most tokens, since Content
Patcher will handle details like context updates and change tracking for you, it's easier to
troubleshoot, and it's guaranteed not to break without a major-version update.

If you really need it, the advanced API gives you full control (almost equivalent to a token in the
Content Patcher core). However:

* <strong>This is experimental. There's no guarantee that future versions will be backwards
  compatible, or that you'll get any warning before it changes.</strong>
* <strong>This is low-level. You must account for the token design considerations documented below,
  unlike the basic API above which handles them for you.</strong>

### 概念<a name="concepts-1"></a>
When registering a token through the advanced API, here are some design considerations to avoid
problems.

<dl>
<dt>Scope and value order</dt>
<dd>

See [_Basic API: concepts_](#concepts) above.

</dd>

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
update their value, it also doesn't need to update dependent tokens/patches (and their dependents,
etc).

</dd>

<dt>Input arguments</dt>
<dd>

See [_input arguments_ in the tokens guide](author-guide/tokens.md#input-arguments) for more info.

Due to limitations in SMAPI's API proxying, your mod will receive a normalised input string
identical to the format shown in the tokens guide instead of a parsed object. Any tokens in the
input will be replaced by their value. Note that if no input arguments were given, the token will
receive `null`.

</dd>
</dl>

### 添加令牌<a name="add-a-token-1"></a>
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
        yield return string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p[0]));
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
   "Format": "2.6.0",
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
* [README](README.md) for other info
