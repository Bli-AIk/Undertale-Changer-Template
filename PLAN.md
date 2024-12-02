# UCT Update Plan

This document outlines the long-term update plans for UCT.

Note that these update plans are not in any particular order and may change, be shelved, or abandoned.

For more information on UCT's long-term update plans, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specifics on UCT's update contents, please check [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The Simplified Chinese update plan/change log will be prioritized, while other languages will be translated using GPT during **version changes**.

## Directory

[General](#General): Contains improvement plans for basic/general functionalities within the template.

[OW Class](#OW-Class): Contains plans for the redesign of various systems involved in the Overworld scenes.

[Combat Class](#Combat-Class): Includes plans for the expansion and reconstruction of the combat system.

[New Additions](#New-Additions): Contains plans for new scenes and features.

[Other](#Other): Contains plans outside the above four categories.

[On Hold](#On-Hold): Contains some plans that were once in consideration but are currently shelved for various reasons.

## General
### Refinement of Existing Systems
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add magnification for 640x480[^2]
- [ ] Refine UI Dr to MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redo settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutine to call language packs instead of calling them all at once

## OW Class
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr style chase battle system

### Story System
- [ ] Introduce a visual story editor
- [ ] Add proofreading scenes for the story system[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store round data in ScriptableObject

### Turn System Redesign
- [ ] Add a visual turn system editor
- [ ] Add path bullet generator

### Expansion of Combat System
- [ ] Add functionality to insert subtitles/dialogue during turns
- [ ] Add multiple battles[^5]

### Fixes and Optimization
- [x] Optimize 3D backgrounds
- [x] Fix blue heart

### Irregular Frames
- [ ] Fix irregular frames
- [ ] Redesign collision system to adapt to irregular frames

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgments
- [ ] Complete the seven player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual expansion in naming scene
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
- [ ] Add UI manager
- [ ] Add customization support for weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other
### Project Localization
- [x] Merge Chinese and English branches, replacing commented texts with scripts

### Cleanup and Maintenance
- [x] Remove redundant original artwork

### Tutorials
- [ ] New wiki and video tutorials

## On Hold

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library

---

[^1]: It will mainly add features such as calculating beats.
[^2]: An additional option will be attempted in settings, allowing the game screen to be set to a low resolution while magnifying the display resolution. This is more friendly to computers with lower performance.
[^3]: This script will encapsulate all Unity's Debug-related functions for calling within it, and these functions will only execute in the editor.
[^4]: This scene will list all story contents, consistent with the in-game display method, facilitating quick proofreading.
[^5]: This allows for multiple battles and several battles within the game.
[^6]: Localizers can provide preliminary localized support to players by releasing a preview version of CC subtitles during the phase when translation is complete but not yet officially embedded.