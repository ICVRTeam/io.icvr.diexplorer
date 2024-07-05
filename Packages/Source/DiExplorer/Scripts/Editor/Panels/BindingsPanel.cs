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
using DiContainerDebugger.Scripts;
using DiExplorer.Data;
using DiExplorer.Extensions;
using DiExplorer.Services;
using UnityEditor;
using UnityEngine;

namespace DiExplorer.Editor.Panels
{
    internal class BindingsPanel
    {
        private const int Unselected = -1;

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
                var icon = EditorGUIUtility.IconContent("Prefab Icon").image;
                
                switch (instanceData.InstanceType)
                {
                    case InstanceType.NoMono:
                    {
                        icon = EditorGUIUtility.IconContent("d_ScriptableObject Icon").image;
                        break;
                    }
                    case InstanceType.DynamicMono:
                    {
                        icon = EditorGUIUtility.IconContent("Prefab Icon").image;
                        break;
                    }
                    case InstanceType.SceneMono:
                    {
                        icon = EditorGUIUtility.IconContent("TerrainInspector.TerrainToolRaise").image;
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
    }
}