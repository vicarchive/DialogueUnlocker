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
Releases will be on nexusmods.com (full link available when I'm done with the first fully working commit).

## List of valid dialogue key prefixes
You may add these lines into any of these two files, the mod will parse through both fine.
- You may add more than one name into a dialogue line, NPCs with their name in the key will find and load it.

- I am assuming you already know how this game loads dialogue, it remains the same here.

Please see the original files or this page (https://stardewvalleywiki.com/Modding:Dialogue#Characters.5CDialogue.5C.3CNPC.3E.xnb) for more guidelines on what these dialogues mean.
| Prefix | Example | Example's Behavior in MarriageDialogue.xnb |
| ------ | ------- | ----- |
| Rainy_Night | Rainy_Nightyyyy______8475934857834 | Universal |
| Rainy_Day | Rainy_Day_h_i_Alex_Jodi | Alex line |
| Indoor_Night | Indoor_Night78032,fdehwoklajdmfmew[][\[ | Universal |
| Indoor_Day | Indoor_Day hi spaces should work | Universal |
| patio | patio_forall_whythough | Universal |
| Good | Good_summer_fbweiufvbew_16_mar_sebastia_harvey | Harvey line |
| Neutral | Neutral3425789347903587423 | Universal |
| Bad | Bad_:(_how_could_you | Universal |
| OneKid | OneKidSpareShane | Shane line |
| TwoKids | TwoKids_are_hell_irl | Universal |
| funLeave / jobLeave | funLeave_help_0_f | Universal |
| funReturn / jobReturn | jobReturnPenny | Penny line |
| Outdoor | Outdoor_Alex_heehee | Alex line |
| spouseRoom | spouseRoom_1_HALEY_cool_name_idk | Haley line |
| spring / summer / fall / winter + spouseName | summer_emILY_youcanputanythinghere | Emily line |
| spring / summer / fall / winter_currentDate | winter_14_okay_SAM | Sam line (wip) |

I'll be adding the Strings\\StringsFromCSFiles prefixes in here soon when I begin patching them.