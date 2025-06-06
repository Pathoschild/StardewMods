← [模组作者指南](../author-guide.md)

一个含有 **`"Action": "EditImage"`** 的补丁会更改游戏已加载的图像的一部分。任意数量的内容包都可以编辑同一素材。你可以用补丁向下延伸图像（Content Patcher将扩展图像以适应新图像）。

**🌐 其他语言： [en (English)](../../author-guide/action-editimage.md)。**

## 目录<a name="contents"></a>
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [参见](#see-also)

## 使用<a name="usage"></a>
### 格式<a name="format"></a>

一个`EditImage`补丁是`Changes`下含有此字段的模型 ([下文有例子](#examples))

<dl>
<dt>必填字段：</dt>
<dd>

字段       | 用途
--------- | -------
`Action`  | 要进行的更改类型。此操作类型设置为`EditImage`。
`Target`  | 需编辑的[游戏素材名](../author-guide.md#what-is-an-asset)（或多个由逗号分隔的素材名），比如`Portraits/Abigail`。该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。
`FromFile` | 内容包文件夹中要修补到目标中的图像的相对路径（例如`assets/dinosaur.png`），或多个逗号分隔的路径。这可以是`.png`或`.xnb`文件。该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。

</dd>
<dt>可选字段：</dt>
<dd>

字段         | 用途
----------- | -------
`FromArea`  | <p>源图片中需拷贝到目标的部分，默认整个源图片</p><p>此字段是一个对象，含有左上角点的X和Y像素坐标，和区域的像素大小，长（Width）与高（Height）。该对象的字段支持[令牌](../author-guide.md#tokens)。</p>
`ToArea`    | <p>目标图片中要替换的部分。默认大小与 `FromArea` 相同，位于贴图的左上角。</p><p>此字段是一个含有左上角点的X和Y像素坐标区域的长（Width）与高（Height）的对象。该对象的字段支持[令牌](../author-guide.md#tokens)。</p><p>如果你指定的区域超出了图像的底部，Content Patcher将自动调整图像大小以适应新图像。</p>
`PatchMode` | <p>如何将 `FromArea` 应用于 `ToArea`。默认为 `Replace`。</p> 可使用的值: <ul><li><code>Replace</code>: 用源图像替换目标区域中的每个像素。如果源图像有透明像素，则目标图像将在那里变为透明。</li><li><code>Overlay</code>: 在目标区域上绘制源图像。如果源图像有透明或半透明像素，则目标图像将“显示”这些像素。不透明像素将替换目标像素。</li></ul>例如，假设你的源图像是具有透明背景的河豚，而目标图像是实心绿色正方形。 以下是它们在不同`PatchMode`下的组合：<br />![](../../screenshots/patch-mode-examples.png)
`When`      | _(可选)_ 使此补丁只有在指定[条件](../author-guide.md#conditions)下生效.
`LogName`   | _(可选)_ 此补丁在日志里显示的名字，有助于理解报错。默认为类似`EditImage Animals/Dinosaur`的名字。
`Update`    | _(可选)_ 此补丁字条的更新频率，详见[update rate](../author-guide.md#update-rate)。
`LocalTokens` | _(可选)_ 一组仅在此补丁中生效的[局部令牌](../author-guide/tokens.md#local-tokens)。

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

 _（可选）_ 当多个补丁编辑同一数据素材时，此字段控制它们应用的顺序。可用的值有`Early`（更早），`Default`（默认），还有`Late`（更晚）。默认值为`Default`。

补丁（包括所有模组）按以下顺序生效：

1. 优先级从早到晚；
2. 按照模组加载顺序（基于依赖关系等因素）；
3. 按照补丁在`content.json`中列出的顺序。

如果需要更具体的顺序，可以使用简单的偏移量，如`"Default + 2"`或者`"Late - 10"`。
默认值为-1000 （`Early`），0（`Default`）和1000（`Late`）。

此字段 _不_ 支持令牌，不区分大小写。

> [!TIP]
> 优先级会让你的更改难以排除故障。推荐做法：
> * 如果可以的话，只使用上述无偏移的优先级（比如外观覆盖设为`Late`）
> * 在 _你自己_ 的补丁里不需要用优先级，因为你可以自己在content.json排列好补丁应用的顺序。

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

 _（可选）_ 素材名称中要匹配的地区代码，比如设置`"TargetLocale": "fr-FR"`只编辑法语形式的素材（比如`Animals/Dinosaur.fr-FR`）。可以为空，只有只编辑没有地域区分的基本素材。

如果省略，它将应用于所有素材，不管有没有本地化。

</td>
</table>
</dd>
</dl>

### 示例<a name="examples"></a>

这个例子改变某一物品的图标

```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "EditImage",
         "Target": "Maps/springobjects",
         "FromFile": "assets/fish-object.png",
         "FromArea": { "X": 0, "Y": 0, "Width": 16, "Height": 16 }, // 可选，默认整个图片
         "ToArea": { "X": 256, "Y": 96, "Width": 16, "Height": 16 } // 可选，默认与FromArea大小相同
      },
   ]
}
```

## 参见<a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
