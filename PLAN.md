# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not arranged in any specific order and may be subject to changes, suspensions, or cancellations.

For more information about the long-term update plans for UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update details regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, while translations for other languages will be generated using GPT upon **version number changes**.

## Table of Contents

[General](#General): Includes improvement plans for basic/general features within the template.

[OW Class](#OW-Class): Includes plans for redoing various systems involved in the Overworld scenes.

[Combat Class](#Combat-Class): Includes plans for expanding and restructuring the combat system.

[New Additions](#New-Additions): Includes plans for newly added scenes and features.

[Others](#Others): Includes plans outside of the above four categories.

[Shelved](#Shelved): Includes plans that were once planned but are currently on hold for various reasons.

## General
### Existing System Refinement
- [x] Refine BGM Controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add magnification for 640x480[^2]
- [ ] Refine / UI Dr style MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redo settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call the language pack instead of calling all at once

## OW Class
### Event System
- [ ] Redo OW event system
- [ ] Add Dr style chase battle system

### Plot System
- [ ] Introduce a visual plot editor
- [ ] Add plot system proofreading scene[^4]
- [ ] Add OW plot controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet curtain data in ScriptableObject
- [ ] Store round data in ScriptableObject

### Round System Redo
- [ ] Add a visual round system editor
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add the ability to insert subtitles/dialogues during rounds
- [ ] Add multiple combats[^5]

### Fixes and Optimization
- [x] Optimize 3D background
- [x] Fix blue hearts

### Irregular Frame
- [ ] Fix irregular frame
- [ ] Redo collision system to fit irregular frame

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete seven player soul colors

## New Additions
### Scene Expansion
- [x] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcast scene
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
- [ ] Add UI manager
- [ ] Add support for custom extensions of weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add color blind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches, replacing comment texts via scripts

### Cleaning and Maintenance
- [x] Remove unnecessary original textures

### Tutorials
- [ ] New wiki and video tutorials

## Shelved

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online libraries

---

[^1]: Primarily to add functions like calculating beats.
[^2]: Will attempt to add an option in the settings to allow the game to be set to a low resolution while magnifying the display resolution. This is more friendly for computers with poorer performance.
[^3]: This script will encapsulate all Unity's Debug-related functions for use, which will only execute in the editor.
[^4]: This scene will list all the plot elements, consistent with how they are displayed in-game for quick proofreading.
[^5]: This allows for multiple battles and multiple combats in the game.
[^6]: Localizers can provide localized support to players in advance by releasing a preview version of CC subtitles during the translation completion but before the formal embedding phase.