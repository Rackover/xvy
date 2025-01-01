using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;

public class Performance : MonoBehaviour
{
    public class PerformanceToggle
    {
        private Func<string> displayText;
        private string name;

        private string cachedText;

        public Action<sbyte> set;

        public PerformanceToggle(string name, Action<sbyte> set, Func<string> displayText)
        {
            this.name = name;
            this.set = (b)=>
            {
                set(b);
                RefreshTxt();
            };

            this.displayText = displayText;

            RefreshTxt();
        }

        public void RefreshTxt()
        {
            cachedText = string.Format("{0}: {1}", name, displayText());
        }

        public string GetDisplayTextSafe()
        {
            return cachedText;
        }
    }

    [SerializeField]
    private GameObject performanceInfoParent;

    [SerializeField]
    private UnityEngine.UI.Text performanceInfo;

    [SerializeField]
    private UnityEngine.UI.Text performanceMenu;

    [SerializeField]
    private MaskableGraphic blackScreen;

    [SerializeField]
    private GameObject split;

    public static Performance i;

    public bool IsPerformanceDisplayed { get { return performanceInfoParent.gameObject.activeSelf; } }

    public bool HasDisabledRenderTarget { get { return !blackScreen.enabled; } }

    private PlayerInput input;

    private bool readyForInput = true;

    private readonly Vector2[] textureSizes = new Vector2[]
    {
        new Vector2(960, 640),
        new Vector2(640, 426),
        new Vector2(480, 320),
        
#if !UNITY_XBOX360
        new Vector2(1920, 1280),
        new Vector2(1280, 853),
#endif
    };

#if !X360
    private const string ON = "<color=#AAFFAA>ON</color>";
    private const string OFF = "<color=#FF5555>OFF</color>";
#else
    private const string ON = "ON";
    private const string OFF = "OFF";
#endif

    private PerformanceToggle[] toggles;

    private int selectedItem = 0;

    private int sizeIndex = 0;

    public void Toggle()
    {
        performanceInfoParent.gameObject.SetActive(!performanceInfoParent.activeSelf);
    }

    void Awake()
    {
        i = this;
    }

    void OnDestroy()
    {
        i = null;
    }


    private void Start()
    {
        List<PerformanceToggle> toggles = new List<PerformanceToggle>();

        toggles.Add(new PerformanceToggle(
            "Motion blur", (_)=> Toggle<UnityStandardAssets.ImageEffects.MotionBlur>(), ()=> Get<UnityStandardAssets.ImageEffects.MotionBlur>() ? ON : OFF
        ));

        toggles.Add(new PerformanceToggle(
            "Outline", (_)=>Toggle<UnityStandardAssets.ImageEffects.EdgeDetection>(), () => Get<UnityStandardAssets.ImageEffects.EdgeDetection>() ? ON : OFF
        ));

        toggles.Add(new PerformanceToggle(
            "RTSize", (sign) => ResizeRenderTextures(sign), () => string.Format("{0}x{1}", textureSizes[sizeIndex].x, textureSizes[sizeIndex].y)
        ));

        toggles.Add(new PerformanceToggle(
            "BGM", (_) => ToggleMusic(), () => MusicEnabled() ? ON : OFF
        ));

        toggles.Add(new PerformanceToggle(
            "RenderTarget", (_) => KillRenderTarget(), ()=> blackScreen.enabled ? ON : OFF
        ));

        this.toggles = toggles.ToArray();

        input = PlayerInput.MakeForPlatform();
        input.SetPlayerIndex(sizeIndex);

        performanceInfoParent.gameObject.SetActive(Game.i.ShowPerformanceInfo);
    }

