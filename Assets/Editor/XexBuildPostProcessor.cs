using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections;

public class XexBuildPostProcessor {
#if UNITY_XENON
    [MenuItem("Build/XEX")]
    public static void BuildXeX()
    {
        BuildXeX(BuildOptions.None);
    }

    [MenuItem("Build/XEX Debug")]
    public static void BuildXeXDebug()
    {
        BuildXeX(BuildOptions.AllowDebugging | BuildOptions.Development);
    }

    private static void BuildXeX(BuildOptions options)
    {
        EditorUserBuildSettings.xboxBuildSubtarget = XboxBuildSubtarget.Master;
        EditorUserBuildSettings.xboxRunMethod = XboxRunMethod.HDD;

        PlayerSettings.xboxTitleId = "33334444";
        PlayerSettings.stripEngineCode = true;

        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            "BUILD_X360",
            BuildTarget.XBOX360,
            options
        );
    }


    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.XBOX360)
        {

            /*
             
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
            }
            else
            {
                Debug.LogError("Xextool returned exit code "+ exitCode + "\n" + pStart.StandardOutput.ReadToEnd());
            }
            */

            // Upload
            try
            {

                var files = Directory.GetFiles(pathToBuiltProject, "*.*", SearchOption.AllDirectories);

                int i = 0;

                {
                    var credentials = new System.Net.NetworkCredential("xboxftp", ".");
                    foreach (string file in files)
                    {
                        i++;

                        if (file.StartsWith("."))
                        {
                            continue;
                        }

                        if (file.Contains("XenonPlayer"))
                        {
                            continue;
                        }

                        const string xboxPath = "Usb0/X360/xplane";
                        string relativePath = file.Substring(pathToBuiltProject.Length+1);

                        string targetPath = "ftp://169.254.8.8/" + xboxPath + "/" + relativePath.Replace("\\", "/");

                        UnityEditor.EditorUtility.DisplayProgressBar("Uploading " + relativePath + " ("+ i + "/" + files.Length + ")", targetPath, (i/(float)files.Length));

                        var request = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(targetPath);
                        request.Credentials = credentials;

                        request.KeepAlive = true;
                        request.UseBinary = true;
                        request.UsePassive = true;

                        System.Net.WebResponse response;

                        System.DateTime transferStarted = System.DateTime.Now;

                        if (Directory.Exists(file))
                        {
                            request.Method = System.Net.WebRequestMethods.Ftp.MakeDirectory;
                            response = request.GetResponse(); 
                        }
                        else
                        {
                            request.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
                            byte[] data = File.ReadAllBytes(file);
                            {
                                using (Stream writeStream = request.GetRequestStream())
                                {
                                    writeStream.Write(data, 0, data.Length);
                                }

                                response = request.GetResponse();
                            }
                        }

                        var ftpResponse = (System.Net.FtpWebResponse) response;

                        if (ftpResponse.StatusCode == System.Net.FtpStatusCode.CommandOK 
                            || ftpResponse.StatusCode == System.Net.FtpStatusCode.FileActionOK
                            || ftpResponse.StatusCode == System.Net.FtpStatusCode.ClosingControl
                            )
                        {
                            // All good

                            System.DateTime transferFinished = System.DateTime.Now;

                            Debug.Log("Uploaded "+targetPath+" in "+(transferFinished-transferStarted).TotalSeconds+" seconds");

                        }
                        else
                        {
                            Debug.LogError(ftpResponse.StatusCode);
                            Debug.LogError(ftpResponse.StatusDescription);
                        }
                    }
                }

            }
            catch (System.Exception e)
            {
                Debug.Log(e);

                System.Diagnostics.Process.Start("explorer", pathToBuiltProject);
            }

        }
    }
#endif
}
