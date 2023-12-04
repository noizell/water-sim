using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using static STVR.JSON.JsonHelper;

namespace STVR.CustomBuild.Editors
{

    public class CustomBuildSys
    {
        public const string PROJECT_DIR = "Assets";
        public const string DEFAULT_JSON_ASSET_DIR = "Assets/StreamingAssets/ReqFiles";
        public const string DEFAULT_JSON_ASSET_DIR_RUNTIME = "ReqFiles";
        public const string ASSET_BUNDLE_DEFAULT_ASSET_LOADER = "default.json";
        public const string DEFAULT_BUILD_SETUP_JSON = "buildSetup";
    }

    [CreateAssetMenu(fileName = "Custom Build Settings", menuName = "STVR/Editor/Create Custom Build Settings")]
    public class CustomBuildSettings : ScriptableObject
    {
        public struct CustomBuildJson : IJsonData
        {
            public string[] ExcludedFolder;

            public CustomBuildJson(string[] excludedFolder)
            {
                ExcludedFolder = excludedFolder;
            }
        }

        //[Header("Scene Setup")]
        //public SceneAsset[] Scenes;

        [Header("Build Setup")]
        //public string GameTitle;
        public string[] ExcludedFolder;

        static object moveLocker = new object();
        static object unloadLocker = new object();

        public void SaveToJson()
        {
            CustomBuildJson json = new CustomBuildJson(ExcludedFolder);
            WriteToJson(json, CustomBuildSys.DEFAULT_BUILD_SETUP_JSON, CustomBuildSys.DEFAULT_JSON_ASSET_DIR);
        }

        public CustomBuildJson LoadJson()
        {
            var json = ReadJson<CustomBuildJson>(Path.Combine(CustomBuildSys.DEFAULT_JSON_ASSET_DIR, $"{CustomBuildSys.DEFAULT_BUILD_SETUP_JSON}.json"));

            return json;
        }

        public void ProceedExcludedFolder()
        {
#if UNITY_EDITOR

            var jsonfile = LoadJson();

            //if (jsonfile.ExcludedFolder == null || jsonfile.ExcludedFolder.Length == 0) return;

            System.Threading.Thread.Sleep(2000);

            for (int i = 0; i < jsonfile.ExcludedFolder.Length; i++)
            {
                //var dirs = Directory.GetDirectories(CustomBuildSys.PROJECT_DIR, jsonfile.ExcludedFolder[i], SearchOption.AllDirectories);
                if (Directory.Exists(jsonfile.ExcludedFolder[i]))
                {
                    //check if empty, if it empty skip to next dir.
                    if (!Directory.EnumerateFileSystemEntries(jsonfile.ExcludedFolder[i]).Any()) continue;

                    string modifiedDirs = $"{jsonfile.ExcludedFolder[i]}~";

                    if (Directory.Exists(jsonfile.ExcludedFolder[i]))
                    {
                        try
                        {
                            AssetDatabase.MoveAsset(jsonfile.ExcludedFolder[i], modifiedDirs);
                            AssetDatabase.Refresh();
                            Debug.Log($"move folder {jsonfile.ExcludedFolder[i]} to temporary location.");
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("cannot move file with error : " + e.Message);
                        }

                    }
                }
                //foreach (var dir in dirs)
                //{
                //    //check if empty, if it empty skip to next dir.
                //    if (!Directory.EnumerateFileSystemEntries(dir).Any()) continue;

                //    string modifiedDirs = $"{dir}~";

                //    if (Directory.Exists(dir))
                //    {
                //        try
                //        {
                //            AssetDatabase.MoveAsset(dir, modifiedDirs);
                //            AssetDatabase.Refresh();
                //        }
                //        catch (System.Exception e)
                //        {
                //            Debug.Log("cannot move file with error : " + e.Message);
                //        }

                //    }
                //}
            }
#endif
        }
        public void RestoreExcludedFolder()
        {
#if UNITY_EDITOR

            var jsonfile = LoadJson();

            //if (jsonfile.ExcludedFolder == null || jsonfile.ExcludedFolder.Length == 0) return;

            System.Threading.Thread.Sleep(2000);

            string[] modifiedFolder = new string[jsonfile.ExcludedFolder.Length];
            for (int i = 0; i < modifiedFolder.Length; i++)
            {
                modifiedFolder[i] = $"{jsonfile.ExcludedFolder[i]}~";
            }

            for (int i = 0; i < modifiedFolder.Length; i++)
            {
                //var dirs = Directory.GetDirectories(CustomBuildSys.PROJECT_DIR, modifiedFolder[i], SearchOption.AllDirectories);
                if (Directory.Exists(jsonfile.ExcludedFolder[i]))
                {
                    string modifiedDirs = jsonfile.ExcludedFolder[i][..^1];
                    if (Directory.Exists(jsonfile.ExcludedFolder[i]))
                    {
                        try
                        {
                            AssetDatabase.MoveAsset(jsonfile.ExcludedFolder[i], modifiedDirs);
                            AssetDatabase.Refresh();

                            Debug.Log($"move folder {jsonfile.ExcludedFolder[i]} to original location.");
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("cannot move file with error : " + e.Message);
                        }

                    }
                }

                //    foreach (var dir in dirs)
                //{
                //    string modifiedDirs = dir[..^1];
                //    if (Directory.Exists(dir))
                //    {
                //        try
                //        {
                //            AssetDatabase.MoveAsset(dir, modifiedDirs);
                //            AssetDatabase.Refresh();
                //        }
                //        catch (System.Exception e)
                //        {
                //            Debug.Log("cannot move file with error : " + e.Message);
                //        }

                //    }
                //}
            }
#endif
        }
    }
}