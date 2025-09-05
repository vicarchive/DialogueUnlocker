using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
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

        harmony.Patch(
            original: AccessTools.DeclaredConstructor(typeof(MarriageDialogueReference), new Type[] { typeof(string), typeof(string), typeof(bool), typeof(string[]) }),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(marriageDialogueReference_postfix))
        );
        EntryMonitor.Log("MarriageDialogueReference postfix has been applied!");
    }

    /// <summary> Postfix for any MarriageDialogueReference (required for marriage dialogue) </summary>
    /// <param name="__instance"> The created MarriageDialogueReference to fix. </param>
    /// The other params are references to the members of the .ctor.
    private static void marriageDialogueReference_postfix(MarriageDialogueReference __instance, ref NetString ____dialogueKey, ref NetString ____dialogueFile, ref NetBool ____isGendered, ref NetStringList ____substitutions)
    {
        Random daySaveRandom = Utility.CreateDaySaveRandom(Game1.timeOfDay);
        List<string> listOfNPCNames = initializeNPCNameList(Game1.content);
        string spouseName = Game1.player.getSpouse().Name; // This SHOULD work with multiplayer ?? But not with polyamory mods. Might find a different way but this is the most clever I could think of
        EntryMonitor.Log($"Entered postfix with spouse name {spouseName}, and dialogue file {__instance.DialogueFile} and key {__instance.DialogueKey}!");

        // Since this is the general MarriageDialogueReference, we also account for the vanilla game usage of the StringsFromCSFiles that we'll add variance for here as well.
        if (__instance.DialogueFile == "Strings\\StringsFromCSFiles")
        {
            EntryMonitor.Log("Found Strings!");

            string startsKey = "";
            /*
            switch (__instance.DialogueKey)
            {  // add every use of strings to cs files in here.....

            }
            */

            if (startsKey == "")
            {
                EntryMonitor.Log($"Could not find a match for {__instance.DialogueKey}!");
            }
            else
            {
                string chosenKey = pickRandomDialogue(spouseName, daySaveRandom, startsKey, false, __instance.DialogueKey);
                EntryMonitor.Log($"Changing our key to {chosenKey}!");
                if (!chosenKey.Contains("NPC") {
                    ____dialogueFile = new NetString("MarriageDialogue");
                    ____dialogueKey = new NetString(chosenKey);
                }
                // Otherwise it's the same key as before and just let the game handle it as is
            }
            
        }
        else if (__instance.DialogueFile == "MarriageDialogue")
        {
            EntryMonitor.Log("Found MarriageDialogue!");
            bool nameToggleBool = __instance.DialogueKey.Contains(spouseName) && __instance.DialogueKey.Contains(Game1.currentSeason); // Season specific dialogue lookup. Otherwise no reason to toggle name search on. And might remove this later I have no idea
            /* !!!!!!!!!!!!!!!!!!!! also consider stuff named like winter_14_alex, but that takes precedense but needs the date in there */

            string startsKey = "";
            List<string> acceptableKeys = new List<string> { "Rainy_Night", "Rainy_Day", "Indoor_Night", "Indoor_Day", "Outdoor", "Good", "Neutral", "Bad", "OneKid", "TwoKids", $"{Game1.currentSeason}", "spouseRoom", "funLeave", "jobLeave", "funReturn", "jobReturn", "patio" };

            // Match with one of the accepted keys above
            bool found = false;
            int i = 0;
            while (!found && i < acceptableKeys.Count)
            {
                if (__instance.DialogueKey.StartsWith(acceptableKeys[i]))
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
                // And let the game handle it..
            }
            else
            {
                // Set the dialogue key to whatever we want it to be
                string chosenKey = pickRandomDialogue(spouseName, daySaveRandom, startsKey, nameToggleBool);
                EntryMonitor.Log($"Changing current key to {chosenKey}!");
                ____dialogueKey = new NetString(chosenKey);
            }
        }
        else
        {
            EntryMonitor.Log($"Could not classify dialogue file!: {__instance.DialogueFile}");
            // Do not touch, let the game handle it
        }
        
    }

    /// <summary> Rewrite of the dialogue thing in 1.6.15 </summary>
    /// <param name="npcName"> Spouse name. </param>
    /// <param name="daySaveRandom_Game"> Game specific random function. </param>
    /// <param name="prefixDialogue"> The prefix of dialogue to match for. </param>
    /// <param name="generalNameToggle"> Whether to look for the NPC's name in the general marriage XNB or not. </param>
    /// <param name="optional"> Any extra string to include in the list to consider. Used exclusively for Strings\\StringsFromCSFiles. </param>
    private static string pickRandomDialogue(string npcName, Random daySaveRandom_Game, string prefixDialogue, bool generalNameToggle, string optional = "")
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

            List<string> extraString = new List<string>();
            if (optional != "")
            {
                extraString.Add(optional);
            }

            // Get all unique keys from both files
            marriageDialogueSpouseKeys.AddRange(marriageDialogueUniversalKeys);
            marriageDialogueSpouseKeys.AddRange(marriageDialogueUnivSpouseKeys);
            marriageDialogueSpouseKeys.AddRange(extraString);
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

    /// <summary> Creates a list of dateable NPCs based on unique marriage dialogue files. </summary>
    /// <param name="content"> The game's content loader. </param>
    /// <returns> List of all dateable NPCs. </returns>
    private static List<string> initializeNPCNameList(LocalizedContentManager content)
    {
        try
        {
            // Getting the files in the marriage directory
            string rootLocation = content.GetContentRoot();
            string dialogueDirectory = "Characters\\Dialogue";
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
            EntryMonitor.Log($"Found {otherNPCs.Count} dateable NPCs!");
            return otherNPCs;

        }
        catch (Exception ex)
        {
            throw new Exception($"List of NPc's could not be instantiated! \n{ex}");
        }
    }
}