← [README](README.md)

此文档帮助SMAPI模组作者在自己的模组中使用Content Patcher的令牌字符串。

**若需要添加给其他内容包使用的自定义令牌，请参阅[扩展性API](extensibility.md)。其他信息请参见[主README](README.md)**

**🌐 其他语言： [en (English)](../token-strings-api.md)。**

## 目录
* [概述](#overview)
* [访问API](#access-the-api)
* [解析令牌字符串](#parse-token-strings)
* [管理令牌字符串](#manage-token-strings)
* [注意事项](#caveats)
* [参见](#see-also)

## 概述<a name="overview"></a>
Content Patcher有一个令牌系统(author-guide/tokens.md)，允许内容包组成复杂并遵守上下文的字符串。
```js
"My favorite season is {{Season}}." // 如果有加载存档，{{Season}}会被当前季节所替换
```

其他SMAPI模组也可以使用此系统，通过API解析一个包含令牌的字符串来获得一个'托管令牌字符串'对象，之后使用此对象来管理令牌字符串。

## 访问API<a name="access-the-api"></a>

访问API的步骤为：

1. 将Content Patcher设为[`manifest.json`中的**必要**依赖](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest#Dependencies_属性):
   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "MinimumVersion": "2.7.0" }
   ]
   ```
2. 在你模组的`.csproj`里添加对Content Patcher DLL的引用。将此引用设置为`Private="False"`，确保它不被复制到你的模组文件夹中：
_译者注：这段似乎没更新，使用API只需将IContentPatcherAPI.cs和IManagedTokenString.cs复制到你自己模组中即可，_
   ```xml
   <ItemGroup>
     <Reference Include="ContentPatcher" HintPath="$(GameModsPath)\ContentPatcher\ContentPatcher.dll" Private="False" />
   </ItemGroup>
   ```
3. 在你的模组代码中（如[`GameLaunched`事件](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Events#GameLoop.GameLaunched)中，获取Content Patcher的API：
   ```c#
   var api = this.Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```

## 解析令牌字符串<a name="parse-token-strings"></a>
**注意:** 使用API前请阅读[_注意事项_](#caveats)。

有API以后你就可以解析令牌字符串了。

1. 创建一个需解析的字符串。这个字符串可包含[令牌](author-guide/tokens.md)例如：
   ```c#
   string rawTokenString = "The current time is {{Time}} on {{Season}} {{Day}}, year {{Year}}.";
   ```
2. 用API将条件解析为一个`IManagedTokenString`对象。其中的`formatVersion`对应[作者指南中描述的`Format`字段](author-guide.md#overview)，用于维持跟未来的Content Patcher版本兼容。

   ```c#
   var tokenString = api.ParseTokenString(
      manifest: this.ModManifest,
      rawValue: rawTokenString,
      formatVersion: new SemanticVersion("2.7.0")
   );
   ```
3. 从`Value`属性中获取结果，例如：
   ```cs
   tokenString.UpdateContext();
   string value = tokenString.Value; // 此变量值为 "The current time is 1430 on Spring 5, year 2."
   ```

如果你想使用其他SMAPI模组添加的令牌，你可以用`assumeModIds`来指定所安装的模组id。你不需要把自己ID和必要依赖的ID添加到`assumeModIds`中。
```c#
var tokenString = api.ParseTokenString(
   manifest: this.ModManifest,
   rawValue: rawTokenString,
   formatVersion: new SemanticVersion("2.7.0"),
   assumeModIds: new[] { "spacechase0.JsonAssets" }
);
```

## 管理令牌字符串<a name="manage-token-strings"></a>
你获取的`IManagedTokenString`对象提供一系列属性和方法，用于管理已解析的条件。你可以通过Visual Studio中的IntelliSense来查看可用的属性和方法。以下列出最有用的：

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

令牌字符串是否成功解析（不管它的令牌是否在当前范围中）。

</td>
</tr>
<tr>
<td><code>ValidationError</code></td>
<td><code>string</code></td>
<td>

当`IsValid`为否，描述令牌字符串为何解析失败。格式如下：
> 'seasonz' isn't a valid token name; must be one of &lt;token list&gt;

如果令牌字符串成功解析，这是`null`。

</td>
</tr>
<tr>
<td><code>IsReady</code></td>
<td><code>bool</code></td>
<td>

所需要的令牌是否都在当前上下文。例如，如果还没有加载存档，`{{Season}}`还没准备好，所以这是false。

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td><code>string?</code></td>
<td>

如果`IsReady`是true，解析后的字符串。

如果`IsReady`是false，`Value`则为null。

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
<td><code>UpdateContext</code></th>
<td><code>bool</code></td>
<td>

根据Content Patcher的当前上下文更新令牌字符串的上下文，并返回`Value`是否有更改。这个方法可随时使用，但它只有在Content Patcher的上下文更新后才有效果。

</td>
</tr>
</table>

## 注意事项<a name="caveats"></a>
<dl>
<dt>令牌字符串API不可立即使用。</dt>
<dd>

令牌字符串API在`GameLaunched`两tick（更新）后可使用。这和Content Patcher的生命周期有关：

1. `GameLaunched`: 其他模组可以注册自定义令牌。
2. `GameLaunched + 1 tick`: Content Patcher初始化令牌上下文（包括自定义令牌）。
3. `GameLaunched + 2 ticks`: 其他模组可使用令牌字符串API。

译：tick指游戏更新循环中的一刻

</dd>
<dt>令牌字符串应缓存。</dt>
<dd>

通过API运行条件解析令牌字符串是一个相对昂贵的操作。如果你需要经常使用某些令牌字符串，最好保存并重利用同一个`IManagedTokenString`对象。

</dd>
<dt>令牌字符串不会自动更新。</dt>
<dd>

当使用一个缓存的`IManagedTokenString`对象，你必须在需要时用`tokenString.UpdateContext()`来更新它。

注意，令牌字符串更新频率限于Content Patcher的[更新频率](author-guide.md#update-rate)。当你使用`tokenString.UpdateContext()`时，它会更新到Content Patcher的内置上下文最近一次更新的状态。

</dd>
<dt>令牌字符串自动处理本地多人双屏模式。</dt>
<dd>

比如说`Value`会返回对于 _当前屏幕_ 的上下文的值。
`UpdateContext`例外，这会更新所有屏幕的上下文。

</dd>
</dl>

## 参见<a name="see-also"></a>
* 其他信息详见[README](README.md)
