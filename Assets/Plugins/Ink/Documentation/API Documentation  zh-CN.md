# 运行你的 ink

## 快速开始

_请注意，虽然这些说明是针对 Unity 编写的，但在非 Unity 的 C# 环境中运行 ink 也是可能的（并且很简单）。_

- 下载最新版本的 ink-unity-integration Unity 包，并将其添加到你的项目中。
- 在 Unity 中选择你的 .ink 文件，你应该会在文件的检查器中看到一个 _Play_ 按钮。
- 点击它，你应该会得到一个编辑器窗口，允许你播放（预览）你的故事。
- 要将其集成到你的游戏中，请参见下面的运行时 API 入门。

## 更多信息

Ink 使用一种中间的 .json 格式，该格式是从原始的 .ink 文件编译而来的。ink 的 Unity 集成包会自动为你编译 ink 文件，但你也可以在命令行中编译它们。有关更多信息，请参见 README 中的“在命令行中使用 inklecate”。

主要的运行时代码包含在 ink-engine.dll 中。

我们建议你为 ink 的 Story 创建一个包装的 MonoBehaviour 组件。在这里，我们将该组件称为“Script”——这里的“Script”指的是“电影剧本”意义上的脚本，而不是“Unity 脚本”意义上的脚本！

```csharp
using Ink.Runtime;

public class Script : MonoBehaviour {

    // 将此文件设置为你的编译后的 json 资源
    public TextAsset inkAsset;

    // 我们正在包装的 ink 故事
    Story _inkStory;
```

## 运行时 API 入门

如上所述，你的 .ink 文件会被编译为单个 .json 文件。Unity 将其视为一个 TextAsset，然后你可以在游戏中加载它。

加载和运行你的故事的 API 非常简单。构造一个新的 Story 对象，传入来自 TextAsset 的 JSON 字符串。例如，在 Unity 中：

```csharp
using Ink.Runtime;

...

void Awake()
{
    _inkStory = new Story(inkAsset.text);
}
```

从那里开始，你在循环中调用故事。有两个重复的阶段：

1. **呈现内容：** 你反复调用 `Continue()`，它会返回单个字符串内容行，直到 `canContinue` 属性变为 false。例如：

```csharp
while (_inkStory.canContinue) {
    Debug.Log (_inkStory.Continue ());
}
```

实现上述功能的一种更简单的方法是通过一次调用 `_inkStory.ContinueMaximally()`。

2. **做出选择：** 当没有更多内容时，你应该检查是否有任何选择要呈现给玩家。为此，可以使用类似以下代码：

```csharp
if( _inkStory.currentChoices.Count > 0 )
{
    for (int i = 0; i < _inkStory.currentChoices.Count; ++i) {
        Choice choice = _inkStory.currentChoices [i];
        Debug.Log("Choice " + (i + 1) + ". " + choice.text);
    }
}

//...当玩家提供输入时：
_inkStory.ChooseChoiceIndex (index);

//现在你已经准备好返回步骤 1，再次呈现内容。
```

### 保存和加载

要在游戏中保存故事的状态，请调用：

```csharp
string savedJson = _inkStory.state.ToJson();
```

...然后再次加载它：

```csharp
_inkStory.state.LoadJson(savedJson);
```

### 错误处理

如果你在 ink 中犯了编译器无法捕捉的错误，那么故事会抛出异常。为了避免这种情况并获取标准的 Unity 错误，你可以使用一个错误处理程序，你应在创建故事时分配它：

```csharp
_inkStory = new Story(inkAsset.text);

_inkStory.onError += (msg, type) => {
    if( type == Ink.ErrorType.Warning )
        Debug.LogWarning(msg);
    else
        Debug.LogError(msg);
};
```

### 就这样吗？

就是这样！仅通过这些简单的步骤，你就可以实现很多功能，但如果你想了解更多高级用法，包括与游戏的深度集成，请继续阅读。
要查看一个带有最小 UI 的 Unity 示例项目，请参见 Aaron Broder 的 Blot 仓库。

