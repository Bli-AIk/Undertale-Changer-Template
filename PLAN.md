# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any specific order and may change, be put on hold, or be abandoned.

For UCT's long-term update plan, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update details regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, while updates in other languages will be translated using GPT at the time of **version number changes**.

## Table of Contents

[General](#General): Includes improvement plans for foundational/general features within templates.

[OW Class](#OW Class): Includes plans for redoing various systems related to the Overworld scene.

[Combat Class](#Combat Class): Includes expansion and restructuring plans for the combat system.

[New Additions](#New Additions): Includes plans for new scenes and features.

[Others](#Others): Includes plans that do not fall into the above four categories.

[On Hold](#On Hold): Includes plans that were initially set but have been put on hold for various reasons.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Save item data to ScriptableObject


### UI Optimization and Refinement
- [ ] Add magnification to 640x480[^2]
- [ ] Refine / Dr style MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling them all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr-style chase scene system

### Story System
- [ ] Integrate [ink](https://github.com/inkle/ink) scripting language into the story system
- [ ] Add story system proofreading scene[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Save room data to ScriptableObject

## Combat Class
### Data Storage
- [x] Save bullet data to ScriptableObject
- [ ] Save round data to ScriptableObject

### Round System Redo
- [ ] Visualize round system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality for inserting subtitles/dialogue during rounds
- [ ] Add multi-battle[^5]

### Fixes and Optimizations
- [x] Optimize 3D background
- [x] Fix blue core

### Irregular Frame
- [ ] Fix irregular frame
- [ ] Redo collision system to accommodate irregular frame

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete seven types of player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion in the naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene
- [ ] Add credits/thanks scene
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

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches by replacing comment text with scripts

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki and video tutorials

## On Hold
- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library

---

[^1]: Mainly will add features like calculating beats.
[^2]: Will attempt to add an option in the settings to allow the game screen to be set to low resolution while magnifying the display resolution. This is more friendly for computers with lower performance.
[^3]: This script will encapsulate all Unity's Debug-related functions to be called, which are only executed in the editor.
[^4]: This scene will list all story content, consistent with the display method in-game, for quick proofreading.
[^5]: This allows for multiple battles and several battles within the game.
[^6]: Localization personnel can provide early localization support for players by releasing a preview version of CC subtitles during the translation phase before formal embedding.