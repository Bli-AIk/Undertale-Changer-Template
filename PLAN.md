# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not in any particular order and may change, be postponed, or abandoned.

For UCT's long-term update plans, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates about UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, while updates in other languages will be translated using GPT during **version number changes**.

## Table of Contents

[General](#General): Contains improvement plans for basic/general functions within the template. 

[OW Class](#OW-Class): Contains redo plans for various systems related to Overworld scenes.

[Battles Class](#Battles-Class): Contains expansion and reconstruction plans for the battle system.

[New Class](#New-Class): Contains plans for new scenes and features.

[Others Class](#Others-Class): Contains plans beyond the four mentioned categories.

[Postponed Class](#Postponed-Class): Contains plans that were once considered but have since been postponed for various reasons.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Store item data to ScriptableObject

### UI Optimization and Refinement
- [ ] Add low-resolution magnification display function[^2]
- [ ] Separate MENU interface, settings interface, and OW save interface UI rendering logic and specific execution logic
- [ ] Refine MENU interface, settings interface, and OW save interface functions

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr-style chase system

### Story System
- [ ] Introduce visual story editor
- [ ] Add story system proofing scene[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data to ScriptableObject

## Battles Class
### Data Storage
- [x] Store bullet data to ScriptableObject
- [ ] Store round data to ScriptableObject

### Round System Redo
- [ ] Add visual round system editor
- [ ] Add path bullet generator

### Battle System Expansion
- [ ] Add function to insert subtitles/dialogues during rounds
- [ ] Add multiple battles[^5]

### Fixes and Optimizations
- [x] Optimize 3D background
- [x] Fix blue heart

### Shape Frame
- [ ] Fix shape frame
- [ ] Redo collision system to accommodate shape frame

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgments
- [ ] Complete seven types of player soul colors

## New Class
### Scene Expansion
- [x] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene
- [ ] Add staff credits/thank you scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Function Expansion
- [ ] Add MIDI detection system
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Add support for custom expansion of weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Others Class
### Project Localization
- [x] Merge Chinese and English branches by replacing comment text through scripts

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki and video tutorials

## Postponed Class

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library

---

[^1]: Mainly will add functions like calculating beat counts, etc.
[^2]: Will try to add an extra option in the settings that allows the game's graphics to be set to low resolution while enlarging the display resolution. This is more friendly for lower-performing computers.
[^3]: This script will encapsulate all Unity's Debug related functions to be called from it, and these functions will only execute within the editor.
[^4]: This scene will list all story content, consistent with the game's display method, for quick proofreading.
[^5]: Meaning allowing multiple battles within the game and multiple battles.
[^6]: Localization personnel can provide localized support to players in advance by releasing a CC subtitle preview version during the stage when the translation is completed but not formally embedded.