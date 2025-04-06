using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(SpectateCamera))]
static class SpectateCameraPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(SpectateCamera.StateNormal))]
    private static void StateNormal_Prefix() {
        if ((SemiFunc.RunIsLevel() || SemiFunc.RunIsShop()) && !DeadMap.spectating && SpectateCamera.instance.player != null) {
            DeadMap.SetSpectating(true);
        }
    }
}