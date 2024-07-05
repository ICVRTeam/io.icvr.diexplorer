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
using DiExplorer.Data;
using DiExplorer.Entities;
using DiExplorer.Extensions;
using DiExplorer.Services;
using UnityEngine;

namespace DiExplorer.Editor.Panels
{
    internal class SignalsPanel
    {
        private const int Unselected = -1;
        
        private Dictionary<string, List<SignalCallData>> _signalCalls = new Dictionary<string, List<SignalCallData>>();

        private SubscriptionData[] _subscriptionData;
        private string[] _signalNames = Array.Empty<string>();

        private int _subscriptionsSelectedIndex = Unselected;
        private string _subscriptionsSearchString = string.Empty;


        private bool[] _signalsToggles = Array.Empty<bool>();
        private int _prevSignalCount;

        private DiExplorerService _diExplorerService;
        private SignalCallsCollector _signalCallsCollector;

        public Dictionary<string, List<SignalCallData>> SignalCalls => _signalCalls;
        public string SubscriptionsSearchString => _subscriptionsSearchString;
        public int SubscriptionsSelectedIndex => _subscriptionsSelectedIndex;

        public SignalsPanel(DiExplorerService diExplorerService, SignalCallsCollector signalCallsCollector)
        {
            _diExplorerService = diExplorerService;
            _signalCallsCollector = signalCallsCollector;
        }

        public void UpdateData()
        {
            _subscriptionData = _diExplorerService.GetAllSubscriptions();
        }
        
        public void UpdateDataByContainer(string containerName)
        {
            _subscriptionData = _diExplorerService.GetSubscriptions(containerName);
        }

        public IEnumerable<GUIContent> CreateSignalsContent()
        {
            _signalNames = _subscriptionData
                .Select(sub => sub.SignalTypeName)
                .Distinct()
                .ToArray();
            
            
            
            if (_signalNames.Length != _prevSignalCount)
            {
                _signalsToggles = new bool[_signalNames.Length];
                _prevSignalCount = _signalNames.Length;
                
                _signalCallsCollector.UpdateSignalCalls();
                
                
                _signalCalls = _signalCallsCollector.SignalCalls;
            }
            
            return _signalNames.Select(signalName => new GUIContent(signalName));
        }

        public IEnumerable<GUIContent> ProcessItemSelection()
        {
            var signalSubscriptions = new List<string>();
            for (var i = 0; i < _signalsToggles.Length; i++)
            {
                if (_signalsToggles[i])
                {
                    var selectedSignalName = _signalNames[i];
                    var subscriptions = _subscriptionData.Where(data => data.SignalTypeName == selectedSignalName).Select(data => data.TypeName);

                    signalSubscriptions.AddRange(subscriptions);
                }
            }

            var subscriptionsContent = new List<GUIContent>();
            
            if (signalSubscriptions.Count != 0)
            {
                foreach (var instance in signalSubscriptions)
                {
                    var pattern = _subscriptionsSearchString;
                    if (RegexExtension.IsContainMatch(pattern, instance))
                    {
                        subscriptionsContent.Add(new GUIContent(instance));
                    }
                }
            }

            return subscriptionsContent;
        }

        public void ClearSignalCalls()
        {
            _signalCalls.Clear();
        }

        public bool GetSignalToggleValue(int index)
        {
            return _signalsToggles.Length > 0 && _signalsToggles[index];
        }

        public void SetSelectedSubscriptionIndex(int index)
        {
            _subscriptionsSelectedIndex = index;
        }

        public void SetSearchString(string searchString)
        {
            _subscriptionsSearchString = searchString;
        }

        public void SetSignalToggleValue(int index, bool value)
        {
            if (_signalsToggles.Length > 0)
                _signalsToggles[index] = value;
        }
    }
}