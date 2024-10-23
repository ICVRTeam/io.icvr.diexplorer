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
using DiExplorer.Data;
using DiExplorer.Editor.Panels;
using DiExplorer.Entities;
using DiExplorer.Services;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace DiContainerDebugger.Editor
{
    internal class SignalsWindow : ZenjectEditorWindow
    {
        private const int NumberDisplayedSignals = 10;
        
        private const float WidthSignalNameArea = 200f;
        private const float MinScaleValue = 0.8f;
        private const float MaxScaleValue = 5f;
        private const float TimelineWidth = 1000f;
        private const float MinTimerInEditorOffset = 20f;
        private const float SignalRowHeight = 30f;

        private readonly Color[] _rowColors = { Color.gray, new Color(0.392f, 0.392f, 0.392f, 1f) };
        private readonly Color _colorDividingLine = new Color(0.6352941f, 0.6352941f, 0.6352941f);
        
        private Dictionary<string, List<SignalCallData>> _signalCallsDictionary =
            new Dictionary<string, List<SignalCallData>>();

        private List<GUIContent> _signals = new List<GUIContent>();
        private List<GUIContent> _subscriptions = new List<GUIContent>();
        
        private bool _isShowWindow;
        private Vector2 _minWindowSize = new Vector2(800, 800);
        
        private Vector2 _signalsScrollPosition;
        private Vector2 _subscriptionsScrollPosition;
        private Vector2 _signalCallsScrollPosition;
        
        private Vector2 _timelineScrollPos;
        private Vector2 _signalNameScrollPos;
        private Vector2 _signalScrollPos;
        
        private float _scaleValue = 1f;
        private float _timer;

        private bool _autoScroll = true;
        private bool _forceAutoScroll;
        
        private GUIContent _scaleIcon;

        private DiExplorerService _diExplorerService;
        private ContextPanel _signalsContextPanel;
        private SignalsPanel _signalsPanel;

        [Inject]
        private void Construct(DiExplorerService service, SignalCallsCollector signalCallsCollector)
        {
            _diExplorerService = service;
            _signalsContextPanel = new ContextPanel(service);
            _signalsPanel = new SignalsPanel(service, signalCallsCollector);
        }

        [MenuItem("Custom/DI Explorer/Signals")]

        public static void ShowWindow()
        {
            var window = GetWindow<SignalsWindow>("Signals");
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
            EditorApplication.update += OnUpdate;
            
            _scaleIcon = EditorGUIUtility.IconContent("d_ViewToolZoom");
        }

        private void OnDisable()
        {
            _isShowWindow = false;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnUpdate;

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
                {
                    _diExplorerService.UpdateExploreData();
                    
                    var actualTime = _diExplorerService.GetElapsedTimePlayModeTimer();
                    
                    _timer = (float)actualTime.TotalSeconds;
                    
                    break;
                }
                case false:
                {
                    _diExplorerService.LoadModel();
                    
                    if (_signalCallsDictionary.Count > 0)
                    {
                        var maxFiringTime = _signalCallsDictionary
                            .SelectMany(kvp => kvp.Value)
                            .Max(signalCall => signalCall.FiringTime);

                        var timer = (float)maxFiringTime.TotalSeconds;

                        _timer = timer + MinTimerInEditorOffset;
                    }
                    
                    break;
                }
            }

            UpdateSignalsData();
            CreateContent();
            ProcessItemSelection();
            Repaint();
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
                var containerName = _signalsContextPanel.ContextSelectedName;
                _signalsPanel.UpdateDataByContainer(containerName);
            }

            _signalCallsDictionary = _signalsPanel.SignalCalls;
        }

        private void CreateContent()
        {
            _signals = _signalsPanel.CreateSignalsContent().ToList();
        }

        private void ProcessItemSelection()
        {
            _subscriptions = _signalsPanel.ProcessItemSelection().ToList();
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

                // Top Panel
                EditorGUILayout.BeginHorizontal();
                {
                    // Dropdown context List
                    GUILayout.BeginHorizontal(GUILayout.Width(position.width / 5f));
                    {
                        EditorGUILayout.LabelField("Context", EditorStyles.label, GUILayout.Width(50f));

                        var newSelectedIndex =
                            EditorGUILayout.Popup(_signalsContextPanel.ContextSelectedIndex,
                                _signalsContextPanel.Contexts, GUILayout.Width(150f));

                        if (newSelectedIndex != _signalsContextPanel.ContextSelectedIndex)
                        {
                            _signalsContextPanel.SetSelectedIndex(newSelectedIndex);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    // Subscriptions Search String
                    GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), GUILayout.Width(position.width / 2f));
                    {
                        _signalsPanel.SetSearchString(GUILayout.TextField(_signalsPanel.SubscriptionsSearchString,
                            EditorElementStyle.ToolbarSearchTextField,
                            GUILayout.ExpandWidth(true)));
                        
                        if (GUILayout.Button("", EditorElementStyle.ToolbarSearchCancelButton))
                        {
                            _signalsPanel.SetSearchString(string.Empty);
                            GUI.FocusControl(null);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Separator();

                // Signals and Subscriptions panel
                EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width / 2f));
                {
                    // Signals
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
                    {
                        EditorGUILayout.LabelField("Signals", EditorStyles.boldLabel);

                        _signalsScrollPosition = EditorGUILayout.BeginScrollView(_signalsScrollPosition,
                            GUILayout.Height(position.height / 2.45f));
                        {
                            for (var i = 0; i < _signals.Count; i++)
                            {
                                _signalsPanel.SetSignalToggleValue(i,
                                    GUILayout.Toggle(_signalsPanel.GetSignalToggleValue(i), _signals[i],
                                        EditorElementStyle.ToggleDefaultStyle));
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();

                    // Subscriptions
                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
                    {
                        EditorGUILayout.LabelField("Subscriptions", EditorStyles.boldLabel);

                        _subscriptionsScrollPosition = EditorGUILayout.BeginScrollView(_subscriptionsScrollPosition,
                            GUILayout.Height(position.height / 2.45f));
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
                
                // Signal Calls Log
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.Space();

                    // Timeline buttons
                    EditorGUILayout.BeginHorizontal();
                    {
                        switch (EditorApplication.isPlaying)
                        {
                            case true when _isShowWindow:
                            {
                                if (GUILayout.Button(_autoScroll ? "Disable autoscroll" : "Enable autoscroll",
                                        GUILayout.Width(150f)))
                                {
                                    _autoScroll = !_autoScroll;
                                }
                                break;
                            }
                            case false:
                            {
                                _autoScroll = false;
                                break;
                            }
                        }

                        if (!_autoScroll && GUILayout.Button("Scroll to actual", GUILayout.Width(150f)))
                        {
                            _forceAutoScroll = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // Timer and Scale
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label($"Timer: {_timer:F2} seconds", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(_scaleIcon, GUILayout.Width(20f), GUILayout.Height(20f));

                        var displayedScale = (int)(_scaleValue * 100f);
                        
                        GUILayout.Label(displayedScale.ToString("F0") + "%", EditorStyles.boldLabel);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space();
                    
                    HandleMouseScroll();

                    // Timeline
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(WidthSignalNameArea + 10f);
                        
                        _timelineScrollPos =
                            EditorGUILayout.BeginScrollView(_timelineScrollPos, GUILayout.Height(40f),
                                GUILayout.ExpandWidth(true));
                        {
                            DrawTimeScale();
                        }
                        EditorGUILayout.EndScrollView();
                        
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Signal log panel
                    EditorGUILayout.BeginHorizontal();
                    {
                        // Signal names
                        _signalNameScrollPos = EditorGUILayout.BeginScrollView(_signalNameScrollPos, 
                            GUILayout.Height(SignalRowHeight * NumberDisplayedSignals + 10f), 
                            GUILayout.Width(WidthSignalNameArea));
                        {
                            EditorGUILayout.BeginVertical(GUILayout.Width(WidthSignalNameArea));
                            
                            foreach (var signal in _signalCallsDictionary.Keys)
                            {
                                GUILayout.Label(signal, GUILayout.Height(SignalRowHeight - 2f));
                            }

                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndScrollView();
                        
                        EditorGUILayout.Space(10f);

                        // Signal calls log
                        _signalScrollPos = EditorGUILayout.BeginScrollView(_signalScrollPos,
                            GUILayout.Height(SignalRowHeight * NumberDisplayedSignals + 10f), GUILayout.ExpandWidth(true));
                        {
                            DrawSignalEvents();
                        }
                        EditorGUILayout.EndScrollView();
                        
                        GUILayout.FlexibleSpace();
                    }

                    EditorGUILayout.EndHorizontal(); // End signal log panel

                    SyncScrolls();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void SyncScrolls()
        {
            if (_autoScroll || _forceAutoScroll)
            {
                _forceAutoScroll = false;

                _timelineScrollPos.x = Mathf.Max(TimelineWidth, _timer * 150f);
                _timelineScrollPos.y = Mathf.Infinity;

                _signalScrollPos.x = _timelineScrollPos.x;
            }
            else
            {
                _timelineScrollPos.x = _signalScrollPos.x;
            }

            _signalNameScrollPos.y = _signalScrollPos.y;
        }

        private void DrawTimeScale()
        {
            var totalWidth = Mathf.Max(TimelineWidth, _timer * 150f) * _scaleValue;
            
            var timeScaleRect = GUILayoutUtility.GetRect(totalWidth, 20f);
            
            for (var t = 0f; t < _timer; t += 0.5f)
            {
                var markerX = t / _timer * totalWidth;
                
                if (markerX >= timeScaleRect.x && markerX <= timeScaleRect.x + totalWidth)
                {
                    GUI.Label(new Rect(markerX, timeScaleRect.y, 50f, 20f), t.ToString("F1") + "s");
                }
            }
        }

        private void DrawSignalEvents()
        {
            var rowIndex = 0;
            var totalWidth = Mathf.Max(TimelineWidth, _timer * 150f) * _scaleValue;

            EditorGUILayout.BeginVertical();
            
            foreach (var signal in _signalCallsDictionary)
            {
                EditorGUILayout.BeginHorizontal();

                var timelineRect = GUILayoutUtility.GetRect(totalWidth, SignalRowHeight);
                
                EditorGUI.DrawRect(timelineRect, _rowColors[rowIndex % _rowColors.Length]);
                
                DrawVerticalLines(timelineRect, totalWidth);

                foreach (var signalCallData in signal.Value)
                {
                    var firingTime = signalCallData.FiringTime;
                    var time = (float)firingTime.TotalSeconds;

                    var buttonX = time / _timer * totalWidth;
                    
                    var buttonWidth = Mathf.Max(50f,
                        GUI.skin.label.CalcSize(new GUIContent(time.ToString("F2") + " s")).x + 10f);

                    if (!(buttonX >= timelineRect.x) || !(buttonX <= timelineRect.x + totalWidth))
                    {
                        continue;
                    }
                    
                    if (GUI.Button(new Rect(buttonX, timelineRect.y, buttonWidth, SignalRowHeight - 10f),
                            time.ToString("F2") + " s"))
                    {
                        Debug.Log($"Signal {signal.Key} triggered at {time} seconds.");
                    }
                }

                EditorGUILayout.EndHorizontal();
                
                rowIndex++;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawVerticalLines(Rect timelineRect, float totalWidth)
        {
            Handles.color = _colorDividingLine;

            for (var t = 0f; t <= _timer; t += 0.5f)
            {
                var lineX = (t / _timer) * totalWidth;

                if (!(lineX >= timelineRect.x) || !(lineX <= timelineRect.x + totalWidth))
                {
                    continue;
                }

                var lineStart = new Vector3(lineX, timelineRect.y, 0f);
                var lineEnd = new Vector3(lineX, timelineRect.y + timelineRect.height, 0f);

                Handles.DrawLine(lineStart, lineEnd);
            }
        }
        
        private void HandleMouseScroll()
        {
            var e = Event.current;

            if (e.type != EventType.ScrollWheel)
            {
                return;
            }

            var newScaleValue = _scaleValue - e.delta.y * 0.05f;
            
            if (newScaleValue > MaxScaleValue)
            {
                _scaleValue = MaxScaleValue;
            }
            else if (newScaleValue < MinScaleValue)
            {
                _scaleValue = MinScaleValue;
            }
            else
            {
                _scaleValue = newScaleValue;
            }
            
            e.Use();
        }
    }
}