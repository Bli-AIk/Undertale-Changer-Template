# UCT Update Plan

This document outlines UCT's long-term update plan.

Note that these update plans are not in any specific order and may be subject to change, suspension, or abandonment.

For the long-term update plan of UCT, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For specific updates regarding UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plan/update log for Simplified Chinese will be prioritized, and translations into other languages will be done using GPT when the **version number changes**.

## General Category
### Refinement of the Existing System
- [x] Refine BGM Controller[^1]
- [ ] Store item data to ScriptableObject


### UI Optimization and Refinement
- [ ] Add pixel perfection and magnification 640x480[^2]
- [ ] Refine / UI Drug-ification MENU interface and OW save interface
- [ ] Refine the settings interface
- [ ] Redo the settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
Use coroutines to call language packs instead of calling them all at once.

## OW Class
### Event System
- [ ] Redo the OW event system
- [ ] Add Dr. style chase system

### Plot System
- Introduce the [Fungus](https://github.com/snozbot/fungus) library to create a visual dialogue editing system.
- [ ] Add OW storyline controller

### Data Storage
- [ ] Store room data to ScriptableObject

## Battle Category
### Data Storage
- [x] Store bullet screen data to ScriptableObject
- [ ] Store round data to ScriptableObject

### Turn System Redesign
- [ ] Visualization Round System
- [ ] Add path bullet screen generator

### Combat System Expansion
- [ ] Add the functionality to insert subtitles/dialogue during turns.
- [ ] Add multiple battles[^4]

### Repair and Optimization
- [x] Optimize 3D background
- [x] Fix Blue Heart

### Irregular Frame
- [ ] Repair the irregular frame
- [ ] Redo the collision system to fit irregular frames

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven player soul colors


## New Class
### Scene Expansion
- [ ] Add multilingual expansion in the naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM to the broadcast scene

### Rendering Extension
- [x] Add built-in projection box
- [x] Add 3D renderer


### Function Expansion
- [ ] Add CC subtitle system[^5]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add color blind filter

## Other Categories
### Project Localization
- [x] Merge Chinese and English branches, replace comment texts through scripts

### Cleaning and Maintenance
- [x] Remove unnecessary original artwork textures

### Tutorial
- [ ] New wiki and video tutorial

Please provide the source text that you would like to have translated into English.

The main addition will be features such as calculating the number of beats.
I will try to add two additional options in the settings, allowing you to use pixel-perfect filters / enlarge to a larger resolution at 640x480 instead of directly using a larger resolution to save performance.
This script will encapsulate all Unity's Debug-related functions for calling, which only execute within the editor.
This allows for multiple battles and several fights within the game.
Localization staff can provide localized support to players by releasing a preview version of CC subtitles during the phase when the translation is complete but not yet officially embedded.