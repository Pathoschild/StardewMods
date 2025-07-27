← [模组作者指南](../author-guide.md)

带有 **`"Action": "Include"`** 的补丁可以让您从另外一个 JSON 文件里加载其他补丁。

**🌐 其他语言：[en (English)](../../author-guide/action-include.md)。**

## 目录
* [用法](#usage)
  * [概述](#overview)
  * [格式](#format)
  * [示例](#examples)
* [常见问题](#faqs)
  * [可以 Include 的文件是否有限制？](#are-there-limits-to-the-files-i-can-include)
  * [可以使用 Include 加载非补丁文件吗？](#can-i-load-non-patches-using-include)
* [参见](#see-also)

## 用法<a name="usage"></a>
### 概述<a name="overview"></a>
您不必将模组所有补丁都定义在一个 `content.json`，您可以在其他文件里定义补丁，然后在 `content.json` 中使用用 `Include` 来引用其他文件的补丁。这些引用的补丁会像您自己这些补丁复制到 `Include` 的位置那样工作。例如，它们可以使用与 `content.json` 中相同的功能（如 [Tokens 和条件](../author-guide.md#tokens)），并且任何本地文件路径仍然相对于 `content.json`。

被引用的文件必须是包含 `"Changes"` 字段的 `.json` 文件
```js
{
    // 不能有Format
    "Changes": [
        /* 补丁放这里 */
    ]
}
```

### 格式<a name="format"></a>
一个 `Include` 补丁由 `Changes`（请参阅下文[示例](#examples)）下的一个模型组成，包含以下字段。

<dl>
<dt>必填字段：</dt>
<dd>

字段       | 用途
--------- | -------
`Action`  | 要进行的更改类型。此操作类型设置为 `Include`。
`FromFile` | 包含补丁的 `.json` 文件的相对路径，或多个逗号分隔的路径。此路径始终相对于您的 `content.json`（即使包含文件包含着另一个文件）。

</td>
</tr>

</dd>
<dt>可选字段：</dt>
<dd>

类型       | 作用
--------- | -------
`When`        | （可选）仅在给定的[条件](../author-guide.md#conditions)匹配时应用这个内容补丁。
`LogName`     | （可选）在日志中显示的补丁名称。这有助于查找错误。如果省略，则默认为类似 `Include patches/data.json` 的名字。
`Update`      | （可选）补丁字段的更新频率。请参阅[补丁更新频率](../author-guide.md#update-rate)。
`LocalTokens` | （可选）一组仅在此补丁中生效的[局部令牌](../author-guide/tokens.md#local-tokens)。所有被引用的补丁都会继承这些令牌。

</dd>
</dl>

### 示例<a name="examples"></a>
最简单的情况是使用 `Include` 把您的补丁分类到子文件里：
```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "Include",
         "FromFile": "assets/John NPC.json, assets/Jane NPC.json"
      },
   ]
}
```

您也可以将其与令牌和条件结合起来，从而动态地加载文件：
```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "Include",
         "FromFile": "assets/John_{{season}}.json",
         "When": {
            "EnableJohn": true
         }
      }
   ]
}
```

## 常见问题<a name="faqs"></a>
### 可以Include的文件是否有限制？<a name="are-there-limits-to-the-files-i-can-include"></a>
没有。您可以从任意数量的文件中 `Include` 补丁，这些补丁里也可以使用 `Include` 补丁来加载其他文件，您还可以多次 `Include` 同一个文件。在每种情况下，它的工作原理都和您将所有补丁粘贴到“content.json”中的该位置一样。

唯一的限制是您不可以有循环的 `Include`（例如文件 A 引用文件 B，而文件 B 也引用文件 A）。

### 可以使用 Include 加载非补丁文件吗？<a name="can-i-load-non-patches-using-include"></a>
不可以。`Include` 的文件只能包含一个 `Changes` 字段。如果您试图添加 `ConfigSchema`、`CustomLocations`、`DynamicTokens` 字段，那么 Content Patcher 会报错。

## 参见<a name="see-also"></a>
* 其他操作和选项请参见[模组作者指南](../author-guide.md)。