# 引擎使用与哲学

在 Unity 中，我们建议使用你自己的组件类来包装 `Ink.Runtime.Story`。运行时 ink 引擎被设计为相当通用，并具有简单的 API。我们还建议包装而不是继承 `Story`，以便你可以向游戏仅暴露你需要的功能。

在设计游戏流程时，玩家与故事之间的交互序列可能并不完全匹配 **ink** 的评估方式。例如，对于经典的“选择你自己的冒险”类型的故事，你可能希望一次显示多行（段落）文本和选择。对于视觉小说，你可能希望每屏显示一行。

此外，由于 **ink** 引擎输出的是纯文本行，它可以有效地用于你自己的简单子格式。例如，对于基于对话的游戏，你可以编写：

```ink
* Lisa: 他去哪了？
	Joe: 我想他跳过了花园的篱笆。
	* * Lisa: 我们去看一看。
	* * Lisa: 所以他真的走了？
```

对于 **ink** 引擎来说，`:` 字符只是文本。但作为游戏生成的文本行和选择，你可以进行一些简单的文本解析，将字符串 `Joe: 怎么了？` 转换为一个特定于游戏的对话对象，引用说话者和文本（甚至音频）。

这种方法甚至可以进一步扩展到灵活指示非内容指令的文本。同样，这些指令作为文本从引擎输出，但可以由你的游戏解析以用于特定目的：

```
PROPLIST table, chair, apple, orange
```

在我们的当前游戏中，上述方法用于让作者声明他们期望在场景中的道具。这些可能会在游戏编辑器中被拾取，以便自动用占位符对象填充场景，或者只是验证关卡设计师是否正确填充了场景。

要更明确地标记内容，你可能希望使用标签或外部函数——见下文。在 inkle，我们发现我们混合使用了这些方法，但实际上我们发现上述方法对于我们与游戏的大部分交互非常有用——它非常灵活。

# 使用标签标记你的 ink 内容

标签可用于向游戏内容添加不打算向玩家显示的元数据。在 ink 中，添加一个 `#` 字符，后跟你想传递给游戏的任何字符串内容。你可以在三个主要位置添加这些哈希标签：

## 逐行标签

一个用例是图形冒险游戏，其中角色根据他们的面部表情有不同的艺术表现。因此，你可以这样做：

```
Passepartout: 真的，先生。 # 愠怒
```

在游戏端，每次你通过 `_inkStory.Continue()` 获取内容时，你都可以通过 `_inkStory.currentTags` 获取标签列表，它将返回一个 `List<string>`，在上述情况下只有一个元素：`"愠怒"`。

标签可以写在行的上方，也可以写在行的末尾：

```
# 第一个标签
# 第二个标签
这是内容行。 # 第三个标签
```

上述所有标签都将包含在 `currentTags` 列表中。

## 节点标签

你可以在节点的最顶部包含任何标签：

```
=== 慕尼黑 ===
# 地点: 德国
# 概述: munich.ogg
# 需求: 火车票
节点中的第一行内容...
```

...通过调用 `_inkStory.TagsForContentAtPath("your_knot")` 可以访问这些标签，这对于在游戏实际到达该节点之前获取元数据非常有用。

请注意，这些标签也会出现在节点中第一行内容的 `currentTags` 列表中。

## 全局标签

在主 ink 文件的最顶部提供的任何标签都可以通过 Story 的 `globalTags` 属性访问，该属性也返回一个 `List<string>`。任何顶级故事元数据都可以包含在那里。

我们建议按照以下约定，如果你希望公开分享你的 ink 故事：

```
# 作者: Joseph Humfrey
# 标题: 我的精彩 Ink 故事
```

请注意，Inky 将使用这种格式的标题标签作为导出为网页的故事中的 `<h1>` 标签。

### 选择标签

标签也可以应用于选择。根据标签在 ink 行中的位置，标签将出现在选择和该选择生成的内容上；仅在选择上；或仅在输出内容上：

