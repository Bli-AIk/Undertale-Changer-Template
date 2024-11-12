# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not in any particular order and may change, be postponed, or discarded.

For the long-term update plan regarding UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update content related to UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

Updates in Simplified Chinese will be prioritized, while other languages will use GPT for translation at the **version number change**.

## General
### Refinement of Existing Systems
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and 640x480 scaling[^2]
- [ ] Refine / UI Dr style MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all simultaneously

## OW Category
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr style chase battle system

### Story System
- [ ] Introduce [Fungus](https://github.com/snozbot/fungus) library for creating a visual dialogue editing system
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Battle Category
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redesign
- [ ] Visualize turn system
- [ ] Add path bullet generator

### Battle System Expansion
- [ ] Add the ability to insert subtitles/dialogues during turns
- [ ] Add multiple battles[^4]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix Blue Heart

### Asymmetrical Frame
- [ ] Fix asymmetrical frame
- [ ] Redesign collision system to accommodate asymmetrical frame

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete seven player soul colors

## New Category
### Scene Expansion
- [ ] Add multilingual expansion in the naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene

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
- [ ] Add color blindness filter

## Other Categories
### Project Localization
- [x] Merge Chinese and English branches, replacing comment text via scripts

### Cleanup and Maintenance
- [x] Delete unnecessary original textures

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Primarily will add functionalities like calculating beats.
[^2]: Will try to add two options in the settings to allow you to use pixel-perfect filters / scale up to a larger resolution from 640x480 instead of directly using a larger resolution to save performance.
[^3]: This script will encapsulate all Unity's Debug related functions for use, which are only executed in the editor.
[^4]: That is, allow multiple battles and multiple engagements within the game.
[^5]: Localization personnel can preview localized support for players by releasing a CC subtitle preview version before formal embedding, after the translation is completed.