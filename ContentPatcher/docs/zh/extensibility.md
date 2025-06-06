← [README](README.md)

此文档帮助SMAPI模组作者延伸Content Patcher的功能。

**如果你想在你的模组中使用条件，详见[条件API](conditions-api.md)。其他信息请参见[主README](README.md)**

**🌐 其他语言： [en (English)](../extensibility.md)。**

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

你的SMAPI模组可以使用Content Patcher的[模组API](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Integrations#模组API)来添加自定义令牌。自定义令牌的前缀为提供它们的模组，如`your-mod-id/SomeTokenName`。

你可以用这两种API：

* 大部分模组推荐使用 **基本API**。你可以在不了解Content Patcher内部结构的情况下创建令牌；Content Patcher会自动管理各种细节，而且你的令牌和未来版本更兼容。

* **进阶API**提供更多控制选项。但是你的令牌会更加复杂，你需要了解Content Patcher的内在运行逻辑，而且你的令牌可能会在未来Content Patcher内在运行逻辑更改时失效。进阶API强烈不推荐使用，除非基本API不可用。

你可以同时使用基本和进阶API。

## 访问API<a name="access-the-api"></a>

访问API的步骤为：

1. 将Content Patcher设为[`manifest.json`中的依赖](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest#Dependencies_属性):

   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "IsRequired": false }
   ]
   ```

2. 把[`IContentPatcherAPI`](../../IContentPatcherAPI.cs) 复制到你模组里并删除**任何你不需要用的方法，为了兼容未来更改**.
3. 在你的模组代码中（如[`GameLaunched`事件](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Events#GameLoop.GameLaunched)中，获取Content Patcher的API：
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

你返回的顺序会影响`valueAt`。推荐使用对于你令牌来说最有意义的顺序，因为内容包作者无法改变此顺序。大部分令牌使用字母数字顺序即可（如 `.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)`）。

</dd>
</dl>

### 添加令牌<a name="add-a-token"></a>

你可以在`GameLaunched`事件中通过`RegisterToken`添加简单令牌（详见 _[访问API](#access-the-api)_ ）。例如，以下代码创建一个`{{your-mod-id/PlayerName}}`令牌代表当前玩家名：

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
`mod`      | `IManifest` | 提供此令牌模组的manifest，你可以直接使用ModEntry的`this.ModManifest`
`name`     | `string` | 令牌名称，这个名字只需要在你自己模组中独一。Content Patcher会自动给这个名字添加前缀。以上例子的`PlayerName`会变成`your-mod-id/PlayerName`。
`getValue` | `Func<IEnumerable<string>>` | 一个函数，反馈当前值。如果这反馈null或空列表，这个令牌会以不存在于当前范围来处理，任何使用它的补丁和动态令牌也会被禁用。

现在，任何将你的模组列为依赖的内容包都可以在其字段中使用令牌：
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

## 进阶API<a name="advanced-api"></a>
### 注意事项<a name="caveats"></a>

大部分时候，强烈推荐使用以上的 _基本API_，因为Content Patcher会帮你处理上下文更新和更改追踪等细节，这方便排查错误，而且无大型更新时绝对不会失效。

如果你真的非常需要更多控制，你可以使用进阶API添加令牌（基本和Content Patcher自带的令牌一致），但是：

* <strong>这是实验性功能。无法保证未来版本会倒退兼容，或在更改之前会收到任何警告。</strong>
* <strong>这是底层功能。与以上的基本API不同，你必须考虑下面记录的令牌设计注意事项。</strong>

### 概念<a name="concepts-1"></a>

当通过进阶API添加令牌时，你需要考虑这些因素来避免问题：

<dl>
<dt>范围和数据顺序</dt>
<dd>

详见 [_基本API：概念_](#concepts)。

</dd>

<dt>上下文更新</dt>
<dd>

令牌值可以视为缓存的游戏状态，在特定时刻更新（如每天开始）。所有令牌的集合为“上下文”；而“上下文更新”是Content Patcher刷新令牌，生成缓存，检查条件，重加载素材，等等。

**令牌值不能在`UpdateContext`以外更改**。这样做可能会导致很多严重并不明确的问题，如图像错误和游戏彻底崩溃。

这并不等于令牌不可有动态计算出的值（如`FileExists`）只要这个计算本身不改变。如果一个令牌会在上下文更新之间改变数值，你必须将令牌值缓存。

</dd>

<dt>有界值</dt>
<dd>

如果一个令牌的值必定在某一范围之间，这个令牌是 _有界_ 的。

这影响了两件事：
* 此令牌在哪里可使用。例如，只有必定会反馈数值的令牌才可在数值字段中使用，其他令牌就算现在是 _数字_ 时也不能在数值字段中使用。
* 令牌在`When`条件中的验证。例如，这个条件永远为否，所以会显示一个警告。
  ```js
  "When": {
     "Season": "totally not a valid season"
  }
   ```

有界性按参数分开计算。例如，你的令牌可能在有参数时为 _有界_ ，而没参数时为无界。
```js
"When": {
   "Relationship": "Abigail:Married", // 无界：可返回任意值（如自定义NPC）
   "Relationship:Abigail": "Married"  // 有界：只可返回'married'或'dating'等提前定义的值
}
```


当你添加令牌时，你实现`HasBoundedValues`，将令牌变成有界。你还可以实现`TryValidateValues`来无界令牌的实现自定义验证机制。

</dd>
<dt>不变值</dt>
<dd>

如果一个令牌的值在同一参数在整个游戏行程（从启动到退出）必定不变，这个令牌是 _不变_ 的。大部分令牌为可变的，意味它们的值会根据状态改变。

不变特性允许一些优化。例如，Content Patcher不需要更新一个令牌，所以也不需要更新依赖这个令牌的令牌/补丁。

</dd>

<dt>输入参数</dt>
<dd>

详见 [令牌指南中的 _输入参数_](author-guide/tokens.md#input-arguments)。

由于SMAPI API代理的限制，你的模组受到的输入是一个归一化后的字符串，格式和令牌指南上一致，而不是已解析后的令牌对象。输入参数中的令牌会被它们实际值替代。如果没有任何输入值，令牌会收到`null`。

</dd>
</dl>

### 添加令牌<a name="add-a-token-1"></a>

用进阶API添加自定义令牌：

<ol>
<li>

创建一个令牌类，此类需要有在这个文件里[所列出的方法的任意组合](../../Framework/Tokens/ValueProviders/ModConvention/ConventionDelegates.cs)
你的类中的方法必须有完全一样的名字，返回类，和参数。Content Patcher发现不对应的公开方法时会报错并退回此令牌。

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

接下来我们在`GameLanched`事件中通过API来添加这个令牌（详见[访问API](#access-the-api)）：

```cs
api.RegisterToken(this.ModManifest, "Initials", new InitialsToken());
```

</li>
</ul>

现在，任何将您的mod列为依赖项列出的内容包都可以在其字段中使用令牌：
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
* 其他信息详见[README](README.md)
