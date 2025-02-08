# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not listed in any particular order and are subject to change, postponement, or abandonment.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update content regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, and other languages will utilize GPT for translation during **version number changes**.

## Directory

[General](#General): Contains improvement plans for basic/general functionalities within the template.

[OW Class](#OW-Class): Contains plans for remaking systems related to the Overworld scenes.

[Combat Class](#Combat-Class): Contains plans for expanding and restructuring the combat system.

[New Additions](#New-Additions): Contains plans for new scenes and functionalities.

[Other](#Other): Contains plans outside of the above four categories.

[Postponed](#Postponed): Contains some plans that were once scheduled but have been postponed for various reasons.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add low-resolution magnification display feature[^2]
- [ ] Separate UI drawing logic and execution logic for MENU interface, settings interface, and OW save interface
- [ ] Refine functionalities of MENU interface, settings interface, and OW save interface

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW Class
### Event System
- [x] Redo OW event system
- [ ] Add a Dr-style chase system

### Plot System
- [ ] Introduce a visual plot editor
- [ ] Add plot system proofreading scene[^4]
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store turn data in ScriptableObject

### Turn System Redo
- [ ] Add visual turn system editor
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogue during turns
- [ ] Add multiple battles[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frame
- [ ] Fix irregular frame
- [ ] Redo collision system to accommodate irregular frames

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven types of player soul colors

## New Additions
### Scene Expansion
- [x] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene
- [ ] Add credits/thanks scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add MIDI detection system
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add Buff system[^7]
- [ ] Add UI manager
- [ ] Add customizable weapon/armor expansion support

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other
### Project Localization
- [x] Merge Chinese and English branches by replacing comment texts through scripts
- [ ] Add Rich Text TAG: Custom optional translated names (for selective translation of names, etc.)

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki and video tutorials

## Postponed

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online libraries
- [ ] Add team system
- [ ] Add task system

---

[^1]: Mainly will add features such as beat count calculation.
[^2]: An additional setting option will be attempted that allows setting the game screen to a low resolution while magnifying the display resolution. This is more friendly for computers with lower performance.
[^3]: This script will encapsulate all Unity's Debug-related functions for calling, which will only execute within the editor.
[^4]: This scene will list all plot contents, consistent with how they are displayed in the game, facilitating quick proofreading.
[^5]: This allows multiple battles and multiple fights within the game.
[^6]: Localizers can provide localized support to players in advance by releasing preview versions of CC subtitles once the translation is completed but not formally integrated.
[^7]: The KR system is implemented through Buffs.