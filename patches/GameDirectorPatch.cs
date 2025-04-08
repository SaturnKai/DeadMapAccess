using System;
using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess.patches;

[HarmonyPatch(typeof(GameDirector))]
static class GameDirectorPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(GameDirector.Revive))]
    private static void Revive_Prefix() {
        if (DeadMap.spectating) {
            DeadMap.SetSpectating(false);
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(GameDirector.gameStateStart))]
    private static void GameStateStart_Prefix() {
        if (SemiFunc.IsMasterClient() && Configuration.hideValuables.Value) {
            NetworkController controller = Map.Instance.gameObject.GetComponent<NetworkController>();
            if (controller == null) {
                DeadMap.Logger.LogWarning("Failed to send hide valuables event: Network controller is null.");
                return;
            }

            controller.photonView.RPC("HideValuables", Photon.Pun.RpcTarget.All, new object[0]{});
        }

        if (DeadMap.spectating) {
            DeadMap.SetSpectating(false);
        }
    }
}