# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any specific order and are subject to change, be put on hold, or be abandoned.

For the long-term update plans of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, and updates in other languages will be translated using GPT during **version number changes**.

## General
### Refining Existing Systems
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and upscale to 640x480[^2]
- [ ] Refine / UI Dr fraction MENU and OW save interface
- [ ] Refine settings interface
- [ ] Redo settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs, rather than calling all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr style chase system

### Plot System
- [ ] Introduce [ink](https://github.com/inkle/ink) scripting language to the plot system 
- [ ] Add plot system proofing scene[^4]
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redo
- [ ] Visualize the turn system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add the ability to insert subtitles/dialogue during turns
- [ ] Add multiple combats[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Asymmetrical Box
- [ ] Fix asymmetrical box
- [ ] Redo collision system to adapt to the asymmetrical box

### Judgement and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion in the naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene
- [ ] Add credits/thank you scene

### Rendering Expansion
- [x] Add built-in projection box
- [x] Add 3D renderer

### Function Expansion
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other
### Project Localization
- [x] Merge Chinese and English branches, replacing comment text through scripts

### Cleanup and Maintenance
- [x] Remove unnecessary original texture maps

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Primarily adding functions like calculating the beat count.
[^2]: Will attempt to add two extra options in the settings to allow for pixel-perfect filters / upscaling to larger resolutions at 640x480 instead of directly using larger resolutions to save performance.
[^3]: This script will encapsulate all Unity's Debug-related functions for calling, which will only execute within the editor.
[^4]: This scene will list all storyline content in a manner consistent with how it displays in-game for quick proofing.
[^5]: That is, allowing for multiple battles in-game and multiple instances of combat.
[^6]: Localizers can provide localization support in advance for players through the release of a CC subtitle preview version at the stage where translation is completed but not officially embedded.