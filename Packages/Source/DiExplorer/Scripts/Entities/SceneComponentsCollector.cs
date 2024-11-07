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

using System.Collections.Generic;
using System.Linq;
using DiExplorer.Interfaces;
using DiExplorer.Scripts.Data;
using DiExplorer.Storages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiExplorer.Entities
{
    internal class SceneComponentsCollector
    {
        private struct SceneComponentsData
        {
            public readonly Dictionary<string, List<ComponentData>> ComponentsInBuildScenes;

            public SceneComponentsData(Dictionary<string, List<ComponentData>> componentsInBuildScenes)
            {
                ComponentsInBuildScenes = componentsInBuildScenes;
            }
        }
        
        private const string SceneComponentsFileName = "SceneComponentsSaves.json";

        private readonly FileDataManager _fileDataManager;
        private readonly ISerializator _serializator;

        public Dictionary<string, List<ComponentData>> СomponentsInBuildScenes { get; private set; } =
            new Dictionary<string, List<ComponentData>>();

        public SceneComponentsCollector(FileDataManager fileDataManager, ISerializator serializator)
        {
            _fileDataManager = fileDataManager;
            _serializator = serializator;
        }

        public void CollectAllSceneComponents()
        {
            СomponentsInBuildScenes.Clear();

            var scenes = EditorBuildSettings.scenes;

            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    var scenePath = scene.path;
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

                    if (sceneAsset != null)
                    {
                        var openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                        var rootObjects = openedScene.GetRootGameObjects();

                        foreach (var rootObj in rootObjects)
                        {
                            AddGameObjectNames(openedScene.name, rootObj);
                        }

                        EditorSceneManager.CloseScene(openedScene, true);
                    }
                }
            }

            SaveData();
        }

        public void LoadSceneComponents()
        {
            var stringData = _fileDataManager.Load(SceneComponentsFileName);
            var savedData = _serializator.Deserialize<SceneComponentsData>(stringData);

            if (savedData.ComponentsInBuildScenes != null)
            {
                СomponentsInBuildScenes = savedData.ComponentsInBuildScenes;
            }
        }

        private void SaveData()
        {
            var savedData = new SceneComponentsData(СomponentsInBuildScenes);
            var savedString = _serializator.Serialize(savedData);
            _fileDataManager.Save(SceneComponentsFileName, savedString);
        }

        private void AddGameObjectNames(string sceneName, GameObject obj)
        {
            var gameObjectHash = obj.GetHashCode();
            var componentTypeNames = GetComponentTypeNames(obj);

            AddComponents(sceneName, componentTypeNames, gameObjectHash);

            foreach (Transform child in obj.transform)
            {
                AddGameObjectNames(sceneName, child.gameObject);
            }
        }

        private string[] GetComponentTypeNames(GameObject gameObject)
        {
            var componentTypeNames = new List<string>();
            var allComponents = gameObject.GetComponents<Component>();

            foreach (var component in allComponents)
            {
                if (component == null) continue;

                var type = component.GetType();
                componentTypeNames.Add(type.ToString());
            }

            return componentTypeNames.ToArray();
        }

        public void AddComponents(string sceneName, string[] componentTypeNames, int gameObjectHash)
        {
            if (СomponentsInBuildScenes.TryGetValue(sceneName, out _))
            {
                foreach (var typeName in componentTypeNames)
                {
                    var sceneComponentData = СomponentsInBuildScenes[sceneName]
                        .FirstOrDefault(data => data.InstanceTypeName == typeName);

                    if (sceneComponentData.InstanceTypeName == null)
                    {
                        var newComponentData = new ComponentData(sceneName, typeName, new[] { gameObjectHash });

                        СomponentsInBuildScenes[sceneName].Add(newComponentData);
                    }
                    else
                    {
                        var existingComponentData = СomponentsInBuildScenes[sceneName]
                            .First(data => data.InstanceTypeName == typeName);

                        var updatedGameObjectHashes = new List<int>(existingComponentData.GameObjectHashes)
                            { gameObjectHash }.ToArray();

                        var updatedComponentData = new ComponentData(sceneName, typeName, updatedGameObjectHashes);
                        var index = СomponentsInBuildScenes[sceneName]
                            .FindIndex(data => data.InstanceTypeName == typeName);

                        СomponentsInBuildScenes[sceneName][index] = updatedComponentData;
                    }
                }
            }
            else
            {
                var newComponentsDataList = new List<ComponentData>();

                foreach (var typeName in componentTypeNames)
                {
                    var newComponentData = new ComponentData(sceneName, typeName, new[] { gameObjectHash });

                    newComponentsDataList.Add(newComponentData);
                }

                СomponentsInBuildScenes.Add(sceneName, newComponentsDataList);
            }
        }
    }
}