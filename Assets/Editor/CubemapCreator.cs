using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class CubemapCreator {

#if UNITY_EDITOR
    [MenuItem("Tools/Make cubemap")]
    public static void MakeCubemap()
    {
        GameObject obj = new GameObject("cubecam");

        Dictionary<string, Vector3> shotDirections = new Dictionary<string, Vector3>()
        {
            { "Top", Vector3.up },
            {"Bottom", Vector3.down },
            {"Left", Vector3.left },
            {"Right", Vector3.right },
            {"Forward", Vector3.forward },
            {"Backward", Vector3.back },
        };

        Camera camera = obj.AddComponent<Camera>();
        camera.cullingMask = 0;
        camera.orthographic = false;
        camera.fieldOfView = 90f;

        foreach(var kv in shotDirections)
        {
            camera.transform.forward = kv.Value;

            const int resWidth = 256;
            const int resHeight = 256;

            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            screenShot.filterMode = FilterMode.Bilinear;
            screenShot.wrapMode = TextureWrapMode.Clamp;

            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            string dir = Application.dataPath + "/CubemapRenders/LAST/";

            Directory.CreateDirectory(dir);

            string filename = dir + kv.Key + ".PNG"; 
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
        }


        GameObject.DestroyImmediate(obj);
    }

#endif
}
