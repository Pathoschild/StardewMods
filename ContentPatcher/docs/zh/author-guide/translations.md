← [模组作者指南](../author-guide.md)

翻译功能让你可以自动按照当前语言从`i18n`文件里加载正确翻译。

**🌐 其他语言： [en (English)](../../author-guide/translations.md)。**

## 目录
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [常见问题](#faqs)
  * [我可以在`i18n`文件里用令牌吗？](#can-i-use-content-patcher-tokens-in-i18n-files)
  * [翻译还可以用来干什么？](#what-else-can-i-do-with-translations)
* [参见](#see-also)

## 用法<a name="usage"></a>
### 格式<a name="format"></a>

你可以将翻译存在名为`i18n`的子目录，然后用`i18n` 令牌来调用翻译文本。当某个语言没有翻译时，Content Patcher会自动使用默认翻译文本。

翻译文件的格式详见维基上[_i18n_ 文件夹](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Translation#i18n_文件夹)的文档。文件里的翻译键必须逐字提供给`i18n`代币。（请参阅以下）。

补丁里可以用`{{i18n: <键>}}`令牌，把`<键>`替换成`i18n`文档里需调用的翻译键。你可以给`i18n`提供以下参数：

<table>
<tr>
<th>参数</th>
<th>描述</th>
</tr>
<tr>
<td><code>default</code></td>
<td>

如果一个翻译键不存在于现有语言和`default.json`，此令牌会显示类似`"missing translation: key"`的默认值。你可以用`default`提供自定义的默认值：
```js
"{{i18n:some-key |default=此为默认文本}}"
```

默认文本里可以使用令牌:
```js
"{{i18n:some-key |default=你好{{PlayerName}}! }}"
```

</td>
</tr>
<tr>
<td><code>defaultKeys</code></td>
<td>

如果一个翻译键不存在于现有语言和`default.json`，默认使用这个翻译键。第一个存在的键会被使用

其他参数(如`default`)会提供给被选中的翻译键。

假设你的翻译文件中只有`valid-key`这个翻译键，这个例子会显示`valid-key`的翻译文本。
```js
"{{i18n: missing-key |defaultKeys=missing-key-2, valid-key}}"
```

</td>
</tr>
<tr>
<td>

_其他_

</td>
<td>

其他所有函数均为翻译键里的替换值。假设你的的翻译如下：
```json
{
   "dialogue": "你好{{name}}，真是一个美丽的{{day}}上午！"
}
```

那可以像这样提供替换值：
```json
"{{i18n: dialogue |day={{DayOfWeek}} |name=阿比盖尔 }}"
```

</td>
</table>

## 示例<a name="examples"></a>

假设你有这两个翻译文件：

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

你可以用`i18n`[令牌](../author-guide.md#tokens)在补丁里引用翻译文本，不需要重复任何无翻译数据：

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
* [性别切换](https://stardewvalleywiki.com/Modding:Dialogue#Gender_switch)只有在文本在被游戏解译的场合下可用，例如对话。

## 常见问题<a name="faqs"></a>
### 我可以在`i18n`文件里用令牌吗?<a name="can-i-use-content-patcher-tokens-in-i18n-files"></a>

可以。不过因为翻译是SMAPI的系统，所以不直接支持令牌。你必须用参数将令牌替换到翻译文本里。

例如你的翻译如下：
```js
{
   "today": "today is {{DayOfWeek}}"
}
```

假设你直接用`{{i18n: today}}`那文本会显示为`"今天是{{DayOfWeek}}"`；
你可以像这样把令牌传入：`{{i18n: today |dayOfWeek={{DayOfWeek}} }}`，这样做以后文本会显示为`"今天是Monday"`。

### 翻译还可以用来干什么？<a name="what-else-can-i-do-with-translations"></a>
此功能可以理解为一个文本存储系统，有很多使用方法。

这个例子是一个在制作NPC时很方便的技巧，利用令牌来动态加载季节对话对应的翻译键。

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

然后在你的翻译文档里提供这些翻译键。
```js
{
    "festival-default": "I love these little events!",

    "festival-spring13.married": "Hello dear! Don't think I'll go easy on you in the egg hunt.",
    "festival-spring13.engaged": "Our last egg hunt before our wedding, can you believe it?"
    "festival-spring13": "Hi there. Good luck in the egg hunt!"
}
```

如果你和这个NPC结婚了并在复活节和他对话，Content Patcher会依次使用`festival-spring13.married`, `festival-spring13`,
`festival-default`，并调用第一个存在的翻译。以后需添加对话时只需要添加翻译，不需要更改补丁。

## 参见 <a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
* 更多信息请参考维基上的[_翻译模组_](https://zh.stardewvalleywiki.com/模组:翻译模组)文档
