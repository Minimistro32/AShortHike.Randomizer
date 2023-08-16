﻿using HarmonyLib;
using UnityEngine;

namespace AShortHike.Randomizer
{
    /// <summary>
    /// When starting the intro conversation, skip it if the setting is enabled
    /// </summary>
    [HarmonyPatch(typeof(DialogueController), nameof(DialogueController.StartConversation))]
    class DialogueController_SkipStart_Patch
    {
        public static bool Prefix(string startNode, ref IConversation __result)
        {
            if (startNode == "TitleScreenIntroStart" && Main.Randomizer.MultiworldSettings.skipCutscenes)
            {
                // If starting the intro cutscene, set conversation to null and skip start
                __result = null;
                return false;
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(Cutscene), nameof(Cutscene.Start))]
    class Cutscene_Start_Patch
    {
        public static bool Prefix(Cutscene __instance)
        {
            if (Main.Randomizer.MultiworldSettings.skipCutscenes)
            {
                // If starting the intro cutscene, set give the phone and skip
                Singleton<GlobalData>.instance.gameData.AddCollected(CollectableItem.Load("Cellphone"), 1, false);
                Object.Destroy(__instance.gameObject);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// When in dialog where it makes you wait for a long time, dont wait
    /// </summary>
    [HarmonyPatch(typeof(YarnCommands), nameof(YarnCommands.Wait))]
    class Dialog_Wait_Patch
    {
        public static void Prefix(string[] args)
        {
            if (Main.Randomizer.MultiworldSettings.skipCutscenes && args.Length > 0 && float.Parse(args[0]) >= 5)
            {
                Main.Log("Shortening waiting cutscene");
                args[0] = "1";
            }
        }
    }

}