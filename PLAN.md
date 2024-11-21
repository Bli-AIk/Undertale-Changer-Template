# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans do not have any specific order and may change, be put on hold, or be abandoned.

For UCT's long-term update plans, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For the specific update content of UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The Simplified Chinese update plans/changelogs will be updated first, while other languages will be translated using GPT when **the version number changes**.

## Table of Contents

[General](#General): Includes plans for improvements to basic/general functions within the template.

[OW Class](#OW Class): Includes redo plans for various systems involved in the Overworld scenario.

[Battle Class](#Battle Class): Includes plans for the expansion and restructuring of the battle system.

[New Class](#New Class): Includes plans for new scenes and functions.

[Others](#Others): Includes plans other than the four categories above.

[On Hold](#On Hold): Includes plans that were once proposed but are now on hold for various reasons.

## General
### Existing System Refinement
- [x] Refine the BGM controller[^1]
- [ ] Store item data to ScriptableObject

### UI Optimization and Refinement
- [ ] Add magnification to 640x480[^2]
- [ ] Refine the MENU interface and OW save interface with UI Dr
- [ ] Refine the settings interface
- [ ] Redesign the settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling them all at once

## OW Class
### Event System
- [ ] Redo the OW event system
- [ ] Add a Dr-style chase system

### Story System
- [ ] Introduce the [ink](https://github.com/inkle/ink) scripting language to the story system
- [ ] Add story system proofreading scenes[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data to ScriptableObject

## Battle Class
### Data Storage
- [x] Store bullet data to ScriptableObject
- [ ] Store turn data to ScriptableObject

### Turn System Redo
- [ ] Visualize the turn system
- [ ] Add path bullet generator

### Battle System Expansion
- [ ] Add a feature to insert subtitles/dialogues during turns
- [ ] Add multiple battles[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Box
- [ ] Fix the irregular box
- [ ] Redo the collision system to accommodate the irregular box

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven types of player soul colors

## New Class
### Scene Expansion
- [ ] Add multilingual expansion in the naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene
- [ ] Add credits/thank you scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection box
- [x] Add 3D renderer

### Function Expansion
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Add customization support for weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches, replacing comment text through scripts

### Cleanup and Maintenance
- [x] Remove redundant original artwork textures

### Tutorials
- [ ] New wiki and video tutorials

## On Hold
- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library

---

[^1]: Primarily to add functions like calculating beats, etc.
[^2]: Will attempt to add an extra option in the settings that allows the game display to be set to a low resolution while enlarging the display resolution. This is friendlier for computers with lower performance.
[^3]: This script wraps all Unity Debug-related functions within it to call; these functions execute only in the editor.
[^4]: This scene will list all story content, consistent with the display method in the game, facilitating quick proofreading.
[^5]: Allowing multiple battles and instances of fighting in the game.
[^6]: Localizers can provide localization support to players in advance by publishing a CC subtitle preview version, even before the translation is fully embedded.