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

        // load render texture
        if (DeadMap.camera != null) {
            Map.Instance.StartCoroutine(LoadRenderTexture(DeadMap.camera));
        }
    }

    private static IEnumerator LoadRenderTexture(Camera camera) {
        RenderTexture renderTexture = camera.activeTexture;
        while (renderTexture == null) {
            yield return null;
            renderTexture = camera.activeTexture;
        }

        DeadMap.renderTexture = renderTexture;
        DeadMap.Logger.LogInfo("Loaded map render texture.");
    }
}