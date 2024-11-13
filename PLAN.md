# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any particular order, and they may change, be paused, or discarded.

For long-term update plans regarding UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, while updates in other languages will be translated using GPT during **version number changes**.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and upscale to 640x480[^2]
- [ ] Refine / UI Dr styled MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr-style chase battle system

### Plot System
- [ ] Introduce [Fungus](https://github.com/snozbot/fungus) library for creating a visual dialogue editing system
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat
### Data Storage
- [x] Store bullet curtain data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redesign
- [ ] Visual turn system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogue during turns
- [ ] Add multiple combats[^4]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frame
- [ ] Fix irregular frame
- [ ] Redesign collision system to accommodate irregular frame

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven colors of player souls

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add CC subtitle system[^5]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other
### Project Localization
- [x] Merge Chinese and English branches by replacing commented text through scripts

### Cleanup and Maintenance
- [x] Remove unnecessary original artwork textures

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Mainly will add features such as calculating beats.
[^2]: Will attempt to add two additional settings options, allowing you to use a pixel-perfect filter / upscale to a larger resolution rather than directly using a larger resolution to save performance.
[^3]: This script encapsulates all Unity's related debug functions for calls that execute only in the editor.
[^4]: This allows multiple battles and combats to occur within the game.
[^5]: Localization personnel can provide localization support for players in advance by releasing a preview version of CC subtitles after translation is completed but not formally embedded.