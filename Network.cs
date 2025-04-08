using Photon.Pun;

namespace DeadMapAccess;

internal class NetworkController : MonoBehaviourPun {
    [PunRPC]
    internal void HideValuables() {
        if (!DeadMap.hideValuables) {
            DeadMap.hideValuables = true;
            DeadMap.Logger.LogInfo("Valuables set to hidden.");
        }
    }
}