```
* 一个选择！ # 一个同时出现在选择和内容上的标签
* [一个选择 # 一个不出现在内容上的选择标签 ]
* 一个选择[!] 继续。 # 一个仅出现在输出内容上的标签
```

你可以在同一 ink 行中使用所有三种方式。

```
* 一个选择 # shared_tag [ 带有细节 #choice_tag ] 和内容 # content_tag
```

选择标签存储在 `List<string> tags` 中的选择对象中。

### 高级：标签是动态的

请注意，标签的内容可以包含任何内联 ink，例如随机、循环、函数调用和变量替换。

```
{character}: 你好！ #{character}_greeting.jpg
我打开了门。 #suspense_music{RANDOM(1, 4)}.mp3
```

# 跳转到特定的“场景”

ink 中顶层的命名部分称为节点（参见写作教程）。你可以告诉运行时引擎跳转到特定的命名节点：

```csharp
_inkStory.ChoosePathString("myKnotName");
```

然后像往常一样调用 `Continue()`。

要直接跳转到节点内的一个 stitch，使用 `.` 作为分隔符：

```csharp
_inkStory.ChoosePathString("myKnotName.theStitchWithin");
```

（请注意，此路径字符串是运行时路径，而不是 ink 格式中使用的路径。它的设计使得对于基本的节点和 stitch，格式是相同的。但遗憾的是，你不能通过这种方式引用 gather 或 choice 标签。）

# 设置/获取 ink 变量

ink 引擎中的变量状态适当地存储在故事的 `variablesState` 对象中。你可以直接在此对象上获取和设置变量：

```csharp
_inkStory.variablesState["player_health"] = 100;
int health = (int) _inkStory.variablesState["player_health"];
```

# 读取/访问计数

要查找 ink 引擎访问节点或 stitch 的次数，你可以使用此 API：

```csharp
_inkStory.state.VisitCountAtPathString("...");
```

路径字符串的形式为 `"yourKnot"` 表示节点，`"yourKnot.yourStitch"` 表示 stitch。

# 变量观察者

你可以注册一个委托函数，每当特定变量发生变化时调用。这对于直接在 UI 中反映某些 ink 变量的状态非常有用。例如：

```csharp
_inkStory.ObserveVariable ("health", (string varName, object newValue) => {
    SetHealthInUI((int)newValue);
});
```

传递变量名称的原因是你可以让一个观察者函数观察多个不同的变量。

# 运行函数

你可以使用 `EvaluationFunction` 直接从 C# 运行 ink 函数。

你可以传递 ink 函数的预期参数（如果有）。

如果 ink 函数有返回值，它将由 `EvaluationFunction` 返回。你不需要 `Continue()` 函数中可能存在的任何文本行；它会运行到结束。任何内容都会写入 `textOutput` 参数，每行之间有一个换行符。

```csharp
var returnValue = _inkStory.EvaluationFunction("myFunctionName", out textOutput, params);
```

# 外部函数

你可以定义在C#中定义的函数，并直接从ink中调用。具体步骤如下：

1. 在ink文件的全局范围内声明一个外部函数，例如：

   ```ink
   EXTERNAL playSound(soundName)
   ```

2. 绑定你的C#函数。例如：

   ```csharp
   _inkStory.BindExternalFunction ("playSound", (string name) => {
       _audioController.Play(name);
   });
   ```

`BindExternalFunction`方法提供了多个便捷的重载版本，支持最多四个参数，适用于`System.Func`和`System.Action`。如果需要处理超过四个参数的情况，可以使用通用的`BindExternalFunctionGeneral`方法，它接受一个对象数组作为参数。

3. 然后你可以在ink中调用该函数：

   ```ink
   ~ playSound("whack")
   ```

可以作为参数和返回值的类型包括`int`、`float`、`bool`（自动从ink的内部int转换）和`string`。

## 外部函数的替代方案

除了外部函数，还有其他几种方式可以在ink和游戏之间进行通信：

