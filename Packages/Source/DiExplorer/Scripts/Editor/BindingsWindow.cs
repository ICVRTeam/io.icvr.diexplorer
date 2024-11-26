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
using DiExplorer.Extensions;
using DiExplorer.Services;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace DiContainerDebugger.Editor
{
    internal class BindingsWindow : ZenjectEditorWindow
    {
        private const int Unselected = -1;
        
        private readonly Vector2 _minWindowSize = new Vector2(950, 400);

        private List<GUIContent> _relatedItems = new List<GUIContent>();
        private List<(GUIContent Content, int Count)> _realtimeInstances = new List<(GUIContent, int)>();
        private List<GUIContent> _bindings = new List<GUIContent>();

        private Vector2 _relatedItemsScrollPosition;
        private Vector2 _realtimeInstancesScrollPosition;
        private Vector2 _bindingsScrollPosition;

        private BindingsPanel.SearchGroup _searchGroup = BindingsPanel.SearchGroup.Bindings;

        private GUIContent _passedIcon;
        private GUIContent _errorIcon;
        
        private bool _isShowWindow;

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
                case PlayModeStateChange.ExitingEditMode:
                {
                    _diExplorerService.CollectSceneInstances();
                    break;
                }
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
            _realtimeInstances = _bindingsPanel.CreateInstancesContent(position).ToList();
            _bindings = _bindingsPanel.CreateBindingsContent(position).ToList();
        }

        private void ProcessItemSelection()
        {
            _relatedItems = _bindingsPanel.ProcessItemSelection(position).ToList();
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
                EditorGUILayout.Separator();
                
                var buttonContent = new GUIContent("Delete Cash", EditorIconExtension.WinCloseIcon.image);
                
                if (GUILayout.Button(buttonContent, EditorStyles.miniButton, GUILayout.Width(100f)))
                {
                    _diExplorerService.DeleteBindingsSaveFiles();
                }
                
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
                            _bindingsPanel.ResetRelatedSelectedIndex();
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

                    // Bindings and Instances Search String
                    GUILayout.BeginHorizontal(GUILayout.Width(position.width / 2f));
                    {
                        var popupOptions = System.Enum.GetNames(typeof(BindingsPanel.SearchGroup));
                        var selectedIndex = (int)_searchGroup;

                        GUILayout.Label("Search Group", GUILayout.Width(85f));
                        
                        selectedIndex = EditorGUILayout.Popup(selectedIndex, popupOptions,
                            GUILayout.Width(85f));
                        
                        _searchGroup = (BindingsPanel.SearchGroup)selectedIndex;
                        
                        GUILayout.BeginHorizontal(EditorElementStyle.Toolbar);
                        {
                            _bindingsPanel.SetSearchString(GUILayout.TextField(_bindingsPanel.SearchString,
                                EditorElementStyle.ToolbarSearchTextField,
                                GUILayout.ExpandWidth(true)), _searchGroup);
                            if (GUILayout.Button("", EditorElementStyle.ToolbarSearchCancelButton))
                            {
                                _bindingsPanel.SetSearchString(string.Empty, _searchGroup);
                                GUI.FocusControl(null);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Separator();

                // Realtime and Bindings Panel
                EditorGUILayout.BeginHorizontal();
                {
                    // Realtime Instances Zone
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField($"Realtime Instances: {_realtimeInstances.Count}",
                                EditorStyles.boldLabel, GUILayout.Width(180f));

                            var noMonoIcon = EditorGUIUtility.IconContent(BindingsPanel.NoMonoIconName).image;
                            var dynamicMonoIcon = EditorGUIUtility.IconContent(BindingsPanel.DynamicMonoIconName).image;
                            var sceneMonoIcon = EditorGUIUtility.IconContent(BindingsPanel.SceneMonoIconName).image;
                            
                            _bindingsPanel.SetShowNoMono(
                                GUILayout.Toggle(_bindingsPanel.ShowNoMonoInstances, noMonoIcon,
                                    EditorElementStyle.ToggleDefaultStyle, GUILayout.Width(60f)));

                            _bindingsPanel.SetShowDynamicMono(
                                GUILayout.Toggle(_bindingsPanel.ShowDynamicMonoInstances, dynamicMonoIcon,
                                    EditorElementStyle.ToggleDefaultStyle, GUILayout.Width(60f)));

                            _bindingsPanel.SetShowSceneMono(
                                GUILayout.Toggle(_bindingsPanel.ShowSceneMonoInstances, sceneMonoIcon,
                                    EditorElementStyle.ToggleDefaultStyle, GUILayout.Width(60f)));
                            
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Number", GUILayout.Width(87f));
                        }
                        GUILayout.EndHorizontal();
                        
                        _realtimeInstancesScrollPosition =
                            EditorGUILayout.BeginScrollView(_realtimeInstancesScrollPosition,
                                GUILayout.Height(position.height / 2));
                        {
                            for (var i = 0; i < _realtimeInstances.Count; i++)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    ShowIndex(i);

                                    var style = _bindingsPanel.RealtimeSelectedIndex == i
                                        ? EditorElementStyle.SelectedListElementStyle
                                        : EditorElementStyle.ListElementDefaultStyle;

                                    if (_bindingsPanel.RelatedItemSelectedIndex != Unselected)
                                    {
                                        if (_realtimeInstances[i].Content.tooltip ==
                                            _relatedItems[_bindingsPanel.RelatedItemSelectedIndex].tooltip)
                                        {
                                            style = EditorElementStyle.InfoListElementStyle;

                                            if (!_bindingsPanel.InstancesIsScrollToIndex)
                                            {
                                                _realtimeInstancesScrollPosition.y = i * 20f;
                                                _bindingsPanel.SetInstancesIsScrollToIndex(true);
                                            }
                                            
                                        }
                                    }

                                    if (GUILayout.Button(_realtimeInstances[i].Content, style,
                                            GUILayout.Width(position.width / 2f - 160f)))
                                    {
                                        var contentList = _realtimeInstances.Select(tuple => tuple.Content).ToList();
                                        HandleRealtimeInstanceClick(contentList, i);
                                    }

                                    var countInstance = _realtimeInstances[i].Count;

                                    GUILayout.Label(countInstance.ToString(),
                                        EditorElementStyle.BindingCountLabel);
                                    
                                    if (GUILayout.Button(EditorIconExtension.ScriptIcon.image, style,
                                            GUILayout.Width(20f)))
                                    {
                                        var contentList = _realtimeInstances.Select(tuple => tuple.Content).ToList();
                                        InstanceInfoEditorExtension.ShowInProjectFile(contentList, i);
                                    }
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();

                    // Bindings zone
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label($"Bindings: {_bindings.Count}", EditorStyles.boldLabel);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Instances", GUILayout.Width(60f));
                            GUILayout.Label("Inheritors", GUILayout.Width(64f));
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        _bindingsScrollPosition = EditorGUILayout.BeginScrollView(_bindingsScrollPosition,
                            GUILayout.Height(position.height / 2));
                        {
                            for (var i = 0; i < _bindings.Count; i++)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    ShowIndex(i);
                                    
                                    var style = _bindingsPanel.BindingSelectedIndex == i
                                        ? EditorElementStyle.SelectedListElementStyle
                                        : EditorElementStyle.ListElementDefaultStyle;

                                    if (_bindingsPanel.RelatedItemSelectedIndex != Unselected)
                                    {
                                        if (_bindings[i].tooltip == _relatedItems[_bindingsPanel.RelatedItemSelectedIndex].tooltip)
                                        {
                                            style = EditorElementStyle.InfoListElementStyle;
                                            
                                            if (!_bindingsPanel.BindingsIsScrollToIndex)
                                            {
                                                _bindingsScrollPosition.y = i * 20f;
                                                _bindingsPanel.SetBindingsIsScrollToIndex(true);
                                            }
                                        }
                                    }

                                    if (GUILayout.Button(new GUIContent(_bindings[i]), style,
                                            GUILayout.Width(position.width / 2f - 185f)))
                                    {
                                        _bindingsPanel.SetBindingSelectedIndex(i);

                                        if (_bindingsPanel.BindingSelectedIndex !=
                                            _bindingsPanel.PrevBindingSelectedIndex)
                                        {
                                            _bindingsPanel.SetRelatedItemSelectedIndex(Unselected);
                                        }

                                        _bindingsPanel.ResetSelectedIndexExceptBindings();
                                    }

                                    var countInstance = _diExplorerService
                                        .GetInstanceNamesFromBinding(_bindings[i].tooltip).Length;

                                    GUILayout.Label(countInstance.ToString(),
                                        EditorElementStyle.BindingCountLabel);

                                    var countInheritors =
                                        _diExplorerService.GetInheritorsCountByClass(_bindings[i].tooltip);
                                    
                                    GUILayout.Label(countInheritors.ToString(),
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
                    EditorGUILayout.LabelField(
                        $"Related items {_bindingsPanel.SelectedItemForRelated}: {_relatedItems.Count}",
                        EditorStyles.boldLabel);
                    
                    _relatedItemsScrollPosition =
                        EditorGUILayout.BeginScrollView(_relatedItemsScrollPosition);
                    {
                        for (var i = 0; i < _relatedItems.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                ShowIndex(i);
                                
                                var style = _bindingsPanel.RelatedItemSelectedIndex == i
                                    ? EditorElementStyle.SelectedListElementStyle
                                    : EditorElementStyle.ListElementDefaultStyle;

                                if (GUILayout.Button(_relatedItems[i], style, 
                                        GUILayout.Width(position.width - 130f)))
                                {
                                    HandleRelatedItemsClick(_relatedItems, i);
                                }
                                
                                if (GUILayout.Button(EditorIconExtension.ScriptIcon.image, 
                                        EditorElementStyle.ListElementDefaultStyle, GUILayout.Width(20f)))
                                {
                                    InstanceInfoEditorExtension.ShowInProjectFile(_relatedItems, i);
                                }
                                
                                if (GUILayout.Button(EditorIconExtension.CameraIcon.image, 
                                        EditorElementStyle.ListElementDefaultStyle, GUILayout.Width(20f)))
                                {
                                    InstanceInfoEditorExtension.ShowInHierarchyInstance(_relatedItems, i);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();
                
            EditorGUILayout.Separator();
        }

        private void ShowIndex(int i)
        {
            var displayedIndex = i + 1;

            GUILayout.Label(displayedIndex.ToString(),
                EditorElementStyle.BindingCountLabel);
        }

        private void HandleRealtimeInstanceClick(IReadOnlyList<GUIContent> content, int index)
        {
            if (Event.current.button == 0) // Left mouse button
            {
                _bindingsPanel.SetRealtimeSelectedIndex(index);
                
                if (_bindingsPanel.RealtimeSelectedIndex != _bindingsPanel.PrevRealtimeSelectedIndex)
                {
                    _bindingsPanel.SetRelatedItemSelectedIndex(Unselected);
                }

                _bindingsPanel.ResetSelectedIndexExceptInstances();
            }
            else if (Event.current.button == 1) // Right mouse button
            {
                _bindingsPanel.ShowInstanceContextMenu(content, index);
            }
        }
        private void HandleRelatedItemsClick(IReadOnlyList<GUIContent> content, int index)
        {
            if (Event.current.button == 0) // Left mouse button
            {
                _bindingsPanel.SetRelatedItemSelectedIndex(index);
            }
            else if (Event.current.button == 1) // Right mouse button
            {
                _bindingsPanel.ShowRelatedItemContextMenu(content, index);
            }
        }
    }
}