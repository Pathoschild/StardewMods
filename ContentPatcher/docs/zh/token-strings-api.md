← [README](README.md)

此文档帮助 SMAPI 模组作者在自己的模组中使用 Content Patcher 的令牌字符串。

**若需要添加给其他内容包使用的自定义令牌，请参阅[扩展性 API](extensibility.md)。其他信息请参见[主README](README.md)**

**🌐 其他语言：[en (English)](../token-strings-api.md)。**

## 目录
* [概述](#overview)
* [访问API](#access-the-api)
* [解析令牌字符串](#parse-token-strings)
* [管理令牌字符串](#manage-token-strings)
* [注意事项](#caveats)
* [参见](#see-also)

## 概述<a name="overview"></a>
Content Patcher 拥有一个[令牌系统](author-guide/tokens.md)，它允许内容包使用上下文值生成复杂的字符串。例如：
```js
"My favorite season is {{Season}}." // 如果有加载存档，{{Season}}会被当前季节所替换
```

其他 SMAPI 模组也可以使用此系统，通过 API 解析一个包含令牌的字符串来获得一个“托管令牌字符串”对象，之后使用此对象来管理令牌字符串。

## 访问 API<a name="access-the-api"></a>

访问 API 的步骤为：

1. 在 `manifest.json` 将 Content Patcher 设为[**必要依赖**](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest#Dependencies_属性)：
   ```js
   "Dependencies": [
      { "UniqueID": "Pathoschild.ContentPatcher", "MinimumVersion": "2.7.0" }
   ]
   ```
2. 在您模组的 `.csproj` 里添加对 `ContentPatcher.dll` 的引用。同时将此引用设置为 `Private="False"` 以确保它不被复制到您的模组文件夹中：
_译者注：这段似乎没更新，使用 API 只需将 `IContentPatcherAPI.cs` 和 `IManagedTokenString.cs` 复制到您自己模组中即可。_
   ```xml
   <ItemGroup>
     <Reference Include="ContentPatcher" HintPath="$(GameModsPath)\ContentPatcher\ContentPatcher.dll" Private="False" />
   </ItemGroup>
   ```
3. 在您的模组代码中（例如 [`GameLaunched` 事件](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Events#GameLoop.GameLaunched)）中，获取 Content Patcher 的 API：
   ```c#
   var api = this.Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
   ```

## 解析令牌字符串<a name="parse-token-strings"></a>
**注意**：在调用此 API 之前请参阅[注意事项](#caveats)。

现在您已经可以访问 API 和解析令牌字符串了。

1. 创建一个需解析的字符串。这个字符串可使用 Content Patcher 的功能，例如[令牌](author-guide/tokens.md)：
   ```c#
   string rawTokenString = "The current time is {{Time}} on {{Season}} {{Day}}, year {{Year}}.";
   ```
2. 调用 API 将条件解析为一个 `IManagedTokenString` 对象。其中的 `formatVersion` 对应[模组作者指南](author-guide.md#overview)中描述的 `Format` 字段，以确保与 Content Patcher 未来版本的兼容性。
   ```c#
   var tokenString = api.ParseTokenString(
      manifest: this.ModManifest,
      rawValue: rawTokenString,
      formatVersion: new SemanticVersion("2.7.0")
   );
   ```
3. 从 `Value` 属性中获取解析后的字符串结果，例如：
   ```cs
   tokenString.UpdateContext();
   string value = tokenString.Value; // 此变量值为 "The current time is 1430 on Spring 5, year 2."
   ```

如果您想使用由其他 SMAPI 模组添加的自定义令牌，您可以使用 `assumeModIds` 来指定一个模组的 ID 列表。您不需要把自己的模组 ID 和必要依赖的 ID 添加到 `assumeModIds` 中。
```c#
var tokenString = api.ParseTokenString(
   manifest: this.ModManifest,
   rawValue: rawTokenString,
   formatVersion: new SemanticVersion("2.7.0"),
   assumeModIds: new[] { "spacechase0.JsonAssets" }
);
```

## 管理令牌字符串<a name="manage-token-strings"></a>
您获取的 `IManagedTokenString` 对象提供一系列属性和方法，用于管理已解析的条件。您可以通过 Visual Studio 中的 IntelliSense 来查看可用的属性和方法。这里列出部分最有用的属性：

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

令牌字符串是否成功解析（不论其令牌当前是否处于作用域中）。

</td>
</tr>
<tr>
<td><code>ValidationError</code></td>
<td><code>string</code></td>
<td>

当 `IsValid` 为 false 时，描述令牌字符串解析失败原因的错误提示字符串。格式如下：
> 'seasonz' isn't a valid token name; must be one of &lt;token list&gt;

如果令牌字符串解析成功，则此属性将为 `null`。

</td>
</tr>
<tr>
<td><code>IsReady</code></td>
<td><code>bool</code></td>
<td>

令牌字符串内的所有令牌在当前上下文内是否都有效。例如，如果令牌字符串使用了 `{{Season}}` 且当前尚未加载存档，则此属性为 false。

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td><code>string?</code></td>
<td>

当 `IsReady` 为 true 时，将返回解析后的字符串；

当 `IsReady` 为 false，将返回 `null`。

</td>
</tr>
</table>

以及最有用的方法：

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

根据 Content Patcher 的当前上下文更新令牌字符串的上下文，并返回 `Value` 是否发生变化。这个方法可随时使用，但如果自您上次调用以来 Content Patcher 的上下文未发生任何变化，则不会有任何效果。

</td>
</tr>
</table>

## 注意事项<a name="caveats"></a>
<dl>
<dt>令牌字符串 API 不是立即可用的。</dt>
<dd>

令牌字符串 API 需要在 `GameLaunched` 事件发生后两刻（Tick）起才可使用。这和 Content Patcher 的生命周期有关：

1. `GameLaunched`：其他模组可以注册自定义令牌。
2. `GameLaunched + 1 刻`：Content Patcher 初始化令牌上下文（包括自定义令牌）。
3. `GameLaunched + 2 刻`：其他模组可使用令牌字符串 API。

注：此处的“刻”指游戏更新循环中的一刻

</dd>
<dt>令牌字符串应缓存。</dt>
<dd>

通过 API 运行条件解析令牌字符串是一个相对昂贵的操作。如果您需要频繁使用相同的令牌字符串，请尽可能保存并重复使用同一个 `IManagedTokenString` 实例。

</dd>
<dt>令牌字符串不会自动更新。</dt>
<dd>

当使用缓存的 `IManagedTokenString` 对象时，您必须在需要时使用 `tokenString.UpdateContext()` 来更新它。

注意，令牌字符串更新频率受限于 Content Patcher 的[更新频率](author-guide.md#update-rate)。当您使用 `tokenString.UpdateContext()` 时，它会更新到 Content Patcher 的内置上下文最近一次更新的状态。

</dd>
<dt>令牌字符串自动处理本地分屏游玩模式。</dt>
<dd>

例如，`Value` 会返回对于当前分屏下的上下文值。`UpdateContext` 则是例外，它会更新所有活动屏幕的上下文。

</dd>
</dl>

## 参见<a name="see-also"></a>
* 其他操作和选项请参见[README](README.md)。
