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
using DiContainerDebugger.Editor.Styles;
using DiExplorer.Bootstrap;
using DiExplorer.Data;
using DiExplorer.Editor.Panels;
using DiExplorer.Entities;
using DiExplorer.Services;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace DiContainerDebugger.Editor
{
    internal class DiExplorerWindow : ZenjectEditorWindow
    {
        private const int Unselected = -1;
        private bool _isShowWindow;

        private Dictionary<string, List<SignalCallData>> _signalCallsDictionary =
            new Dictionary<string, List<SignalCallData>>();
        
        private List<GUIContent> _relatedItems = new List<GUIContent>();
        
        private List<GUIContent> _realtimeInstances = new List<GUIContent>();
        private List<GUIContent> _bindings = new List<GUIContent>();
        private List<GUIContent> _signals = new List<GUIContent>();
        private List<GUIContent> _subscriptions = new List<GUIContent>();

        private Vector2 _relatedItemsScrollPosition;
        
        private Vector2 _realtimeInstancesScrollPosition;
        private Vector2 _bindingsScrollPosition;
        private Vector2 _signalsScrollPosition;
        private Vector2 _subscriptionsScrollPosition;
        private Vector2 _signalCallsScrollPosition;
        
        private int _relatedItemsSelectedIndex = Unselected;

        private DiExplorerService _diExplorerService;
        private ContextPanel _bindingsContextPanel;
        private ContextPanel _signalsContextPanel;
        private BindingsPanel _bindingsPanel;
        private SignalsPanel _signalsPanel;

        [Inject]
        private void Construct(DiExplorerService service, SignalCallsCollector signalCallsCollector)
        {
            _diExplorerService = service;
            _bindingsContextPanel = new ContextPanel(service);
            _signalsContextPanel = new ContextPanel(service);
            _signalsPanel = new SignalsPanel(service, signalCallsCollector);
            _bindingsPanel = new BindingsPanel(service);
        }

        [MenuItem("Custom/DI Explorer")]

        public static void ShowWindow()
        {
            var window = GetWindow<DiExplorerWindow>("DI Explorer");
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
                    _signalsPanel.ClearSignalCalls();
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
                    Debug.LogWarning($"[{nameof(DiExplorerWindow)}] Undefined state of the editor!");
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
            UpdateSignalsData();
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
        
        private void UpdateSignalsData()
        {
            _signalsContextPanel.UpdateData();
                
            if (_signalsContextPanel.ContextSelectedIndex == 0)
            {
                _signalsPanel.UpdateData();
            }
            else
            {
                var containerName =_signalsContextPanel.ContextSelectedName;
                _signalsPanel.UpdateDataByContainer(containerName);
            }

            _signalCallsDictionary = _signalsPanel.SignalCalls;
        }

        private void CreateContent()
        {
            _realtimeInstances = _bindingsPanel.CreateInstancesContent().ToList();
            _bindings = _bindingsPanel.CreateBindingsContent().ToList();
            _signals = _signalsPanel.CreateSignalsContent().ToList();
        }

        private void ProcessItemSelection()
        {
            _relatedItems = _bindingsPanel.ProcessItemSelection().ToList();
            _subscriptions = _signalsPanel.ProcessItemSelection().ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                OnGUILeftContainer();
                OnGUIRightContainer();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnGUILeftContainer()
        {
            // Left Main Container
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
            {
                EditorGUILayout.LabelField("Left Container", EditorStyles.boldLabel);

                // Top Panel
                EditorGUILayout.BeginHorizontal();
                {
                    // Dropdown context List
                    GUILayout.BeginHorizontal(GUILayout.Width(position.width / 6f));
                    {
                        var newSelectedIndex =
                            EditorGUILayout.Popup(_bindingsContextPanel.ContextSelectedIndex, _bindingsContextPanel.Contexts);
                        
                        if (newSelectedIndex != _bindingsContextPanel.ContextSelectedIndex)
                        {
                            _bindingsContextPanel.SetSelectedIndex(newSelectedIndex);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    // Bindings Search String
                    GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), GUILayout.Width(position.width / 4f));
                    {
                        _bindingsPanel.SetBindingsSearchString(GUILayout.TextField(_bindingsPanel.BindingsSearchString,
                            GUI.skin.FindStyle("ToolbarSeachTextField"),
                            GUILayout.ExpandWidth(true)));
                        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                        {
                            _bindingsPanel.SetBindingsSearchString(string.Empty);
                            GUI.FocusControl(null);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();

                // Realtime and Bindings Panel
                EditorGUILayout.BeginHorizontal();
                {
                    // Realtime Instances Zone
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 4f));
                    {
                        EditorGUILayout.LabelField("Realtime Instances", EditorStyles.boldLabel);
                        _realtimeInstancesScrollPosition =
                            EditorGUILayout.BeginScrollView(_realtimeInstancesScrollPosition, GUILayout.Height(200f));
                        {
                            _bindingsPanel.SetRealtimeSelectedIndex(GUILayout.SelectionGrid(
                                _bindingsPanel.RealtimeSelectedIndex, _realtimeInstances.ToArray(), 1,
                                EditorElementStyle.ListElementDefaultStyle));

                            if (_bindingsPanel.RealtimeSelectedIndex != _bindingsPanel.PrevRealtimeSelectedIndex)
                            {
                                _relatedItemsSelectedIndex = Unselected;
                            }

                            _bindingsPanel.ResetSelectedIndexExceptInstances();
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();

                    // Bindings zone
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 4f));
                    {
                        EditorGUILayout.LabelField("Bindings", EditorStyles.boldLabel);
                        _bindingsScrollPosition =
                            EditorGUILayout.BeginScrollView(_bindingsScrollPosition, GUILayout.Height(200f));
                        {
                            for (var i = 0; i < _bindings.Count; i++)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUIStyle style = _bindingsPanel.BindingSelectedIndex == i
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
                    EditorGUILayout.LabelField("Related items", EditorStyles.boldLabel);
                    _relatedItemsScrollPosition =
                        EditorGUILayout.BeginScrollView(_relatedItemsScrollPosition, GUILayout.Height(200f));
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
        
        private void OnGUIRightContainer()
        {
            // Right Main Container
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
            {
                EditorGUILayout.LabelField("Right Container", EditorStyles.boldLabel);
                
                // Top Panel
                EditorGUILayout.BeginHorizontal();
                {
                    // Dropdown context List
                    GUILayout.BeginHorizontal(GUILayout.Width(position.width / 6f));
                    {
                        var newSelectedIndex =
                            EditorGUILayout.Popup(_signalsContextPanel.ContextSelectedIndex, _signalsContextPanel.Contexts);

                        if (newSelectedIndex != _signalsContextPanel.ContextSelectedIndex)
                        {
                            _signalsContextPanel.SetSelectedIndex(newSelectedIndex);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    // Subscriptions Search String
                    GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), GUILayout.Width(position.width / 4f));
                    {
                        //This Style fixed in Unity 2022.3
                        _signalsPanel.SetSearchString(GUILayout.TextField(_signalsPanel.SubscriptionsSearchString,
                            GUI.skin.FindStyle("ToolbarSeachTextField"),
                            GUILayout.ExpandWidth(true)));
                        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                        {
                            _signalsPanel.SetSearchString(string.Empty);
                            GUI.FocusControl(null);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();
                    
                // Signals and Subscriptions panel
                EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width / 2f));
                {
                    // Signals
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 4f));
                    {
                        EditorGUILayout.LabelField("Signals", EditorStyles.boldLabel);
                        
                        _signalsScrollPosition =
                            EditorGUILayout.BeginScrollView(_signalsScrollPosition, GUILayout.Height(220f));
                        {
                            for (int i = 0; i < _signals.Count; i++)
                            {
                                var s = _signalsPanel.GetSignalToggleValue(i);
                                _signalsPanel.SetSignalToggleValue(i,
                                    GUILayout.Toggle(_signalsPanel.GetSignalToggleValue(i), _signals[i],
                                        EditorElementStyle.ToggleDefaultStyle));
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    } 
                    EditorGUILayout.EndVertical();
                        
                    // Subscriptions
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 4f));
                    {
                        EditorGUILayout.LabelField("Subscriptions", EditorStyles.boldLabel);

                        _subscriptionsScrollPosition =
                            EditorGUILayout.BeginScrollView(_subscriptionsScrollPosition, GUILayout.Height(200f));
                        {
                            for (var i = 0; i < _subscriptions.Count; i++)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUIStyle style = _signalsPanel.SubscriptionsSelectedIndex == i
                                        ? EditorElementStyle.SelectedListElementStyle
                                        : EditorElementStyle.ListElementDefaultStyle;

                                    if (GUILayout.Button(new GUIContent(_subscriptions[i]), style))
                                    {
                                        _signalsPanel.SetSelectedSubscriptionIndex(i);
                                    }
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

                // Signal Calls
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Signal Calls", EditorStyles.boldLabel);
                    
                    _signalCallsScrollPosition = EditorGUILayout.BeginScrollView(_signalCallsScrollPosition);
                    {
                        GUILayout.BeginHorizontal();

                        if (_signalCallsDictionary.Keys.Count > 0)
                        {
                            foreach (var callTime in _signalCallsDictionary.Keys)
                            {
                                GUILayout.BeginVertical(GUILayout.Width(100));
                                {
                                    var timeSpan = TimeSpan.Parse(callTime);
                                    var formattedTime = $@"{timeSpan:mm\:ss\.fff}";

                                    GUILayout.Label(formattedTime, GUILayout.Height(20));

                                    foreach (var signalCallData in _signalCallsDictionary[callTime])
                                    {
                                        if (GUILayout.Button(signalCallData.SignalTypeName, GUILayout.Height(20)))
                                        {
                                            Debug.Log(signalCallData.SignalTypeName + " clicked");
                                        }
                                    }
                                }
                                GUILayout.EndVertical();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
    }
}