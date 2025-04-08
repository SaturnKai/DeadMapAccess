using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(PlayerAvatar))]
static class PlayerAvatarPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(PlayerAvatar.SetSpectate))]
    private static void SetSpectate_Prefix() {
        // set map to active
        Map.Instance.ActiveParent.SetActive(true);

        // find camera if render texture unset
        if (DeadMap.renderTexture == null) {
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>(includeInactive: true);
            foreach (var camera in cameras) {
                if (camera.name == "Dirt Finder Map Camera") {
                    Map.Instance.StartCoroutine(LoadMapTexture(camera));
                }
            }
        }
    }

    private static IEnumerator LoadMapTexture(Camera camera) {
        RenderTexture renderTexture = camera.activeTexture;
        while (renderTexture == null) {
            yield return null;
            renderTexture = camera.activeTexture;
        }

        DeadMap.renderTexture = renderTexture;
        DeadMap.Logger.LogInfo("Loaded map render texture.");
    }
}