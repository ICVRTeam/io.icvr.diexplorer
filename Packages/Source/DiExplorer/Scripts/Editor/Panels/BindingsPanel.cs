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
using DiContainerDebugger.Editor;
using DiContainerDebugger.Scripts;
using DiExplorer.Data;
using DiExplorer.Extensions;
using DiExplorer.Services;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DiExplorer.Editor.Panels
{
    internal class BindingsPanel
    {
        private const int Unselected = -1;
        private const string NoMonoIconName = "d_ScriptableObject Icon";
        private const string DynamicMonoIconName = "d_Prefab Icon";
        private const string SceneMonoIconName = "d_TerrainInspector.TerrainToolRaise";

        private BindingData[] _bindingsData;
        private InstanceData[] _instancesData;

        private List<GUIContent> _bindings = new List<GUIContent>();
        private List<GUIContent> _realtimeInstances = new List<GUIContent>();

        private int _bindingSelectedIndex = Unselected;
        private int _prevBindingSelectedIndex = Unselected;
        private int _realtimeSelectedIndex = Unselected;
        private int _prevRealtimeSelectedIndex = Unselected;

        private string _bindingsSearchString = string.Empty;
        private string _prevBindingsSearchString = "string.Empty";
        
        private DiExplorerService _diExplorerService;
        
        public int BindingSelectedIndex => _bindingSelectedIndex;
        public int PrevBindingSelectedIndex => _prevBindingSelectedIndex;
        public int RealtimeSelectedIndex => _realtimeSelectedIndex;
        public int PrevRealtimeSelectedIndex => _prevRealtimeSelectedIndex;
        public string BindingsSearchString => _bindingsSearchString;


        public BindingsPanel(DiExplorerService diExplorerService)
        {
            _diExplorerService = diExplorerService;
        }
        
        public void UpdateData()
        {
            _bindingsData = _diExplorerService.GetAllBindings();
            _instancesData = _diExplorerService.GetAllInstances();
        }

        public void UpdateDataByContainer(string containerName)
        {
            _bindingsData = _diExplorerService.GetBindings(containerName);
            _instancesData = _diExplorerService.GetInstances(containerName);
        }

        public IEnumerable<GUIContent> CreateInstancesContent()
        {
            _realtimeInstances.Clear();
            
            foreach (var instanceData in _instancesData)
            {
                var icon = EditorGUIUtility.IconContent(DynamicMonoIconName).image;
                
                switch (instanceData.InstanceType)
                {
                    case InstanceType.NoMono:
                    {
                        icon = EditorGUIUtility.IconContent(NoMonoIconName).image;
                        break;
                    }
                    case InstanceType.DynamicMono:
                    {
                        icon = EditorGUIUtility.IconContent(DynamicMonoIconName).image;
                        break;
                    }
                    case InstanceType.SceneMono:
                    {
                        icon = EditorGUIUtility.IconContent(SceneMonoIconName).image;
                        break;
                    }
                }
                
                _realtimeInstances.Add(new GUIContent(instanceData.TypeName, icon));
            }

            return _realtimeInstances;
        }

        public IEnumerable<GUIContent> CreateBindingsContent()
        {
            _bindings.Clear();
            
            if (_bindingsSearchString != _prevBindingsSearchString)
            {
                _prevBindingsSearchString = _bindingsSearchString;
                _bindingSelectedIndex = Unselected;
            }
            
            foreach (var bindingData in _bindingsData)
            {
                var pattern = _bindingsSearchString;
                if (RegexExtension.IsContainMatch(pattern, bindingData.TypeName))
                {
                    _bindings.Add(new GUIContent(bindingData.TypeName));
                }
            }

            return _bindings;
        }
        
        public IEnumerable<GUIContent> ProcessItemSelection()
        {
            var relatedItems = new List<GUIContent>();
            
            if (_realtimeSelectedIndex != Unselected && _realtimeSelectedIndex < _realtimeInstances.Count)
            {
                var selectedInstanceTypeName = _realtimeInstances[_realtimeSelectedIndex].text;
                var injectablesName = _diExplorerService.GetInjectableNamesFromInstance(selectedInstanceTypeName);
                
                foreach (var injectable in injectablesName)
                {
                    relatedItems.Add(new GUIContent(injectable));
                }
                
                return relatedItems;
            }
            
            if (_bindingSelectedIndex != Unselected && _bindingSelectedIndex < _bindings.Count)
            {
                var selectedBindingName = _bindings[_bindingSelectedIndex].text;
                var instancesName = _diExplorerService.GetInstanceNamesFromBinding(selectedBindingName);

                foreach (var instance in instancesName)
                {
                    relatedItems.Add(new GUIContent(instance));
                }
            }

            return relatedItems;
        }

        public void SetBindingsSearchString(string value)
        {
            _bindingsSearchString = value;
        }
        
        public void SetRealtimeSelectedIndex(int index)
        {
            _realtimeSelectedIndex = index;
        }

        public void SetBindingSelectedIndex(int index)
        {
            _bindingSelectedIndex = index;
        }

        public void ResetSelectedIndexExceptInstances()
        {
            if (_realtimeSelectedIndex != _prevRealtimeSelectedIndex)
            {
                _prevRealtimeSelectedIndex = _realtimeSelectedIndex;
                _bindingSelectedIndex = Unselected;
                _prevBindingSelectedIndex = Unselected;
            }
        }
        
        public void ResetSelectedIndexExceptBindings()
        {
            if (_bindingSelectedIndex != _prevBindingSelectedIndex)
            {
                _prevBindingSelectedIndex = _bindingSelectedIndex;
                _realtimeSelectedIndex = Unselected;
                _prevRealtimeSelectedIndex = Unselected;
            }
        }

        public void ShowContextMenu(int itemIndex)
        {
            var menu = new GenericMenu();

            var iconName = _realtimeInstances[itemIndex].image.name;
            
            menu.AddItem(new GUIContent("Show in Project"), false, () => ShowInProject(itemIndex));
            
            if (iconName == DynamicMonoIconName || iconName == SceneMonoIconName)
            {
                menu.AddItem(new GUIContent("Show in Hierarchy"), false, () => ShowInHierarchy(itemIndex));
            }
            
            menu.ShowAsContext();
        }

        private void ShowInProject(int index)
        {
            Debug.Log("Project " + _realtimeInstances[index].text);
            
            var instanceTypeName = _realtimeInstances[index].text;
            var pattern = "[^.]+$";

            var match = RegexExtension.FindMatch(pattern, instanceTypeName);
            
            if (!match.Success)
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {_realtimeInstances[index].text} Not found from Project!");
                return;
            }
            
            var guids = AssetDatabase.FindAssets($"{match.Value} t:Script");

            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids.First());
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(BindingsWindow)}] {_realtimeInstances[index].text} Asset not found! ");
                }
            }
            else
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {_realtimeInstances[index].text} No objects of this type were found in the project!");
            }
        }

        private void ShowInHierarchy(int index)
        {
            var instanceTypeName = _realtimeInstances[index].text;
            var pattern = "[^.]+$";

            var match = RegexExtension.FindMatch(pattern, instanceTypeName);
            
            if (!match.Success)
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {_realtimeInstances[index].text} Not found from Project!");
                return;
            }
            
            var type = FindTypeByName(instanceTypeName);
        
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogError($"[{nameof(BindingsWindow)}] {_realtimeInstances[index].text} Type not found or not a Component!");
                return;
            }
            
            var components = Object.FindObjectsOfType(type) as Component[];

            if (components != null && components.Length > 0)
            {
                var gameObjects = components.Select(component => component.gameObject).ToArray();

                Selection.objects = gameObjects; // select object
            }
            else
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {_realtimeInstances[index].text} Object not found in the scene!");
            }
        }
        
        private static Type FindTypeByName(string typeName)
        {
            // Find type in the uploaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                
                if (type != null)
                {
                    return type;
                }
            }
            
            return null;
        }
    }
}