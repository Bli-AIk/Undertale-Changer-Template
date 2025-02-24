# UCT Update Plan

This document outlines the long-term update plan for UCT.

Please note that these update plans are not in any particular order and may be subject to changes, suspension, or abandonment.

For more information on UCT's long-term update plan, please refer to [PLAN.md](PLAN.md), [PLAN_zh-CN.md](PLAN_zh-CN.md), or [PLAN_zh-TW.md](PLAN_zh-TW.md).

For details on UCT's specific updates, please refer to [CHANGELOG.md](CHANGELOG.md), [CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md), or [CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md).

The update plans/change logs in Simplified Chinese will be prioritized, and translations into other languages will be done using GPT **when version numbers change**.

## Directory

[General](#通用类): Includes improvement plans for basic/general functionalities within the template.

[OW Class](#OW 类): Includes plans for the redesign of various systems related to Overworld scenes.

[Combat Class](#战斗类): Includes plans for the expansion and reconstruction of the battle system.

[New Additions](#新增类): Includes plans for new scenes and functionalities.

[Others](#其他类): Includes plans that fall outside the above four categories.

[On Hold](#搁置类): Includes plans that were once planned but have been put on hold for various reasons.

## General
### Refinement of Existing Systems
- [x] Refine BGM controller[^1]
- [ ] Store item data to ScriptableObject


### UI Optimization and Refinement
- [ ] Add low-resolution magnification display feature[^2]
- [ ] Separate UI rendering logic and execution logic of MENU interface, settings interface, and OW save interface
- [ ] Refine functionalities of MENU interface, settings interface, and OW save interface

### Performance Optimization
- [x] Add DebugLogger script[^3]
- [ ] Use coroutine to call language packs instead of calling all at once

## OW Class
### Event System
- [x] Redo OW event system
- [ ] Add Dr style chase battle system
- [ ] Save event system data

### Plot System
- [ ] Enrich dialogue system[^4]
- [ ] Add proofreading scene for all texts[^5]

## Combat Class
### Data Storage
- [x] Store bullet data to ScriptableObject
- [ ] Read JSON round data

### Redesign of Round System
- [ ] Add visual round system editor (separate project) to generate JSON round data
- [ ] Add path bullet generator

### Battle System Expansion
- [ ] Add functionality to insert subtitles/dialogues during rounds
- [ ] Add multiple battles[^6]

### Fixes and Optimizations
- [x] Optimize 3D background
- [x] Fix blue heart

### Irregular Frames
- [ ] Fix irregular frames
- [ ] Redo collision system to accommodate irregular frames

### Judgment and Collision
- [ ] Refine FIGHT-related judgments
- [ ] Add monster death judgment
- [ ] Complete the seven player soul colors

## New Additions
### Scene Expansion
- [x] Add multilingual expansion in naming scene
- [ ] Add battle settlement scene
- [ ] Add BGM broadcasting scene
- [ ] Add credits/acknowledgments scene
- [ ] Add shop scene

### Rendering Expansion
- [x] Add built-in projection frame
- [x] Add 3D renderer

### Functionality Expansion
- [ ] Add MIDI detection system
- [ ] Add historical text system
- [ ] Add storage system
- [ ] Add CC subtitle system[^7]
- [ ] Add achievement system
- [ ] Add Buff system[^8]
- [ ] Add UI manager
- [ ] Add customization support for weapons/armor

### Accessibility
- [ ] Add narrator
- [ ] Add color blind filter

## Others
### Project Localization
- [x] Merge Chinese and English branches, replace comment texts through scripts
- [ ] Add rich text TAG: custom optional translation names (for selective translation of character names, etc.)

### Cleanup and Maintenance
- [x] Remove redundant original textures

### Tutorials
- [ ] New wiki and video tutorials

## On Hold

- [ ] Add pixel-perfect post-processing
- [ ] Introduce online library
- [ ] Add team system
- [ ] Add task system

---

[^1]: Mainly will add functions such as calculating the number of beats.
[^2]: Will attempt to add an extra option in the settings to allow the game screen to be set to low resolution while enlarging the displayed resolution. This is more friendly for computers with poorer performance.
[^3]: This script encapsulates all Unity's debug-related functions to call within it, and these functions only execute in the editor.
[^4]: The current dialogue system struggles to support branching choices, logical jumps, and other functions. Perhaps it could try integrating Ink language.
[^5]: This scene will list all texts, consistent with the display method in the game, facilitating quick proofreading.
[^6]: That is, allowing for multiple battles and multiple combat scenarios within the game.
[^7]: Localizers can provide localized support to players through the release of a CC subtitle preview version during the phase when translation is complete but not formally embedded.
[^8]: The KR system realizes Buff through Buff.