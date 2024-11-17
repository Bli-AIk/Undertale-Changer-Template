# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these updates are not in any particular order and may change, be postponed, or abandoned.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update details about UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plan/change log in Simplified Chinese will be prioritized. Other languages will be translated using GPT during **version number changes**.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and upscale to 640x480[^2]
- [ ] Refine / UI Dr-style MENU and OW save interface
- [ ] Refine settings interface
- [ ] Redo settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr-style chase system

### Story System
- [ ] Introduce [ink](https://github.com/inkle/ink) scripting language into the story system
- [ ] Add story system proofreading scene[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store round data in ScriptableObject

### Round System Redesign
- [ ] Visualize round system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add feature to insert subtitles/dialogue during rounds
- [ ] Add multiple battles[^5]

### Fixes and Optimizations
- [x] Optimize 3D backgrounds
- [x] Fix blue heart

### Asymmetric Frame
- [ ] Fix asymmetric frame
- [ ] Redo collision system to accommodate asymmetric frame

### Judgement and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM airing scene

### Rendering Expansion
- [x] Add built-in projection frame
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
- [x] Merge Chinese and English branches by replacing comment text through scripts

### Cleanup and Maintenance
- [x] Remove unnecessary original textures

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Mainly will add features like calculating beats.
[^2]: Will attempt to add two additional options in the settings, allowing you to use a pixel-perfect filter / upscale to a higher resolution from 640x480, rather than directly using a higher resolution to save performance.
[^3]: This script will encapsulate all Unity's debug-related functions in order to call them, and these functions will execute only in the editor.
[^4]: This scene will list all story content in a manner consistent with what is displayed in-game for quick proofreading.
[^5]: This allows for multiple battles in the game.
[^6]: Localization personnel can provide pre-localization support to players by releasing a preview version of CC subtitles at the stage when translation is complete but not formally embedded.