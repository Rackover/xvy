using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections;


public class ElfBuildPostProcessor
{
#if UNITY_PS3
    [MenuItem("Build/ELF")]
    public static void BuildElf()
    {
        BuildElf(BuildOptions.None);
    }

    [MenuItem("Build/ELF Debug")]
    public static void BuildElfDebug()
    {
        BuildElf(BuildOptions.Development | BuildOptions.AllowDebugging);
    }

    private static void BuildElf(BuildOptions options)
    {
        PlayerSettings.stripEngineCode = true;
        EditorUserBuildSettings.sceBuildSubtarget = SCEBuildSubtarget.PCHosted;

        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
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
            // Upload
            try
            {

                var files = Directory.GetFiles(pathToBuiltProject, "*.*", SearchOption.AllDirectories);

                int i = 0;

                {
                    var credentials = new System.Net.NetworkCredential();
                    foreach (string file in files)
                    {
                        i++;

                        if (file.StartsWith("."))
                        {
                            continue;
                        }

                        if (file.Contains("MapFiles"))
                        {
                            continue;
                        }

                        const string ps3Path = "dev_hdd0/GAMES/BUILD_PS3";
                        string relativePath = file.Substring(pathToBuiltProject.Length + 1);

                        string targetPath = "ftp://169.254.3.0/" + ps3Path + "/" + relativePath.Replace("\\", "/");

                        UnityEditor.EditorUtility.DisplayProgressBar("Uploading " + relativePath + " (" + i + "/" + files.Length + ")", targetPath, (i / (float)files.Length));
                        System.Console.WriteLine("Uploading " + relativePath + " (" + i + "/" + files.Length + ")", targetPath, (i / (float)files.Length));

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

                        var ftpResponse = (System.Net.FtpWebResponse)response;

                        if (ftpResponse.StatusCode == System.Net.FtpStatusCode.CommandOK
                            || ftpResponse.StatusCode == System.Net.FtpStatusCode.FileActionOK
                            || ftpResponse.StatusCode == System.Net.FtpStatusCode.ClosingControl
                            )
                        {
                            // All good

                            System.DateTime transferFinished = System.DateTime.Now;

                            Debug.Log("Uploaded " + targetPath + " in " + (transferFinished - transferStarted).TotalSeconds + " seconds");

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
