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

using DiExplorer.Interfaces;
using DiExplorer.Storages;
using UnityEditor;
using UnityEngine;

namespace DiExplorer.Entities
{
    [InitializeOnLoad]
    public class EditorStartupHandler
    {
        private static SceneComponentsCollector _sceneComponentsCollector;
        private static FileDataManager _fileDataManager;
        private static ISerializator _serializator;

        static EditorStartupHandler()
        {
            _fileDataManager = new FileDataManager();
            _serializator = new JsonSerializator();
            _sceneComponentsCollector = new SceneComponentsCollector(_fileDataManager, _serializator);
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                {
                    _sceneComponentsCollector.CollectAllSceneComponents();
                    
                    Debug.Log("Collect of components on the scenes is completed!");
                    
                    break;
                }
            }
        }

        ~EditorStartupHandler()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }
}