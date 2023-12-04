#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;

namespace STVR.CustomBuild.Editors
{

    [CustomEditor(typeof(CustomBuildSettings))]
    public class CustomBuildSettingsEditor : Editor
    {
        SerializedProperty excludedFolderProperty;

        string defaultDirectory
        {
            get
            {
                return CustomBuildSys.PROJECT_DIR;
            }
        }

        public override void OnInspectorGUI()
        {
            FindProperty();

            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select Folder", GUILayout.Width(130)))
            {
                //var path = StandaloneFileBrowser.OpenFolderPanel("Select Excluded Folder", Application.dataPath, false);
                var path = EditorUtility.OpenFolderPanel("Select Excluded Folder", Application.dataPath, "");
                if (excludedFolderProperty.isArray)
                {
                    excludedFolderProperty.InsertArrayElementAtIndex(excludedFolderProperty.arraySize);
                    excludedFolderProperty.GetArrayElementAtIndex(excludedFolderProperty.arraySize - 1).stringValue = path;
                    serializedObject.ApplyModifiedProperties();
                }
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Remove Last Folder", GUILayout.Width(130)))
            {
                if (excludedFolderProperty.arraySize > 0)
                {
                    excludedFolderProperty.DeleteArrayElementAtIndex(excludedFolderProperty.arraySize - 1);
                    serializedObject.ApplyModifiedProperties();
                }
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Apply Setup", GUILayout.Width(130)))
            {
                StvrCustomBuildSetup cs = new StvrCustomBuildSetup();
                cs.BuildWithoutCertainFolder();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void FindProperty()
        {
            if (excludedFolderProperty == null)
            {
                excludedFolderProperty = serializedObject.FindProperty("ExcludedFolder");
            }
        }
    }
}