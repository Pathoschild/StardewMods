← [模组作者指南](../author-guide.md)

此页记录Content Patcher所添加的自定义[触发动作](https://zh.stardewvalleywiki.com/模组:触发动作)

**🌐 其他语言： [en (English)](../../author-guide/trigger-actions.md)。**

## 目录
* [`MigrateIds`](#migrateids)
* [参见](#see-also)

## `MigrateIds`
`Pathoschild.ContentPatcher_MigrateIds` [触发动作](https://zh.stardewvalleywiki.com/模组:触发动作)
用于更新原有存档的事件，物品，信件，配方，和音频。

此触发动作可帮助旧模组迁移到[唯一字符串ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#唯一字符串ID).

参数格式为`<类型> [<旧ID> <新ID>]+`：

<table>
<tr>
<th>参数</th>
<th>使用</th>
</tr>
<tr>
<td><code>&lt;类型&gt;</code></td>
<td>

`CookingRecipes`, `CraftingRecipes`, `Events`, `Items`, `Mail`, `Songs`之一

</td>
</tr>
<tr>
<td><code>&lt;旧ID&gt;</code></td>
<td>

原有需迁移的ID。

如果类型是`Items`，并曾经定义在
* 数据素材，如`Data/Objects`:  
  使用[qualified item ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#物品ID), like `(O)OldId`。
* **未安装**的Json Assets内容包
  使用`"JsonAssets:<类型>:<名称>"`形式的ID。可使用`big-craftables`，`clothing`，`hats`，`objects`，和`weapons`。例如原名为 _Puffer Hat_ 的ID为`"JsonAssets:hats:Puffer Hat"`。
* **安装**的Json Assets内容包
  使用[Json Assets token](https://github.com/spacechase0/StardewValleyMods/blob/develop/JsonAssets/docs/author-guide.md#integration-with-content-patcher)
  获取实际ID，用作[qualified item ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#物品ID)。
  例如`(O){{spacechase0.JsonAssets/ObjectId: Puffer Hat}}`。

</td>
</tr>
<tr>
<td><code>&lt;新ID&gt;</code></td>
<td>

迁移后的新ID。

物品推荐使用[qualified item ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#物品ID)，避免产生歧义。

</td>
</tr>
</table>

你可以使用任意数量的旧ID/新ID配对

以下范例将`Puffer Plush`迁移到`{{ModId}}_PufferPlush`，`Puffer Sofa`迁移到`{{ModId}}_PufferSofa`:

```js
{
    "Action": "EditData",
    "Target": "Data/TriggerActions",
    "Entries": {
        "{{ModId}}_MigrateIds": {
            "Id": "{{ModId}}_MigrateIds",
            "Trigger": "DayStarted",
            "Actions": [
                // Note: 有空格的参数需使用双引号. 此范例的Action使用单引号，所以不需要转义其中的双引号。
                'Pathoschild.ContentPatcher_MigrateIds CraftingRecipes "Puffer Plush" {{ModId}}_PufferPlush "Puffer Sofa" {{ModId}}_PufferSofa'
            ],
            "HostOnly": true
        }
    }
}
```

> [!IMPORTANT]  
> Content Patcher需要访问完整游戏状态才能执行此触发动作。如果出现以下情况，触发动作会报错：
>* TriggerAction没有设为`"Trigger": "DayStarted"`和`"HostOnly": true`
>* 触发动作被`Data/TriggerActions`以外的机制触发

## 参见<a name="see-also"></a>
* [模组作者指南](../author-guide.md)
* [触发动作wiki](https://zh.stardewvalleywiki.com/模组:触发动作)
