using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;

public class Performance : MonoBehaviour
{

    [SerializeField]
    private GameObject performanceInfoParent;

    [SerializeField]
    private UnityEngine.UI.Text performanceInfo;

    private PlayerInput input;

    private bool ready = true;

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

    private int sizeIndex = 0;

    private void Start()
    {
        input = PlayerInput.MakeForPlatform();
        input.SetPlayerIndex(sizeIndex);

        performanceInfoParent.gameObject.SetActive(Game.i.ShowPerformanceInfo);
    }

    private void Update()
    {
        PushPerformanceInfo();
        input.Refresh();

        Vector2 dpad = input.GetDPad();

        if (dpad.x > 0.5f)
        {
            Toggle<UnityStandardAssets.ImageEffects.MotionBlur>();
            ready = false;
        }
        else if (dpad.x < -0.5f)
        {
            Toggle<UnityStandardAssets.ImageEffects.EdgeDetection>();
            ready = false;
        }
        else if (dpad.y > 0.5f)
        {
            ResizeRenderTextures();
            ready = false;
        }
        else if (dpad.y < -0.5f)
        {
            performanceInfoParent.SetActive(!performanceInfoParent.gameObject.activeSelf);
            ready = false;
        }
        else
        {
            ready = true;
        }
    }

    void ResizeRenderTextures()
    {
        if (!performanceInfoParent.gameObject.activeSelf)
        {
            return;
        }

        if (!ready)
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

        sizeIndex = (sizeIndex + 1) % textureSizes.Length;
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

    void Toggle<T>() where T : Behaviour
    {
        if (!performanceInfoParent.gameObject.activeSelf)
        {
            return;
        }

        if (!ready)
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
