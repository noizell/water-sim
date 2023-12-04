#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

namespace STVR.CustomBuild.Editors
{
    public class CustomBuildNative : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        CustomBuildSettings buildSetups;

        public void OnPreprocessBuild(BuildReport report)
        {
            string[] guid = AssetDatabase.FindAssets("t:CustomBuildSettings");
            for (int i = 0; i < guid.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid[i]);
                var buildSetups = AssetDatabase.LoadAssetAtPath<CustomBuildSettings>(path);
                var b = ScriptableObject.CreateInstance<CustomBuildSettings>();
                buildSetups = b;
                this.buildSetups = buildSetups;

                this.buildSetups.ProceedExcludedFolder();

                System.Threading.Thread.Sleep(2000);
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            string[] guid = AssetDatabase.FindAssets("t:CustomBuildSettings");
            for (int i = 0; i < guid.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid[i]);
                var buildSetups = AssetDatabase.LoadAssetAtPath<CustomBuildSettings>(path);
                var b = ScriptableObject.CreateInstance<CustomBuildSettings>();
                buildSetups = b;
                this.buildSetups = buildSetups;

                this.buildSetups.RestoreExcludedFolder();
            }
        }

    }
}