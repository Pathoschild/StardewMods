← [README](README.md)

此文档描述中文翻译的选词

## 关于此汉化

不汉化以下文档：
- `release-notes.md` - 长，而且内容跟正式文档重复
- `author-tokens-guide.md` - 已转移到`author-guide/tokens.md`

不汉化以下名词：
- 任何需要写入模组json的词，如"Action"

Wiki链接换成[中文wiki](https://zh.stardewvalleywiki.com/模组:目录)

## 词汇表

__内容包__: Content Pack

内容包是给框架模组提供数据的子模组。文档里提到的“内容包”特指Content Patcher的内容包。

__数据素材__: Data Assets

素材是游戏数据的组织单位。每一个素材对应一个目标（Target）。

__列表__: List

列表是一组没有明确键的非唯一值。列表中的键值对必须写在 `[` 和 `]`之中。

__字典__: Dictionary

字典是包含许多键值对的列表。其中的键必须有唯一ID。同一个字典中的键值对必须写在 `{` 和 `}` 之中。

__模型__: Model

模型是一种预定义的数据结构。对于内容包来说，它和字典相同，只是你不能添加新字段（只能编辑已有字段）。

__条目__: Entry

条目是目标数据中的顶层数据块（即字典中的键值对或列表中的值）。

__字段__: Field

字段是是条目中的一个子块。

__补丁__: Patch

一个补丁是`"Changes"`以下的一个条目，类别以`"Action"`字条为准。

__翻译键__: Translation Key/Translation Token

翻译系统的tokens和Content Patcher的tokens不是同一个概念，它指的是翻译文档里左手边的键。

英文文档里有混合使用Translation Key和Translation Token，中文翻译统一使用翻译键。

__"local token"__: 局部令牌，专属令牌

Content Patcher有两个不同的local tokens概念，翻译中为了区分选了两个不同的词
* `LocalTokens`字段里出现的令牌是“局部令牌”，仅生效于此补丁或Include的补丁。
* 每个内容包独有的令牌是“专属令牌”，如`{{ModId}}`，`{{i18n}}`等。这些令牌的值和在不同内容包中不一样。

__上下文__: Context

计算机术语，在Content Patcher中指某一个范围中令牌的状态，可理解为“当下场景”。

## 参见<a name="see-also"></a>
* 其他信息详见[README](README.md)
