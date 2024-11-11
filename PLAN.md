# UCT Update Plan

This document outlines the long-term update plan for UCT.

Note that these update plans are not in any specific order and may be subject to change, postponement, or cancellation.

For specific update details about UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

## General Category
### Refinement of the Existing System
- [x] Refine BGM Controller[^1]
- [ ] Store item data to ScriptableObject


### UI Optimization and Refinement
- [ ] Add pixel perfection and magnification 640x480[^2]
- [ ] Refine / UI Dr transformation of the MENU interface and OW save interface
- [ ] Refine settings interface
- [ ] Redo the settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
Use coroutine to call language packages instead of calling them all at once.

## OW Class
### Event System
- [ ] Redo the OW event system
- [ ] Add a Dr. style chase system

### Story System
- [ ] Introduce the [Fungus](https://github.com/snozbot/fungus) library to create a visual dialogue editing system
- [ ] Add OW story controller

### Data Storage
- [ ] Store room data to ScriptableObject

## Combat Category
### Data Storage
- [x] Store bullet screen data to ScriptableObject
- [ ] Store round data into ScriptableObject

### Turn System Redesign
- [ ] Visualization Round System
- [ ] Add Path Danmaku Generator

### Battle System Expansion
- [ ] Add the ability to insert subtitles/dialogues during the turn
- [ ] Add more battles[^4]

### Repair and Optimization
- [x] Optimize 3D background
- [x] Repair Blue Heart

### Irregular Frame
- [ ] Fix irregular frame
Redesign the collision system to accommodate irregular frames.

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven types of player soul colors


## New Class
### Scene Expansion
- [ ] Add multilingual expansion in the naming scenario
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene

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
- [ ] Add color blindness filter

## Other categories
Project Localization
- [x] Merge Chinese and English branches, replace comment text using scripts.

### Cleaning and Maintenance
- [x] Remove excess original artwork textures

### Tutorial
- [ ] New wiki and video tutorials

Please provide the text you'd like me to translate.

The main functionality will be to add features such as calculating the number of beats.
It will attempt to add two additional options in the settings, allowing you to use a pixel-perfect filter / upscaling to a larger resolution from 640x480, rather than directly using a larger resolution, to save performance.
This script will encapsulate all Unity's debug-related functions for calling within it, which are executed only in the editor.
This allows for multiple battles and several fights within the game.
Localization staff can provide localization support to players in advance by releasing a preview version of the CC subtitles during the stage when the translation is completed but not yet formally integrated.