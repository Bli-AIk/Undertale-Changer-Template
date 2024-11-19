# UCT Update Plan

This document describes the long-term update plan for UCT.

Note that these update plans are not in any particular order and may change, be postponed, or abandoned.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update content of UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/changelogs in Simplified Chinese are prioritized, while other languages will be translated using GPT upon **version number changes**.

## General
### Refinement of Existing Systems
- [x] Refine BGM controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect scaling and enlarging to 640x480[^2]
- [ ] Refine / UI Dr styled MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Call language packs using coroutines instead of calling all at once

## OW
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr style chase system

### Story System
- [ ] Introduce [ink](https://github.com/inkle/ink) scripting language to the story system 
- [ ] Add proofreading scene for the story system[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store round data in ScriptableObject

### Round System Redesign
- [ ] Visualize round system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add the ability to insert subtitles/dialogues during rounds
- [ ] Add multiple combats[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Odd Frame
- [ ] Fix odd frame
- [ ] Redesign collision system to accommodate odd frames

### Judgement and Collision
- [ ] Refine FIGHT related judgements
- [ ] Add monster death judgement
- [ ] Complete seven player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene
- [ ] Add production team list/thank you scene
- [ ] Add store scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Function Expansion
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other
### Project Localization
- [x] Merge Chinese and English branches, replacing comment texts via script

### Cleaning and Maintenance
- [x] Remove unnecessary original textures

### Tutorial
- [ ] New wiki and video tutorials

---

[^1]: Primarily will add features such as calculating beat counts.
[^2]: Will attempt to add two additional options in the settings to allow you to use pixel-perfect filters / enlarge to a larger resolution at 640x480 instead of directly using a larger resolution to save performance.
[^3]: This script will encapsulate all Unity's Debug related functions for calling, which are executed only within the editor.
[^4]: This scene will list all story content, consistent with in-game display, for quick proofreading.
[^5]: Allowing for multiple battles and several combats within the game.
[^6]: Localization personnel can provide localized support in advance by releasing a CC subtitle preview version after translation is completed but not formally integrated.