using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess;

[BepInPlugin("dev.saturnkai.deadmapaccess", "DeadMapAccess", "1.0.4")]
public class DeadMap : BaseUnityPlugin
{
    internal static DeadMap Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    internal Harmony? Harmony { get; set; }
    private ManualLogSource _logger => base.Logger;

    // map values
    internal static RenderTexture? renderTexture = null;
    internal static Camera? camera = null;
    internal static float cameraOrthographicDefault = 2.5f;

    internal static bool spectating = false;
    internal static bool hideValuables = false;
    private bool active = false;
    private bool activePrev = false;

    // map scale animation
    private readonly float scaleSpeed = 5f;
    private float scale = 0.5f;
    private float targetScale = 1f;

    // map zoom
    private readonly float zoomSpeed = 0.2f;
    private readonly float zoomMin = 1f;
    private readonly float zoomMax = 20f;

    private void Awake() {
        // init plugin
        Instance = this;
        gameObject.transform.parent = null;
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        // load config
        Configuration.Init(Config);

        // patches
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    internal void Unpatch() {
        // unpatch
        Harmony?.UnpatchSelf();
    }

    private void Update() {
        // map toggle key
        if (Configuration.toggle.Value && spectating) {
            if (SemiFunc.InputDown(InputKey.Map)) 
                active = !active;
        } else if (spectating)
            active = SemiFunc.InputHold(InputKey.Map);

        // animate map
        targetScale = active ? 1f : 0.5f;
        scale = Mathf.Lerp(scale, targetScale, Time.deltaTime * scaleSpeed);

        // update camera transform
        if (spectating && SpectateCamera.instance != null && SpectateCamera.instance.currentState == SpectateCamera.State.Normal) {
            Transform transform = SpectateCamera.instance.transform;

            if (DirtFinderMapPlayer.Instance.PlayerTransform == null) {
                Logger.LogWarning("DirtFinderMapPlayer transform null.");
                DirtFinderMapPlayer.Instance.PlayerTransform = new GameObject().transform;
            }

            DirtFinderMapPlayer.Instance.PlayerTransform.position = transform.position;
            DirtFinderMapPlayer.Instance.PlayerTransform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            // update tracer
            PlayerController.instance.playerAvatarScript.LastNavmeshPosition = SpectateCamera.instance.player.LastNavmeshPosition;

            // show stats
            if (active && Configuration.showUpgrades.Value)
                StatsUI.instance.Show();

            // map zoom
            if (active) {
                float scrollDelta = SemiFunc.InputScrollY() * 0.01f;
                if (scrollDelta != 0f && camera != null) {
                    camera.orthographicSize -= scrollDelta * zoomSpeed;
                    camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, zoomMin, zoomMax);
                }
            }
        }
    }

    private void OnGUI() {
        // ensure player is spectating
        if (!spectating)
            return;

        // play map sound
        if (active != activePrev) {
            activePrev = active;
            Sound sound = activePrev ? PlayerAvatar.instance.mapToolController.SoundStart : PlayerAvatar.instance.mapToolController.SoundStop;
            if (SpectateCamera.instance != null)
                sound.Play(SpectateCamera.instance.transform.position, 1f, 1f, 1f, 1f);
        }

        // update map instance
        Map.Instance.Active = active;

        // inactive map
        if (!active) {
            targetScale = 0.5f;
            return;
        }

        // fade camera
        CameraTopFade.Instance.Set(0.5f, 0.1f);

        // map position and size
        float width = Configuration.width.Value * scale;
        float height = Configuration.height.Value * scale;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // draw border
        Rect border = new(
            x - Configuration.borderSize.Value,
            y - Configuration.borderSize.Value,
            width + Configuration.borderSize.Value * 2,
            height + Configuration.borderSize.Value * 2
        );
        GUI.color = Configuration.borderColor;
        GUI.DrawTexture(border, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // draw map
        GUI.DrawTexture(new Rect(x, y, width, height), renderTexture, ScaleMode.StretchToFill, false);
    }

    internal static void SetSpectating(bool isSpectating) {
        // update spectating state
        spectating = isSpectating;

        // reset camera zoom
        if (camera != null) {
            camera.orthographicSize = cameraOrthographicDefault;
        }

        // toggle valuables
        if (hideValuables) {
            MapValuable[] valuables = Map.Instance.OverLayerParent.GetComponentsInChildren<MapValuable>(true);
            foreach (MapValuable v in valuables) {
                v.gameObject.SetActive(!spectating);
            }
        }
    }
}