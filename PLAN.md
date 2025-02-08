# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not listed in any particular order, and may change, be shelved, or discarded.

For the long-term update plan for UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md) or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update contents regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md) or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

Updates in Simplified Chinese will be prioritized, while updates in other languages will use GPT for translation when the **version number changes**.

## Table of Contents

[General](#General): Includes improvement plans for basic/general functions within the template.

[OW](#OW): Includes plans for the redesign of systems involved in Overworld scenes.

[Combat](#Combat): Includes plans for the expansion and reconstruction of the combat system.

[New Features](#New Features): Includes plans for newly added scenes and functions.

[Others](#Others): Includes plans that fall outside the four categories mentioned above.

[On Hold](#On Hold): Includes some plans that were once scheduled but are now on hold for various reasons.

## General
### Refinement of Existing Systems
- [x] Refine BGM controller[^1]
- [ ] Store item data to ScriptableObject

### UI Optimization and Refinement
- [ ] Add low-resolution zoom display feature[^2]
- [ ] Separate the UI rendering logic and specific execution logic of the MENU interface, settings interface, and OW save interface
- [ ] Refine functions of the MENU interface, settings interface, and OW save interface

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW
### Event System
- [x] Redesign the OW event system
- [ ] Add a Dr-style chase system
- [ ] Store event system data

### Story System
- [ ] Enrich the dialogue system[^4]
- [ ] Add proofreading scenes for all text[^5]

## Combat
### Data Storage
- [x] Store bullet data to ScriptableObject
- [ ] Read JSON turn data

### Redesign of Turn System
- [ ] Add a visual turn system editor (separate project) that generates JSON turn data
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogues during turns
- [ ] Add multi-battles[^6]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frames
- [ ] Fix irregular frames
- [ ] Redesign collision system to adapt to irregular frames

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgments
- [ ] Complete seven player soul colors

## New Features
### Scene Expansion
- [x] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene
- [ ] Add credits/acknowledgment scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add MIDI detection system
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^7]
- [ ] Add achievement system
- [ ] Add Buff system[^8]
- [ ] Add UI manager
- [ ] Add custom expansion support for weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches by replacing comment texts with scripts
- [ ] Add rich text TAG: custom optional translation names (for selective translation of names, etc.)

### Cleaning and Maintenance
- [x] Remove redundant original textures

### Tutorial
- [ ] New wiki and video tutorials

## On Hold

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online libraries
- [ ] Add team system
- [ ] Add task system

---

[^1]: Primarily will add functions like calculating beat counts.
[^2]: Will attempt to add an extra item in settings that allows the game display to be set to low resolution while magnifying the display resolution. This is friendly for computers with lower performance.
[^3]: This script will encapsulate all Unity Debug related functions for calling, and these functions will execute only in the editor.
[^4]: The current dialogue system is difficult to support branching choices, logical jumps, etc. Maybe we can try integrating Ink language.
[^5]: This scene will list all texts in a way consistent with in-game display, facilitating quick proofreading.
[^6]: This allows for multiple battles in-game and multiple combats.
[^7]: Localization personnel can provide localization support in advance for players by releasing a CC subtitle preview version after translation is completed but not yet officially integrated.
[^8]: The KR system is realized through Buffs.