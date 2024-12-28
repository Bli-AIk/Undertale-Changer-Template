# UCT - 貢獻指南

本文為Undertale-Changer-Template的貢獻指南。

這篇貢獻指南可能相當不完善。如果你有任何改進建議，歡迎透過以下方式回饋：

- [創建 Issue](https://github.com/Bli-AIk/Undertale-Changer-Template/issues)
- 加入 UCT 的 [QQ交流群](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=wK7VVbI0VU8mPpG94nDLsHaLRbky5cMT&authKey=LQfQsHtX TqbrRdmhSgUtxesNz9UmiETjymrhJF%2BFT1pAiEy0HUxSfM%2Fx%2FsTdOUC%2F&noverify=0&group_code=289694657)
- 在 [Discord 伺服器](https://discord.gg/xvYKa2pSN6) 提出修改建議

如果你希望參與貢獻，但在閱讀此指南後仍感到困惑，或不熟悉 Github 的 Issue 和 Pull Request 等功能，歡迎隨時與我聯繫。

## 行為守則

在為專案貢獻之前，請務必先閱讀，並遵守 [**行為守則**](CODE_OF_CONDUCT_zh-CN.md)。

## 基本要求
在為專案貢獻之前，請確保你全程使用 Unity 2021.3.15f1 LTS 國際版進行開發。

請確保你的所有改動都是為了解決專案本身的問題，而不是針對建置環境、IDE 等外部工具的限製或問題。

請盡量使用 **Rider** 進行編碼，以遵循專案內的程式碼格式和風格，保持程式碼的一致性。

提交改動前，請確保你的修改已經通過充分的測試。

請為 **Develop** 分支進行貢獻，而非 **Main** 分支。

## 目錄
- [報告 BUG](#報告 BUG)
- [提議更改](#提議更改)
- [受理問題](#受理問題)
- [提交 Pull Request](#提交 Pull Request)
---

### 報告 BUG

在報告 BUG 之前，請先瀏覽 [Issue 清單](https://github.com/motion-canvas/motion-canvas/issues)。可能已經有與你現有問題相關的討論。

如果有相關討論，請勿重複提交 Issue，而是在對應的討論中分享你的觀點。

如果沒有找到相關討論，歡迎隨時提交新 Issue，並將 Issue 命名為 “Bug: xxx”，然後等待審查。

審閱後的 Issue 會加上 **c-accepted** 標籤，這表示此 Issue 可以被接受。

### 提議更改

如果你打算對此項目進行更改，請先[創建Issue](https://github.com/Bli-AIk/Undertale-Changer-Template/issues) ，並將Issue 命名為“Feat: xxx”，然後等待審閱。

我們在對應的Issue下進行討論，如果你的計劃可行，那麼我會給對應的 Issue 加上 **c-accepted** 標籤。然後你就可以開乾了。

如果你只是修復 Bug 或修改錯字，可以直接提交 Pull Request，但請確保其中包含對 Bug 的清晰簡潔的描述。

### 受理問題

在處理問題之前，請確保問題已標示 **c-accepted** 標籤，且尚未被他人受理，以避免重複工作。

開始處理後，請在該問題下留言通知其他人。

若某人已受理某問題但兩週內未有跟進，您可接手處理，但需在評論中說明接手情況，以告知他人。


### 提交 Pull Request

1. Fork 本倉庫，確保包含 **develop** 分支。

2. 在你的 Fork 倉庫中為你的變更建立一個新分支。

3. 更新程式碼。

4. 使用描述性提交資訊提交更改。

5. 將分支推送到 GitHub。

6. 在 GitHub 上向主分支發送 Pull Request ，並**請求審查**。

## 致謝

本貢獻指南部分參考了[React](https://reactjs.org/docs/how-to-contribute.html) 和[Angular](https://github.com/angular/angular/blob/main/CONTRIBUTING .md) 的貢獻指南。