// ICVR CONFIDENTIAL
// __________________
// 
// [2016] -  [2024] ICVR LLC
// All Rights Reserved.
// 
// NOTICE:  All information contained herein is, and remains
// the property of ICVR LLC and its suppliers,
// if any.  The intellectual and technical concepts contained
// here in are proprietary to ICVR LLC and its suppliers and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from ICVR LLC.

using DiContainerDebugger.Bootstrap;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace DiContainerDebugger.Editor
{
    internal abstract class ZenjectEditorWindow : EditorWindow
    {
        protected abstract void InstallBindings(DiContainer container);

        private string _prefabPath;
        private DiContainer _container;

        protected virtual void OnEnable()
        {
            if (EditorApplication.isPlaying)
            {
                var prefab = FindProjectContextPrefab();
                if (prefab.GetComponent<DiExplorerMonoInstaller>() == null)
                {
                    Debug.LogWarning
                    (
                        $"[{nameof(ZenjectEditorWindow)}] " +
                        $"Turn off the Play mode and restart DiExplorer window. Then turn on play mode"
                    );
                }
                
                InjectFromProjectContext();
            }
            else
            {
                SetupZenject();
                InstallToProjectPrefab();
            }
        }

        protected void InjectFromProjectContext()
        {
            _container = ProjectContext.Instance.Container;
            _container.Inject(this);
        }

        private void SetupZenject()
        {
            _container = new DiContainer();
            InstallBindings(_container);
            _container.Inject(this);
        }

        private void InstallToProjectPrefab()
        {
            var prefab = FindProjectContextPrefab();
            
            if (prefab != null)
            {
                var tempInstance = Instantiate(prefab);
                
                if (tempInstance.GetComponent<DiExplorerMonoInstaller>() == null)
                {
                    AddUpComponent<DiExplorerMonoInstaller>(tempInstance);
                    PrefabUtility.SaveAsPrefabAsset(tempInstance, _prefabPath);

                    Debug.Log
                    (
                        $"[{nameof(ZenjectEditorWindow)}] " +
                        $"Component added and prefab saved successfully."
                    );
                }
                else
                {
                    Debug.Log
                    (
                        $"[{nameof(ZenjectEditorWindow)}] " +
                        $"The prefab already contains the {nameof(DiExplorerMonoInstaller)} component."
                    );
                }
                
                DestroyImmediate(tempInstance);
            }
            else
            {
                Debug.LogWarning
                (
                    $"[{nameof(ZenjectEditorWindow)}] " +
                    $"Prefab with ProjectContext not found from the Assets/Resources/"
                );
            }
        }
        
        public GameObject FindProjectContextPrefab()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" });

            foreach (string guid in prefabGuids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab != null)
                {
                    var component = prefab.GetComponent<ProjectContext>();
                    
                    if (component != null)
                    {
                        _prefabPath = prefabPath;
                        
                        Debug.Log($"[{nameof(ZenjectEditorWindow)}] Prefab with ProjectContext found: {prefabPath}");
                        
                        return prefab;
                    }
                }
            }
            
            Debug.LogWarning
            (
                $"[{nameof(ZenjectEditorWindow)}] " +
                $"Prefab with ProjectContext not found from the Assets/Resources/"
            );
            
            return null;
        }
        
        private void AddUpComponent<T>(GameObject gameObject) where T : Component
        {
            var newComponent = gameObject.AddComponent<T>();
            var components = gameObject.GetComponents<Component>();
            var newComponentIndex = System.Array.IndexOf(components, newComponent);
            
            if (newComponentIndex > 0)
            {
                for (int i = newComponentIndex; i > 2; i--)
                {
                    UnityEditorInternal.ComponentUtility.MoveComponentUp(newComponent);
                }
            }
        }

    }
}