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
using DiContainerDebugger.Scripts;
using DiExplorer.Data;
using DiExplorer.Entities;
using DiExplorer.Scripts.Data;
using DiExplorer.Storages;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.Internal;
using Object = UnityEngine.Object;

namespace DiExplorer.Containers
{
    internal class SceneContainer : AbstractContainer
    {
        private SceneComponentsCollector _sceneComponentsCollector;
        public override string ContainerName => GetContainerName();

        public SceneContainer(
            SignalBus signalBusInstance, 
            InheritorsStorage inheritorsStorage,
            SceneComponentsCollector sceneComponentsCollector)
            : base(signalBusInstance, inheritorsStorage)
        {
            _sceneComponentsCollector = sceneComponentsCollector;
            _sceneComponentsCollector.LoadSceneComponents();
        }

        protected override string GetContainerName()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            return sceneName;
        }

        protected override Context GetContext()
        {
            var sceneContext = Object.FindObjectsOfType<SceneContext>().First();
            return sceneContext;
        }

        public override InstanceData[] GetInstances()
        {
            var instancesDataList = base.GetInstances().ToList();
            var contextName = GetContainerName();
            var activeScene = SceneManager.GetActiveScene();
            var monoBehaviourList = new List<MonoBehaviour>();
            
            ZenUtilInternal.GetInjectableMonoBehavioursInScene(activeScene, monoBehaviourList);
        
            var componentList = new List<MonoBehaviour>();

            foreach (var monoBehaviour in monoBehaviourList)
            {
                ZenUtilInternal.GetInjectableMonoBehavioursUnderGameObject(monoBehaviour.gameObject, componentList);
            }

            var uniqComponents = componentList.Distinct();

            foreach (var component in uniqComponents)
            {
                var componentType = component.GetType();
                var typeInfo = TypeAnalyzer.TryGetInfo(componentType);
                
                if (typeInfo == null)
                {
                    continue;
                }
                
                var injectables = typeInfo.AllInjectables.ToArray();

                if (injectables.Any())
                {
                    var injectableTypes = injectables.Select(info => info.MemberType.ToString()).ToArray();
                    var sceneName = GetContainerName();

                    var componentInScene = new ComponentData();

                    if (_sceneComponentsCollector.СomponentsInBuildScenes.TryGetValue(sceneName, out var components))
                    {
                        componentInScene =
                            components.FirstOrDefault(data => data.InstanceTypeName == componentType.ToString());
                    }

                    if (componentInScene.InstanceTypeName == null)
                    {
                        instancesDataList.Add(new InstanceData(contextName, componentType.ToString(), InstanceType.DynamicMono, injectableTypes));
                    }
                    else
                    {
                        instancesDataList.Add(new InstanceData(contextName, componentType.ToString(), InstanceType.SceneMono, injectableTypes));
                    }
                }
            }

            return instancesDataList.ToArray();
        }
    }
}