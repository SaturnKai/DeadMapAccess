using HarmonyLib;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(RoundDirector))]
static class RoundDirectorPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(RoundDirector.StartRound))]
    private static void StartRound_Postfix() {
        // reset hidden valuables
        DeadMap.hideValuables = false;
    }
}