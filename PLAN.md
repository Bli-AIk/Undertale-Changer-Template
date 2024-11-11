# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any particular order and may be subject to changes, delays, or cancellations.

For specific update information about UCT, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

## General Category
### Refinement of Existing System
- [x] Refine BGM controller[^1]
"Store item data in ScriptableObject."


### UI Optimization and Refinement
- [ ] Add pixel perfect and magnification 640x480[^2]
- [ ] Refine / UI Dr. the MENU interface and OW save interface
- [ ] Refine the settings interface
- [ ] Redo the settings interface UI

### Performance Optimization
- [x] Add DebugLogger script[^3]
Use coroutine to call the language pack, rather than calling all at once.

## OW Category
### Event System
- [ ] Redo the OW event system
- [ ] Add a Dr. style chase system.

### Plot System
- [ ] Introduce the [Fungus](https://github.com/snozbot/fungus) library to create a visual dialogue editing system.
- [ ] Add OW storyline controller

### Data Storage
Store room data to ScriptableObject.

## Combat Class
### Data Storage
- [x] Store barrage data in ScriptableObject
- [ ] Store round data to ScriptableObject

### Turn System Redesign
- [ ] Visualized Round System
- [ ] Add path bullet screen generator

### Combat System Expansion
Add the feature to insert subtitles/dialogues during the round.
The translation of the text to English is: "Add more battles."

### Fixes and Optimizations
- [x] Optimize 3D background
- [x] Repair Blue Heart

### Irregular Frame
- [ ] Fix the irregular frame
- [ ] Redo the collision system to accommodate irregular frames.

### Judgment and Collision
Refine judgments related to FIGHT.
Add monster death determination
Complete the seven types of player soul colors.


## New Category
### Scene Expansion
- [ ] Add multilingual expansion in the naming scene.
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer


### Function Expansion
- [ ] Add CC subtitle system[^5]
- [ ] Add achievement system
- [ ] Add UI Manager
- [ ] Introduce online library

### Accessibility
- [ ] Add narrator
- [ ] Add color blindness filter

## Other Categories
### Project Localization
- [x] Merging Chinese and English branches, replacing comment text through scripts.

### Cleaning and Maintenance
- [x] Remove redundant original artwork textures

### Tutorial
- [ ] New wiki and video tutorials

Please provide the text you would like me to translate into English.

The main addition will be features such as calculating the number of beats.
[2]: Will attempt to add two additional options in the settings, allowing you to use a pixel-perfect filter / enlarge to a larger resolution at 640x480 instead of directly using a larger resolution, in order to save performance.
The script will encapsulate all Unity debug-related functions to be called within it, and these functions are only executed in the editor.
The translation of the text to English is: "That is, allowing for multiple battles and several fights within the game."
Localization personnel can provide localization support to players in advance by releasing a CC subtitle preview version during the stage when the translation is completed but not yet officially integrated.