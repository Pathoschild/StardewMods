← [README](README.md)

此文档帮助SMAPI模组作者在自己模组中使用Content Patcher的条件系统。

**如果你想添加新的令牌给内容包使用，请参考[拓展API](extensibility.md)，其他信息详见[主README](README.md)**

**🌐 其他语言： [en (English)](../conditions-api.md)。**

## 目录
* [概述](#overview)
* [访问API](#access-the-api)
* [解析条件](#parse-conditions)
* [管理条件](#manage-conditions)
* [注意事项](#caveats)
* [参见](#see-also)

## 概述<a name="overview"></a>

Content Patcher有一个[条件系统](author-guide/tokens.md)。内容包作者可以使用各种根据情况改变的值来实现条件化的更改。比如说
```js
"When": {
   "PlayerGender": "male",             // 玩家为男性
   "Relationship: Abigail": "Married", // 玩家和阿比盖尔结婚了
   "HavingChild": "{{spouse}}",        // 阿比盖尔准备生孩子
   "Season": "Winter"                  // 现在是冬天
}
```

其他SMAPI模组也可以用这个系统。使用方式为创建一个代表需检查的条件的字典，然后通过API来获得一个'托管条件'的对象，然后用这个对象来管理条件。

## 访问API<a name="access-the-api"></a>

访问API的步骤为：

1. 将Content Patcher设为[`manifest.json`中的**必要**依赖](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest#Dependencies_属性):
   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "MinimumVersion": "2.7.0" }
   ]
   ```
2. 在你模组的`.csproj`里添加对Content Patcher DLL的引用。将此引用设置为`Private="False"`，确保它不被复制到你的模组文件夹中：
_译者注：这段似乎没更新，使用API只需将IContentPatcherAPI.cs和IManagedConditions.cs复制到你自己模组中即可_
   ```xml
   <ItemGroup>
     <Reference Include="ContentPatcher" HintPath="$(GameModsPath)\ContentPatcher\ContentPatcher.dll" Private="False" />
   </ItemGroup>
   ```
3. 在你的模组代码中（如[`GameLaunched`事件](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Events#GameLoop.GameLaunched)中，获取Content Patcher的API：
   ```c#
   var api = this.Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```

## 解析条件<a name="parse-conditions"></a>
**注意:** 使用API前请阅读[_注意事项_](#caveats)。

获取API以后你可以解析条件。

1. 创建一个`Dictionary<string, string>`对象，代表你想检查的条件。这里可以使用Content Patcher功能，如令牌。假设你有这些条件（格式详见[条件文档](author-guide/tokens.md)）：
   ```c#
   var rawConditions = new Dictionary<string, string>
   {
      ["PlayerGender"] = "male",             // 玩家为男性
      ["Relationship: Abigail"] = "Married", // 玩家和阿比盖尔结婚了
      ["HavingChild"] = "{{spouse}}",        // 阿比盖尔准备生孩子
      ["Season"] = "Winter"                  // 现在是冬天
   };
   ```

2. 用API将条件解析为一个`IManagedConditions`对象。其中的`formatVersion`对应[作者指南中描述的`Format`字段](author-guide.md#overview)，用于维持跟未来的Content Patcher版本兼容。

   ```c#
   var conditions = api.ParseConditions(
      manifest: this.ModManifest,
      raw: rawConditions,
      formatVersion: new SemanticVersion("1.20.0")
   );
   ```

3. 从`IsMatch`属性中获取结果，例如：
   ```cs
   conditions.UpdateContext();
   if (conditions.IsMatch)
      ...
   ```

如果你想使用其他SMAPI模组添加的令牌，你可以用`assumeModIds`来指定所安装的模组id。你不需要把自己ID，必要依赖的ID,和任何在条件字段中以`HasMod`指定的Id添加到`assumeModIds`中。
```c#
var conditions = api.ParseConditions(
   manifest: this.ModManifest,
   raw: rawConditions,
   formatVersion: new SemanticVersion("1.20.0"),
   assumeModIds: new[] { "spacechase0.JsonAssets" }
);
```

## 管理条件<a name="manage-conditions"></a>

你获取的`IManagedConditions`对象提供一系列属性和方法，用于管理已解析的条件。你可以通过Visual Studio中的IntelliSense来查看可用的属性和方法。以下列出最有用的：

<table>
<tr>
<th>属性</th>
<th>类型</th>
<th>描述</th>
</tr>

<tr>
<td><code>IsValid</code></th>
<td><code>bool</code></td>
<td>

条件是否成功解析（不管它是否在当前范围中）。

</td>
</tr>
<tr>
<td><code>ValidationError</code></td>
<td><code>string</code></td>
<td>

当`IsValid`为否，描述条件为何解析失败。格式如下：
> 'seasonz' isn't a valid token name; must be one of &lt;token list&gt;

如果条件成功解析，这是`null`。

</td>
</tr>
<tr>
<td><code>IsReady</code></td>
<td><code>bool</code></td>
<td>

所需要的令牌是否都在当前上下文。例如，如果还没有加载存档，`Season`还没准备好，所以这是false。

</td>
</tr>
<tr>
<td><code>IsMatch</code></td>
<td><code>bool</code></td>
<td>

`IsReady`是否为true，并且所有条件在当前上下文都成立。

如果没有任何条件（你解析了一个空字典），这永远为true。

</td>
</tr>
<tr>
<td><code>IsMutable</code></td>
<td><code>bool</code></td>
<td>

`IsMatch`是否会根据当前上下文改变。例如，`Season`可改变，因为它对标游戏内的季节。`HasMod`不可改变，因为游戏加载后模组列表不会改变。

</td>
</tr>
</table>

和方法：

<table>
<tr>
<th>方法</th>
<th>类型</th>
<th>描述</th>
</tr>

<tr>
<td><code>GetReasonNotMatched</code></th>
<td><code>string</code></td>
<td>

如果`IsMatch`为否，分析条件和上下文并返回一个人类可读的原因，描述为何此条件不成立。例如：
> conditions don't match: season

如果条件成立，这是`null`。

</td>
</tr>
<tr>
<td><code>UpdateContext</code></th>
<td><code>bool</code></td>
<td>

根据Content Patcher的当前上下文更新条件的上下文，并返回`IsMatch`是否有更改。这个方法可随时使用，但它只有在Content Patcher的上下文更新后才有效果。

</td>
</tr>
</table>

## 注意事项<a name="caveats"></a>
<dl>
<dt>条件API不可立即使用。</dt>
<dd>


条件API在`GameLaunched`两tick（更新）后可使用。这和Content Patcher的生命周期有关：

1. `GameLaunched`: 其他模组可以注册自定义令牌。
2. `GameLaunched + 1 tick`: Content Patcher初始化令牌上下文（包括自定义令牌）。
3. `GameLaunched + 2 ticks`: 其他模组可使用条件API。

译：tick指游戏更新循环中的一刻

</dd>
<dt>条件应缓存。</dt>
<dd>

通过API解析条件是一个相对昂贵的操作。如果你需要经常使用某些条件，最好保存并重利用同一个`IManagedConditions`对象。

</dd>
<dt>条件不会自动更新。</dt>
<dd>

当使用一个缓存的`IManagedConditions`对象，你必须在需要时用`conditions.UpdateContext()`来更新它。

注意，条件更新频率限于Content Patcher的[更新频率](author-guide.md#update-rate)。当你使用`conditions.UpdateContext()`时，它会更新到Content Patcher的内置上下文最近一次更新的状态。

</dd>
<dt>条件自动处理本地多人双屏模式。</dt>
<dd>

比如说`IsMatch`会返回对于 _当前屏幕_ 的上下文的值。
`UpdateContext`例外，这会更新所有屏幕的上下文。

</dd>
</dl>

## 参见<a name="see-also"></a>
* 其他信息详见[README](README.md)
