← [模组作者指南](../author-guide.md)

带有 **`"Action": "Load"`** 的补丁使用您提供的文件替换整个素材。

**🌐 其他语言：[en (English)](../../author-guide/action-load.md)。**

## 目录
* [注意事项](#caveats)
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [参见](#see-also)

## 注意事项<a name="caveats"></a>
`Load` 的功能很简单明了，但是每个素材只能被一个补丁替换。因此您的内容包无法兼容其他 `Load` 了同一素材的内容包（只使用了 `Edit*` 的内容包不会出现这个问题）。

您应当仅在必要的情况下使用 `Load`，对于其他情况请优先考虑使用[其他操作](../author-guide.md#actions)。

## 用法<a name="usage"></a>
### 格式<a name="format"></a>
一个 `Load` 补丁由 `Changes`（请参阅下文[示例](#examples)）下的一个模型组成，包含以下字段。

<dl>
<dt>必填字段：</dt>
<dd>

字段       | 用途
--------- | -------
`Action`  | 要进行的更改类型。此操作类型设置为 `Load`。
`Target`  | 要替换的目标[游戏素材名称](../author-guide.md#what-is-an-asset)（或多个由逗号分隔的素材名），例如`Portraits/Abigail`。该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。
`FromFile` | 内容包文件夹中用于替换目标的文件的相对路径（例如 `assets/dinosaur.png`），或多个逗号分隔的路径。这可以是 `.json`（数据）、`.png`（图片）、`.tbin`或`.tmx`（地图）以及 `.xnb` 文件。该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。

</dd>
<dt>可选字段：</dt>
<dd>

字段       | 用途
--------- | -------
`When`        | （可选）仅在给定的[条件](../author-guide.md#conditions)匹配时应用这个内容补丁。
`LogName`     | （可选）在日志中显示的补丁名称。这有助于查找错误。如果省略，则默认为类似 `Load Data/Achievements` 的名字。
`Update`      | （可选）补丁字段的更新频率。请参阅[补丁更新频率](../author-guide.md#update-rate)。
`LocalTokens` | （可选）一组仅在此补丁中生效的[局部令牌](../author-guide/tokens.md#local-tokens)。

</dd>
<dt>进阶字段：</dt>
<dd>

<table>
  <tr>
    <td>字段</td>
    <td>用途</td>
  </tr>
  <tr>
  <td><code>Priority</code></td>
    <td>

（可选）当多个补丁的 `Target` 相同时，此字段决定了哪个补丁将被应用。默认值为 `Exclusive`。

可用的值有：

* `Low`（低）、`Medium`（中）或 `High`（高）：只有最高优先级的补丁会被应用。如果多个补丁的优先级一致，则按加载顺序 + 补丁顺序列表中靠前的补丁生效。
* `Exclusive`（独占）：如果某一个补丁使用此优先级，那么其他所有补丁都无效。如果多个补丁使用此优先级，**所有补丁都不生效**，并且显示错误信息。

  在条件允许的情况下请尽量避免使用 `Exclusive`。因为该优先级会显著降低内容包的兼容性。但因为 Content Patcher 无法知道如果加载另一个补丁后您的内容包是否能够正常运行，所以 `Exclusive` 是默认值。

如果需要更具体的顺序，可以使用简单的偏移量，如 `"High + 2"` 或者 `"Medium - 10"`。默认值为 -1000（`Low`），0（`Medium`）和 1000（`High`）。通常不需要这样做，因为相同优先级的补丁已经按照您在内容包中列出的顺序排序。

此字段**不支持**令牌，不区分大小写。

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

（可选）素材名称中要匹配的地区代码，例如设置 `"TargetLocale": "fr-FR"` 将会只编辑法语的素材（例如 `Data/Achievements.fr-FR`）。可以为空，只有只编辑没有地域区分的基本素材。

如果省略，则将应用于所有素材，不论其是否存在本地化。

</td>
</table>
</dd>
</dl>

### 示例<a name="examples"></a>
此补丁将阿比盖尔的肖像替换为您提供的图片（请参阅[模组：NPC 数据](https://zh.stardewvalleywiki.com/模组:NPC数据)）：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail.png"
        },
    ]
}
```

同一内容包可以有多个 `Load` 补丁，但是对于同一个 `Target`，只有一个补丁会生效：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail.png"
        },
        {
            "Action": "Load",
            "Target": "Portraits/Penny",
            "FromFile": "assets/penny.png"
        },
    ]
}
```

您可以使用用[令牌](../author-guide.md#tokens)，如 `{{TargetWithoutPath}}`，来同时 `Load` 多个文件。
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail, Portraits/Penny",
            "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png, assets/Penny.png
        },
    ]
}
```

您可以用 `Priority` 实现非必需的 `Load`（例如，当另一个模组先于您的内容包 `Load` 了某素材时，您的内容包仍然可以正常运行）。
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Data/Events/AdventureGuild",
            "FromFile": "assets/empty-event-file.json",
            "Priority": "Low"
        }
    ]
}
```

## 参见<a name="see-also"></a>
* 其他操作和选项请参见[模组作者指南](../author-guide.md)。
