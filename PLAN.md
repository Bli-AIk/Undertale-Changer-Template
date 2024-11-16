# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any specific order and may change, be put on hold, or be abandoned.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

Simplified Chinese update plans/changelogs will be prioritized for updates; other languages will be translated using GPT during **version number changes**.

## General
### Refinement of Existing Systems
- [x] Refine BGM controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and scale to 640x480[^2]
- [ ] Refine / UI Dr. style MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling them all at once

## OW Category
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr. style chase battle system

### Plot System
- [ ] Introduce [ink](https://github.com/inkle/ink) scripting language into plot system
- [ ] Add plot system proofreading scene[^4]
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Category
### Data Storage
- [x] Store bullet curtain data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redesign
- [ ] Visualize turn system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogue during turns
- [ ] Add multiple battles[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Box
- [ ] Fix irregular box
- [ ] Redesign collision system to accommodate irregular box

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete seven player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion in naming scene
- [ ] Add battle conclusion scene
- [ ] Add BGM broadcasting scene

### Rendering Expansion
- [x] Add built-in projection box
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other Categories
### Project Localization
- [x] Merge Chinese and English branches, replace comment texts through scripts

### Cleanup and Maintenance
- [x] Delete unnecessary original textures

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Mainly will add functionalities like calculating beat counts.
[^2]: Will try to add two extra options in the settings to allow using pixel-perfect filter / scaling to larger resolutions from 640x480, rather than directly using higher resolutions to save performance.
[^3]: This script will encapsulate all Unity's debug-related functions for calling, which will only execute in the editor.
[^4]: This scene will list all narrative content consistent with in-game display style for quick proofreading.
[^5]: Allows multiple battles and engagements within the game.
[^6]: Localization personnel can provide localization support to players ahead of the official embedding phase by releasing a CC subtitle preview version after translation completion.