﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections;


public class ElfBuildPostProcessor
{
    [MenuItem("Build/ELF")]
    public static void BuildElf()
    {
        BuildElf(BuildOptions.None);
    }

    [MenuItem("Build/ELF Debug")]
    public static void BuildElfDebug()
    {
        BuildElf(BuildOptions.AllowDebugging | BuildOptions.Development);
    }

    private static void BuildElf(BuildOptions options)
    {
        PlayerSettings.stripEngineCode = true;
        EditorUserBuildSettings.sceBuildSubtarget = SCEBuildSubtarget.HddTitle;
        
        BuildPipeline.BuildPlayer(
            new string[] { "Assets/Scene/RUN.unity" },
            "BUILD_PS3",
            BuildTarget.PS3,
            options
        );
    }


    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.PS3)
        {
            System.Diagnostics.Process.Start("explorer", pathToBuiltProject);
        }
    }
}
