# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not in any particular order and may change, be postponed, or discarded.

For the long-term update plans for UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates about UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

Updates and changelogs in Simplified Chinese will be prioritized, while other languages will use GPT for translation during **version number changes**.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect scaling at 640x480[^2]
- [ ] Refine/UI Dr style MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW Class
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr style chase system

### Story System
- [ ] Introduce [Fungus](https://github.com/snozbot/fungus) library for creating a visual dialogue editing system
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redesign
- [ ] Visual turn system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogues during turns
- [ ] Add multiple combats[^4]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Unusual Frames
- [ ] Fix unusual frames
- [ ] Redesign collision system to accommodate unusual frames

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgments
- [ ] Complete seven types of player soul colors

## New Class
### Scene Expansion
- [ ] Add multilingual support in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene

### Rendering Expansion
- [x] Add built-in projection box
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add CC subtitle system[^5]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other Class
### Project Localization
- [x] Merge Chinese and English branches, replacing comments through scripts

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Mainly adding features such as calculating beats.
[^2]: Will attempt to add two additional options in the settings to allow the use of pixel-perfect filters / scaling to a larger resolution from 640x480, rather than directly using a larger resolution to save performance.
[^3]: This script will encapsulate all Unity's Debug related functions to call them, which will only execute in the editor.
[^4]: Allowing for multiple battles and multiple combats within the game.
[^5]: Localization personnel can provide localized support to players in advance by releasing a CC subtitle preview version once the translation is complete but not officially embedded.