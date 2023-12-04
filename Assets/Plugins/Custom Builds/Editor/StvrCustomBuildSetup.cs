using System.Collections.Generic;
using UnityEditor;

namespace STVR.CustomBuild.Editors
{
    public class StvrCustomBuildSetup
    {
        CustomBuildSettings buildSetups;
        public StvrCustomBuildSetup()
        {
#if UNITY_EDITOR
            string[] guid = AssetDatabase.FindAssets("t:CustomBuildSettings");
            for (int i = 0; i < guid.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid[i]);
                buildSetups = AssetDatabase.LoadAssetAtPath<CustomBuildSettings>(path);
            }
#endif
        }

        public void BuildWithoutCertainFolder()
        {
#if UNITY_EDITOR
            if (buildSetups != null)
            {
                buildSetups.SaveToJson();
            }
#endif
        }


#if UNITY_EDITOR
        private void BuildGame()
        {
            //// Get filename.
            //string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

            //List<EditorBuildSettingsScene> sceneToBuild = new List<EditorBuildSettingsScene>();

            //for (int i = 0; i < buildSetups.Scenes.Length; i++)
            //{
            //    string scenePath = AssetDatabase.GetAssetPath(buildSetups.Scenes[i]);
            //    if (!string.IsNullOrEmpty(scenePath))
            //        sceneToBuild.Add(new EditorBuildSettingsScene(scenePath, true));
            //}

            //string[] levels = new string[sceneToBuild.Count];

            //for (int i = 0; i < sceneToBuild.Count; i++)
            //{
            //    levels[i] = sceneToBuild[i].path;
            //}

            //// Build player.
            //BuildPipeline.BuildPlayer(levels, path + $"/{buildSetups.GameTitle}.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        }
#endif
    }
}