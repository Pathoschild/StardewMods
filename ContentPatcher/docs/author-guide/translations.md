← [author guide](../author-guide.md)

The translation feature lets you store translatable text in `i18n` files and load the correct text
for the current language automatically.

**🌐 In other languages: [zh (中文)](../zh/author-guide/translations.md)**.

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
</tr>
<tr>
<td><code>default</code></td>
<td>

If the translation doesn't exist (in both the current language _and_ `default.json`), the text to
return instead of "_missing translation: key_".

For example:
```js
"{{i18n:some-key |default=default text to display}}"
```

You can also use tokens in the default text:
```js
"{{i18n:some-key |default=Hi {{PlayerName}}! }}"
```

</td>
</tr>
<tr>
<td><code>defaultKeys</code></td>
<td>

If the translation doesn't exist (in both the current language _and_ `default.json`), the
translation keys to use instead. The first key which exists in the translation files is used.
The other arguments (like `default`) are applied to the selected translation key.

For example, let's say your translation files only contain a `valid-key` translation. This code
would return that translation:
```js
"{{i18n: missing-key |defaultKeys=missing-key-2, valid-key}}"
```

</td>
</tr>
<tr>
<td>

_any other_

</td>
<td>

Any other arguments provide values for translation tokens (not case-sensitive). For example, if you
have a translation like this:
```js
{
   "dialogue": "Hi {{name}}, it's a beautiful {{day}} morning!"
}
```

Then you can do this to provide the token values in your patch:
```json
"{{i18n: dialogue |day={{DayOfWeek}} |name=Abigail }}"
```

</td>
</table>

## Example
Let's say you have these two translation files:

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

You can inject the translated text directly into your patches using the `i18n`
[token](../author-guide.md#tokens), without duplicating any of the untranslated data:

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

## Limitations
* [Gender-switch blocks](https://stardewvalleywiki.com/Modding:Dialogue#Gender_switch) only work in cases where you
  pass the text to the game (e.g. via dialogue), since the game will parse them.

## FAQs
### Can I use Content Patcher tokens in `i18n` files?
Yes. The translations are handled by SMAPI though, so they don't support built-in tokens directly.
Instead you need to pass the values into the `i18n` token.

For example, let's say you have this translation:
```js
{
   "today": "today is {{DayOfWeek}}"
}
```

If you just use `{{i18n: today}}`, it'll show the literal text "_today is {{DayOfWeek}}_".
You can pass in the token like this instead: `{{i18n: today |dayOfWeek={{DayOfWeek}} }}`, in which
case it'll show something like "_today is Monday_".

### What else can I do with translations?
The feature is essentially a text storage system, so there's a lot of ways you can use it.

For example, the festival translation trick is popular with NPC modders. It involves a single patch
which edits every festival in the game to add dynamic dialogue based on the translation file:

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

Then in your translation file, you can add translations like this:
```js
{
    "festival-default": "I love these little events!",

    "festival-spring13.married": "Hello dear! Don't think I'll go easy on you in the egg hunt.",
    "festival-spring13.engaged": "Our last egg hunt before our wedding, can you believe it?"
    "festival-spring13": "Hi there. Good luck in the egg hunt!"
}
```

If you're married to the NPC and talk to them at the Egg Festival, Content Patcher will use the
first translation that exists in this order: `festival-spring13.married`, `festival-spring13`, or
`festival-default`. Then you can add more dialogue anytime by just adding translations, no need to
change individual festival patches.

## See also
* [Author guide](../author-guide.md) for other actions and options
* [_translations_ on the wiki](https://stardewvalleywiki.com/Modding:Translations) for more info
