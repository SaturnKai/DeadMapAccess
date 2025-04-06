using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(SpectateCamera))]
static class SpectateCameraPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(SpectateCamera.StateNormal))]
    private static void StateNormal_Prefix() {
        if ((SemiFunc.RunIsLevel() || SemiFunc.RunIsShop()) && !DeadMap.spectating) {
            if (SpectateCamera.instance.player != null)
                DeadMap.SetSpectating(true);
            else
                DeadMap.Logger.LogWarning("[SpectateCamera] Player instance is null, so death map will not be activated.");
        }
    }
}