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
using DiExplorer.Containers;
using DiExplorer.Data;
using DiExplorer.Entities;
using DiExplorer.Interfaces;
using DiExplorer.Storages;

namespace DiExplorer.Services
{
    internal class DiExplorerService
    {
        private List<AbstractContainer> _abstractContainers;
        private DiExplorerModel _diExplorerModel;
        private DependenciesRepository _dependenciesRepository;
        private PlayModeTimer _playModeTimer;
        private ISerializator _serializator;

        internal DiExplorerService(
            List<AbstractContainer> abstractContainers,
            DiExplorerModel diExplorerModel, 
            DependenciesRepository dependenciesRepository,
            PlayModeTimer playModeTimer,
            ISerializator serializator)
        {
            _abstractContainers = abstractContainers;
            _diExplorerModel = diExplorerModel;
            _dependenciesRepository = dependenciesRepository;
            _playModeTimer = playModeTimer;
            _serializator = serializator;
        }

        internal void UpdateExploreData()
        {
            foreach (var abstractContainer in _abstractContainers)
            {
                var bindings = abstractContainer.GetBindings();
                _diExplorerModel.SetBindings(abstractContainer.ContainerName, bindings);
            
                var instances = abstractContainer.GetInstances();
                _diExplorerModel.SetInstances(abstractContainer.ContainerName, instances);

                var subscriptions = abstractContainer.GetSubscriptions();
                _diExplorerModel.SetSubscriptions(abstractContainer.ContainerName, subscriptions);

                var signals = abstractContainer.GetSignals(subscriptions);
                _diExplorerModel.SetSignals(abstractContainer.ContainerName, signals);
            }
        }

        internal List<AbstractContainer> GetContainers()
        {
            return _abstractContainers;
        }

        internal void StartPlayModeTimer()
        {
            _playModeTimer.StartTimer();
        }

        internal void SetSignalCalls(Dictionary<string, SignalCallData[]> signalCalls)
        {
            _diExplorerModel.SetSignalCalls(signalCalls);
        }

        internal void SaveModel()
        {
            var savedData = _diExplorerModel.GetSavedData();
            var savedString = _serializator.Serialize(savedData);
            _dependenciesRepository.Save(savedString);
        }

        internal void LoadModel()
        {
            var stringData = _dependenciesRepository.Load();
            var savedData = _serializator.Deserialize(stringData);
            _diExplorerModel.SetSavedData(savedData);
        }

        public BindingData[] GetBindings(string containerName)
        {
            return _diExplorerModel.GetBindings(containerName);
        }

        public InstanceData[] GetInstances(string containerName)
        {
            return _diExplorerModel.GetInstances(containerName);
        }

        public SignalData[] GetSignals(string containerName)
        {
            return _diExplorerModel.GetSignals(containerName);
        }

        public SubscriptionData[] GetSubscriptions(string containerName)
        {
            return _diExplorerModel.GetSubscriptions(containerName);
        }

        public BindingData[] GetAllBindings()
        {
            return _diExplorerModel.GetAllBindings();
        }

        public InstanceData[] GetAllInstances()
        {
            return _diExplorerModel.GetAllInstances();
        }

        public SignalData[] GetAllSignals()
        {
            return _diExplorerModel.GetAllSignals();
        }

        public SubscriptionData[] GetAllSubscriptions()
        {
            return _diExplorerModel.GetAllSubscriptions();
        }

        public SignalCallData[] GetAllSignalCalls()
        {
            return _diExplorerModel.GetAllSignalCalls();
        }

        public Dictionary<string, SignalCallData[]> GetSignalCallDictionary()
        {
            return _diExplorerModel.GetSignalCallDictionary();
        }

        public string[] GetContainerNames()
        {
            return _diExplorerModel.GetContainerNames();
        }

        public string[] GetInjectableNamesFromInstance(string instanceTypeName)
        {
            return _diExplorerModel.GetInjectableNamesFromInstance(instanceTypeName);
        }

        public string[] GetInstanceNamesFromBinding(string bindingTypeName)
        {
            return _diExplorerModel.GetInstanceNamesFromBinding(bindingTypeName);
        }

        public string[] GetSignalNamesFromSubscription(string subscriptionTypeName)
        {
            return _diExplorerModel.GetSignalNamesFromSubscription(subscriptionTypeName);
        }

        public string[] GetSubscriptionNamesFromSignal(string signalTypeName)
        {
            return _diExplorerModel.GetSubscriptionNamesFromSignal(signalTypeName);
        }

        public TimeSpan GetElapsedTimePlayModeTimer()
        {
            return _playModeTimer.GetElapsedTime();
        }
    }
}