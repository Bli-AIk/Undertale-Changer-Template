# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not in any particular order and may change, be put on hold, or be abandoned.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update content regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, and other languages will utilize GPT translation upon **version number changes**.

## Table of Contents

[General](#General): Includes improvement plans for basic/general functions within the templates.

[OW Class](#OW Class): Includes plans for redoing various systems involved in the Overworld scenes.

[Combat Class](#Combat Class): Includes expansion and restructuring plans for the combat system.

[New Additions](#New Additions): Includes plans for new scenes and functions.

[Others](#Others): Includes plans beyond the above four categories.

[On Hold](#On Hold): Includes plans that were once planned but are now on hold for various reasons.

## General
### Existing System Refinement
- [x] Refine BGM controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add magnification to 640x480[^2]
- [ ] Refine / UI Dr Menu interface and OW save interface
- [ ] Refine settings interface
- [ ] Redo settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutine to call language packs, instead of calling all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr-style chase battle system

### Story System
- [ ] Introduce [ink](https://github.com/inkle/ink) scripting language into story system
- [ ] Add proofreading scene for story system[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store barrage data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redo
- [ ] Visualize turn system
- [ ] Add path barrage generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogues during turns
- [ ] Add multiple combat[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Asymmetrical Frame
- [ ] Fix asymmetrical frame
- [ ] Redo collision system to accommodate asymmetrical frame

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete seven player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion to naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene
- [ ] Add credits/thank you scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Add support for custom extensions of weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches, replace comment text through scripts

### Cleanup and Maintenance
- [x] Delete unnecessary original textures

### Tutorials
- [ ] New wiki and video tutorials

## On Hold

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library

---

[^1]: Mainly to add functions such as calculating beats.
[^2]: Will attempt to add an extra option in the settings to allow the game to run at a low resolution while enlarging the display resolution. This is friendlier for computers with lower performance.
[^3]: This script will encapsulate all Unity Debug-related functions for calling, and these functions will only execute in the editor.
[^4]: This scene will list all story contents in a manner consistent with the in-game display, making it easier to proofread quickly.
[^5]: This allows for multiple battles and several combats within the game.
[^6]: Localizers can provide pre-release localization support for players through a CC subtitle preview version during the translation completion but before formal integration.