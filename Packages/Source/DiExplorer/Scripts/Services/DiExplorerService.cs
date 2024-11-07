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
        private const string DiExplorerFileName = "DiExplorerSaves.json";
        
        private List<AbstractContainer> _abstractContainers;
        private InheritorsStorage _inheritorsStorage;
        private DiExplorerModel _diExplorerModel;
        private FileDataManager _fileDataManager;
        private PlayModeTimer _playModeTimer;
        private SceneComponentsCollector _sceneComponentsCollector;
        private ISerializator _serializator;

        internal DiExplorerService(
            List<AbstractContainer> abstractContainers,
            InheritorsStorage inheritorsStorage, 
            DiExplorerModel diExplorerModel,
            FileDataManager fileDataManager,
            PlayModeTimer playModeTimer,
            SceneComponentsCollector sceneComponentsCollector,
            ISerializator serializator)
        {
            _abstractContainers = abstractContainers;
            _inheritorsStorage = inheritorsStorage;
            _diExplorerModel = diExplorerModel;
            _fileDataManager = fileDataManager;
            _playModeTimer = playModeTimer;
            _sceneComponentsCollector = sceneComponentsCollector;
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

                var inheritors = _inheritorsStorage.GetInheritorsDictionary();
                _diExplorerModel.SetInheritors(inheritors);
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
            _fileDataManager.Save(DiExplorerFileName, savedString);
        }

        internal void LoadModel()
        {
            var stringData = _fileDataManager.Load(DiExplorerFileName);
            var savedData = _serializator.Deserialize<SavedData>(stringData);

            var containerBindings = new Dictionary<string, BindingData[]>();
            var containerInstances = new Dictionary<string, InstanceData[]>();
            var containerSignals = new Dictionary<string, SignalData[]>();
            var containerSubscriptions = new Dictionary<string, SubscriptionData[]>();
            var signalCalls = new Dictionary<string, SignalCallData[]>();
            var inheritors = new Dictionary<string, InheritorsData>();

            if (savedData.ContainerBindings != null)
            {
                containerBindings = savedData.ContainerBindings;
            }

            if (savedData.ContainerInstances != null)
            {
                containerInstances = savedData.ContainerInstances;
            }

            if (savedData.ContainerSignals != null)
            {
                containerSignals = savedData.ContainerSignals;
            }

            if (savedData.ContainerSubscriptions != null)
            {
                containerSubscriptions = savedData.ContainerSubscriptions;
            }

            if (savedData.SignalCalls != null)
            {
                signalCalls = savedData.SignalCalls;
            }

            if (savedData.Inheritors != null)
            {
                inheritors = savedData.Inheritors;
            }

            var newSavedData = new SavedData(containerBindings, containerInstances, containerSignals,
                containerSubscriptions, signalCalls, inheritors);
            
            _diExplorerModel.SetSavedData(newSavedData);
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

        internal InheritorsData GetInheritors(string baseType)
        {
            return _diExplorerModel.GetInheritors(baseType);
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

        public int GetInheritorsCountByClass(string baseClass)
        {
            return _diExplorerModel.GetInheritorsCountByClass(baseClass);
        }

        public TimeSpan GetElapsedTimePlayModeTimer()
        {
            return _playModeTimer.GetElapsedTime();
        }

        public void CollectSceneInstances()
        {
            _sceneComponentsCollector.CollectAllSceneComponents();
        }
    }
}