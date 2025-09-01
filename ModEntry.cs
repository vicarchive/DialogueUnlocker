using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace MarriageDialogueUnlocker;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Harmony harmony = new(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(StardewValley.NPC), nameof(NPC.setRandomAfternoonMarriageDialogue)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Afternoon_Transpile))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(StardewValley.NPC), nameof(NPC.setSpouseRoomMarriageDialogue)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SpouseRoom_Transpile))
        );
        /*
         * NEXT ITERATION
         * 
        harmony.Patch(
            original: AccessTools.Method(typeof(StardewValley.NPC), nameof(NPC.marriageDuties)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MarriageDuties_Transpile))
        );
        */
    }

    public static IEnumerable<CodeInstruction> Afternoon_Transpile(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher match = new CodeMatcher(instructions);
        MethodInfo 

    }

    public static IEnumerable<CodeInstruction> SpouseRoom_Transpile(IEnumerable<CodeInstruction> instructions)
    {

    }
    /*
     * NEXT ITERATION
     * 
    public static IEnumerable<CodeInstruction> MarriageDuties_Transpile(IEnumerable<CodeInstruction> instructions)
    {

    }
    */
    /// <summary> Rewrite of the dialogue thing in 1.6.15 </summary>
    /// <param name="npc"> Current NPC to be able to use their functions. </param>
    /// <param name="daySaveRandom_Game"> The random function already created in the function to use. </param>
    /// <param name="prefixDialogue"> The prefix of dialogue to match for. </param>
    /// <param name="generalNameToggle"> Whether to look for the NPC's name in the general marriage XNB or not. </param>
    public void randomAfternoonDialogueRewrite(NPC npc, Random daySaveRandom_Game, string prefixDialogue, bool generalNameToggle = true)
    {
        Dictionary<string, string> dialogueDictionaryNPCSpecific = Game1.content.Load<Dictionary<string, string>>($"Characters\\NPC\\MarriageDialogue"+ npc.Name);
        Dictionary<string, string> dialogueDictionaryGeneral = Game1.content.Load<Dictionary<string, string>>($"Characters\\NPC\\MarriageDialogue");
        List<string> dialogueDictionaryAll = new List<string>(dialogueDictionaryNPCSpecific.Keys);
        List<string> dDGeneral = new List<string>(dialogueDictionaryGeneral.Keys);
        if (generalNameToggle)
        {
            // Default to looking for name specific ones here. Turns this off if we're looking for more dialogue in the general file (used for date specific dialogue, mostly).
            dDGeneral.GroupBy(x => x.Contains(npc.Name));
        }
        dialogueDictionaryAll.AddRange(dDGeneral);
        dialogueDictionaryAll.GroupBy(x => x.Contains(prefixDialogue)).Select(g => g.First()).ToList();

        npc.addMarriageDialogue("MarriageDialogue", prefixDialogue + daySaveRandom_Game.Next(dialogueDictionaryAll.Count).ToString());
    }
    /*
     * NEXT ITERATION
     * 
    /// <summary> Rewrite of the dialogue stuff when the game tries to look for something in the StringsFromCSFiles. Look above for the param definitions of those that are named the same. </summary>
    /// <param name="fallbackDialogueFile"> The StringsToCSFile to look at when the function can't find valid dialogue in the marriage files. </param>
    /// <param name="fallbackDialogueLocation"> The line to look at for the dialogue to load. </param>
    public void stringSpecificDialogueRewrite(NPC npc, Random daySaveRandom_Game, string prefixDialogue, string fallbackDialogueFile, string fallbackDialogueLocation, bool generalNameToggle = true)
    {

    }
    */
}