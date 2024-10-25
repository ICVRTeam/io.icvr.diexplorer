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

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.Internal;

namespace DiExplorer.Entities
{
    public class StaticAnalyzer
    {
        public bool IsBoundSignalBus { get; private set; }

        public void ValidateActiveScenes()
        {
            ZenUnityEditorUtil.SaveThenRunPreserveSceneSetup(() =>
            {
                var numValidated = ValidateAllActiveScenes();
                Log.Info("Validated all '{0}' active scenes successfully", numValidated);
            });
        }
        
        private int ValidateAllActiveScenes()
        {
            var activeScenePaths = EditorBuildSettings.scenes.Where(x => x.enabled)
                .Select(x => x.path).ToList();

            foreach (var scenePath in activeScenePaths)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ValidateCurrentSceneSetup();
            }

            return activeScenePaths.Count;
        }

        private void ValidateCurrentSceneSetup()
        {
            var encounteredError = false;

            Application.LogCallback logCallback = (condition, stackTrace, type) =>
            {
                if (type == LogType.Error || type == LogType.Assert
                                          || type == LogType.Exception)
                {
                    encounteredError = true;
                    IsBoundSignalBus = !CheckSignalBusInMessage(condition);
                }
            };

            Application.logMessageReceived += logCallback;

            try
            {
                Assert.That(!ProjectContext.HasInstance);

                ProjectContext.ValidateOnNextRun = true;

                foreach (var sceneContext in GetAllSceneContexts())
                {
                    sceneContext.Validate();
                }
            }
            catch (Exception e)
            {
                Log.ErrorException(e);
                encounteredError = true;
            }
            finally
            {
                Application.logMessageReceived -= logCallback;
            }

            if (encounteredError == false)
            {
                IsBoundSignalBus = true;
            }
            else
            {
                throw new ZenjectException("Zenject Validation Failed!  See errors below for details.");
            }
            
        }

        private IEnumerable<SceneContext> GetAllSceneContexts()
        {
            var decoratedSceneNames = new List<string>();

            for (var i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);

                var sceneContext = TryGetSceneContextForScene(scene);
                var decoratorContext = TryGetDecoratorContextForScene(scene);

                if (sceneContext != null)
                {
                    Assert.That(decoratorContext == null,
                        "Found both SceneDecoratorContext and SceneContext in the same scene '{0}'.  This is not allowed",
                        scene.name);

                    decoratedSceneNames.RemoveAll(x => sceneContext.ContractNames.Contains(x));

                    yield return sceneContext;
                }
                else if (decoratorContext != null)
                {
                    Assert.That(!string.IsNullOrEmpty(decoratorContext.DecoratedContractName),
                        "Missing Decorated Contract Name on SceneDecoratorContext in scene '{0}'", scene.name);

                    decoratedSceneNames.Add(decoratorContext.DecoratedContractName);
                }
            }

            Assert.That(decoratedSceneNames.IsEmpty(),
                "Found decorator scenes without a corresponding scene to decorator.  Missing scene contracts: {0}",
                decoratedSceneNames.Join(", "));
        }

        private SceneContext TryGetSceneContextForScene(Scene scene)
        {
            if (!scene.isLoaded)
            {
                return null;
            }

            var sceneContexts = scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<SceneContext>()).ToList();

            if (sceneContexts.IsEmpty())
            {
                return null;
            }

            Assert.That(sceneContexts.Count == 1,
                $"Found multiple SceneContexts in scene {scene.name}.  Expected a maximum of one.");

            return sceneContexts[0];
        }

        private SceneDecoratorContext TryGetDecoratorContextForScene(Scene scene)
        {
            if (!scene.isLoaded)
            {
                return null;
            }

            var decoratorContexts = scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<SceneDecoratorContext>()).ToList();

            if (decoratorContexts.IsEmpty())
            {
                return null;
            }

            Assert.That(decoratorContexts.Count == 1,
                $"Found multiple DecoratorContexts in scene {scene.name}.  Expected a maximum of one.");

            return decoratorContexts[0];
        }

        private bool CheckSignalBusInMessage(string message)
        {
            var signalBusPatterns = new[] { "Detected multiple SignalBus bindings", "Unable to resolve 'SignalBus'"};

            return signalBusPatterns.Any(message.Contains);
        }
    }
}