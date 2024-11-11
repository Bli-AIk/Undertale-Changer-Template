# UCT renewal program

This document is written about UCT's long-term update plan.

Note that these update plans are not in any order and are subject to change, shelving, or deprecation.

For specific updates to UCT, please refer to [CHANGELOG.md] (CHANGELOG.md), [CHANGELOG_zh-CN.md] (CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md] (CHANGELOG_zh-TW.md).

## Generic class
### Refinement of existing systems
- [ x ] Refinement of BGM controller[^1]
- [ ] Store item data to ScriptableObject


### UI optimization and refinement
- [ ] Add Pixel Perfection & Zoom Out to 640x480 [^2]
- [ ] Refine / UI Drill MENU screen and OW archive screen
- [ ] Refine Settings UI
- [ ] Redo Settings UI

### Performance Optimization
- [ x ] Add DebugLogger scripts[^3]
- [ ] Call language packs using a concatenation instead of calling them all at the same time

## OW classes
### Event system
- [ ] Rework the OW event system
- [ ] Add a Dr. style chase system.

### Plot system
- [ ] Introduced the [Fungus](https://github.com/snozbot/fungus) library to create a visual dialog editing system
- [ ] Add OW Plot Controller

### Data Storage
- [ ] Store room data to a ScriptableObject.

## Battle class
### Data storage
- [ x ] Stores pop-up data to ScriptableObject
- [ ] Store round data to ScriptableObject

### Round System Rework
- [ ] Visualize Round System
- [ ] Add Path Popup Generator

### Battle System Expansion
- [ ] Add the ability to insert subtitles / dialog during rounds
- [ ] Add multiple battles[^4]

### Fixes and optimizations
- [ x] Optimized 3D backgrounds
- [ x] Fixed blue hearts

### Alien Frame
- [ ] Fix shaped frames
- [ ] Rework collision system to fit shaped frames

### Judgment and Collision
- [ ] Refine FIGHT related judgments
- [ ] Add monster death judgment
- [ ] Completing the seven player soul colors.


## New Classes
### Scene Expansion
- [ ] Added multi-language expansion to the naming scene.
- [ ] Add Battle Settlement Scene
- [ ] Add a BGM playback scene

### Render Expansion
- [ x] Add built-in projector frame
- [ x] Add 3D Renderer


### Functionality Expansion
- [ ] Add CC captioning system[^5]
- [ ] Add achievement system
- [ ] Add UI manager
- [ ] Introduce online library

### Accessibility
- [ ] Add Narrator
- [ ] Add color blind filter

## Other classes
### Project localization
- [ x ] Merge Chinese and English branches, replace comment text with scripts

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki with video tutorials

---

[^1]: Will mainly add features like counting beats to it.
[^2]: Will try to add two extra items to the settings options that will allow you to use pixel-perfect filters / zoom to a larger resolution at 640x480 instead of just using a larger resolution to save performance.
[^3]: This script will wrap all of Unity's Debug related functions into it to be called, and these functions will only be executed within the editor.
[^4]: i.e. allow multiple battles and multiple fights within the game.
[^5]: Localizers can provide early localization support to players by releasing a preview version with CC subtitles at a stage when the translation is complete but not yet officially embedded.