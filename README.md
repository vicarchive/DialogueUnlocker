# DialogueUnlocker
An SDV 1.6.15 ONLY mod that unlocks marriage dialogues. At the moment I've only begun impllementing unlocked afternoon marriage dialogues and spouse room dialogues, but I plan on unlocking the stuff in MarriageDuties too.

## This mod is for 1.6.15 ONLY!!!!
This issue has, if I'm correct, been fixed in 1.6.16. Do NOT use this mod for 1.6.16!!!!
I am unsure if this mod works properly for anything below 1.6.15, if there are compatibility issues then update your game.

## How to use (in the future)
With the dll in the mod folder, with GMCM, make sure "Insert SMAPI" is selected.
Any entries added via "EditData" with Content Patcher should now be recognized by the game under specific circumstances:
- the prefixes must match the ones the game uses (ex: outdoor dialogue should begin with "Outdoor_" and spouse room dialogue should begin with "spouseRoom_"
- dialogue in MarriageDialogues.xnb should either be NAME or DATE specific. This is to prevent dialogue for different characters from showing up
- MarriageDialogue<name>.xnb can have all the dialogues you want as long as they are prefixed properly for the game

## Releases
Releases will be on nexusmods.com (full link available when I'm done with the first working commit).
