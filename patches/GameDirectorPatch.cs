using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(GameDirector))]
static class GameDirectorPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(GameDirector.Revive))]
    private static void Revive_Prefix() {
        if (DeadMap.spectating) {
            DeadMap.spectating = false;
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(GameDirector.gameStateStart))]
    private static void GameStateStart_Prefix() {
        if (DeadMap.spectating) {
            DeadMap.spectating = false;
        }
    }
}