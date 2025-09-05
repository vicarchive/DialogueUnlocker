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

            switch (__instance.DialogueKey)
            {
                case string a when a.Contains("4406"): startsKey = "Obstacle"; break; // Spouse encountered an obstacle and stays in bed
                case string a when a.Contains("4420"): // Is more likely to come up if you have low hearts with spouse
                case string b when b.Contains("4421"):
                case string c when c.Contains("4423"):
                case string d when d.Contains("4424"):
                case string e when e.Contains("4425"):
                case string f when f.Contains("4426"):
                case string g when g.Contains("4431"):
                case string h when h.Contains("4432"):
                case string i when i.Contains("4433"): startsKey = "Moody"; break;
                case string a when a.Contains("4422"): startsKey = "MoodyGendered"; break; // Same as previous but IsGendered is set to True
                case string a when a.Contains("4427"):
                case string b when b.Contains("4429"): startsKey = "MoodyFem"; break; // Female specific dialogue. Adding this into a husband's dialogue sheet will not do anything
                                                                                      // Same sex couples
                case string a when a.Contains("4439"): startsKey = "AdoptionObstacle"; break; // Expecting to adopt a child, obstacle found
                case string a when a.Contains("4440"): startsKey = "AdoptionName"; break; // Expecting to adopt a child, uses farmer's name
                case string a when a.Contains("4441"): startsKey = "AdoptionEndearment"; break; // Expecting to adopt a child, uses endearment term
                                                                                                // Opposite sex couples - female spouses
                case string a when a.Contains("4442"): startsKey = "PregnancyObstacle"; break; // Pregnant, obstacle found
                case string a when a.Contains("4445"): startsKey = "PregnancyEndearment"; break; // Pregnant, uses endearment term
                case string a when a.Contains("4444"): startsKey = "PregnancyName"; break; // Pregnant, uses farmer's name
                                                                                           // Opposite sex couples - male spouses
                case string a when a.Contains("4446"): startsKey = "ExpectingObstacle"; break; // Expecting, obstacle found, gendered
                case string a when a.Contains("4448"): startsKey = "ExpectingEndearment"; break; // Expecting, uses endearment term
                case string a when a.Contains("4447"): startsKey = "ExpectingName"; break; // Expecting, uses farmer's name, gendered
                                                                                           // Parent
                case string a when a.Contains("4449"): startsKey = "ParentOneObstacle"; break; // Parent of one child, found obstacle, gendered
                case string a when a.Contains("4452"): startsKey = "ParentTwoObstacle"; break; // Parent of two children, found obstacle, gendered

                case string a when a.Contains("4455"): startsKey = "BreakfastNevermind"; break; // Obstacle found and previous marriage dialogue was for breakfast, gendered
                case string a when a.Contains("4462"): startsKey = "WateredCrops"; break; // Spouse watered crops, gendered
                case string a when a.Contains("MultiplePetBowls_watered"): startsKey = "FilledMultiplePetBowls"; break; // Spouse watered multiple pet bowls
                case string a when a.Contains("4463"): startsKey = "FilledOnePetBowl"; break; // Spouse filled one pet bowl
                case string a when a.Contains("4465"): startsKey = "GreetingWorkDone"; break; //Not sure, uses endearment
                case string a when a.Contains("4466"): startsKey = "GreetingNoWork"; break; //Also not sure, uses endearment
                case string a when a.Contains("4470"): startsKey = "Sprinkler"; break; // Spouse wanted to water crops, but player has sprinklers, gendered
                case string a when a.Contains("4474"): startsKey = "FeedAnimals"; break; // Spouse fed the animals, gendered
                case string a when a.Contains("4481"): startsKey = "RepairFences"; break; // Spouse repaired fences, gendered
                case string a when a.Contains("4486"): startsKey = "IntroduceFurniture"; break; // Spouse got some furniture, first line, uses endearment lower
                case string a when a.Contains("4488"): // gendered
                case string b when b.Contains("4489"): startsKey = "ShowFurniture"; break; // Spouse got some furniture, second line
                case string a when a.Contains("4490"): startsKey = "FurnitureObstacle"; break; // Spouse wanted to redecorate, but was unable to
                case string a when a.Contains("4496"): startsKey = "ChangeWallpaper"; break; // Spouse changed the wallpaper
                case string a when a.Contains("4497"): startsKey = "ChangeFlooring"; break; // Spouse changed the flooring
                case string a when a.Contains("4498"): startsKey = "FurnitureRemnisce"; break; //Unsure, this seems to be tied to a furniture item valued at 13 when its raining?, gendered
                case string a when a.Contains("4499"): startsKey = "Remnisce"; break; //Also unsure, as above is tied to low hearts during a rainy day
                                                                                      // NPC:cs:4500 is called via regular SDV dialogue, so changing it here won't do anything
            }


            if (startsKey == "")
            {
                EntryMonitor.Log($"Could not find a match for {__instance.DialogueKey}!");
            }
            else
            {
                string chosenKey = pickRandomDialogue(spouseName, daySaveRandom, startsKey, __instance.DialogueKey);
                EntryMonitor.Log($"Changing our key to {chosenKey}!");

                // Why
                if (chosenKey != "" && !chosenKey.Contains("NPC") && !chosenKey.Contains("MultiplePetBowls_watered"))
                {
                    ____dialogueFile = new NetString("MarriageDialogue");
                    ____dialogueKey = new NetString(chosenKey);
                }
                // Otherwise it's the same key as before and just let the game handle it as is
                // Or we could not find a key and we let the game handle it
            }

        }
        else if (__instance.DialogueFile == "MarriageDialogue")
        {
            EntryMonitor.Log("Found MarriageDialogue!");

            string startsKey = "";
            List<string> acceptableKeys = new List<string> { $"{Game1.currentSeason}_{Game1.dayOfMonth}", $"{Game1.currentSeason}_{spouseName}", "Rainy_Night", "Rainy_Day", "Indoor_Night", "Indoor_Day", "Outdoor", "Good", "Neutral", "Bad", "OneKid", "TwoKids", "spouseRoom", "funLeave", "jobLeave", "funReturn", "jobReturn", "patio" };
            // Game season stuff is first. Day of month presides over regular.

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
                string chosenKey = pickRandomDialogue(spouseName, daySaveRandom, startsKey);
                if (chosenKey != "") // Found a new key
                {
                    EntryMonitor.Log($"Changing current key to {chosenKey}!");
                    ____dialogueKey = new NetString(chosenKey);
                }
                // If another key wasn't found then we just let the game handle it
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
    private static string pickRandomDialogue(string npcName, Random daySaveRandom_Game, string prefixDialogue, string optional = "")
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
                Dictionary<string, string> marriageDialogueSpouseDictionary = Game1.content.Load<Dictionary<string, string>>(asset2);
                List<string> marriageDialogueSpouseDictionaryKeys = new List<string>(marriageDialogueSpouseDictionary.Keys);

                foreach (string key in marriageDialogueSpouseDictionaryKeys)
                {
                    if (key.Contains(prefixDialogue))
                    {
                        marriageDialogueSpouseKeys.Add(key);
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
            if (marriageDialogueKeys.Count == 0)
            {
                return ""; // We handle this in the main function
            }
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
            
            /* Doesn't work because we need a full list to filter names out. Might be useful to find your spouse, but I'm not sure how this will work on Ginger Island.
            List<string> otherNPCs = new List<string>();
            foreach (NPC character in Game1.currentLocation.characters)
            {
                if (character.datable.Value)
                {
                    otherNPCs.Add(character.Name);
                }
            }
            */

            // Check list of NPCs (debug)
            EntryMonitor.Log($"Found {otherNPCs.Count} dateable NPCs!");
            return otherNPCs;

        }
        catch (Exception ex)
        {
            throw new Exception($"List of NPCs could not be instantiated! \n{ex}");
        }
    }
}