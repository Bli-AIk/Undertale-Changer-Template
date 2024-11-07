# CHANGELOG 更新日志

Currently, there is no English version of the update log available. I apologize for any inconvenience this may cause.

目前，更新日志暂时没有英文版本，敬请谅解。


## v1.0.2 - 前言

首先我犯了一个大错——UCT自更新至v1.0.2至今，已经经过了非常多的更新，但是它的版本号仍然停留在v1.0.2。这是长期以来忽视更新日志所带来的结果。

事实上，当初将UCT的版本更新至v1.0.0，在某种意义上是错误的——那时我认为我的模板已经制作完毕，但现在看来仍然差得多。但不管怎样，版本号再改回去会更麻烦。所以，先这样吧。

在v1.0.2的最后一个更新里，我新增了这个文件——**CHANGELOG.md**，作为更新日志记录每次更新的内容和下次更新的计划。

版本号更新会依据这个规则：

1. 当**PATCH**更新时(1.0.X之中的**X**)，通常意味着这次更新是bug修复或原有功能修改/重构，而没有增加新的功能或内容。
2. 当**MINOR**更新时(1.X.0之中的**X**)，通常意味着这次更新添加了新的功能或内容。
3. 当**MAJOR**更新时(X.0.0之中的**X**)，通常意味着…… <small>*好吧，我估计MAJOR永远都不会修改。*</small>
4. 在更新到下一个版本之前时，版本号后会加上后缀 -part X。X为完成的任务项的项数。

更新的格式如下：

#### vX.X.0 -> vX.X.1 更新内容概述

- [x] 任务1（已完成）
- [x] 任务2（已完成）

XX年/XX月/XX日 计划
XX年/XX月/XX日 完成

#### vX.X.1 -> vX.X.2 更新内容概述

- [ ] 任务1（未完成）
- [ ] 任务2（未完成）

XX年/XX月/XX日 计划 (x-x项数)
XX年/XX月/XX日 完成



完成的时间即最后一项完成的时间，以上。

2024/11/4 计划



2024/11/4 完成


## v1.0.2 -> v1.0.3 BUG修复
- [x] 修复Battle场景中执行Fight后敌人对话卡死的问题
- [x] 修复Battle场景中使用item后游戏/编辑器崩溃的问题
- [x] 修复Battle场景中在ButtonLayer打字机输入未完毕时使用item后打字机显示仍为ButtonLayer文本的问题
- [x] 修复OW场景中仍然使用废弃框UI的问题
- [x] 修复打字机无法识别\<X\>富文本的问题
- [x] 增加新的富文本\<JumpText\>用于跳过打字机输入大段文本



2024/11/4 计划 (1-2, 4-5)

2024/11/5 计划 (3, 6)



2024/11/6 完成

附：OW场景UI修复目前仅在长廊场景有效，将在v1.0.4修复。

## v1.0.3 -> v1.0.4 UI显示修复

- [ ] 将OW场景中的框UI轴点均改为正中
- [ ] 将主摄像机放置在DontDestroyOnLoad，由MainControlSummon生成（类似于MainControl、BGMControl的生成方式）
- [ ] 对各个场景UI位置、大小进行检查和修复
- [ ] 使用\<indent\>替换透明字符缩进
- [ ] 将英文字体进行调整，修复字符重合的问题
- [ ] 引进[Fusion](https://github.com/TakWolf/fusion-pixel-font)8px字体
- [ ] 在起名界面引入拉丁字符、数字与中日韩字符等



2024/11/4 计划 (1-2)

2024/11/6 计划 (3-4)

2024/11/7 计划 (5-7)

## v1.0.4 -> v1.0.5 设置页面重构与拓展

- [ ] 对CanvasController进行拆分与重构
- [ ] 细化设置项：音频设置、视频设置等大设置项，以及具体开关后处理/光效等小设置项。
- [ ] 提升设置页面拓展性并加入若干新的设置选项。



2024/11/4 计划 (1-3)