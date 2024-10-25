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
using DiContainerDebugger.Editor.Styles;
using DiExplorer.Bootstrap;
using DiExplorer.Editor.Panels;
using DiExplorer.Entities;
using DiExplorer.Services;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace DiContainerDebugger.Editor
{
    internal class BindingsWindow : ZenjectEditorWindow
    {
        private const int Unselected = -1;
        
        private List<GUIContent> _relatedItems = new List<GUIContent>();
        private List<GUIContent> _realtimeInstances = new List<GUIContent>();
        private List<GUIContent> _bindings = new List<GUIContent>();
        
        private bool _isShowWindow;
        private Vector2 _minWindowSize = new Vector2(800, 800);
        
        private Vector2 _relatedItemsScrollPosition;
        private Vector2 _realtimeInstancesScrollPosition;
        private Vector2 _bindingsScrollPosition;
        
        private int _relatedItemsSelectedIndex = Unselected;
        
        private GUIContent _passedIcon;
        private GUIContent _errorIcon;

        private DiExplorerService _diExplorerService;
        private ContextPanel _bindingsContextPanel;
        private BindingsPanel _bindingsPanel;
        private StaticAnalyzer _staticAnalyzer;

        [Inject]
        private void Construct(DiExplorerService service, StaticAnalyzer staticAnalyzer)
        {
            _diExplorerService = service;
            _bindingsContextPanel = new ContextPanel(service);
            _bindingsPanel = new BindingsPanel(service);
            _staticAnalyzer = staticAnalyzer;
        }

        [MenuItem("Custom/DI Explorer/Bindings")]

        public static void ShowWindow()
        {
            var window = GetWindow<BindingsWindow>("Bindings");
            window.Show();
        }

        protected override void InstallBindings(DiContainer container)
        {
            container.Install<DiExplorerEditorInstaller>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Initialize();
        }

        private void Initialize()
        {
            _isShowWindow = true;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += Repaint;
            EditorApplication.update += OnUpdate;
            
            _passedIcon = EditorGUIUtility.IconContent("TestPassed");
            _errorIcon = EditorGUIUtility.IconContent("console.erroricon");
        }

        private void OnDisable()
        {
            _isShowWindow = false;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update -= Repaint;
            
            if (EditorApplication.isPlaying)
            {
                _diExplorerService.SaveModel();
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                {
                    InjectFromProjectContext();
                    break;
                }
                case PlayModeStateChange.ExitingPlayMode:
                {
                    _diExplorerService.SaveModel();
                    break;
                }
                default:
                {
                    Debug.LogWarning($"[{nameof(BindingsWindow)}] Undefined state of the editor!");
                    break;
                }
            }
        }

        private void OnUpdate()
        {
            switch (EditorApplication.isPlaying)
            {
                case true when _isShowWindow:
                    _diExplorerService.UpdateExploreData();
                    break;
                case false:
                    _diExplorerService.LoadModel();
                    break;
            }

            UpdateBindingsData();
            CreateContent();
            ProcessItemSelection();
            
        }

        private void UpdateBindingsData()
        {
            _bindingsContextPanel.UpdateData();
                
            if (_bindingsContextPanel.ContextSelectedIndex == 0)
            {
                _bindingsPanel.UpdateData();
            }
            else
            {
                var containerName =_bindingsContextPanel.ContextSelectedName;
                _bindingsPanel.UpdateDataByContainer(containerName);
            }
        }

        private void CreateContent()
        {
            _realtimeInstances = _bindingsPanel.CreateInstancesContent().ToList();
            _bindings = _bindingsPanel.CreateBindingsContent().ToList();
        }

        private void ProcessItemSelection()
        {
            _relatedItems = _bindingsPanel.ProcessItemSelection().ToList();
        }

        private void OnGUI()
        {
            minSize = _minWindowSize;
            
            EditorGUILayout.BeginHorizontal();
            {
                OnGUIContentContainer();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnGUIContentContainer()
        {
            EditorGUILayout.BeginVertical();
            {
                //EditorGUILayout.LabelField("Bindings", EditorStyles.boldLabel);

                EditorGUILayout.Separator();
                
                // Top Panel
                EditorGUILayout.BeginHorizontal();
                {
                    // Dropdown context List
                    GUILayout.BeginHorizontal(GUILayout.Width(position.width / 5f));
                    {
                        EditorGUILayout.LabelField("Context", EditorStyles.label, GUILayout.Width(50f));
                        
                        var newSelectedIndex =
                            EditorGUILayout.Popup(_bindingsContextPanel.ContextSelectedIndex, _bindingsContextPanel.Contexts, GUILayout.Width(150f));
                        
                        if (newSelectedIndex != _bindingsContextPanel.ContextSelectedIndex)
                        {
                            _bindingsContextPanel.SetSelectedIndex(newSelectedIndex);
                        }
                        
                        GUILayout.Space(5f);

                        if (!EditorApplication.isPlaying && _isShowWindow)
                        {
                            var signalBusStatusIcon = _staticAnalyzer.IsBoundSignalBus ? _passedIcon : _errorIcon;

                            GUILayout.Label("Bind status", GUILayout.Width(70f));
                            GUILayout.Label(signalBusStatusIcon, GUILayout.Width(20f), GUILayout.Height(20f));

                            if (GUILayout.Button("Validate", GUILayout.Width(60f)))
                            {
                                _staticAnalyzer.ValidateActiveScenes();
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    // Bindings Search String
                    GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), GUILayout.Width(position.width / 2f));
                    {
                        _bindingsPanel.SetBindingsSearchString(GUILayout.TextField(_bindingsPanel.BindingsSearchString,
                            EditorElementStyle.ToolbarSearchTextField,
                            GUILayout.ExpandWidth(true)));
                        if (GUILayout.Button("", EditorElementStyle.ToolbarSearchCancelButton))
                        {
                            _bindingsPanel.SetBindingsSearchString(string.Empty);
                            GUI.FocusControl(null);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Separator();

                // Realtime and Bindings Panel
                EditorGUILayout.BeginHorizontal();
                {
                    // Realtime Instances Zone
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
                    {
                        EditorGUILayout.LabelField("Realtime Instances", EditorStyles.boldLabel);
                        _realtimeInstancesScrollPosition =
                            EditorGUILayout.BeginScrollView(_realtimeInstancesScrollPosition,
                                GUILayout.Height(position.height / 2));
                        {
                            for (var i = 0; i < _realtimeInstances.Count; i++)
                            {
                                // Используем кнопку для каждого элемента в списке
                                if (GUILayout.Button(_realtimeInstances[i], EditorElementStyle.ListElementDefaultStyle))
                                {
                                    // Обработка нажатия на элемент
                                    HandleRealtimeInstanceClick(i);
                                }
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();

                    // Bindings zone
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
                    {
                        EditorGUILayout.LabelField("Bindings", EditorStyles.boldLabel);
                        _bindingsScrollPosition = EditorGUILayout.BeginScrollView(_bindingsScrollPosition,
                            GUILayout.Height(position.height / 2));
                        {
                            for (var i = 0; i < _bindings.Count; i++)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    var style = _bindingsPanel.BindingSelectedIndex == i
                                        ? EditorElementStyle.SelectedListElementStyle
                                        : EditorElementStyle.ListElementDefaultStyle;

                                    if (GUILayout.Button(new GUIContent(_bindings[i]), style))
                                    {
                                        _bindingsPanel.SetBindingSelectedIndex(i);
                                        
                                        if (_bindingsPanel.BindingSelectedIndex != _bindingsPanel.PrevBindingSelectedIndex)
                                        {
                                            _relatedItemsSelectedIndex = Unselected;
                                        }

                                        _bindingsPanel.ResetSelectedIndexExceptBindings();
                                    }

                                    var countInstance = _diExplorerService
                                        .GetInstanceNamesFromBinding(_bindings[i].text).Length;

                                    GUILayout.Label(countInstance.ToString(),
                                        EditorElementStyle.BindingCountLabel);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();

                // Related Items
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Related items", EditorStyles.boldLabel);
                    _relatedItemsScrollPosition =
                        EditorGUILayout.BeginScrollView(_relatedItemsScrollPosition);
                    {
                        _relatedItemsSelectedIndex = GUILayout.SelectionGrid(_relatedItemsSelectedIndex,
                            _relatedItems.ToArray(), 1, EditorElementStyle.ListElementDefaultStyle);
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();
                
            EditorGUILayout.Separator();
        }
        
        private void HandleRealtimeInstanceClick(int index)
        {
            if (Event.current.button == 0) // Left mouse button
            {
                _bindingsPanel.SetRealtimeSelectedIndex(index);
                
                if (_bindingsPanel.RealtimeSelectedIndex != _bindingsPanel.PrevRealtimeSelectedIndex)
                {
                    _relatedItemsSelectedIndex = Unselected;
                }

                _bindingsPanel.ResetSelectedIndexExceptInstances();
            }
            else if (Event.current.button == 1) // Right mouse button
            {
                _bindingsPanel.ShowContextMenu(index);
            }
        }
        
       
    }
}