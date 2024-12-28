# UCT - 贡献指南

本文为Undertale-Changer-Template的贡献指南。

这篇贡献指南可能相当地不完善。如果你有任何改进建议，欢迎通过以下方式反馈：

- [创建 Issue](https://github.com/Bli-AIk/Undertale-Changer-Template/issues)
- 加入 UCT 的 [QQ 交流群](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=wK7VVbI0VU8mPpG94nDLsHaLRbky5cMT&authKey=LQfQsHtXTqbrRdmhSgUtxesNz9UmiETjymrhJF%2BFT1pAiEy0HUxSfM%2Fx%2FsTdOUC%2F&noverify=0&group_code=289694657)
- 在 [Discord 服务器](https://discord.gg/xvYKa2pSN6) 提出修改建议

如果你希望参与贡献，但在阅读此指南后仍感到困惑，或不熟悉 Github 的 Issue 和 Pull Request 等功能，欢迎随时与我联系。

## 行为守则

在为项目贡献之前，请务必先阅读，并遵守 [**行为守则**](CODE_OF_CONDUCT_zh-CN.md)。

## 基本要求
在为项目贡献之前，请确保你全程使用 Unity 2021.3.15f1 LTS 国际版进行开发。

请确保你的所有改动都是为了解决项目本身的问题，而不是针对构建环境、IDE 等外部工具的限制或问题。

请尽量使用 **Rider** 进行编码，以遵循项目内的代码格式和风格，保持代码的一致性。

提交改动前，请确保你的修改已经通过充分的测试。

请为 **Develop** 分支进行贡献，而非 **Main** 分支。

## 目录
- [报告 BUG](#报告 BUG)
- [提议更改](#提议更改)
- [受理问题](#受理问题)
- [提交 Pull Request](#提交 Pull Request)
---

### 报告 BUG

在报告 BUG 之前，请先浏览 [Issue 列表](https://github.com/motion-canvas/motion-canvas/issues)。可能已经有与你现有问题相关的讨论。

如果有相关讨论，请勿重复提交 Issue，而是在对应的讨论中分享你的观点。

如果没有找到相关讨论，欢迎随时提交新 Issue，并将 Issue 命名为 “Bug: xxx”，然后等待审阅。

审阅后的 Issue 会加上 **c-accepted** 标签，这表示此 Issue 可以被受理。

### 提议更改

如果你打算对此项目进行更改，请先 [创建 Issue](https://github.com/Bli-AIk/Undertale-Changer-Template/issues) ，并将 Issue 命名为 “Feat: xxx”，然后等待审阅。

我们在对应的Issue下进行讨论，如果你的计划可行，那么我会给对应的 Issue 加上 **c-accepted** 标签。然后你就可以开干了。

如果你只是修复 Bug 或修改错别字，可以直接提交 Pull Request，但请确保其中包含对 Bug 的清晰简洁的描述。

### 受理问题

在处理问题之前，请确保该问题已标注 **c-accepted** 标签，且尚未被他人受理，以避免重复工作。

开始处理后，请在该问题下留言通知其他人。

若某人已受理某问题但两周内未有跟进，您可接手处理，但需在评论中说明接手情况，以告知他人。


### 提交 Pull Request

1. Fork 本仓库，确保包含 **develop** 分支。

2. 在你的 Fork 仓库中为你的更改创建一个新分支。

3. 更新代码。

4. 使用描述性提交信息提交更改。

5. 将分支推送到 GitHub。

6. 在 GitHub 上向主分支发送 Pull Request ，并**请求审查**。

## 致谢

本贡献指南部分参考了 [React](https://reactjs.org/docs/how-to-contribute.html) 和 [Angular](https://github.com/angular/angular/blob/main/CONTRIBUTING.md) 的贡献指南。