using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess;

[BepInPlugin("dev.saturnkai.deadmapaccess", "DeadMapAccess", "1.0.2")]
public class DeadMap : BaseUnityPlugin
{
    internal static DeadMap Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    internal Harmony? Harmony { get; set; }
    private ManualLogSource _logger => base.Logger;

    // map texture
    public static RenderTexture? renderTexture = null;

    // map flags
    public static bool spectating = false;
    private bool active = false;
    private bool activePrev = false;

    // map scale animation
    private readonly float scaleSpeed = 5f;
    private float scale = 0.5f;
    private float targetScale = 1f;

    // map config
    private readonly Color borderColor = new Color32(19, 19, 19, 255);
    private static ConfigEntry<float> width = null!;
    private static ConfigEntry<float> height = null!;
    private static ConfigEntry<int> borderSize = null!;
    private static ConfigEntry<bool> toggle = null!;

    private void Awake() {
        // init plugin
        Instance = this;
        gameObject.transform.parent = null;
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        // load config
        width = Config.Bind("General", "Width", 600f, "The width of the map.");
        height = Config.Bind("General", "Height", 600f, "The height of the map.");
        borderSize = Config.Bind("General", "Border", 6, "The size of the map border.");
        toggle = Config.Bind("General", "Toggle", false, "Set the map to toggle instead of hold.");

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
        // keybinds
        if (toggle.Value && spectating) {
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
        float currentWidth = width.Value * scale;
        float currentHeight = height.Value * scale;
        float x = (Screen.width - currentWidth) / 2;
        float y = (Screen.height - currentHeight) / 2;

        // draw border
        Rect border = new(x - borderSize.Value, y - borderSize.Value, currentWidth + borderSize.Value * 2, currentHeight + borderSize.Value * 2);
        GUI.color = borderColor;
        GUI.DrawTexture(border, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // draw map
        GUI.DrawTexture(new Rect(x, y, currentWidth, currentHeight), renderTexture, ScaleMode.StretchToFill, false);
    }
}