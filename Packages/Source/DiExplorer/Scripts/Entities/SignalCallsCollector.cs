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
using DiExplorer.Containers;
using DiExplorer.Data;
using DiExplorer.Extensions;
using DiExplorer.Services;
using UnityEditor;
using Zenject;

namespace DiExplorer.Entities
{
    internal class SignalCallsCollector
    {
        private Dictionary<string, List<SignalCallData>> _signalCalls = new Dictionary<string, List<SignalCallData>>();
        
        private SignalData[] _signalsData;
        private SignalBus _signalBus;
        private int _prevSignalCount;

        private DiExplorerService _diExplorerService;
        private List<AbstractContainer> _abstractContainers;

        public Dictionary<string, List<SignalCallData>> SignalCalls => _signalCalls;

        public SignalCallsCollector(DiExplorerService diExplorerService)
        {
            _diExplorerService = diExplorerService;
        }

        public void UpdateSignalCalls()
        {
            _signalsData = _diExplorerService.GetAllSignals();
            
            if (_signalsData.Length != _prevSignalCount)
            {
                _prevSignalCount = _signalsData.Length;
                
                if (EditorApplication.isPlaying)
                {
                    _abstractContainers = _diExplorerService.GetContainers();
                    UnsubscribeFakeSubscriptions();
                    SubscribeFakeSubscriptions();
                }
                else
                {
                    var savedDictionary = _diExplorerService.GetSignalCallDictionary();
                    var signalCallDictionary = savedDictionary.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToList());
                    
                    _signalCalls = signalCallDictionary;
                }
            }
        }

        private void SubscribeFakeSubscriptions()
        {
            if (_signalsData == null) return;
            
            foreach (var signal in _signalsData)
            {
                foreach (var abstractContainer in _abstractContainers)
                {
                    _signalBus = abstractContainer.GetSignalBus();

                    if (_signalBus.IsSignalDeclared(signal.Type))
                    {
                        _signalBus.Subscribe(signal.Type, ProcessSignalCall);
                        break;
                    }
                }
            }
        }

        private void UnsubscribeFakeSubscriptions()
        {
            foreach (var abstractContainer in _abstractContainers)
            {
                _signalBus = abstractContainer.GetSignalBus();
                
                foreach (var signal in _signalsData)
                {
                    _signalBus?.TryUnsubscribe(signal.Type, ProcessSignalCall);
                }
            }
        }

        private void ProcessSignalCall(object signal)
        {
            var signalTypeName = signal.GetType().ToString();
            var timeSpan = _diExplorerService.GetElapsedTimePlayModeTimer();
            var signalCallData = new SignalCallData("", signalTypeName, timeSpan);
            
            DictionaryExtension.AddData(_signalCalls, timeSpan.ToString(), signalCallData);

            var savedCalls = _signalCalls.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray());
            
            _diExplorerService.SetSignalCalls(savedCalls);
            
            //Debug.LogWarning($@"Signal received: {signalTypeName} Time: {timeSpan:mm\:ss\.fff}");
        }
    }
}