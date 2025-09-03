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

        EntryMonitor.Log("Dialogue patch started!");
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.addMarriageDialogue)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(addMarriageDialogue_prefix))
        );
        EntryMonitor.Log("Dialogue patch done!");
    }

    public static void addMarriageDialogue_prefix(NPC __instance, string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
    {
        Random dialogueRandom = Utility.CreateDaySaveRandom(Game1.timeOfDay);

        switch (dialogue_file)
        {
            case "MarriageDialogue":
                EntryMonitor.Log("MarriageDialogue detected!");

                // Instantiation
                bool nameToggleBool = dialogue_key.Contains(__instance.Name);
                string startsKey = "";
                List<string> acceptableKeys = new List<string>{ "Rainy_Night", "Rainy_Day", "Indoor_Night", "Indoor_Day", "Outdoor", "Good", "Neutral", "Bad", "OneKid", "TwoKids", $"{Game1.currentSeason}", "spouseRoom", "funLeave", "jobLeave", "funReturn", "jobReturn", "patio"};

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
                __instance.currentMarriageDialogue.Add(new MarriageDialogueReference(dialogue_file, pickRandomDialogue(__instance.Name, dialogueRandom, startsKey, nameToggleBool), gendered, substitutions));
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
        List<string> listOfAllOtherNPCs = initializeNPCNameList(Game1.content, npcName, prefixDialogue);
        try
        {
            /*
             * Want to rewrite this to handle the two docs separately and then merge their keys and take distinct. It works as is but it's hard to manipulate based on the doc the key was found in.
             */

            // Load dictionary and only take its keys
            string asset1 = "Characters\\Dialogue\\MarriageDialogue";
            string asset2 = asset1 + npcName;
            Dictionary<string, string> dialogueDictionaryNPCSpecific = Game1.content.Load<Dictionary<string, string>>(asset2);
            Dictionary<string, string> dialogueDictionaryGeneral = Game1.content.Load<Dictionary<string, string>>(asset1);
            List<string> dialogueDictionaryAll = new List<string>(dialogueDictionaryNPCSpecific.Keys);
            List<string> dDGeneral = new List<string>(dialogueDictionaryGeneral.Keys);

            // Get all unique keys from both files
            dialogueDictionaryAll.AddRange(dDGeneral);
            List<string> dialogueDictionary = dialogueDictionaryAll.Distinct().ToList();

            // Find the keys that match our queries
            List<string> matchedKeys = new List<string>();

            for (int i = 0; i < dialogueDictionary.Count; i++)
            {
                if (dialogueDictionary[i].Contains(prefixDialogue) && !listOfAllOtherNPCs.Any(dialogueDictionary[i].Contains))
                {
                    if (!generalNameToggle || dialogueDictionary[i].Contains(npcName))
                    {
                        matchedKeys.Add(dialogueDictionary[i]);
                    }
                }
            }

            // For debugging purposes
            EntryMonitor.Log("Looked for keys matching the prefix: " + prefixDialogue);
            EntryMonitor.Log("Found " + matchedKeys.Count + " keys!");
            for (int i = 0; i < matchedKeys.Count; i++)
            {
                EntryMonitor.Log(matchedKeys[i].ToString());
            }

            // Return a key from our matched list
            return matchedKeys[daySaveRandom_Game.Next(matchedKeys.Count)];
        }
        catch (Exception ex)
        {
            EntryMonitor.Log($"Could not find a matching key! \n{ex}", LogLevel.Error);
            return "patio_"+npcName;
        }
    }
    public static List<string> initializeNPCNameList(LocalizedContentManager content, string exlude, string key)
    {
        /*
         * Want to rewrite this to have all NPC names and probably include multiple in one entry so that more than one can have an entry in it.
         * Maybe filter out all NPCs (just general dialogue) and then the rest filter by what has the NPC name in it
         */

        try
        {
            EntryMonitor.Log("Beginning sort!");
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
                    if (npcName != exlude && npcName != "")
                    {
                        fileNames.Add(npcName);
                    }
                }
            }
            List<string> otherNPCs = fileNames.Distinct().ToList();
            EntryMonitor.Log("Parsed NPC files!");

            // Check list of NPCs (debug)
            EntryMonitor.Log($"NPC names found (exluded {exlude}):");
            for (int i = 0; i < otherNPCs.Count; i++)
            {
                EntryMonitor.Log("->"+otherNPCs[i]+"<-");
            }

            return otherNPCs;

        }
        catch
        {
            EntryMonitor.Log("Could not get NPC names!");
            return new List<string>();
        }
    }
}