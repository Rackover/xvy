using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections;

public class XexBuildPostProcessor {

    [MenuItem("Build/XEX")]
    public static void BuildXeX()
    {
        EditorUserBuildSettings.xboxBuildSubtarget = XboxBuildSubtarget.Master;
        EditorUserBuildSettings.xboxRunMethod = XboxRunMethod.HDD;

        PlayerSettings.xboxTitleId = "33334444";
        PlayerSettings.stripEngineCode = true;

        BuildPipeline.BuildPlayer(
            new string[] { "Assets/Scene/RUN.unity" }, 
            "BUILD", 
            BuildTarget.XBOX360,
            BuildOptions.None
        );
    }

    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.XBOX360)
        {
            string assets = Application.dataPath;

            string projectPath = Path.GetDirectoryName(assets);

            string xexTool = Path.Combine(projectPath, "xextool.exe");

            string xexPath = Path.Combine(pathToBuiltProject, "default.xex");
            File.Copy(xexPath, Path.Combine(pathToBuiltProject, "backup.xex"));

            var pInfo = new System.Diagnostics.ProcessStartInfo(xexTool, "-m r "+xexPath);
            pInfo.RedirectStandardOutput = true;
            pInfo.UseShellExecute = false;

            var pStart = System.Diagnostics.Process.Start(pInfo);
            pStart.WaitForExit();

            int exitCode = pStart.ExitCode;


            if (exitCode == 0)
            {
                Debug.Log("Xextool returned exit code " + exitCode);
                System.Diagnostics.Process.Start("explorer", pathToBuiltProject);
            }
            else
            {
                Debug.LogError("Xextool returned exit code "+ exitCode + "\n" + pStart.StandardOutput.ReadToEnd());
            }
        }
    }
}
