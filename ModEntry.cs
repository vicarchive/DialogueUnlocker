using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace MarriageDialogueUnlocker;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod

{
    private static IMonitor YOOO;

    // call this method from your Entry class
    internal static void Initialize(IMonitor monitor)
    {
        YOOO = monitor;
    }
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Harmony harmony = new(ModManifest.UniqueID);
        Initialize(Monitor);
        YOOO.Log($"patches started!");
        // PREFIXES
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.setRandomAfternoonMarriageDialogue)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(setRandomAfternoonMarriageDialogue_prefix))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.setSpouseRoomMarriageDialogue)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(setSpouseRoomMarriageDialogue_prefix))
        );
        YOOO.Log("patches done! <3");
        /*
         * NEXT ITERATION
         * 
        harmony.Patch(
            original: AccessTools.Method(typeof(StardewValley.NPC), nameof(NPC.marriageDuties)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MarriageDuties_Transpile))
        );
        */
    }

    public static bool setRandomAfternoonMarriageDialogue_prefix(NPC __instance, ref NetBool ___hasSaidAfternoonDialogue, int time, GameLocation location, bool countAsDailyAfternoon = false)
    {
        YOOO.Log($"setting dialogue!!");
        try
        {
            if (__instance.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri" || ___hasSaidAfternoonDialogue.Value)
                return true;
            if (countAsDailyAfternoon)
                ___hasSaidAfternoonDialogue.Value = true;
            Random daySaveRandom = Utility.CreateDaySaveRandom((double)time);
            int heartLevelForNpc = __instance.getSpouse().getFriendshipHeartLevelForNPC(__instance.Name);
            switch (location)
            {
                case FarmHouse _:
                    if (!daySaveRandom.NextBool())
                        break;
                    if (heartLevelForNpc < 9)
                    {
                        __instance.currentMarriageDialogue.Clear();
                        __instance.addMarriageDialogue("MarriageDialogue", randomAfternoonDialogueRewrite(__instance.Name, daySaveRandom, (daySaveRandom.NextDouble() < (double)heartLevelForNpc / 11.0 ? "Neutral_" : "Bad_"), false));
                        break;
                    }
                    if (daySaveRandom.NextDouble() < 0.05)
                    {
                        __instance.currentMarriageDialogue.Clear();
                        __instance.addMarriageDialogue("MarriageDialogue", randomAfternoonDialogueRewrite(__instance.Name, daySaveRandom, $"{Game1.currentSeason}_{__instance.Name}"));
                        break;
                    }
                    if (heartLevelForNpc >= 10 && daySaveRandom.NextBool() || heartLevelForNpc >= 11 && daySaveRandom.NextDouble() < 0.75 || heartLevelForNpc >= 12 && daySaveRandom.NextDouble() < 0.95)
                    {
                        __instance.currentMarriageDialogue.Clear();
                        __instance.addMarriageDialogue("MarriageDialogue", randomAfternoonDialogueRewrite(__instance.Name, daySaveRandom, "Good_", false));
                    }
                    __instance.currentMarriageDialogue.Clear();
                    __instance.addMarriageDialogue("MarriageDialogue", randomAfternoonDialogueRewrite(__instance.Name, daySaveRandom, "Neutral_", false));
                    break;
                case Farm _:
                    __instance.currentMarriageDialogue.Clear();
                    if (daySaveRandom.NextDouble() < 0.2)
                    {
                        __instance.addMarriageDialogue("MarriageDialogue", randomAfternoonDialogueRewrite(__instance.Name, daySaveRandom, $"Outdoor_{__instance.Name}", false));
                        break;
                    }
                    __instance.addMarriageDialogue("MarriageDialogue", randomAfternoonDialogueRewrite(__instance.Name, daySaveRandom, "Outdoor_"));
                    break;
            }
            return false;
        }
        catch (Exception e)
        {
            YOOO.Log("no dialogue for you :( .. \n{e}", LogLevel.Error);
            return true;
        }
        

    }
    public static bool setSpouseRoomMarriageDialogue_prefix(NPC __instance)
    {
        YOOO.Log("setting spouse room dialogue!!");
        try
        {
            Random random = Utility.CreateDaySaveRandom();
            __instance.currentMarriageDialogue.Clear();
            __instance.addMarriageDialogue("MarriageDialogue", randomAfternoonDialogueRewrite(__instance.Name, random, "spouseRoom_"));
            return false;
        }
        catch (Exception e)
        {
            YOOO.Log("no spouse room dialogue for you :( .. \n{e}", LogLevel.Error);
            return true;
        }
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
    public static string randomAfternoonDialogueRewrite(string npcName, Random daySaveRandom_Game, string prefixDialogue, bool generalNameToggle = true)
    {
        try
        {
            // Load dictionary and only take its keys
            string asset1 = "Characters\\Dialogue\\MarriageDialogue";
            string asset2 = asset1 + npcName;
            Dictionary<string, string> dialogueDictionaryNPCSpecific = Game1.content.Load<Dictionary<string, string>>(asset2);
            Dictionary<string, string> dialogueDictionaryGeneral = Game1.content.Load<Dictionary<string, string>>(asset1);
            List<string> dialogueDictionaryAll = new List<string>(dialogueDictionaryNPCSpecific.Keys);
            List<string> dDGeneral = new List<string>(dialogueDictionaryGeneral.Keys);

            // Get all unique keys from both files
            dialogueDictionaryAll.AddRange(dDGeneral);
            dialogueDictionaryAll.Select(g => g.First()).ToList();

            // Find the keys that match our queries
            List<string> matchedKeys = new List<string>();

            for (int i = 0; i < dialogueDictionaryAll.Count; i++)
            {
                if (dialogueDictionaryAll[i].Contains(prefixDialogue))
                {
                    if (!generalNameToggle || dialogueDictionaryAll[i].Contains(npcName))
                    {
                        matchedKeys.Add(dialogueDictionaryAll[i]);
                    }
                }
            }

            // For debugging purposes
            YOOO.Log("looked for keys matching the prefix: " + prefixDialogue);
            YOOO.Log("found " + matchedKeys.Count + " keys!!!");
            for (int i = 0; i < matchedKeys.Count; i++)
            {
                YOOO.Log(matchedKeys[i].ToString());
            }

            // Return a key from our matched list
            return matchedKeys[daySaveRandom_Game.Next(matchedKeys.Count)];
        }
        catch (Exception ex)
        {
            YOOO.Log($"error in the main func, oopsies!! .. \n{ex}", LogLevel.Error);
            return "patio_"+npcName;
        }
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