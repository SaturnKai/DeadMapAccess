using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(DirtFinderMapPlayer))]
static class DirtFinderMapPlayerPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(DirtFinderMapPlayer.Awake))]
    private static void StartRound_Postfix(DirtFinderMapPlayer __instance) {
        // find camera
        Camera camera = __instance.GetComponentInChildren<Camera>();
        if (camera.name != "Dirt Finder Map Camera") {
            DeadMap.Logger.LogWarning("Dirt Finder Map Camera not found in map children.");
            return;
        }

        // update camera
        if (DeadMap.camera == null) {
            DeadMap.camera = camera;
            DeadMap.cameraOrthographicDefault = camera.orthographicSize;
        }
    }
}