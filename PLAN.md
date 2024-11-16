# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any particular order and may change, be put on hold, or be discarded.

For the long-term update plan for UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update details regarding UCT, please consult [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

Simplified Chinese updates and changelogs are prioritized; other languages will use GPT for translation during **version number changes**.

## General 
### Existing System Refinement
- [x] Refine BGM controller[^1]
- [ ] Store item data to ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and upscale to 640x480[^2]
- [ ] Refine / UI Dr requirements for MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling them all at once

## OW Class
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr style chasing system

### Plot System
- [ ] Introduce [ink](https://github.com/inkle/ink) scripting language into the plot system 
- [ ] Add proofreading scenes for the plot system[^4]
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data to ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data to ScriptableObject
- [ ] Store turn data to ScriptableObject

### Turn System Redesign
- [ ] Visualize turn system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogue during turns
- [ ] Add multiple combat[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frame
- [ ] Fix irregular frame
- [ ] Redesign collision system to accommodate irregular frame

### Judgment and Collision
- [ ] Refine FIGHT related judgment
- [ ] Add monster death judgment
- [ ] Complete the seven colors of player souls

## New Class
### Scene Expansion
- [ ] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Functional Expansion
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other Class
### Project Localization
- [x] Merge Chinese and English branches, replacing comment text via scripts

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Primarily will add functions such as calculating beats.
[^2]: Will attempt to add two additional options in settings to allow you to use pixel-perfect filters / upscale to a higher resolution rather than directly using a larger resolution to save performance.
[^3]: This script will encapsulate all Unity's Debug related functions for calling, which will be executed only within the editor.
[^4]: This scene will list all narrative content in a manner consistent with the game's display, facilitating quick proofreading.
[^5]: Allowing for multiple fights and battles within the game.
[^6]: Localization personnel can preview CC subtitles during the stage where translations are completed but not formally embedded, to provide localized support to players in advance.