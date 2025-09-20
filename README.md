# DialogueUnlocker
An SDV 1.6.15 mod that unlocks marriage dialogues.

## Warnings!
This issue has (probably) been fixed in 1.6.16. Do NOT use this mod for 1.6.16. Should I be dissatisfied with 1.6.16's implementation, I will create a 1.6.16 version.
I am unsure if this mod works properly for anything below 1.6.15, if there are compatibility issues then update your game.

This mod uses these functions:
- MarriageDialogueReference (postfixed)
- Game1.player.getSpouse().Name (referenced for spouse name)

This mod may not compatible with other mods that edit, prefix, or otherwise modify these methods.
(I tried to make it compatible with Free Love but I can't find another place to reference the speaker, so it will not work with it. Sorry)

## How to use (in the future)
Place the mod folder into the "Stardew Valley/Mods" folder.
Any entries added via "EditData" with Content Patcher should now be recognized by the game under specific circumstances, listed in the table below.

## Releases
Releases will be on Nexus Mods here: https://www.nexusmods.com/stardewvalley/mods/37749.

## List of valid dialogue key prefixes
You may add these lines into any of these two files, the mod will parse through both fine.
- Not case sensitive
- You may add more than one name into a dialogue line, NPCs with their name in the key will find and load it.
- These prefixes must be the first part of your key, adding them into the middle or end will prevent the mod from finding them.
- I suggest adding mod titles in the keys so that dialogue entries are not overridden by multiple edits to the same file
- I am assuming you already know how this game loads dialogue, it remains the same here.
I've included an example file in this repository.

Please see the original files or this page (https://stardewvalleywiki.com/Modding:Dialogue#Marriage_dialogue) for more guidelines on what these dialogues mean.
MarriageDialogue.xnb // MarriageDialogue(spouse).xnb (preexisting keys)
| Prefix | When |
| ------ | ------- |
| Rainy_Night | After 5pm, raining |
| Rainy_Day | Before 5pm, raining |
| Indoor_Night | After 5pm, clear and spouse is indoors |
| Indoor_Day | Before 5pm, clear and spouse is indoors |
| patio* | Spouse is in patio area |
| Good | More likely when heart level is 11+ |
| Neutral | More likely when heart level is 9-10 |
| Bad | More likely when heart level is <9 |
| OneKid | Family has one child, indoors |
| TwoKids | Family has two children, indoors |
| funLeave / jobLeave** | Spouse leaves the house for the day |
| funReturn / jobReturn** | Spouse returns after leaving |
| Outdoor | Before 5pm, spouse is outside the farmhouse |
| spouseRoom | Spouse is in their room |
| (currentSeason)_(spouseName) | Chance to be shown during a season |
| (currentSeason)_(currentDate)*** | Shown on a specific day |

StringsFromCSFiles (new keys)
| Prefix | When |
| ------ | ------ |
| Obstacle | Spouse cannot pathfind in the house |
| displeasedGeneral | Dialogue that shows up more often when having low hearts |
| displeasedGendered | Displeased but is gendered |
| displeasedFem | Female spouse specific dialogue |
| Adoption_Obstacle | Spouse cannot pathfind - adopting a child |
| Adoption_Name | Adoption dialogue, refers to the farmer by name |
| Adoption_Endearment | Adopion dialogue, uses an endearment term |
| Pregnancy_Obstacle | Female spouse expecting a child, cannot pathfind |
| Pregnancy_Endearment | Female spouse expecting a child, uses an endearment term |
| Pregnancy_Name | Female spouse expecting a child, refers to the farmer by name |
| Expecting_Obstacle | Male spouse expecting a child, cannot pathfind |
| Expecting_Endearment | Male spouse expecting a child, uses an endearment term |
| Expecting_Name | Male spouse expecting a child, refers to the farmer by name |
| ParentObstacle_One | One child in the family, spouse cannot pathfind |
| ParentObstacle_Two | Two children in the family, spouse cannot pathfind |
| Breakfast_Nevermind**** | Spouse was planning to make breakfast, but was not able to |
| WateredCrops | Spouse watered the crops |
| PetBowl_Multiple | Spouse watered multiple pet bowls |
| PetBowl_One | Spouse watered one pet bowl |
| Greeting_WorkDone | Spouse greets the farmer as they come outside, the spouse did work on the farm |
| Greeting_NoWork | Spouse greets the farmer as they come outside, the spouse did no work on the farm |
| Sprinkler | The spouse wanted to water crops, player has sprinklers for every tile |
| FeedAnimals | Spouse fed the animals |
| RepairFences | Spouse repaired the fences |
| Furniture_Introduce | First dialogue line when furniture is bought ("How does it look?") |
| Furniture_Show | Second dialogue line when furniture is bought ("I got this chair recently") |
| Furniture_Obstacle | Spouse wanted to put down furniture but was unable to |
| Change_Wallpaper | Spouse changed the wallpaper |
| Change_Flooring | Spouse changed the flooring |
| Furniture_Reminisce**** | Unsure |
| Reminisce**** | Unsure |

* Yes, it is lowercase
** Penny, Harvey, and Maru use jobLeave/ jobReturn. The others use funLeave/ funReturn
*** Overrides other dialogue options
**** May be unused

## Credits and Resources
Big BIG thank you to the SDV Discord modding channel, and especially to chu√e and selph for helping me out with syntax and postfixing.

Resources used:
Visual Studio Community 2022 - https://visualstudio.microsoft.com/vs/community/
+ Setup for SDV modding instructions - https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started#Create_the_project
Decompiled game code via dotPeek (you'll need a copy of the game to decompile, obviously) - https://www.jetbrains.com/decompiler/
Dialogue documentation - https://stardewvalleywiki.com/Modding:Dialogue
SDV specific Harmony patching examples - https://stardewmodding.wiki.gg/wiki/Tutorial:_Harmony_Patching
Harmony patching docs - https://harmony.pardeike.net/articles/patching.html