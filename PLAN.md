# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any specific order and may change, be shelved, or abandoned.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates regarding UCT, please check [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, while other languages will use GPT for translation during **version changes**.

## Table of Contents

[General](#General): Contains improvement plans for basic/general functions within templates.

[OW Class](#OW Class): Contains redo plans for systems involved in the Overworld scenes.

[Combat Class](#Combat Class): Contains expansion and restructuring plans for the combat system.

[New Additions](#New Additions): Contains plans for new scenes and functions.

[Others](#Others): Contains plans that fall outside the above four categories.

[On Hold](#On Hold): Contains some previously planned items that are currently shelved for various reasons.

## General
### Refinement of Existing Systems
- [x] Refine BGM controller[^1]
- [ ] Store item data to ScriptableObject

### UI Optimization and Refinement
- [ ] Add zoom feature for 640x480[^2]
- [ ] Refine / rework UI Dr MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Call language packs using coroutines instead of calling all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr-style chase battle system

### Story System
- [ ] Introduce a visual story editor
- [ ] Add story system proofreading scene[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data to ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data to ScriptableObject
- [ ] Store turn data to ScriptableObject

### Turn System Redesign
- [ ] Add visual turn system editor
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogue during turns
- [ ] Add multiple battles[^5]

### Bugs and Optimization
- [x] Optimize 3D background
- [x] Fix blue heart

### Monster Frame
- [ ] Fix monster frame
- [ ] Redo collision system to adapt to monster frame

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgments
- [ ] Complete the seven player soul colors

## New Additions
### Scene Expansion
- [x] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene
- [ ] Add staff credits/thanks scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection box
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add MIDI detection system
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Add custom expansion support for weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add color blind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches, replacing comment texts through scripts

### Cleanup and Maintenance
- [x] Remove unnecessary original textures

### Tutorials
- [ ] New wiki and video tutorials

## On Hold

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library

---

[^1]: Mainly will add functions such as calculating beats.
[^2]: Will attempt to add an option in settings that allows the game to set a low resolution while magnifying the display resolution. This is more friendly for lower performance computers.
[^3]: This script will encapsulate all of Unity’s Debug-related functions for calling, which only execute within the editor.
[^4]: This scene will list all story content, consistent with the game's display method, making proofreading easier.
[^5]: Allowing multiple battles and battles within the game.
[^6]: Localization personnel can provide players localization support in advance by releasing a preview version of CC subtitles during the phase of completed but not officially embedded translations.