<<<<<<< HEAD
﻿← [author guide](../author-guide.md)

A patch with **`"Action": "Load"`** replaces an entire asset with your own version.

## Contents
* [Caveats](#caveats)
* [Usage](#usage)
  * [Format](#format)
  * [Examples](#examples)
* [See also](#see-also)

## Caveats
`Load` is very simple, but each asset can only be replaced by one patch, so your content pack won't
be compatible with other content packs that `Load` the same one. (It'll work fine with content
packs that only edit the file, though.)

It's fine to use if you really need it, but consider using one of the [other action
types](../author-guide.md#actions) if possible.

## Usage
### Format
A `Load` patch consists of a model under `Changes` (see examples below) with these fields:

<dl>
<dt>Required fields:</dt>
=======
﻿← [模组作者指南](../author-guide.md)

一个含有**`"Action": "Load"`**的补丁用你提供的文件替换整个素材。


## 内容
* [注意事项](#caveats)
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [参见](#see-also)

## 注意事项<a name="caveats"></a>
`Load`的功能很简单明了，但是每个素材只能被一个补丁替换。一个使用`Load`的内容包将不兼容另一个含有同`Target`的`Load`的内容包。而只使用`Edit`的内容包不会有这个问题。

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
`Target`  | 需编辑的[游戏素材名](../author-guide.md#what-is-an-asset)（或多个由逗号分隔的素材名），比如`Portraits/Abigail`。该字段支持[tokens](../author-guide.md#tokens)，不区分大小写。
`FromFile` | 内容包文件夹中需引用的`.json`文件的相对路径，或多个用逗号分割的的相对路径。这可以是`.json`（数据），`.png`（图片），`.tbin`或`.tmx`（地图），或`.xnb`文件。该字段支持[tokens](../author-guide.md#tokens)，不区分大小写。

</dd>
<dt>可选字段：</dt>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
<dd>

field     | purpose
--------- | -------
<<<<<<< HEAD
`Action`  | The kind of change to make. Set to `Load` for this action type.
`Target`  | The [game asset name](../author-guide.md#what-is-an-asset) to replace (or multiple comma-delimited asset names), like `Portraits/Abigail`. This field supports [tokens](../author-guide.md#tokens), and capitalisation doesn't matter.
`FromFile` | The relative file path in your content pack folder to load instead (like `assets/dinosaur.png`). This can be a `.json` (data), `.png` (image), `.tbin` or `.tmx` (map), or `.xnb` file. This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.

</dd>
<dt>Optional fields:</dt>
<dd>

field     | purpose
--------- | -------
`When`    | _(optional)_ Only apply the patch if the given [conditions](../author-guide.md#conditions) match.
`LogName` | _(optional)_ A name for this patch to show in log messages. This can be useful for understanding errors. If omitted, it defaults to a name like `Load Data/Achievements`.
`Update`  | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.
`LocalTokens` | _(Optional)_ A set of [local tokens](../author-guide/tokens.md#local-tokens) which can be used within this patch's field.

</dd>
<dt>Advanced fields:</dt>
=======
`When`    | _（可选）_ 当给定的[条件](../author-guide.md#conditions)匹配时才应用这个内容补丁。
`LogName`     | _（可选）_ 在日志中显示的补丁名称。这有助于查找错误。如果省略，则默认为类似`Include data/patches.json`的名称。
`Update`      | _（可选）_ 补丁字段多久更新一次。详见[更新速率](../author-guide.md#update-rate)。
`LocalTokens` | _（可选）_ 可在本补丁字段中使用的[局部tokens](../author-guide/tokens.md#local-tokens)。所有被引用的补丁都会继承这些tokens。

</dd>
<dt>进阶字段：</dt>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
<dd>

<table>
  <tr>
<<<<<<< HEAD
    <th>field</th>
    <th>purpose</th>
  </tr>
  <tr>
    <td><code>Priority</code></td>
    <td>

_(optional)_ When multiple patches or mods load the same asset, the priority which decides which
one is applied. Default `Exclusive`.

The possible values are:

* `Low`, `Medium`, or `High`: the highest-priority patch is applied. If multiple patches have the
  same priority, the first one in the list (by load order + patch order) is applied.
* `Exclusive`: all or nothing. If one patch uses it, it's applied and all other load patches are
  ignored. If multiple patches use it, then _no patches_ are applied and an error message is shown.

  Avoid using `Exclusive` when possible, since it significantly reduces mod compatibility. This is
  only the default value because Content Patcher can't know whether your content pack will still
  work if another patch is loaded instead.

If you need very specific ordering, you can use a simple offset like `"High + 2"` or `"Medium - 10"`.
The default levels are -1000 (low), 0 (medium), and 1000 (high). This isn't usually needed though,
since patches with the same priority are already sorted by the order they're listed in your content
pack.

This field does _not_ support tokens, and capitalization doesn't matter.
=======
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
* `Exclusive`（独占）：如果某一个补丁使用此优先级，那其他所有补丁都无效。如果多个补丁使用此优先级，**所有补丁都不生效**，并且显示错误信息。

  这个优先级会大幅度降低一个内容包的兼容性。有可能的话，尽量不要使用`Exclusive`。但因为Content Patcher无法知道如果加载另一个补丁后你的内容包是否能够正常运行，所以`Exclusive`是默认值。

如果需要更具体的顺序，可以使用简单的偏移量，如`"High + 2"`或`"Medium - 10"`。
默认值为-1000 （`Low`），0（`Medium`）和1000（`High`）。通常不需要分这么细，因为同一优先级的补丁会按`Changes`顺序生效。

此字段 _不_ 支持tokens，不区分大小写。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

<<<<<<< HEAD
_(optional)_ The locale code to match in the asset name. For example, setting `"TargetLocale": "fr-FR"`
will only load the French localized form of the asset (e.g. `Data/Achievements.fr-FR`). This can be
an empty string to only load the base unlocalized asset.

If omitted, it's applied to all localized and unlocalized variants of the asset.
=======
 _（可选）_ 素材名称中要匹配的地区代码，比如设置`"TargetLocale": "fr-FR"`只编辑法语形式的素材（比如`Data/Achievements.fr-FR`）。可以为空，只有只编辑没有地域区分的基本素材。

如果省略，它将应用于所有素材，不管有没有本地化。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</table>
</dd>
</dl>

<<<<<<< HEAD
### Examples
This replaces Abigail's portraits with your own image (see [NPC modding](https://stardewvalleywiki.com/Modding:NPC_data)):
```js
{
    "Format": "2.5.0",
=======
### 示例<a name="examples"></a>
此补丁将阿比盖尔的肖像替换为你提供的图片（详见[NPC模组](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:NPC%E6%95%B0%E6%8D%AE)）：
```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail.png"
        },
    ]
}
```

<<<<<<< HEAD
You can list any number of load patches, as long as each asset is only loaded by one patch:

```js
{
    "Format": "2.5.0",
=======
同一内容包可以有多个`Load`，但是同个`Target`只有一个补丁会生效：

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
You can also use [tokens](../author-guide.md#tokens) like `{{TargetWithoutPath}}` to edit several
files at once:

```js
{
    "Format": "2.5.0",
=======
你可以用[tokens](../author-guide.md#tokens)，如`{{TargetWithoutPath}}`，来同时`Load`多个文件。

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail, Portraits/Penny",
            "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png, assets/Penny.png
        },
    ]
}
```

<<<<<<< HEAD
You can use `Priority` to have an optional load (e.g. if it'll still work when another mod loads it first):
```js
{
    "Format": "2.5.0",
=======
你可以用`Priority`实现非必需的`Load`（例如，当另一个模块首先加载它时你的内容包仍然可以正常运行）。
```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
## See also
* [Author guide](../author-guide.md) for other actions and options
=======
## 参见<a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
