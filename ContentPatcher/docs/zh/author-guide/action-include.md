← [模组作者指南](../author-guide.md)

一个含有 **`"Action": "Include"`** 的补丁会从另外一个JSON文件里加载更多补丁。

**🌐 其他语言： [en (English)](../../author-guide/action-include.md)。**

## 目录
* [用法](#usage)
  * [概述](#overview)
  * [格式](#format)
  * [示例](#examples)
* [常见问题](#faqs)
  * [我可以Include的文件是否有限制?](#are-there-limits-to-the-files-i-can-include)
  * [我可以使用Include加载非补丁吗?](#can-i-load-non-patches-using-include)
* [参见](#see-also)

## 用法<a name="usage"></a>
### 概述<a name="overview"></a>
如果你不想在`content.json`定义模组中的所有的补丁，你可以在其他文件里定义补丁然后在`content.json`里用`Include`引用其他文件的补丁。功能上来说，这些引用的补丁等于把这些补丁拷贝到`Include`的位置。所有在`content.json`可使用的功能，如[令牌和条件](../author-guide.md#tokens)），均可在这些`Include`的补丁里使用。任何相对本地文件路径仍然以`content.json`为准。


被引用的文件必须是只有`"Changes"`字段的`.json`文件
```js
{
   // 不能有Format
    "Changes": [
        /* 补丁放这里 */
    ]
}
```

### 格式<a name="format"></a>
一个`Include`补丁是一个`Changes`以下含有这些字段的模型：

<dl>
<dt>必填字段：</dt>
<dd>

类型       | 作用
--------- | -------
`Action`  | 要进行的更改类型。此操作类型设置为`Include`。
`FromFile` | 内容包文件夹中需引用的`.json`文件的相对路径，或多个用逗号分割的的相对路径。此路径相对于你的`content.json`，即使你的`Include`是另一个`Include`里的补丁。

</td>
</tr>

</dd>
<dt>可选字段：</dt>
<dd>

类型       | 作用
--------- | -------
`When`    | _（可选）_ 当给定的[条件](../author-guide.md#conditions)匹配时才应用这个内容补丁。
`LogName`     | _（可选）_ 在日志中显示的补丁名称。这有助于查找错误。如果省略，则默认为类似`Include patches/data.json`的名称。
`Update`      | _（可选）_ 补丁字段多久更新一次。详见[更新速率](../author-guide.md#update-rate)。
`LocalTokens` | _（可选）_ 可在本补丁字段中使用的[局部令牌](../author-guide/tokens.md#local-tokens)。所有被引用的补丁都会继承这些令牌。

</dd>
</dl>

### 示例<a name="examples"></a>
最基本的使用方式是用`Include`把你的补丁分类到子文件里：

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

你可以将其与令牌和条件结合起来，选择性加载某一组补丁：

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
### 我可以Include的文件是否有限制?<a name="are-there-limits-to-the-files-i-can-include"></a>
没有。你可以`Include`的文件里可以有任意数量的补丁，这些补丁里可以有更多`Include`补丁来加载其他文件，而且你可以多次`Include`同一个文件。在每种情况下，它的工作原理都和你将所有补丁粘贴到“content.json”中的该位置一样。

你不可以循环`Include`（例如文件A引用文件B，而文件B也引用文件A）。

### 我可以使用Include加载`Changes`以外的内容吗?<a name="can-i-load-non-patches-using-include"></a>
不可以。`Include`的文件只能有一个`Changes`字节。如果你试图使用`ConfigSchema`，`CustomLocations`，`DynamicTokens`，Content Patcher会报错。

## 参见<a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
