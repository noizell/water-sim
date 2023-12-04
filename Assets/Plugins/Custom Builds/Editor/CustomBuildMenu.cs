using UnityEditor;
using UnityEngine;
using static STVR.JSON.JsonHelper;

namespace STVR.CustomBuild.Editors
{
    public class CustomBuildMenu : MonoBehaviour
    {
        //[MenuItem("SUKG/Release/Build/Generate Custom Build Setup")]
        private static void BuildWithSystem()
        {
            StvrCustomBuildSetup cs = new StvrCustomBuildSetup();
            cs.BuildWithoutCertainFolder();
        }

        [MenuItem("SUKG/Debug/Highlight Config Folder")]
        private static void HighlightBuildConfig()
        {
            string path = "Assets/Resources/Configs";
            SelectAndHighlightFile(path);
        }

        private static void SelectAndHighlightFile(string path)
        {
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}