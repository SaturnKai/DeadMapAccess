using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace DeadMapAccess;

[BepInPlugin("dev.saturnkai.deadmapaccess", "DeadMapAccess", "1.0")]
public class DeadMap : BaseUnityPlugin
{
    internal static DeadMap Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    internal Harmony? Harmony { get; set; }
    private ManualLogSource _logger => base.Logger;

    // map textures
    public static RenderTexture? renderTexture = null;
    private Texture2D? mapTexture = null;

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
        borderSize = Config.Bind("General", "Border", 3, "The size of the map border.");
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

        targetScale = active ? 1f : 0.5f;

        // animate map
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
        if (!spectating)
            return;

        // play map sound
        if (active != activePrev) {
            activePrev = active;
            Sound sound = activePrev ? PlayerAvatar.instance.mapToolController.SoundStart : PlayerAvatar.instance.mapToolController.SoundStop;
            sound.Play(SpectateCamera.instance.transform.position, 1f, 1f, 1f, 1f);
        }

        if (active) {
            // update map texture
            if (renderTexture != null) {

                // initialize main texture if unset
                if (mapTexture == null) {
                    int width = renderTexture.width + 2 * borderSize.Value;
                    int height = renderTexture.height + 2 * borderSize.Value;
                    mapTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

                    // fill map with border color
                    Color[] pixels = new Color[mapTexture.width * mapTexture.height];
                    for (int i = 0; i < pixels.Length; i++) {
                        pixels[i] = borderColor;
                    }
                    mapTexture.SetPixels(pixels);
                }

                RenderTexture.active = renderTexture;
                mapTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), borderSize.Value, borderSize.Value);
                mapTexture.Apply();
                RenderTexture.active = null;
            }

            // set map active
            Map.Instance.Active = true;

            // fade camera
            CameraTopFade.Instance.Set(0.5f, 0.1f);

            // draw map
            float currentWidth = width.Value * scale;
            float currentHeight = height.Value * scale;
            GUI.DrawTexture(new Rect((Screen.width - currentWidth) / 2, (Screen.height - currentHeight) / 2, currentWidth, currentHeight), mapTexture);
        } else {
            Map.Instance.Active = false;
            targetScale = 0.5f;
        }
    }
}