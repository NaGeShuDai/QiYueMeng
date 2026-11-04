using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

// Created by PoetryPuzzleGame so existing scenes and scene rebuilds both get the ending.
public sealed class EndingPortal : MonoBehaviour
{
    PoetryPuzzleGame game;
    GameObject portal;
    Material material;
    Coroutine transition;
    GUIStyle label, button;
    Font font;
    public bool IsTransitioning => transition != null;
    public bool suppressBrowserForValidation;
    public string LastBrowserError { get; private set; }
    public string LastOpenedPath { get; private set; }
    public int LaunchCount { get; private set; }
    public string WebsitePath => Path.Combine(Application.streamingAssetsPath, "OurStory", "index.html");

    public void Initialize(PoetryPuzzleGame owner)
    {
        game = owner;
        if (!game.exitDoor) return;
        var door = game.exitDoor.GetComponentsInChildren<Renderer>(true)
            .FirstOrDefault(r => r.name.StartsWith("EXIT - hinged door", StringComparison.Ordinal));
        if (!door) return;
        // Measure the closed door even when continuing a completed save.
        var rotation = game.exitDoor.localRotation;
        game.exitDoor.localRotation = Quaternion.identity;
        var bounds = door.bounds;
        game.exitDoor.localRotation = rotation;
        portal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portal.name = "Ending - doorway vortex";
        Destroy(portal.GetComponent<Collider>());
        portal.transform.SetParent(game.exitDoor.parent, true);
        portal.transform.position = bounds.center + Vector3.back * .045f;
        portal.transform.localScale = new Vector3(bounds.size.x * .94f, bounds.size.y * .97f, 1);
        var shader = Resources.Load<Shader>("EndingVortex");
        if (!shader) { Debug.LogError("EndingVortex shader missing"); Destroy(portal); return; }
        material = new Material(shader);
        portal.GetComponent<Renderer>().sharedMaterial = material;
        portal.SetActive(false);
    }

    public void Sync(bool escaped)
    {
        if (portal) portal.SetActive(escaped);
        if (!escaped && transition != null) { StopCoroutine(transition); transition = null; }
    }

    public void Begin()
    {
        if (transition == null) transition = StartCoroutine(Reveal());
    }

    IEnumerator Reveal()
    {
        yield return new WaitForSecondsRealtime(3.2f);
        transition = null;
        if (game.State.escaped) OpenWebsite();
    }

    public void OpenWebsite()
    {
        LastBrowserError = null;
        string path = Path.GetFullPath(WebsitePath);
        if (!File.Exists(path)) {
            LastBrowserError = "Missing website: " + path;
            game.Tell("网页文件不完整，请完整解压游戏后重试。");
            Debug.LogError(LastBrowserError);return;
        }
        try {
            if (!suppressBrowserForValidation) {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                // Let the Windows shell open the local HTML path directly, including Chinese and spaces.
                using (var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                    FileName = path, UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(path)
                })) { }
#else
                Application.OpenURL(new Uri(path).AbsoluteUri);
#endif
            }
            LastOpenedPath = path;LaunchCount++;
            Debug.Log("ENDING_BROWSER_OPEN_REQUESTED " + path);
        } catch (Exception e) {
            LastBrowserError = e.Message;
            game.Tell("网页未能打开，可在游戏目录双击“打开纪念网页”。");
            Debug.LogError("Opening ending website failed: " + e);
        }
    }

    void OnGUI()
    {
        if (!game || game.State == null || !game.State.escaped) return;
        if (label == null) {
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei" }, 20);
            label = new GUIStyle(GUI.skin.label) { font = font, fontSize = 20, alignment = TextAnchor.MiddleCenter };
            label.normal.textColor = new Color(1, .91f, .78f);
            button = new GUIStyle(GUI.skin.label) { font = font, fontSize = 16, alignment = TextAnchor.MiddleCenter };
        }
        GUI.Label(new Rect(Screen.width / 2f - 230, Screen.height - 106, 460, 38),
            IsTransitioning ? "穿过这扇门，是我们的故事……" : "第七码头 · 故事仍在继续", label);
        if (!IsTransitioning && GUI.Button(new Rect(Screen.width / 2f - 100, Screen.height - 62, 200, 34), "去往目的地", button)) OpenWebsite();
    }

    void OnDestroy() { if (portal) Destroy(portal); if (material) Destroy(material); if (font) Destroy(font); }
}
