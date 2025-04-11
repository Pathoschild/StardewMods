<<<<<<< HEAD
﻿← [author guide](../author-guide.md)

The translation feature lets you store translatable text in `i18n` files and load the correct text
for the current language automatically.

## Contents
* [Usage](#usage)
  * [Format](#format)
  * [Example](#example)
* [FAQs](#faqs)
  * [Can I use Content Patcher tokens in `i18n` files?](#can-i-use-content-patcher-tokens-in-i18n-files)
  * [What else can I do with translations?](#what-else-can-i-do-with-translations)
* [See also](#see-also)

## Usage
### Format
You can store translations in an `i18n` subfolder of your content pack, and access them through the
`i18n` token. Content Patcher handles fallback automatically; if there's no translation in the
current language, it will show the default text instead.

For the translation file format, see [_i18n folder_](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Translation#i18n_folder)
on the wiki. Translation tokens in the file must be provided explicitly to the `i18n` token (see
below).

You can use translations in your patches by adding the `{{i18n: <key>}}` token, replacing `<key>`
with the key used in the translation files (see the [example below](#example)). You can optionally
specify these arguments to `i18n`:

<table>
<tr>
<th>argument</th>
<th>description</th>
=======
﻿← [模组作者指南](../author-guide.md)

翻译功能让你可以自动按照当前语言从`i18n`文件里加载正确翻译。

## 目录
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [常见问题](#faqs)
  * [我可以在`i18n`文件里用token吗？](#can-i-use-content-patcher-tokens-in-i18n-files)
  * [翻译还可以用来干什么？](#what-else-can-i-do-with-translations)
* [参见](#see-also)

## 用法<a name="usage"></a>
### 格式<a name="format"></a>

你可以将翻译存在名为`i18n`的子目录，然后用`i18n` token来调用翻译文本。当某个语言没有翻译时，Content Patcher会自动使用默认翻译文本。

翻译文件的格式详见维基上[_i18n_文件夹](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Translation#i18n_.E6.96.87.E4.BB.B6.E5.A4.B9)的文档。文件里的翻译键必须逐字提供给`i18n`代币。（请参阅以下）。

补丁里可以用`{{i18n: <键>}}`token，把`<键>`替换成`i18n`文档里需调用的翻译键。你可以给`i18n`提供以下参数：

<table>
<tr>
<th>参数</th>
<th>描述</th>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
</tr>
<tr>
<td><code>default</code></td>
<td>

<<<<<<< HEAD
If a translation doesn't exist (in both the current language _and_ `default.json`), the token will
return text like "missing translation: key". You can provide a different default value using the
`default` argument:
```js
"{{i18n:some-key |default=default text to display}}"
```

You can use tokens in the default text, so you can also default to a different translation:
```js
"{{i18n:some-key |default={{i18n:another-key}} }}"
=======
如果一个翻译键不存在于现有语言和`default.json`，此token会显示类似`"missing translation: key"`的默认值。你可以用`default`提供自定义的默认值：
```js
"{{i18n:some-key |default=此为默认文本}}"
```

默认文本里可以使用tokens:
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
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
```

</td>
</tr>
<tr>
<td>

<<<<<<< HEAD
_any other_
=======
_其他_
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
<td>

<<<<<<< HEAD
Any other arguments provide values for translation tokens (not case-sensitive). For example, if you
have a translation like this:
```json
{
   "dialogue": "Hi {{name}}, it's a beautiful {{day}} morning!"
}
```

Then you can do this to provide the token values in your patch:
```json
"{{i18n: dialogue |day={{DayOfWeek}} |name=Abigail }}"
=======
其他所有函数均为翻译键里的替换值。假设你的的翻译如下：
```json
{
   "dialogue": "你好{{name}}，真是一个美丽的{{day}}上午！"
}
```

那可以像这样提供替换值：
```json
"{{i18n: dialogue |day={{DayOfWeek}} |name=阿比盖尔 }}"
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
```

</td>
</table>

<<<<<<< HEAD
## Example
Let's say you have these two translation files:
=======
## 示例<a name="examples"></a>

假设你有这两个翻译文件：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

```js
// i18n/default.json
{
   "item.name": "Pufferchick",
   "item.description": "A tiny hybrid between a pufferfish and chicken."
}
```
```js
// i18n/fr.json
{
   "item.name": "Poussin-globe",
   "item.description": "Un tout petit hybride entre un poisson-globe et un poussin."
}
```

<<<<<<< HEAD
You can inject the translated text directly into your patches using the `i18n`
[token](../author-guide.md#tokens), without duplicating any of the untranslated data:

```js
{
    "Format": "2.5.0",
=======
你可以用`i18n`[token](../author-guide.md#tokens)在补丁里引用翻译文本，不需要重复任何无翻译数据：

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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
    ]
}
```

<<<<<<< HEAD
## Limitations
* [Gender-switch blocks](https://stardewvalleywiki.com/Modding:Dialogue#Gender_switch) only work in cases where you
  pass the text to the game (e.g. via dialogue), since the game will parse them.

## FAQs
### Can I use Content Patcher tokens in `i18n` files?
Yes. The translations are handled by SMAPI though, so they don't support built-in tokens directly.
Instead you need to pass the values into the `i18n` token.

For example, let's say you have this translation:
=======
## 限制
* [性别切换](https://stardewvalleywiki.com/Modding:Dialogue#Gender_switch)只有特定场合（如对话）可用。

## 常见问题<a name="faqs"></a>
### 我可以在`i18n`文件里用token吗?<a name="can-i-use-content-patcher-tokens-in-i18n-files"></a>

可以。不过因为翻译是SMAPI的系统，所以不直接支持tokens。你必须用参数将token替换到翻译文本里。

例如你的翻译如下：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
```js
{
   "today": "today is {{DayOfWeek}}"
}
```

<<<<<<< HEAD
If you just use `{{i18n: today}}`, it'll show the literal text "_today is {{DayOfWeek}}_").
You can pass in the token like this instead: `{{i18n: today |dayOfWeek={{DayOfWeek}} }}`, in which
case it'll show something like "_today is Monday_".

### What else can I do with translations?
The feature is essentially a text storage system, so there's a lot of ways you can use it.

For example, the festival translation trick is popular with NPC modders. It involves a single patch
which edits every festival in the game to add dynamic dialogue based on the translation file:

```js
{
    "Format": "2.5.0",
=======
假设你直接用`{{i18n: today}}`那文本会显示为`"_今天是{{DayOfWeek}}_"`；
你可以像这样把token传入：`{{i18n: today |dayOfWeek={{DayOfWeek}} }}`，这样做以后文本会显示为`"_今天是Monday_"`。

### 翻译还可以用来干什么？<a name="what-else-can-i-do-with-translations"></a>
此功能可以理解为一个文本存储系统，有很多使用方法。

这个例子是一个在制作NPC时很方便的技巧，利用tokens来动态加载季节对话对应的翻译键。

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Festivals/spring13, Data/Festivals/spring24, Data/Festivals/summer11, Data/Festivals/summer28, Data/Festivals/fall16, Data/Festivals/fall27, Data/Festivals/winter8, Data/Festivals/winter25",
            "Entries": {
                "Alexia": "
<<<<<<< HEAD
                    {{i18n:festival-{{TargetWithoutPath}}.{{Relationship:Alexia}} |default=
                        {{i18n:festival-{{TargetWithoutPath}} |default=
                            {{i18n:festival-default}}
                        }}
=======
                    {{i18n:festival-{{TargetWithoutPath}}.{{Relationship:Alexia}}
                       |defaultKeys=festival-{{TargetWithoutPath}}, festival-default
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
                    }}
                "
            }
        }
    ]
}
```

<<<<<<< HEAD
Then in your translation file, you can add translations like this:
=======
然后在你的翻译文档里提供这些翻译键。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
```js
{
    "festival-default": "I love these little events!",

    "festival-spring13.married": "Hello dear! Don't think I'll go easy on you in the egg hunt.",
    "festival-spring13.engaged": "Our last egg hunt before our wedding, can you believe it?"
    "festival-spring13": "Hi there. Good luck in the egg hunt!"
}
```

<<<<<<< HEAD
If you're married to the NPC and talk to them at the Egg Festival, Content Patcher will use the
first translation that exists in this order: `festival-spring13.married`, `festival-spring13`, or
`festival-default`. Then you can add more dialogue anytime by just adding translations, no need to
change individual festival patches.

## See also
* [Author guide](../author-guide.md) for other actions and options
* [_translations_ on the wiki](https://stardewvalleywiki.com/Modding:Translations) for more info
=======
如果你和这个NPC结婚了并在复活节和他对话，Content Patcher会依次使用`festival-spring13.married`, `festival-spring13`,
`festival-default`，并调用第一个存在的翻译。以后需添加对话时只需要添加翻译，不需要更改补丁。

## 参见 <a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
* 更多信息请参考维基上的[_翻译模组_](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%BF%BB%E8%AF%91%E6%A8%A1%E7%BB%84)文档
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
