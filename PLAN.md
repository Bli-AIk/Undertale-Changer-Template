# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not in any particular order and may change, be postponed, or abandoned.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific update content about UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

Simplified Chinese updates and changelogs will be updated first, while other languages will utilize GPT for translation during **version number changes**.

## General
### Refinement of Existing Systems
- [x] Refine BGM controller[^1]
- [ ] Store item data in ScriptableObject

### UI Optimization and Refinement
- [ ] Add pixel-perfect and magnified 640x480[^2]
- [ ] Refine/UI Dr design MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redesign settings UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutines to call language packs instead of calling all at once

## OW Class
### Event System
- [ ] Redesign OW event system
- [ ] Add Dr-style chase battle system

### Story System
- [ ] Introduce [ink](https://github.com/inkle/ink) scripting language into the story system
- [ ] Add story system proofreading scene[^4]
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data in ScriptableObject

## Combat Class
### Data Storage
- [x] Store bullet data in ScriptableObject
- [ ] Store round data in ScriptableObject

### Round System Redesign
- [ ] Visualize round system
- [ ] Add path bullet generator

### Combat System Expansion
- [ ] Add functionality to insert subtitles/dialogue during rounds
- [ ] Add multiple battles[^5]

### Fixes and Optimizations
- [x] Optimize 3D background
- [x] Fix blue hearts

### Asymmetrical Frame
- [ ] Fix asymmetrical frame
- [ ] Redesign collision system to accommodate asymmetrical frame

### Judgement and Collision
- [ ] Refine FIGHT-related judgements
- [ ] Add monster death judgement
- [ ] Complete seven player soul colors

## New Additions
### Scene Expansion
- [ ] Add multilingual extension to the naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add CC subtitle system[^6]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add color blind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches, replacing annotation text through scripts

### Cleanup and Maintenance
- [x] Remove unnecessary original textures

### Tutorials
- [ ] New wiki and video tutorials

---

[^1]: Primarily will add functions such as calculating beats.
[^2]: Will attempt to add two additional options in settings to allow pixel-perfect filtering / enlarging to higher resolution at 640x480 instead of using a higher resolution directly to save performance.
[^3]: This script will encapsulate all Unity-related Debug functions within to call, which only execute within the editor.
[^4]: This scene will list all story content in a way consistent with in-game display for quick proofreading.
[^5]: Refers to allowing multiple battles within the game.
[^6]: Localization personnel can provide preliminary localization support to players by releasing a CC subtitles preview version during the stage where translations have been completed but not officially integrated.