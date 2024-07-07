using UnityEditor;
using UnityEditor.Callbacks;

public class PCBuildPostProcessor
{
    [MenuItem("Build/PC")]
    public static void BuildPC()
    {
        BuildPC(BuildOptions.None);
    }

    [MenuItem("Build/PC Debug")]
    public static void BuildPCDebug()
    {
        BuildPC(BuildOptions.AllowDebugging | BuildOptions.Development);
    }

    private static void BuildPC(BuildOptions options)
    {
        PlayerSettings.stripEngineCode = true;
        EditorUserBuildSettings.sceBuildSubtarget = SCEBuildSubtarget.HddTitle;

        BuildPipeline.BuildPlayer(
            new string[] { "Assets/Scene/RUN.unity" },
            "BUILD_PC",
            BuildTarget.StandaloneWindows64,
            options
        );
    }


    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.StandaloneWindows64)
        {
            System.Diagnostics.Process.Start("explorer", pathToBuiltProject);
        }
    }
}
