# DialogueUnlocker
An SDV 1.6.15 ONLY mod that unlocks marriage dialogues.

## Warnings!!!!!!
This issue has, if I'm correct, been fixed in 1.6.16. Do NOT use this mod for 1.6.16!!!!
I am unsure if this mod works properly for anything below 1.6.15, if there are compatibility issues then update your game.

This mod overwrites any mods that alter any of the following functions:
- setSpouseRoomMarriageDialogue
- setRandomAfternoonMarriageDialogue
- setUpForOutdoorPatioActivity
- marriageDuties
- arriveAtFarmHouse
- checkForMarriageDialogue

I am not planning on this to remain this way but as of now it is not complatible with those that do (via prefixes).

## How to use (in the future)
Place the mod folder into the "Stardew Valley/Mods" folder.

Any entries added via "EditData" with Content Patcher should now be recognized by the game under specific circumstances:
- the prefixes must match the ones the game uses (ex: outdoor dialogue should begin with "Outdoor_" and spouse room dialogue should begin with "spouseRoom_"
- dialogue in MarriageDialogues.xnb should either be NAME or DATE specific. This is to prevent dialogue for different characters from showing up
- MarriageDialogue[name].xnb can have all the dialogues you want as long as they are prefixed properly for the game
- dialogue in the general MarriageDialogues.xnb should either be NAME (Rainy_Day_Alex) or DATE (spring_6) specific. This is to prevent dialogue for different characters from showing up. The game also looks for numbered ("Rainy_Day_0" format) entries for vanilla compatibility
It's easier to add new dialogue into the specific characters' marriage dialogues you want to modify. I'm working on an alternate solution.

## Releases
Releases will be on nexusmods.com (full link available when I'm done with the first fully working commit).
