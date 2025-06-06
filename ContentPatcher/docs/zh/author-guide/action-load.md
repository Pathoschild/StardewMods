← [模组作者指南](../author-guide.md)

一个含有 **`"Action": "Load"`** 的补丁用你提供的文件替换整个素材。

**🌐 其他语言： [en (English)](../../author-guide/action-load.md)。**

## 目录
* [注意事项](#caveats)
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [参见](#see-also)

## 注意事项<a name="caveats"></a>
`Load`的功能很简单明了，但是每个素材只能被一个补丁替换。一个使用`Load`的内容包将不兼容另一个含有同`Target`的`Load`的内容包。而只使用`Edit*`的内容包不会有这个问题。

有需求时（如加载新素材）可以使用，但是有可能的话优先考虑[其他action](../author-guide.md#actions)。

## 用法<a name="usage"></a>
### 格式<a name="format"></a>
一个`Load`补丁是一个`Changes`以下的含有这些字段的模型：

<dl>
<dt>必填字段：</dt>
<dd>

字段       | 用途
--------- | -------
`Action`  | 要进行的更改类型。此操作类型设置为`Load`。
`Target`  | 需编辑的[游戏素材名](../author-guide.md#what-is-an-asset)（或多个由逗号分隔的素材名），比如`Portraits/Abigail`。该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。
`FromFile` | 内容包文件夹中需引用的`.json`文件的相对路径，或多个用逗号分割的的相对路径。这可以是`.json`（数据），`.png`（图片），`.tbin`或`.tmx`（地图），或`.xnb`文件。该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。

</dd>
<dt>可选字段：</dt>
<dd>

字段       | 用途
--------- | -------
`When`    | _（可选）_ 当给定的[条件](../author-guide.md#conditions)匹配时才应用这个内容补丁。
`LogName`     | _（可选）_ 在日志中显示的补丁名称。这有助于查找错误。如果省略，则默认为类似`Load Data/Achievements`的名称。
`Update`      | _（可选）_ 补丁字段多久更新一次。详见[更新速率](../author-guide.md#update-rate)。
`LocalTokens` | _（可选）_ 可在本补丁字段中使用的[局部令牌](../author-guide/tokens.md#local-tokens)。

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

_（可选）_ 当多个补丁编辑同一数据素材时，此字段控制它们应用的顺序。默认值为`Exclusive`。

_备注：以下描述的“补丁”指同一`Target`的`Load`补丁。_

可用的值有：

* `Low`（低），`Medium`（中），或 `High`（高）：只有最高优先级的补丁会生效。如果多个补丁的优先级一致，列表里靠前的补丁生效。
* `Exclusive`（独占）：如果某一个补丁使用此优先级，那其他所有补丁都无效。如果多个补丁使用此优先级， **所有补丁都不生效** ，并且显示错误信息。

  这个优先级会大幅度降低一个内容包的兼容性。有可能的话，尽量不要使用`Exclusive`。但因为Content Patcher无法知道如果加载另一个补丁后你的内容包是否能够正常运行，所以`Exclusive`是默认值。

如果需要更具体的顺序，可以使用简单的偏移量，如`"High + 2"`或`"Medium - 10"`。
默认值为-1000 （`Low`），0（`Medium`）和1000（`High`）。通常不需要分这么细，因为同一优先级的补丁会按`Changes`顺序生效。

此字段 _不_ 支持令牌，不区分大小写。

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

 _（可选）_ 素材名称中要匹配的地区代码，比如设置`"TargetLocale": "fr-FR"`只编辑法语形式的素材（比如`Data/Achievements.fr-FR`）。可以为空，只有只编辑没有地域区分的基本素材。

如果省略，它将应用于所有素材，不管有没有本地化。

</td>
</table>
</dd>
</dl>

### 示例<a name="examples"></a>
此补丁将阿比盖尔的肖像替换为你提供的图片（详见[NPC模组](https://zh.stardewvalleywiki.com/模组:NPC数据)）：
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

同一内容包可以有多个`Load`，但是同个`Target`只有一个补丁会生效：

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

你可以用[令牌](../author-guide.md#tokens)，如`{{TargetWithoutPath}}`，来同时`Load`多个文件。

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

你可以用`Priority`实现非必需的`Load`（例如，当另一个模块首先加载它时你的内容包仍然可以正常运行）。
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
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