- **变量观察器**：如果你只是想让游戏知道某些状态发生了变化，这非常适用于例如在UI中更新分数。
- **标签**：为ink中的某一行添加不可见的元数据。
- **文本解析**：在inkle的游戏中，如*Heaven's Vault*，我们使用文本本身向游戏发送指令，然后由游戏特定的文本解析器决定如何处理它。例如：

   ```ink
   >>> SHOT: view_over_bridge
   ```

## 动作与纯函数

**警告**：以下部分内容较为复杂！不过不用担心，你通常可以忽略它并使用默认行为。如果你发现某些情况下外部函数的行为与预期不符，或者你只是单纯好奇，可以继续阅读...

外部函数分为两种：

- **动作**：例如播放声音、显示图片等。这些操作可能会改变游戏状态。
- **纯函数**：那些不会产生副作用。具体来说，1) **多次调用它们应该是无害的**，2) **它们不应该影响游戏状态**。例如，数学计算或纯粹的游戏状态检查。

默认情况下，外部函数被视为动作，因为我们认为这是大多数人的主要使用场景。然而，这种区分对于粘合（glue）的工作方式可能有重要影响。当引擎查看内容时，它可能会比预期更远地向前查看，以防未来的内容中有粘合将两行文本合并为一行。然而，对于作为动作运行的外部函数，你不希望它们被前瞻性地运行，因为玩家可能会注意到，因此对于这种类型，我们会取消任何试图粘合内容的尝试。如果引擎在向前查看的过程中发现一个动作，它会在运行之前停止。

相反，如果你只是在做数学计算，你肯定不希望粘合被破坏。例如：

```ink
The square root of 9
~ temp x = sqrt(9)
<> is {x}.
```

你可以在绑定函数时定义它的行为，使用`lookaheadSafe`参数：

```csharp
public void BindExternalFunction(string funcName, Func<object> func, bool lookaheadSafe = false)
```

- **动作**应将`lookaheadSafe`设置为`false`
- **纯函数**应将`lookaheadSafe`设置为`true`

## 外部函数的回退

在测试你的故事时，无论是在Inky中还是在ink-unity集成的播放窗口中，你没有机会在运行故事之前绑定游戏函数。为了解决这个问题，你可以在ink中定义一个回退函数，当找不到外部函数时会运行它。只需创建一个与外部函数同名和参数的ink函数即可。例如，对于上面的`multiply`示例，创建以下ink函数：

```ink
=== function multiply(x,y) ===
// 通常外部函数只能返回占位符结果，否则它们应该在ink中定义！
~ return 1
```

# 多个并行流程（BETA）

可以拥有多个并行的“流程”，从而实现以下场景：

- 两个角色在背景中交谈，而主角与其他人对话。主角可以离开并插入背景对话。
- 非阻塞的交互：你可以与一个对象交互，生成一系列选择，但“暂停”它们，然后去与其他东西交互。原始对象的选择不会阻止你与新的对象交互，你可以在稍后恢复它们。

API相对简单：

- `story.SwitchFlow("Your flow name")` - 创建一个新的流程上下文，或切换到现有的流程。名称可以是任意你喜欢的字符串，不过你可以选择使用与入口结相同的名称，稍后可以使用`story.ChoosePathString("knotName")`来选择。
- `story.SwitchToDefaultFlow()` - 在开始切换流程之前，有一个隐式的默认流程。要返回到它，调用此方法。
- `story.RemoveFlow("Your flow name")` - 销毁之前创建的流程。如果该流程当前处于活动状态，它将返回到默认流程。
- `story.aliveFlowNames` - 当前存活的流程的名称。如果流程之前被切换过且未被销毁，则视为存活。不包括默认流程。
- `story.currentFlowIsDefaultFlow` - 如果默认流程当前处于活动状态，则返回true。根据定义，如果不使用流程功能，也会返回true。
- `story.currentFlowName` — 包含当前活动流程名称的字符串。可能包含默认流程的内部标识符，因此请先使用`currentFlowIsDefault`进行检查。

