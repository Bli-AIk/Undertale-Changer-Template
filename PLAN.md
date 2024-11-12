# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not listed in any particular order and may be subject to change, suspension, or abandonment.

For the long-term update plans of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update details of UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plan/change log in Simplified Chinese will be prioritized, while other languages will use GPT for translation during **version number changes**.

## General
### Existing System Refinement
- [x] Refine BGM controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and 640x480 magnification[^2]
- [ ] Refine/UI Dr style MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redo settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr style chase system

### Plot System
- [ ] Introduce [Fungus](https://github.com/snozbot/fungus) library to create a visual dialogue editing system
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet curtain data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redo
- [ ] Create a visual turn system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add the ability to insert subtitles/dialogues during turns
- [ ] Add multiple combats[^4]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frame
- [ ] Fix irregular frame
- [ ] Redo collision system to fit the irregular frame

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete seven player soul colors

## New Class
### Scene Expansion
- [ ] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Function Expansion
- [ ] Add CC subtitle system[^5]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other
### Project Localization
- [x] Merge Chinese and English branches by replacing comments through scripts

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorial
- [ ] New wiki and video tutorials

---

[^1]: Mainly will add features such as calculating the number of beats.
[^2]: Will attempt to add two additional options in the settings that allow you to use pixel-perfect filters / magnify to a larger resolution from 640x480, instead of directly using a larger resolution, to save performance.
[^3]: This script will encapsulate all of Unity's Debug-related functions for use, and these functions will only execute within the editor.
[^4]: This allows for multiple battles within the game and multiple combats.
[^5]: Localization personnel can provide localization support to players in advance by releasing CC subtitle preview versions during the stage when translations are completed but not officially embedded.