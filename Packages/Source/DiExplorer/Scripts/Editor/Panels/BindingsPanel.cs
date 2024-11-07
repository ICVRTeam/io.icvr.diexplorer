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
        public enum SearchGroup
        {
            Bindings = 0,
            Instances = 1
        }
        
        private const int Unselected = -1;
        private const int CharacterLength = 6;
        private const string NoMonoIconName = "d_ScriptableObject Icon";
        private const string DynamicMonoIconName = "d_Prefab Icon";
        private const string SceneMonoIconName = "d_TerrainInspector.TerrainToolRaise";
        private const string GameObjectIconName = "GameObject Icon";
        private const string BoxColliderIconName = "BoxCollider Icon";
        private const string AnimatorControllerIconName = "AnimatorController Icon";

        private BindingData[] _bindingsData;
        private InstanceData[] _instancesData;

        private List<GUIContent> _bindings = new List<GUIContent>();
        private List<GUIContent> _realtimeInstances = new List<GUIContent>();

        private SearchGroup _searchGroup;
        private string _prevSearchString = "string.Empty";
        
        private DiExplorerService _diExplorerService;
        
        public int BindingSelectedIndex { get; private set; } = Unselected;
        public int PrevBindingSelectedIndex { get; private set; } = Unselected;
        public int RealtimeSelectedIndex { get; private set; } = Unselected;
        public int PrevRealtimeSelectedIndex { get; private set; } = Unselected;
        public int RelatedItemSelectedIndex { get; private set; } = Unselected;
        public string SearchString { get; private set; } = string.Empty;
        public string SelectedItemForRelated { get; private set; }


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

        public IEnumerable<GUIContent> CreateInstancesContent(Rect position)
        {
            _realtimeInstances.Clear();
            
            if (SearchString != _prevSearchString)
            {
                _prevSearchString = SearchString;
                BindingSelectedIndex = Unselected;
            }
            
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
                
                var maxTextLength = (int)Mathf.Round(position.width / 2f - 160f);
                var instanceTypeName = CutString(instanceData.TypeName, maxTextLength);
                
                var pattern = SearchString;
                
                if (_searchGroup == SearchGroup.Instances)
                {
                    if (RegexExtension.IsContainMatch(pattern, instanceData.TypeName))
                    {
                        _realtimeInstances.Add(new GUIContent(instanceTypeName, icon, instanceData.TypeName));
                    }
                }
                else
                {
                    _realtimeInstances.Add(new GUIContent(instanceTypeName, icon, instanceData.TypeName));
                }
            }

            return _realtimeInstances;
        }

        public IEnumerable<GUIContent> CreateBindingsContent(Rect position)
        {
            _bindings.Clear();
            
            if (SearchString != _prevSearchString)
            {
                _prevSearchString = SearchString;
                BindingSelectedIndex = Unselected;
            }
            
            foreach (var bindingData in _bindingsData)
            {
                var maxTextLength = (int)Mathf.Round(position.width / 2f - 220f);
                var bindingTypeName = CutString(bindingData.TypeName, maxTextLength);
                var pattern = SearchString;
                
                if (_searchGroup == SearchGroup.Bindings)
                {
                    if (RegexExtension.IsContainMatch(pattern, bindingData.TypeName))
                    {
                        _bindings.Add(new GUIContent(bindingTypeName, bindingData.TypeName));
                    }
                }
                else
                {
                    _bindings.Add(new GUIContent(bindingTypeName, bindingData.TypeName));
                }
            }

            return _bindings;
        }
        
        public IEnumerable<GUIContent> ProcessItemSelection(Rect position)
        {
            var relatedItems = new List<GUIContent>();
            
            if (RealtimeSelectedIndex != Unselected && RealtimeSelectedIndex < _realtimeInstances.Count)
            {
                var selectedInstanceTypeName = _realtimeInstances[RealtimeSelectedIndex].tooltip;
                var injectablesName = _diExplorerService.GetInjectableNamesFromInstance(selectedInstanceTypeName);
                var icon = EditorGUIUtility.IconContent(BoxColliderIconName).image;
                
                SelectedItemForRelated = selectedInstanceTypeName;
                
                foreach (var injectable in injectablesName)
                {
                    var maxTextLength = (int)Mathf.Round(position.width - 240f);
                    var injectableName = CutString(injectable, maxTextLength);
                    
                    relatedItems.Add(new GUIContent(injectableName, icon, injectable));
                }
                
                AddInheritorsToContent(selectedInstanceTypeName, relatedItems, position);

                return relatedItems;
            }
            
            if (BindingSelectedIndex != Unselected && BindingSelectedIndex < _bindings.Count)
            {
                var selectedBindingName = _bindings[BindingSelectedIndex].tooltip;
                var instancesName = _diExplorerService.GetInstanceNamesFromBinding(selectedBindingName);
                var icon = EditorGUIUtility.IconContent(GameObjectIconName).image;

                SelectedItemForRelated = selectedBindingName;

                foreach (var instance in instancesName)
                {
                    var maxTextLength = (int)Mathf.Round(position.width - 240f);
                    var instanceName = CutString(instance, maxTextLength);
                    
                    relatedItems.Add(new GUIContent(instanceName, icon, instance));
                }
                
                AddInheritorsToContent(selectedBindingName, relatedItems, position);
            }

            return relatedItems;
        }

        public void SetSearchString(string value, SearchGroup searchGroup)
        {
            SearchString = value;
            _searchGroup = searchGroup;
        }

        public void SetRealtimeSelectedIndex(int index)
        {
            RealtimeSelectedIndex = index;
        }

        public void SetBindingSelectedIndex(int index)
        {
            BindingSelectedIndex = index;
        }

        public void SetRelatedItemSelectedIndex(int index)
        {
            RelatedItemSelectedIndex = index;
        }

        public void ResetSelectedIndexExceptInstances()
        {
            if (RealtimeSelectedIndex != PrevRealtimeSelectedIndex)
            {
                PrevRealtimeSelectedIndex = RealtimeSelectedIndex;
                BindingSelectedIndex = Unselected;
                PrevBindingSelectedIndex = Unselected;
                ResetRelatedSelectedIndex();
            }
        }

        public void ResetSelectedIndexExceptBindings()
        {
            if (BindingSelectedIndex != PrevBindingSelectedIndex)
            {
                PrevBindingSelectedIndex = BindingSelectedIndex;
                RealtimeSelectedIndex = Unselected;
                PrevRealtimeSelectedIndex = Unselected;
                ResetRelatedSelectedIndex();
            }
        }

        public void ResetRelatedSelectedIndex()
        {
            RelatedItemSelectedIndex = Unselected;
        }

        public void ShowInstanceContextMenu(IReadOnlyList<GUIContent> content, int itemIndex)
        {
            var menu = new GenericMenu();

            var iconName = content[itemIndex].image.name;
            
            menu.AddItem(new GUIContent("Show in Project"), false, () => ShowInProject(content, itemIndex));
            
            if (iconName == DynamicMonoIconName || iconName == SceneMonoIconName)
            {
                menu.AddItem(new GUIContent("Show in Hierarchy"), false, () => ShowInHierarchy(content, itemIndex));
            }
            
            menu.ShowAsContext();
        }

        public void ShowRelatedItemContextMenu(IReadOnlyList<GUIContent> content, int itemIndex)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Show in Project"), false, () => ShowInProject(content, itemIndex));
            menu.AddItem(new GUIContent("Show in Hierarchy"), false, () => ShowInHierarchy(content, itemIndex));

            menu.ShowAsContext();
        }

        public void ShowInProject(IReadOnlyList<GUIContent> content, int index)
        {
            Debug.Log("Project " + content[index].text);
            
            var instanceTypeName = content[index].text;
            var pattern = "[^.]+$";

            var match = RegexExtension.FindMatch(pattern, instanceTypeName);
            
            if (!match.Success)
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {content[index].text} Not found from Project!");
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
                    Debug.LogWarning($"[{nameof(BindingsWindow)}] {content[index].text} Asset not found! ");
                }
            }
            else
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {content[index].text} No objects of this type were found in the project!");
            }
        }

        public void ShowInHierarchy(IReadOnlyList<GUIContent> content, int index)
        {
            var instanceTypeName = content[index].text;
            var pattern = "[^.]+$";

            var match = RegexExtension.FindMatch(pattern, instanceTypeName);
            
            if (!match.Success)
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {content[index].text} Not found from Project!");
                return;
            }
            
            var type = FindTypeByName(instanceTypeName);
        
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {content[index].text} Type not found or not a Component!");
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
                Debug.LogWarning($"[{nameof(BindingsWindow)}] {content[index].text} Object not found in the scene!");
            }
        }

        private void AddInheritorsToContent(string selectedInstanceTypeName, ICollection<GUIContent> relatedItems, Rect position)
        {
            var inheritorsData = _diExplorerService.GetInheritors(selectedInstanceTypeName);
            var icon = EditorGUIUtility.IconContent(AnimatorControllerIconName).image;

            if (inheritorsData.Inheritors.Length > 0)
            {
                foreach (var inheritor in inheritorsData.Inheritors)
                {
                    var baseInheritorName = inheritor.ToString();
                    var maxTextLength = (int)Mathf.Round(position.width - 240f);
                    var inheritorName = CutString(baseInheritorName, maxTextLength);
                    
                    relatedItems.Add(new GUIContent(inheritorName, icon, baseInheritorName));
                }
            }
        }

        private string CutString(string baseString, int maxTextLength)
        {
            return baseString.Length * CharacterLength > maxTextLength
                ? baseString.Substring(0, maxTextLength / CharacterLength) + "..."
                : baseString;
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