using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(Map))]
static class MapPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(Map.Awake))]
    private static void Awake_Postfix(Map __instance) {
        if (__instance.gameObject.GetComponent<NetworkController>() == null) {
            __instance.gameObject.AddComponent<NetworkController>();
        }
    }

    [HarmonyPostfix, HarmonyPatch(nameof(Map.AddValuable))]
    private static void AddValuable_Postfix(Map __instance) {
        if (DeadMap.spectating && DeadMap.hideValuables) {
            MapValuable[] valuables = __instance.OverLayerParent.GetComponentsInChildren<MapValuable>();
            foreach (MapValuable v in valuables) {
                v.gameObject.SetActive(false);
            }
        }
    }
}