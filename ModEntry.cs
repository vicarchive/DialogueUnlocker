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
    private static IMonitor EntryMonitor;
    internal static void Initialize(IMonitor monitor)
    {
        // Initialize a monitor
        EntryMonitor = monitor;
    }

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Harmony harmony = new(ModManifest.UniqueID);
        Initialize(Monitor);

        // PREFIX
        /* The main method to patch
         * The game runs fine with this, but there are a few quirks, notably the "..." after each line that pops up because marriageDuties could not load dialogue.
         * Indoor_Day also primarily runs from marriageDuties.
         */
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.addMarriageDialogue)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(addMarriageDialogue_prefix))
        );
        EntryMonitor.Log("Prefix patch completed!");
        // TRANSPILERS
        /* To fix the issues mentioned above
         * marriageDuties is a HUGE method that I am not going to prefix just to change a few lines. Given that addMarriage takes in the arguments that the MarriageDialogueReference already needs, it's easiler to transpile these to point to addMarriage instead.
         */
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(marriageDuties_transpiler_1))
        );
        EntryMonitor.Log("Transpiler patches completed!");
        EntryMonitor.Log("All dialogue patches have been applied!");
    }

    /// <summary> Transpiler for MarriageDuties with a few minor changes. </summary>
    public static IEnumerable<CodeInstruction> marriageDuties_transpiler_1(IEnumerable<CodeInstruction> instructions)
    {
        /*
         * NetRef<MarriageDialogueReference> marriageDefaultDialogue = this.marriageDefaultDialogue;
         * num1 = daySaveRandom.Next(5);
         * MarriageDialogueReference dialogueReference = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + num1.ToString(), false, Array.Empty<string>());
         * marriageDefaultDialogue.Value = dialogueReference;
         */
        
        CodeMatcher matcher = new(instructions);
        MethodInfo daySaveRandomMethod = AccessTools.Method(typeof(Random), nameof(Random.Next), new Type[] {typeof(Int32)});
        MethodInfo toStringMethod = AccessTools.Method(typeof(Int32), nameof(Int32.ToString));
        MethodInfo concatMethod = AccessTools.Method(typeof(String), nameof(String.Concat), new Type[] { typeof(string), typeof(string) });

        MethodInfo modifyDialogue = AccessTools.Method(typeof(ModEntry), nameof(addMarriageDialogue_prefix));

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldstr, "MarriageDialogue"),
            new CodeMatch(OpCodes.Ldstr, "Rainy_Day_"),
            new CodeMatch(OpCodes.Ldloc_2), //daySaveRandom
            new CodeMatch(OpCodes.Ldc_I4_5),
            new CodeMatch(OpCodes.Callvirt, daySaveRandomMethod),
            new CodeMatch(OpCodes.Stloc_S), //num1
            new CodeMatch(OpCodes.Ldloca_S), //num1
            new CodeMatch(OpCodes.Call, toStringMethod),
            new CodeMatch(OpCodes.Call, concatMethod),
            new CodeMatch(OpCodes.Ldc_I4_0) // "Rainy_Day_" + num1
            )
            .ThrowIfNotMatch($"Could not find entry point for transpiler {nameof(marriageDuties_transpiler_1)}")
            .Advance(-1)
            .RemoveInstructions(1)
            .Advance(11)
            .RemoveInstructions(2)
            .Insert(
                new CodeInstruction(OpCodes.Call, modifyDialogue)
            );
        return matcher.InstructionEnumeration();
    }

    ///public static IEnumerable<CodeInstruction> marriageDuties_transpiler_2(IEnumerable<CodeInstruction> instructions)
    ///{
        /*
         * NetRef<MarriageDialogueReference> marriageDefaultDialogue = this.marriageDefaultDialogue;
         * num1 = daySaveRandom.Next(5);
         * MarriageDialogueReference dialogueReference = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + num1.ToString(), false, Array.Empty<string>());
         * marriageDefaultDialogue.Value = dialogueReference;
         */
    ///}
    ///public static IEnumerable<CodeInstruction> marriageDuties_transpiler_3(IEnumerable<CodeInstruction> instructions)
    ///{
        /*
         * this.currentMarriageDialogue.Add(new MarriageDialogueReference(this.marriageDefaultDialogue.Value.DialogueFile, this.marriageDefaultDialogue.Value.DialogueKey, this.marriageDefaultDialogue.Value.IsGendered, this.marriageDefaultDialogue.Value.Substitutions));
         */

    /// <summary> The prefix for addMarriageDialogue. Allows more flexibility in marriage dialogues. </summary>
    /// <param name="__instance"> "this" or the NPC loading the function </param>
    /// <param name="dialogue_file"> The file to look for for the dialogue key </param>
    /// <param name="dialogue_key"> The dialogue key to load </param>
    /// <param name="gendered"> I'm just passing this to the add function </param>
    /// <param name="substitutions"> Used for endearment terms </param>
    public static void addMarriageDialogue_prefix(NPC __instance, string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
    {
        Random dialogueRandom = Utility.CreateDaySaveRandom(Game1.timeOfDay);

        switch (dialogue_file)
        {
            case "MarriageDialogue":
                EntryMonitor.Log("MarriageDialogue detected!");

                // Transpiler debug
                EntryMonitor.Log($"I've found: {__instance} | {dialogue_file} | {dialogue_key} | {gendered}");

                // Instantiation
                bool nameToggleBool = dialogue_key.Contains(__instance.Name) && dialogue_key.Contains(Game1.currentSeason); // Season specific dialogue lookup. Otherwise no reason to toggle name search on. And might remove this later I have no idea
                string startsKey = "";
                List<string> acceptableKeys = new List<string> { "Rainy_Night", "Rainy_Day", "Indoor_Night", "Indoor_Day", "Outdoor", "Good", "Neutral", "Bad", "OneKid", "TwoKids", $"{Game1.currentSeason}", "spouseRoom", "funLeave", "jobLeave", "funReturn", "jobReturn", "patio" };

                // Match with one of the accepted keys above
                bool found = false;
                int i = 0;
                while (!found && i < acceptableKeys.Count)
                {
                    if (dialogue_key.StartsWith(acceptableKeys[i]))
                    {
                        startsKey = acceptableKeys[i];
                        found = true;
                    }
                    i++;
                }

                //If the startsKey was not updated according to earlier matches
                if (!found || startsKey == "")
                {
                    EntryMonitor.Log("No dialogue matches detected!");
                    break;
                }

                //Copied from addMarriageDialogue from the original since that one doesn't run
                __instance.shouldSayMarriageDialogue.Value = true;
                string chosenKey = pickRandomDialogue(__instance.Name, dialogueRandom, startsKey, nameToggleBool);
                EntryMonitor.Log("Key has been chosen!");
                EntryMonitor.Log($"I'm using {__instance.Name}, {dialogue_file}, {chosenKey}, {gendered}, and {substitutions}!");
                __instance.currentMarriageDialogue.Add(new MarriageDialogueReference(dialogue_file, chosenKey, gendered, substitutions));
                break;

            case "Strings\\StringsFromCSFiles": // Look into MarriageDuties for this
                EntryMonitor.Log("MarriageDuties detected!");
                break;
            default: EntryMonitor.Log($"No dialogue file detected! See {dialogue_file}!"); break;
        }
    }
    

    /// <summary> Rewrite of the dialogue thing in 1.6.15 </summary>
    /// <param name="npcName"> Current NPC name. </param>
    /// <param name="daySaveRandom_Game"> The random function already created in the function to use. </param>
    /// <param name="prefixDialogue"> The prefix of dialogue to match for. </param>
    /// <param name="generalNameToggle"> Whether to look for the NPC's name in the general marriage XNB or not. </param>
    public static string pickRandomDialogue(string npcName, Random daySaveRandom_Game, string prefixDialogue, bool generalNameToggle)
    {
        bool npcNamePresent = false;
        List<string> listOfAllOtherNPCs = initializeNPCNameList(Game1.content);
        if (listOfAllOtherNPCs.Contains(npcName))
        {
            npcNamePresent = true;
        }
        try
        {
            // General marriage dialogue parsing
            string asset1 = "Characters\\Dialogue\\MarriageDialogue";
            Dictionary<string, string> marriageDialogueDictionary = Game1.content.Load<Dictionary<string, string>>(asset1);
            List<string> marriageDialogueDictionaryKeys = new List<string>(marriageDialogueDictionary.Keys);

            List<string> marriageDialogueUniversalKeys = new List<string>();
            List<string> marriageDialogueUnivSpouseKeys = new List<string>();

            foreach (string key in marriageDialogueDictionaryKeys)
            {
                if (key.Contains(prefixDialogue))
                {
                    if (key.Contains(npcName)) // Contains the NPCs name
                    {
                        marriageDialogueUnivSpouseKeys.Add(key);
                    }
                    if (!listOfAllOtherNPCs.Any(key.Contains) && !key.Contains(npcName)) // Does not contain any NPCs name, including the current one
                    {
                        marriageDialogueUniversalKeys.Add(key);
                    }
                    // Otherwise it is a dialogue line for another NPC
                }
            } 

            // NPC specific dialogue parsing
            List<string> marriageDialogueSpouseKeys = new List<string>();
            if (npcNamePresent)
            {
                string asset2 = asset1 + npcName;
                Dictionary<string,string> marriageDialogueSpouseDictionary = Game1.content.Load<Dictionary<string, string>>(asset2);
                List<string> marriageDialogueSpouseDictionaryKeys = new List<string>(marriageDialogueSpouseDictionary.Keys);

                foreach (string key in marriageDialogueSpouseDictionaryKeys) // use your CS knowledge and get rid of all of these ifs oh my god dude
                {
                    if (key.Contains(prefixDialogue))
                    {
                        if (generalNameToggle) // This is for stuff like winter_Alex in case things formatted that way are present in the spouse dialogue file.
                        {
                            if (key.Contains(npcName))
                            {
                                marriageDialogueSpouseKeys.Add(key);
                            }
                        }
                        else
                        {
                            marriageDialogueSpouseKeys.Add(key);
                        }
                    }
                }
            }

            // Get all unique keys from both files
            marriageDialogueSpouseKeys.AddRange(marriageDialogueUniversalKeys);
            marriageDialogueSpouseKeys.AddRange(marriageDialogueUnivSpouseKeys);
            List<string> marriageDialogueKeys = marriageDialogueSpouseKeys.Distinct().ToList();

            // For debugging purposes
            EntryMonitor.Log("Looked for keys matching the prefix: " + prefixDialogue);
            EntryMonitor.Log("Found " + marriageDialogueKeys.Count + " keys!");
            /*
            for (int i = 0; i < marriageDialogueKeys.Count; i++)
            {
                EntryMonitor.Log(marriageDialogueKeys[i].ToString());
            }
            */
            // Return a key from our matched list
            return marriageDialogueKeys[daySaveRandom_Game.Next(marriageDialogueKeys.Count)];
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not find a matching key! \n{ex}");
        }
    }

    /// <summary> Initialize a list of all marriable NPCs in the game based on the dialogue files present. </summary>
    /// <param name="content"> The game's content loader. </param>
    /// <returns> List of all marriable NPCs </returns>
    public static List<string> initializeNPCNameList(LocalizedContentManager content)
    {
        try
        {
            EntryMonitor.Log("Beginning file lookup!");
            // Getting the files in the marriage directory
            string rootLocation = content.GetContentRoot();
            EntryMonitor.Log("Content root retrieved!");
            string dialogueDirectory = "Characters\\Dialogue";
            EntryMonitor.Log("Dialogue directory set!");
            string[] marriageFiles = Directory.GetFiles(rootLocation + "\\" + dialogueDirectory);
            EntryMonitor.Log("Got NPC files!");

            // Sorting through and parsing them
            List<string> fileNames = new List<string>();
            for (int i = 0; i < marriageFiles.Length; i++)
            {
                if (marriageFiles[i].Contains("MarriageDialogue"))
                {
                    string path = Path.GetFileName(marriageFiles[i]);
                    string marriageFileName = path.Split(new string[] { "." }, StringSplitOptions.None).First();
                    string npcName = marriageFileName.Split(new string[] { "MarriageDialogue" }, StringSplitOptions.None).Last();
                    if (npcName != "")
                    {
                        fileNames.Add(npcName);
                    }
                }
            }
            List<string> otherNPCs = fileNames.Distinct().ToList();
            EntryMonitor.Log("Parsed NPC files!");

            // Check list of NPCs (debug)
            EntryMonitor.Log($"NPC names found:");
            for (int i = 0; i < otherNPCs.Count; i++)
            {
                EntryMonitor.Log("->"+otherNPCs[i]+"<-");
            }

            return otherNPCs;

        }
        catch (Exception ex)
        {
            throw new Exception($"List of NPc's could not be instantiated! \n{ex}");
        }
    }
}