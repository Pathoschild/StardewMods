← [模组作者指南](../author-guide.md)

翻译功能让您可以在 `i18n` 文件夹中存储不同语言的本地化文件，同时自动按照当前语言从文件里加载翻译。

**🌐 其他语言：[en (English)](../../author-guide/translations.md)。**

## 目录
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [常见问题](#faqs)
  * [我可以在`i18n`文件里使用令牌吗？](#can-i-use-content-patcher-tokens-in-i18n-files)
  * [翻译还可以用来干什么？](#what-else-can-i-do-with-translations)
* [参见](#see-also)

## 用法<a name="usage"></a>
### 格式<a name="format"></a>

您可以将本地化翻译文件存储在内容包的 `i18n` 子目录下，然后用 `i18n` 令牌来调用翻译文本。当某个语言没有翻译时，Content Patcher 会自动使用默认翻译文本。

翻译文件的格式请参阅 Wiki 上 [i18n 文件夹](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Translation#i18n_文件夹)的文档。文件中的翻译令牌必须显式提供给 i18n 令牌（请参阅下文）。

您可以通过在补丁中添加 `{{i18n: <键>}}` 令牌来使用翻译，把 `<键>` 替换成 `i18n` 文档里需调用的翻译键（请参阅下文示例）。您可以为 `i18n` 提供以下参数：

<table>
<tr>
<th>参数</th>
<th>描述</th>
</tr>
<tr>
<td><code>default</code></td>
<td>

如果一个翻译键不存在于现有语言和 `default.json`，此令牌会显示类似 `"missing translation: key"` 的默认值。您可以用 `default` 提供自定义的默认值：
```js
"{{i18n:some-key |default=此为默认文本}}"
```

默认文本里可以使用令牌:
```js
"{{i18n:some-key |default=您好{{PlayerName}}! }}"
```

</td>
</tr>
<tr>
<td><code>defaultKeys</code></td>
<td>

如果一个翻译键不存在于现有语言和 `default.json`，则默认使用这个翻译键。第一个存在的键会被使用。其他参数（如 `default`）会提供给被选中的翻译键。

假设您的翻译文件中只有 `valid-key` 这个翻译键，这个示例会显示 `valid-key` 的翻译文本。
```js
"{{i18n: missing-key |defaultKeys=missing-key-2, valid-key}}"
```

</td>
</tr>
<tr>
<td>

其他

</td>
<td>

其他任何参数都将为翻译令牌提供值（不区分大小写）。例如，如果您有一个这样的翻译：
```json
{
   "dialogue": "您好{{name}}，真是一个美丽的{{day}}上午！"
}
```

那么您可以在您的补丁中这样提供令牌值：
```json
"{{i18n: dialogue |day={{DayOfWeek}} |name=阿比盖尔 }}"
```

</td>
</table>

## 示例<a name="examples"></a>

假设您有这两个翻译文件：

```js
// i18n/default.json（默认语言，英文）
{
   "item.name": "Pufferchick",
   "item.description": "A tiny hybrid between a pufferfish and chicken."
}
```
```js
// i18n/fr.json（法语）
{
   "item.name": "Poussin-globe",
   "item.description": "Un tout petit hybride entre un poisson-globe et un poussin."
}
```

您可以使用 `i18n` [令牌](../author-guide.md#tokens)直接在补丁中注入已翻译的文本，而无需重复任何未翻译数据：

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Entries": {
                "{{ModId}}_Pufferchick": {
                    "DisplayName": "{{i18n: item.name}}",
                    "Price": 1200,
                    ...
                }
            }
        }
    ]
}
```

## 限制
* [性别判断](https://zh.stardewvalleywiki.com/模组:对话#性别判断)仅在将文本传递给游戏时有效（例如通过对话），因为游戏会解析它们。

## 常见问题<a name="faqs"></a>
### 我可以在 `i18n` 文件里使用令牌吗?<a name="can-i-use-content-patcher-tokens-in-i18n-files"></a>

可以。但是由于翻译由 SMAPI 处理，因此它们不直接支持内置令牌。您必须用参数将令牌替换到翻译文本里。

例如，假设您有以下翻译：
```js
{
   "today": "今天是{{DayOfWeek}}"
}
```

假设您直接使用 `{{i18n: today}}`，那么游戏内将显示为 `"今天是{{DayOfWeek}}"`。您可以像这样传递令牌：`{{i18n: today |dayOfWeek={{DayOfWeek}} }}`，这样做以后，文本才能正确地显示为 `"今天是Monday"`。

### 翻译还可以用来干什么？<a name="what-else-can-i-do-with-translations"></a>
此功能可以理解为一个文本存储系统，因此您可以利用它做很多事情。

例如，节日翻译技巧在 NPC 模组制作者中很受欢迎。它仅包含单个补丁，该补丁编辑了游戏中的每个节日，以动态加载不同季节节日的对话：

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Festivals/spring13, Data/Festivals/spring24, Data/Festivals/summer11, Data/Festivals/summer28, Data/Festivals/fall16, Data/Festivals/fall27, Data/Festivals/winter8, Data/Festivals/winter25",
            "Entries": {
                "Alexia": "
                    {{i18n:festival-{{TargetWithoutPath}}.{{Relationship:Alexia}}
                       |defaultKeys=festival-{{TargetWithoutPath}}, festival-default
                    }}
                "
            }
        }
    ]
}
```

然后在您的翻译文件里提供这些翻译键。
```js
{
    "festival-default": "我喜欢这个小活动！",
    "festival-spring13.married": "你好啊亲爱的！不要以为我会在复活节彩蛋寻宝中手下留情喔。",
    "festival-spring13.engaged": "这是我们婚礼前的最后一次复活节彩蛋寻宝，你敢相信吗？",
    "festival-spring13": "嗨，你好啊。祝你在复活节彩蛋寻宝中好运！",
}
```

如果您与这个 NPC 结婚了，并在复活节上和 Ta 对话，那么 Content Patcher 会依次使用 `festival-spring13.married`、`festival-spring13`、
`festival-default`，并调用第一个存在的翻译。此后您在添加对话时只需要添加翻译即可，不需要更改补丁。

## 参见 <a name="see-also"></a>
* 其他操作和选项请参见[模组作者指南](../author-guide.md)
* 更多信息请参考 Wiki 上的[翻译模组](https://zh.stardewvalleywiki.com/模组:翻译模组)文档
