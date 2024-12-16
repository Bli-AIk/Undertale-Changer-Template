# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not in any particular order and may change, be postponed, or abandoned.

For the long-term update plans of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates on UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, while other languages will be translated using GPT at the time of **version number changes**.

## Table of Contents

[General Category](#General-Category): Contains improvement plans for basic/general functions within the template.

[OW Category](#OW-Category): Contains plans for the redesign of various systems involved in the Overworld scenarios.

[Combat Category](#Combat-Category): Contains plans for the expansion and restructuring of the combat system.

[New Additions Category](#New-Additions-Category): Contains plans for new scenes and features.

[Other Category](#Other-Category): Contains plans that do not fall into the above four categories.

[On Hold Category](#On-Hold-Category): Contains plans that were once in development but are currently on hold for various reasons.

## General Category
### Existing System Refinement
- [x] Refine BGM controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add magnification to 640x480[^2]
- [ ] Refine / UI Dr style MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Call language pack using coroutines instead of calling all at once

## OW Category
### Event System
- [ ] Redesign the OW event system
- [ ] Add Dr style chase system

### Plot System
- [ ] Introduce a visual plot editor
- [ ] Add plot system proofreading scene[^4]
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Category
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store round data in ScriptableObject

### Round System Redesign
- [ ] Add visual round system editor
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogues during rounds
- [ ] Add multi-combat[^5]

### Fixes and Optimizations
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frame
- [ ] Fix irregular frame
- [ ] Redesign collision system to adapt to irregular frame

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete seven player soul colors

## New Additions Category
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
- [ ] Add midi detection system
- [ ] Add historical text system
- [ ] Add saving system
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Add custom expansion support for weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add colorblind filter

## Other Category
### Project Localization
- [x] Merge Chinese and English branches, replacing comment text via script

### Cleanup and Maintenance
- [x] Remove redundant original artwork textures

### Tutorials
- [ ] New wiki and video tutorials

## On Hold Category

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online libraries

---

[^1]: Mainly to add features such as calculating the number of beats.
[^2]: Will attempt to add an extra option in the settings to allow the game display to be set to a low resolution while enlarging the display resolution. This is more friendly for computers with lower performance.
[^3]: This script will encapsulate all Unity's Debug related functions within it, and these functions will only execute in the editor.
[^4]: This scene will list all plot content in a way consistent with the in-game display, making it easier for quick proofreading.
[^5]: That is, allowing multiple battles and multiple combat scenarios within the game.
[^6]: Localization personnel can provide localized support to players in advance by releasing a CC subtitle preview version during the translation completion, but not yet formally integrated stage.