    private void KillRenderTarget()
    {
        if (blackScreen.enabled)
        {
            Camera[] cameras = GetAllCameras();
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].targetTexture = null;
            }
        }
        else
        {
            ResizeRenderTextures(0);
        }

        split.gameObject.SetActive(!split.gameObject.activeSelf);
        blackScreen.enabled = !blackScreen.enabled;
    }

    private bool MusicEnabled()
    {
        Jukebox jk = FindObjectOfType<Jukebox>();
        if (jk)
        {
            return jk.IsEnabled;
        }

        return false;
    }

    private void ToggleMusic()
    {
        Jukebox jukebox = FindObjectOfType<Jukebox>();
        jukebox.Toggle(!jukebox.IsEnabled);
    }

    private void Update()
    {
        PushPerformanceInfo();
        DrawPerformanceMenu();

        if (performanceInfoParent.activeSelf)
        {
            input.Refresh();

            Vector2 dpad = input.GetDPad();

            if (dpad.x > 0.5f)
            {
                if (readyForInput)
                {
                    toggles[selectedItem].set(1);
                    readyForInput = false;
                }
            }
            else if (dpad.x < -0.5f)
            {
                if (readyForInput)
                {
                    toggles[selectedItem].set(-1);
                    readyForInput = false;
                }
            }
            else if (dpad.y < -0.5f)
            {
                if (readyForInput)
                {
                    selectedItem++;
                    selectedItem = selectedItem % toggles.Length;
                    readyForInput = false;
                }
            }
            else if (dpad.y > 0.5f)
            {
                if (readyForInput)
                {
                    selectedItem--;
                    if (selectedItem < 0)
                    {
                        selectedItem = toggles.Length - 1;
                    }

                    readyForInput = false;
                }
            }
            else
            {
                readyForInput = true;
            }
        }
    }

    void ResizeRenderTextures(int sign)
    {
        if (!performanceInfoParent.gameObject.activeSelf)
        {
            return;
        }

        if (!readyForInput)
        {
            return;
        }

        HashSet<RenderTexture> renderTextures = new HashSet<RenderTexture>();

        RawImage[] rawImages = FindObjectsOfType<RawImage>();

        Dictionary<RenderTexture, List<Action<RenderTexture>>> renderTextureSetters = new Dictionary<RenderTexture, List<Action<RenderTexture>>>();

        foreach (RawImage rawImageRef in rawImages)
        {
            RawImage rawImage = rawImageRef;
            if (rawImage.texture is RenderTexture)
            {
                var rt = (RenderTexture)rawImage.texture;
                renderTextures.Add(rt);

                if (!renderTextureSetters.ContainsKey(rt))
                {
                    renderTextureSetters.Add(rt, new List<Action<RenderTexture>>());
                }

                renderTextureSetters[rt].Add((o) =>
                {
                    rawImage.texture = o;
                });
            }
        }

        Camera[] cameras = GetAllCameras();

        foreach (Camera camRef in cameras)
        {
            Camera cam = camRef;
            if (cam.targetTexture)
            {
                renderTextures.Add(cam.targetTexture);

                if (!renderTextureSetters.ContainsKey(cam.targetTexture))
                {
                    renderTextureSetters.Add(cam.targetTexture, new List<Action<RenderTexture>>());
                }

                renderTextureSetters[cam.targetTexture].Add((o) =>
                {
                    cam.targetTexture = o;
                });
            }
        }

        if (sign > 0)
        {
            sizeIndex = (sizeIndex + 1) % textureSizes.Length;
        }
        else if (sign < 0)
        {
            sizeIndex--;
            if (sizeIndex < 0)
            {
                sizeIndex = textureSizes.Length - 1;
            }
        }

        Vector2 size = textureSizes[sizeIndex];

        int ti = 0;
        // Actually do the resizing
        foreach (var rt in renderTextures)
        {
            ti++;

            RenderTexture newRenderTexture = new RenderTexture((int)size.x, (int)size.y, rt.depth, rt.format);
            newRenderTexture.name = "RT_" + size.x + "x" + size.y + "_" + (ti + 1);

            for (int i = 0; i < renderTextureSetters[rt].Count; i++)
            {
                Game.i.ReplaceTexture(rt, newRenderTexture);
                renderTextureSetters[rt][i](newRenderTexture);
            }

            if (rt.GetInstanceID() < 0)
            {
                Destroy(rt);
            }
        }
    }

    Camera[] GetAllCameras()
    {
        return FindObjectsOfType<Camera>();
    }

    bool Get<T>() where T : Behaviour
    {
        if (!performanceInfoParent.gameObject.activeSelf)
        {
            return false;
        }

        if (!readyForInput)
        {
            return false;
        }

        bool enabled = false;
        var cameras = GetAllCameras();
        foreach (var cam in cameras)
        {
            var comp = cam.GetComponent<T>();

            if (comp)
            {
                enabled |= comp.enabled;
            }
        }

        return enabled;
    }

    void Toggle<T>() where T : Behaviour
    {
        if (!performanceInfoParent.gameObject.activeSelf)
        {
            return;
        }

        if (!readyForInput)
        {
            return;
        }

        var cameras = GetAllCameras();
        foreach (var cam in cameras)
        {
            var comp = cam.GetComponent<T>();

            if (comp)
            {
                comp.enabled = !comp.enabled;
            }
        }
    }

    void DrawPerformanceMenu()
    {
        if (performanceInfoParent.activeSelf)
        {
            StringBuilder sb = new StringBuilder();

            for (int index = 0; index < this.toggles.Length; index++)
            {
                if (index == selectedItem)
                {
                    sb.Append(" > ");
                }

                sb.Append(this.toggles[index].GetDisplayTextSafe());
                
                if (index == selectedItem)
                {
                    sb.Append(" < ");
                }

                sb.AppendLine();
            }

            performanceMenu.text = sb.ToString();
        }
    }

    void PushPerformanceInfo()
    {
        if (performanceInfoParent.activeSelf)
        {
            StringBuilder sb = new StringBuilder();

            {
                {
                    float dt = Time.unscaledDeltaTime;
                    int fps = (int)(1f / dt);

#if !X360
                const int TARGET_FPS = 50;
                const int BAD_FPS = 20;

                string color = "#AAFFAA";
                if (fps <= BAD_FPS)
                {
                    color = "red";
                }
                else if (fps < TARGET_FPS)
                {
                    color = "orange";
                }

                sb.Append("<color=");
                sb.Append(color);
                sb.Append(">");
#endif
                    sb.Append(fps);
                    sb.Append(" fps");

#if !X360
                sb.Append("</color>");
#endif
                    sb.AppendLine();
                }

                {
                    sb.Append("RT: ");
                    sb.Append((int)textureSizes[sizeIndex].x);
                    sb.Append(" x ");
                    sb.Append((int)textureSizes[sizeIndex].y);
                    sb.AppendLine();
                }

                {
                    for (int i = 0; i < Level.PLAYERS; i++)
                    {
                        string dump = Game.i.Level.GetPlayerDebugStateDump(i);
                        sb.AppendLine(dump);
                    }
                }
            }

            performanceInfo.text = sb.ToString();
        }
    }
}
