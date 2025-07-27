← [README](README.md)

此文档帮助 SMAPI 模组作者拓展 Content Patcher 的功能。

**如果您想在您的模组中使用条件，请参阅[条件 API](conditions-api.md)。其他信息请参见[主 README](README.md)**

**🌐 其他语言：[en (English)](../extensibility.md)。**

## 目录
* [简介](#introduction)
* [访问API](#access-the-api)
* [基本API](#basic-api)
  * [概念](#concepts)
  * [添加令牌](#add-a-token)
* [进阶API](#advanced-api)
  * [注意事项](#caveats)
  * [概念](#concepts-1)
  * [添加令牌](#add-a-token-1)
* [参见](#see-also)

## 简介<a name="introduction"></a>

您的 SMAPI 模组可以使用 Content Patcher 的[模组 API](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Integrations#模组API)来添加自定义令牌。自定义令牌的前缀为提供它们的模组，如`your-mod-id/SomeTokenName`。

您可以使用这两种 API：

* **基本 API**：推荐用于大部分模组。这使得您可以在不了解 Content Patcher 内部结构的情况下创建自定义令牌；Content Patcher 会自动处理各种细节，并且您的令牌与未来的 SMAPI 版本的兼容性更佳。
* **进阶 API**：提供更多控制选项，缺点是您的令牌会更加复杂。您需要了解 Content Patcher 的内部工作原理，而且您的令牌可能会在未来 Content Patcher 内部更改时失效。除非基本 API 不可用，否则强烈不建议使用进阶 API。

您可以同时使用基本和进阶 API。

## 访问 API<a name="access-the-api"></a>

访问 API 的步骤为：

1. 将 Content Patcher 设为 [`manifest.json` 中的依赖](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest#Dependencies_属性):

   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "IsRequired": false }
   ]
   ```

2. 把 [`IContentPatcherAPI`](../../IContentPatcherAPI.cs) 复制到您模组里，然后删除**任何您不需要用到的方法，这主要是为了考虑未来版本的兼容性**。
3. 在您的模组代码中（如 [`GameLaunched` 事件](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Events#GameLoop.GameLaunched)中）获取 Content Patcher 的 API：
   ```c#
   var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```
4. 使用 API 延伸 Content Patcher 功能（请参阅下文）。

## 基本 API<a name="basic-api"></a>
### 概念<a name="concepts"></a>

基本 API 会替您处理大部分设计因素。您只需要记住以下两点：

<dl>
<dt>作用域</dt>
<dd>

Content Patcher 会在每次更新令牌时调用您的代码。这可以发生在存档加载前、加载期间和加载后。如果您的令牌还没准备好，您可以返回 null 或空列表。请参阅[添加令牌](#add-a-token)中的示例，该示例处理了所有三种情况。

</dd>

<dt>值顺序</dt>
<dd>

您返回的值顺序会影响 `valueAt`。推荐使用对于您的令牌来说最有意义的顺序，因为内容包作者无法改变此顺序。对于大部分令牌而言，使用字母数字顺序即可（如 `.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)`）。

</dd>
</dl>

### 添加令牌<a name="add-a-token"></a>

您可以在 `GameLaunched` 事件中通过 `RegisterToken` 添加简单令牌（请参阅[访问 API](#access-the-api) 章节）。例如，以下代码创建一个 `{{your-mod-id/PlayerName}}` 令牌代表当前玩家名：

```c#
api.RegisterToken(this.ModManifest, "PlayerName", () =>
{
    // 存档已加载
    if (Context.IsWorldReady)
        return new[] { Game1.player.Name };

    // 存档正在加载
    if (SaveGame.loaded?.player != null)
        return new[] { SaveGame.loaded.player.Name };

    // 未加载存档(e.g. 在主页)
    return null;
});
```

`RegisterToken` 有三个参数：

参数   | 类型 | 用途
---------- | ---- | -------
`mod`      | `IManifest` | 提供此令牌模组的 Manifest，您可以直接使用 ModEntry 的 `this.ModManifest`
`name`     | `string`    | 令牌名称，只需要确保该名称在您自己模组中具有唯一性。Content Patcher 会自动给这个名字添加前缀。以上示例的 `PlayerName` 会变成 `your-mod-id/PlayerName`。
`getValue` | `Func<IEnumerable<string>>` | 一个方法，返回当前值。如果返回了 null 或空列表，则认为令牌在当前上下文中不可用，所有使用它的补丁或动态令牌将被禁用。

现在，任何将您的模组列为依赖的内容包都可以在其字段中使用令牌：
```js
{
   "Format": "2.7.0",
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

## 进阶 API<a name="advanced-api"></a>
### 注意事项<a name="caveats"></a>

对于绝大多数情况，强烈推荐使用以上的基本 API，因为 Content Patcher 会自动帮您处理上下文更新和更改追踪等细节，这更容易排查错误，而且在游戏没有进行大型更新时绝对不会失效。

如果您确实非常需要更多控制，您可以使用高级 API，它几乎提供了与 Content Patcher 核心中的令牌相同的完全控制，但是：

* <strong>这是实验性功能。无法保证未来版本会向后兼容，也无法保证您在更改之前收到任何警告。</strong>
* <strong>这是底层功能。与以上的基本 API 不同，您必须考虑下面记录的令牌设计注意事项，而不像基础 API 那样自动为您处理这些问题。</strong>

### 概念<a name="concepts-1"></a>

当通过进阶 API 添加令牌时，为避免不必要的问题，您需要考虑这些因素：

<dl>
<dt>范围和数据顺序</dt>
<dd>

请参阅 [基本 API：概念](#concepts)。

</dd>

<dt>上下文更新</dt>
<dd>

令牌值可以视为缓存的游戏状态，在特定时刻更新（如每天开始）。所有令牌的集合为“上下文”；而“上下文更新”则是指 Content Patcher 刷新令牌、生成缓存、检查条件、重加载素材等等。

**令牌值不能在 `UpdateContext` 方法外部更改**。这样做可能会导致很多严重并不明确的问题，如图像错误和游戏彻底崩溃。

这并不排除动态计算某值的令牌（如`FileExists`），只要这种计算本身不变即可。如果一个令牌会在上下文更新之间动态更改（例如 `Random`），那么您必须将令牌值缓存以确保其不会更改。

</dd>

<dt>有界值</dt>
<dd>

如果一个令牌的值必定在某一范围之间，这个令牌是**有界**的。

这影响了两件事：
* 令牌的使用场景。例如，只有必定会返回一个数值的令牌才可在数值字段中使用，对于其他令牌，即使其当前返回的是一个数字，也不能在数值字段中使用。
* 当令牌作为 `When` 条件的一部分使用时的验证。例如，这将始终显示一个警告，因为它总是为假：
  ```js
  "When": {
     "Season": "totally not a valid season"
  }
   ```

有界与否可能受您的输入值影响。例如，您的令牌可能在有输入参数时是有界的，没有输入参数则是无限制的：
```js
"When": {
   "Relationship": "Abigail:Married", // 无界：可返回任意值（如自定义 NPC）
   "Relationship:Abigail": "Married"  // 有界：只可返回'married'或'dating'等提前定义的值
}
```

当注册令牌时，如果实现了 `HasBoundedValues` 或 `HasBoundedRangeValues`，则令牌是有界的。您还可以实现 `TryValidateValues` 以添加自定义验证，但这**不会**使令牌有界，因为 Content Patcher 无法获取可能的值列表。

</dd>
<dt>不可变值</dt>
<dd>

如果一个令牌的值在当前游戏实例的整个生命周期内（从启动到完全退出）都不会改变，则该令牌是**不可变**的。大多数令牌是可变的，意味着它们的值可能会改变。

不可变性启用了若干优化。例如，由于 Content Patcher 不需要更新该令牌，所以也不需要更新依赖该令牌的令牌或补丁。

</dd>

<dt>输入参数</dt>
<dd>

请参阅令牌指南中的[输入参数](author-guide/tokens.md#input-arguments)。

由于 SMAPI API 代理的限制，您的模组接收到的输入是一个格式化后的字符串，而不是解析的对象。输入参数中的令牌会被它们实际值替代。如果没有提供任何输入参数，令牌会收到 `null`。

</dd>
</dl>

### 添加令牌<a name="add-a-token-1"></a>

使用进阶 API 添加自定义令牌：

<ol>
<li>

创建一个令牌类，此类需要有在[这个文件里](../../Framework/Tokens/ValueProviders/ModConvention/ConventionDelegates.cs)所列出的方法的任意组合。您的类中的方法必须与名称、返回值和参数完全匹配。如果 Content Patcher 检测到非匹配或未识别的公共方法，它将报错并拒绝令牌。

例如，我们想要一个提供名字缩写的令牌（如`{{Initials:John Smith}}` → `JS`），或无参数时提供玩家的名字缩写。这是一个实现此功能的令牌类：
```c#
/// <summary>返回玩家的缩写或输入名称的缩写的令牌.</summary>
internal class InitialsToken
{
    /*********
    ** 字段
    *********/
    /// <summary>上一次更新时的玩家名</summary>
    private string PlayerName;


    /*********
    ** 公开方法
    *********/
    /****
    ** 元数据
    ****/
    /// <summary>查询此令牌是否允许输入参数(例如一个关系令牌要求NPC名).</summary>
    public bool AllowsInput()
    {
        return true;
    }

    /// <summary>令牌是否可以有多个值</summary>
    /// <param name="input">输入参数，若适用。</param>
    public bool CanHaveMultipleValues(string input = null)
    {
        return false;
    }

    /****
    ** 状态
    ****/
    /// <summary>上下文更新时，更新这个令牌的值。</summary>
    /// <returns>反馈令牌是否更改，若有更改可能会导致补丁更新</returns>
    public bool UpdateContext()
    {
        string oldName = this.PlayerName;
        this.PlayerName = Game1.player?.Name ?? SaveGame.loaded?.player?.Name; // 存档仍在加载时，令牌可能会更新
        return this.PlayerName != oldName;
    }

    /// <summary>查询令牌是否可使用</summary>
    public bool IsReady()
    {
        return this.PlayerName != null;
    }

    /// <summary>查询当前令牌值</summary>
    /// <param name="input">输入参数，若适用。</param>
    public IEnumerable<string> GetValues(string input)
    {
        // 获取名称
        string name = input ?? this.PlayerName;
        if (string.IsNullOrWhiteSpace(name))
            yield break;

        // 获取缩写
        yield return string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p[0]));
    }
}
```

</li>
<li>

接下来我们在 `GameLanched` 事件中通过 API 来注册这个令牌（请参阅[访问 API](#access-the-api)）：

```cs
api.RegisterToken(this.ModManifest, "Initials", new InitialsToken());
```

</li>
</ul>

现在，任何将您的模组列为依赖项的内容包都可以在其字段中使用该令牌：
```js
{
   "Format": "2.7.0",
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

## 参见<a name="see-also"></a>
* 其他操作和选项请参见[README](README.md)。
