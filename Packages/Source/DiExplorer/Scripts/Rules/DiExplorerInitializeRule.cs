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
using DiExplorer.Entities;
using DiExplorer.Services;
using Zenject;

namespace DiExplorer.Rules
{
    internal class DiExplorerInitializeRule : IInitializable, IDisposable
    {
        private IDisposable _disposableUpdate;
        
        private DiExplorerService _diExplorerService;
        private SignalCallsCollector _signalCallsCollector;

        public DiExplorerInitializeRule(
            DiExplorerService diExplorerService, 
            SignalCallsCollector signalCallsCollector)
        {
            _diExplorerService = diExplorerService;
            _signalCallsCollector = signalCallsCollector;
        }

        public void Initialize()
        {
            _diExplorerService.UpdateExploreData();
            _diExplorerService.StartPlayModeTimer();
            _signalCallsCollector.UpdateSignalCalls();
        }

        public void Dispose()
        {
            _disposableUpdate?.Dispose();
        }
    }
}