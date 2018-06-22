using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lowscope.ScriptableObjectUpdater
{
    public class ScriptableUpdaterConfiguration : EditorWindow
    {
        private bool searchSpecificFolders;

        [System.Serializable]
        public struct SearchPaths
        {
            public int arraySize;
            public List<string> pathsToSearch;
        }

        private SearchPaths paths;

        [MenuItem("Tools/Scriptable Object Updater")]
        public static void ShowWindow()
        {
            GetWindow<ScriptableUpdaterConfiguration>(false, "SO Updater", true);
        }

        private void OnEnable()
        {
            searchSpecificFolders = EditorPrefs.GetBool("SOUpdater_SearchSpecificFolders");

            List<string> searchPathList = GetSearchPaths();
            paths = new SearchPaths()
            {
                arraySize = searchPathList.Count,
                pathsToSearch = searchPathList
            };
        }

        void OnGUI()
        {
            EditorGUILayout.TextField("Scriptable Object Updater Configuration", EditorStyles.boldLabel);

            EditorGUILayout.TextField("To optimize the startup time, you can set it to only search for Scriptable Objects in specific folders", EditorStyles.label);
            EditorGUILayout.TextField("Example of a path is: Assets/ScriptableObjects", EditorStyles.label);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();

            searchSpecificFolders = EditorGUILayout.Toggle("Search Specific Folders", searchSpecificFolders);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("SOUpdater_SearchSpecificFolders", searchSpecificFolders);
            }

            EditorGUI.BeginChangeCheck();

            if (!searchSpecificFolders)
            {
                GUI.enabled = false;
            }

            EditorGUI.BeginChangeCheck();

            paths.arraySize = EditorGUILayout.IntField("Paths", paths.arraySize);

            if (paths.pathsToSearch.Count < paths.arraySize)
            {
                paths.pathsToSearch.Add("");
            }
            else
            {
                if (paths.pathsToSearch.Count > paths.arraySize)
                {
                    string[] pathArray = paths.pathsToSearch.ToArray();
                    Array.Resize<string>(ref pathArray, paths.arraySize);
                    paths.pathsToSearch = pathArray.ToList();
                }
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < paths.arraySize; i++)
            {
                paths.pathsToSearch[i] = EditorGUILayout.TextField("Path", paths.pathsToSearch[i]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                string savePathData = JsonUtility.ToJson(paths);
                EditorPrefs.SetString("SOUpdater_PathData", savePathData);
            }

            GUI.enabled = true;

            GUILayout.EndVertical();
        }

        public static List<string> GetSearchPaths()
        {
            string getPrefs = EditorPrefs.GetString("SOUpdater_PathData");

            if (!string.IsNullOrEmpty(getPrefs))
            {
                return JsonUtility.FromJson<SearchPaths>(getPrefs).pathsToSearch;
            }
            else
            {
                return null;
            }
        }
    }
}