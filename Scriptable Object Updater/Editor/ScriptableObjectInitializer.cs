using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
using System.Reflection;
using System;
using UnityEngine.Events;
using UnityEditor.Events;

namespace Lowscope.ScriptableObjectUpdater
{
    [System.Serializable]
    public class ScriptableObjectInitializer
    {
        private static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        // This is required because using the PostProcessScene starts after Start() when using it in the editor
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnLoad()
        {
            LoadItems(true);
        }

        [PostProcessSceneAttribute(-1)]
        public static void OnPostprocessScene()
        {
            if (BuildPipeline.isBuildingPlayer)
            {
                LoadItems(false);
            }
        }

        private static void LoadItems(bool testingInEditor)
        {
            int buildIndex = EditorSceneManager.GetActiveScene().buildIndex;

            bool canCreateAssetDatabase = (testingInEditor) ? true : (buildIndex == 0);

            // We only want to create this object at the first scene.
            if (canCreateAssetDatabase)
            {
                InitializeableAssetContainer startupEventContainer = null;

                if (BuildPipeline.isBuildingPlayer)
                {
                    var scene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(scene);
                }

                GameObject assetContainer = new GameObject("Scriptable Object Initializer");

                startupEventContainer = assetContainer.AddComponent<InitializeableAssetContainer>();

                string[] scriptableObjectGuids;

                if (EditorPrefs.GetBool("SOUpdater_SearchSpecificFolders"))
                {
                    string[] lookPath = ScriptableUpdaterConfiguration.GetSearchPaths().ToArray();
                    scriptableObjectGuids = AssetDatabase.FindAssets("t:ScriptableObject", lookPath);

                    if (scriptableObjectGuids.Length == 0)
                    {
                        Debug.Log("Could not find objects. Go to Tools > Scriptable Object Updater.");
                    }
                }
                else
                {
                    scriptableObjectGuids = AssetDatabase.FindAssets("t:ScriptableObject");
                }

                for (int i = 0; i < scriptableObjectGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(scriptableObjectGuids[i]);

                    var getObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                    if (getObject == null)
                        continue;

                    MethodInfo[] scriptableObjectMethods = getObject.GetType().GetMethods(bindingFlags);

                    for (int i2 = 0; i2 < scriptableObjectMethods.Length; i2++)
                    {
                        // Check if a custom attribute is associated with a method
                        UpdateScriptableObjectAttribute attribute =
                            Attribute.GetCustomAttribute(scriptableObjectMethods[i2],
                            typeof(UpdateScriptableObjectAttribute)) as UpdateScriptableObjectAttribute;

                        // Continue searching methods for attributes if the current method has none. Or when it has more then 0 parameters.
                        if (attribute == null || scriptableObjectMethods[i2].GetParameters().Length != 0)
                        {
                            continue;
                        }

                        if (scriptableObjectMethods[i2].IsPrivate)
                        {
#if NET_4_6
                        Debug.Log($"Can only use UpdateScriptableObject on public methods. {getObject.name} has it set to private");
#else
                            Debug.Log(String.Format("Can only use InitializeRuntimeAssetMethod on public methods. {0} has it set to private", getObject.name));
#endif
                            continue;
                        }


#if NET_4_6
                        UnityAction action = scriptableObjectMethods[i2].CreateDelegate(typeof(UnityAction), getObject) as UnityAction;
#else
                        UnityAction action = Delegate.CreateDelegate(typeof(UnityAction), getObject, scriptableObjectMethods[i2].Name) as UnityAction;
#endif

                        // Create a unityaction from the delegate, so that it can be added to a serialized game object


                        AttachEventToContainer(startupEventContainer, attribute, action);
                    }
                }

                // When the game is being built, set the objects that have changed serialized data dirty.
                // This ensures the changes made persist.
                if (BuildPipeline.isBuildingPlayer)
                {
                    EditorUtility.SetDirty(assetContainer);
                    EditorUtility.SetDirty(startupEventContainer);
                }
                else
                {
                    startupEventContainer.InitializeEvents(true);
                }

            }
        }

        private static void AttachEventToContainer(InitializeableAssetContainer initAssetContainer, UpdateScriptableObjectAttribute attribute, UnityAction action)
        {
            InitializeableAssetContainer.EventData eventData = new InitializeableAssetContainer.EventData()
            {
                eventType = attribute.eventType,
                serializedEvent = new UnityEvent(),
                updateRate = attribute.tickDelay,
                delay = attribute.Delay,
                executionOrder = attribute.ExecutionOrder
            };

            UnityEventTools.AddPersistentListener(eventData.serializedEvent, action);

            initAssetContainer.AddEvent(eventData);
        }
    }

}
