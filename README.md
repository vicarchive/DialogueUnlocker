# DialogueUnlocker
An SDV 1.6.15 ONLY mod that unlocks marriage dialogues.

## Warnings!!!!!!
This issue has (probably) been fixed in 1.6.16. Do NOT use this mod for 1.6.16!!!!
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
- You may add more than one name into a dialogue line, NPCs with their name in the key will find and load it.
- Not case sensitive
- These prefixes must be the first part of your key, adding them into the middle or end will prevent the mod from finding them.

- I am assuming you already know how this game loads dialogue, it remains the same here.

Please see the original files or this page (https://stardewvalleywiki.com/Modding:Dialogue#Marriage_dialogue) for more guidelines on what these dialogues mean.
MarriageDialogue.xnb // MarriageDialogue(spouse).xnb (preexisting keys)
| Prefix | Example |
| ------ | ------- |
| Rainy_Night | Rainy_Nightyyyy______8475934857834 |
| Rainy_Day | Rainy_Day_h_i_Alex_Jodi |
| Indoor_Night | Indoor_Night78032,fdehwoklajdmfmew[][\[ |
| Indoor_Day | Indoor_Day hi spaces should work |
| patio | patio_forall_whythough |
| Good | Good_summer_fbweiufvbew_16_mar_sebastia_harvey |
| Neutral | Neutral3425789347903587423 |
| Bad | Bad_:(how_could_you |
| OneKid | OneKidSpareShane |
| TwoKids | TwoKids_are_hell_irl |
| funLeave / jobLeave | funLeave_help_0_f |
| funReturn / jobReturn | jobReturnPenny |
| Outdoor | Outdoor_Alex_heehee |
| spouseRoom | spouseRoom_1_HALEY_cool_name_idk |
| (currentSeason)_(spouseName) | summer_emILY_youcanputanythinghere |
| (currentSeason)_(currentDate) | winter_14_okay_SAM |

StringsFromCSFiles (new keys)
| Prefix | Example | When |
| Obstacle | | Spouse cannot pathfind in the house |
| Displeased | | Dialogue that shows up more often when having low hearts |
| Displeased_Gendered | | Displeased but is gendered |
| Displeased_Fem | | Female spouse specific dialogue |
| Adoption_Obstacle | | Spouse cannot pathfind - adopting a child |
| Adoption_Name | | Adoption dialogue, refers to the farmer by name |
| Adoption_Endearment | | Adopion dialogue, uses an endearment term |
| Pregnancy_Obstacle | | Female spouse expecting a child, cannot pathfind |
| Pregnancy_Endearment | | Female spouse expecting a child, uses an endearment term |
| Pregnancy_Name | | Female spouse expecting a child, refers to the farmer by name |
| Expecting_Obstacle | | Male spouse expecting a child, cannot pathfind |
| Expecting_Endearment | | Male spouse expecting a child, uses an endearment term |
| Expecting_Name | | Male spouse expecting a child, refers to the farmer by name |
| OneKid_Obstacle | | One child in the family, spouse cannot pathfind |
| TwoKids_Obstacle | | Two children in the family, spouse cannot pathfind |
| Breakfast_Nevermind (may be unused) | | Spouse was planning to make breakfast, but was not able to |
| WateredCrops | | Spouse watered the crops |
| PetBowl_Multiple | | Spouse watered multiple pet bowls |
| PetBowl_One | | Spouse watered one pet bowl |
| Greeting_WorkDone | | Spouse greets the farmer as they come outside, the spouse did work on the farm |
| Greeting_NoWork | | Spouse greets the farmer as they come outside, the spouse did no work on the farm |
| Sprinkler | | The spouse wanted to water crops, player has sprinklers for every tile |
| FeedAnimals | | Spouse fed the animals |
| RepairFences | | Spouse repaired the fences |
| Furniture_Introduce | | First dialogue line when furniture is bought ("How does it look?") |
| Furniture_Show | | Second dialogue line when furniture is bought ("I got this chair recently") |
| Furniture_Obstacle | | Spouse wanted to put down furniture but was unable to |
| Change_Wallpaper | | Spouse changed the wallpaper |
| Change_Flooring | | Spouse changed the flooring |
| Furniture_Reminisce (unused?) | | Unsure |
| Reminisce (unused?) | | Unsure |