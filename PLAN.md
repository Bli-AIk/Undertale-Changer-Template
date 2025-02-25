# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not arranged in any particular order and may change, be put on hold, or be abandoned.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update content regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be updated first, while other languages will use GPT for translation upon **version number changes**.

## Directory

[General](#General): Includes improvement plans for basic/general functions within the template.

[OW Class](#OW Class): Includes plans for the redo of various systems related to the Overworld scene.

[Combat Class](#Combat Class): Includes plans for the expansion and reconstruction of the combat system.

[New Class](#New Class): Includes plans for newly added scenes and functions.

[Other Class](#Other Class): Includes plans outside of the four categories above.

[On Hold Class](#On Hold Class): Includes plans that have been previously scheduled but are now on hold for various reasons.

## General
### Existing System Refinement
- [x] Refine BGM controller[^1]
- [x] Use Builder to restructure item system

### UI Optimization and Refinement
- [ ] Add low-resolution magnification display function[^2]
- [ ] Separate the UI rendering logic and specific execution logic of the MENU interface, settings interface, and OW save interface
- [ ] Refine the functions of the MENU interface, settings interface, and OW save interface

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling them all at once

## OW Class
### Event System
- [x] Redo OW event system
- [ ] Add Dr-style chase system
- [ ] Save event system data

### Story System
- [x] Enrich dialogue system[^4]
- [ ] Add proofreading scene for all text[^5]

## Combat Class
### Data Storage
- [x] Store bullet data to ScriptableObject
- [ ] Read Json round data

### Round System Redo
- [ ] Add visual round system editor (set up as a separate project) to generate Json round data
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add the ability to insert subtitles/dialogues during rounds
- [ ] Add multiple battles[^6]

### Fixes and Optimizations
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frame
- [ ] Fix irregular frames
- [ ] Redo collision system to accommodate irregular frames

### Judgement and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete seven colors for player souls

## New Class
### Scene Expansion
- [x] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene
- [ ] Add staff credits/thank you scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Function Expansion
- [ ] Add MIDI detection system
- [ ] Add historical text system
- [ ] Add storage system/extra backpack
- [ ] Add CC subtitle system[^7]
- [ ] Add achievement system
- [ ] Add Buff system[^8]
- [ ] Add UI manager
- [x] Add custom expansion support for weapons/armor
- [ ] Read original saved data

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other Class
### Project Localization
- [x] Merge Chinese and English branches
- [ ] Replace comment text through scripts

### Rich Text

- [ ] Add rich text TAG - customizable optional translation names (for selective translation of names, etc.)
- [ ] Add rich text <waitForConfirm> - pause, press Z to continue but do not clear the screen
- [ ] Add rich text <directUpdate> - clear the screen directly and play without pressing Z

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki and video tutorials

## On Hold Class

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library
- [ ] Add team system
- [ ] Add task system

---

[^1]: Mainly will add functions such as calculating beat counts.
[^2]: Will attempt to add an option in settings to allow the game display to be set to low resolution while magnifying the display resolution. This is more friendly for computers with lower performance.
[^3]: This script will encapsulate all Unity's Debug related functions inside for calling, these functions will only execute within the editor.
[^4]: Achieved by integrating Ink language.
[^5]: This scene will list all text, consistent with in-game display methods, making it easy for quick proofreading.
[^6]: Allowing multiple battles and battles within the game.
[^7]: Localization personnel can provide localized support to players in advance by releasing a CC subtitle preview version after translation is completed but not yet formally embedded.
[^8]: The KR system is realized through Buff.