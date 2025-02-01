# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any particular order and are subject to change, suspension, or abandonment.

For the long-term update plans for UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update contents of UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized for updates, while other languages will be translated using GPT during **version number changes**.

## Table of Contents

[General](#General): Contains improvement plans for basic/general functions within the template.

[OW Class](#OW Class): Contains remaking plans for various systems related to the Overworld scene.

[Battles Class](#Battles Class): Contains expansion and reconstruction plans for the battle system.

[New Class](#New Class): Contains plans for newly added scenes and functions.

[Others Class](#Others Class): Contains plans outside of the above four categories.

[On Hold Class](#On Hold Class): Contains some plans that were once proposed but are now on hold for various reasons.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Store item data into ScriptableObject

### UI Optimization and Refinement
- [ ] Add low-resolution magnification display function[^2]
- [ ] Separate UI rendering logic and execution logic for MENU interface, settings interface, and OW save interface
- [ ] Refine functionality of MENU interface, settings interface, and OW save interface

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutine to call language pack instead of calling all at once

## OW Class
### Event System
- [x] Redo OW event system
- [ ] Add Dr-style chase system

### Story System
- [ ] Introduce a visual story editor
- [ ] Add story system proofreading scene[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data into ScriptableObject

## Battles Class
### Data Storage
- [x] Store bullet data into ScriptableObject
- [ ] Store turn data into ScriptableObject

### Turn System Redo
- [ ] Add visual turn system editor
- [ ] Add path bullet generator

### Battle System Expansion
- [ ] Add functionality to insert subtitles/dialogue during turns
- [ ] Add multiple battles[^5]

### Fixes and Optimization
- [x] Optimize 3D backgrounds
- [x] Fix blue heart

### Shape Frames
- [ ] Fix shape frames
- [ ] Redo collision system to accommodate shape frames

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete seven player soul colors

## New Class
### Scene Expansion
- [x] Add multilingual expansion in the naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene
- [ ] Add credits/thank you scene
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
- [ ] Add buff system[^7]
- [ ] Add UI manager
- [ ] Add support for custom extensions of weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Others Class
### Project Localization
- [x] Merge Chinese and English branches, replacing comment text via scripts
- [ ] Add rich text TAG: customizable optional translated names (for selective translation of names, etc.)

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorial
- [ ] New wiki and video tutorials

## On Hold Class

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online libraries
- [ ] Add team system
- [ ] Add task system

---

[^1]: Mainly, it will add functionalities like calculating beats, etc.
[^2]: Will attempt to add an extra option in the settings to allow the game to be displayed at low resolution while magnifying the display resolution. This is friendlier for lower-performance computers.
[^3]: This script will encapsulate all Unity-related Debug functions for internal calls, which will execute only within the editor.
[^4]: This scene will list all story contents consistent with in-game display methods for quick proofreading.
[^5]: That is, allowing multiple battles and battles within the game.
[^6]: Localization personnel can provide localization support to players in advance by releasing a CC subtitle preview version during the stage where translations are completed but not formally embedded.
[^7]: The KR system is realized through Buffs.