# 使用LISTs

Ink列表是ink引擎中较为复杂的类型，因此与它们的交互比`int`、`float`和`string`稍微复杂一些。

列表始终需要知道其项目的来源。例如，在ink中你可以这样做：

```ink
~ myList = (Orange, House)
```

即使`Orange`可能来自一个名为`fruit`的列表，而`House`可能来自一个名为`places`的列表。在ink中，这些源列表在编写时会自动解析。然而，在游戏代码中工作时，你必须更加明确，并告诉引擎你的项目属于哪个源列表。

要创建一个包含单一源项目的列表，并将其分配给游戏中的变量：

```csharp
var newList = new Ink.Runtime.InkList("fruit", story);
newList.AddItem("Orange");
newList.AddItem("Apple");
story.variablesState["myList"] = newList;
```

如果你在修改一个列表，并且你知道它已经有/曾经有来自特定源的元素：

```csharp
var fruit = story.variablesState["fruit"] as Ink.Runtime.InkList;
fruit.AddItem("Apple");
```

注意，ink中的单个列表项，例如：

```ink
VAR lunch = Apple
```

实际上只是包含单个项目的列表，而不是不同的类型。因此，要在游戏端创建它们，只需使用上述技术创建一个仅包含一个项目的列表。

你还可以从项目中创建列表，如果你明确知道所有项目的元数据——即源名称以及分配给它的整数值。这在从现有列表构建列表时非常有用。注意，`InkLists`实际上派生自`Dictionary<InkListItem, int>`，其中键是`InkListItem`（它又有`originName`和`itemName`字符串），值是整数值：

```csharp
var newList = new Ink.Runtime.InkList();
var fruit = story.variablesState["fruit"] as Ink.Runtime.InkList;
var places = story.variablesState["places"] as Ink.Runtime.InkList;

foreach(var item in fruit) {
    newList.Add(item.Key, item.Value);
}
foreach (var item in places) {
    newList.Add(item.Key, item.Value);
}

story.variablesState["myList"] = newList;
```

要测试你的列表是否包含特定项目：

```csharp
fruit = story.variablesState["fruit"] as Ink.Runtime.InkList;
if( fruit.ContainsItemNamed("Apple") ) {
    // 我们今晚要吃苹果！
}
```

列表还暴露了许多在ink中可以进行的操作：

- `list.minItem`    // 相当于在ink中调用`LIST_MIN(list)`
- `list.maxItem`    // 相当于在ink中调用`LIST_MAX(list)`
- `list.inverse`    // 相当于在ink中调用`LIST_INVERT(list)`
- `list.all`        // 相当于在ink中调用`LIST_ALL(list)`
- `list.Union(otherList)`      // 相当于在ink中`(list + otherList)`
- `list.Intersect(otherList)`  // 相当于在ink中`(list ^ otherList)`
- `list.Without(otherList)`    // 相当于在ink中`(list - otherList)`
- `list.Contains(otherList)`   // 相当于在ink中`(list ? otherList)`

# 使用编译器

预编译你的故事比在运行时加载.ink文件更高效。尽管如此，在某些情况下，加载.ink文件是一个有用的方法，可以通过以下代码实现：

```csharp
// inkFileContents: 链接的TextAsset，或Resources.Load，甚至StreamingAssets
var compiler = new Ink.Compiler(inkFileContents);
Ink.Runtime.Story story = compiler.Compile();
Debug.Log(story.Continue());
```

注意，如果你的故事使用`INCLUDE`分为多个ink文件，你需要使用：

```csharp
var compiler = new Ink.Compiler(inkFileContents, new Compiler.Options
{
countAllVisits = true,
fileHandler = new UnityInkFileHandler(Path.GetDirectoryName(inkAbsoluteFilePath))
});
Ink.Runtime.Story story = compiler.Compile();
Debug.Log(story.Continue());
```
