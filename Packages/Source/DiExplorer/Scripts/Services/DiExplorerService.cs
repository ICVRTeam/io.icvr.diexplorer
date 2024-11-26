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
using DiExplorer.Extensions;
using DiExplorer.Interfaces;
using DiExplorer.Storages;

namespace DiExplorer.Services
{
    internal class DiExplorerService
    {
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
            var bindingsSavedData = _diExplorerModel.GetBindingsSavedData();
            var savedBindingsString = _serializator.Serialize(bindingsSavedData);
            _fileDataManager.Save(CashFileConst.BindingsFileName, savedBindingsString);
            
            var signalsSavedData = _diExplorerModel.GetSignalsSavedData();
            var savedSignalsString = _serializator.Serialize(signalsSavedData);
            _fileDataManager.Save(CashFileConst.SignalsFileName, savedSignalsString);
        }

        internal void LoadModel()
        {
            var bindingsData = LoadBindingsData();
            var signalsData = LoadSignalsData();

            _diExplorerModel.SetSavedData(bindingsData, signalsData);
        }

        private BindingsSavedData LoadBindingsData()
        {
            var stringBindingsData = _fileDataManager.Load(CashFileConst.BindingsFileName);
            var savedDataBindings = _serializator.Deserialize<BindingsSavedData>(stringBindingsData);

            var containerBindings = new Dictionary<string, BindingData[]>();
            var containerInstances = new Dictionary<string, InstanceData[]>();
            var inheritors = new Dictionary<string, InheritorsData>();

            if (savedDataBindings.ContainerBindings != null)
            {
                containerBindings = savedDataBindings.ContainerBindings;
            }

            if (savedDataBindings.ContainerInstances != null)
            {
                containerInstances = savedDataBindings.ContainerInstances;
            }

            if (savedDataBindings.Inheritors != null)
            {
                inheritors = savedDataBindings.Inheritors;
            }
            
            return new BindingsSavedData(containerBindings, containerInstances, inheritors);
        }

        private SignalsSavedData LoadSignalsData()
        {
            var stringSignalsData = _fileDataManager.Load(CashFileConst.SignalsFileName);
            var savedDataSignals = _serializator.Deserialize<SignalsSavedData>(stringSignalsData);

            var containerSignals = new Dictionary<string, SignalData[]>();
            var containerSubscriptions = new Dictionary<string, SubscriptionData[]>();
            var signalCalls = new Dictionary<string, SignalCallData[]>();
            
            if (savedDataSignals.ContainerSignals != null)
            {
                containerSignals = savedDataSignals.ContainerSignals;
            }

            if (savedDataSignals.ContainerSubscriptions != null)
            {
                containerSubscriptions = savedDataSignals.ContainerSubscriptions;
            }

            if (savedDataSignals.SignalCalls != null)
            {
                signalCalls = savedDataSignals.SignalCalls;
            }

            return new SignalsSavedData(containerSignals, containerSubscriptions, signalCalls);
        }

        public void DeleteBindingsSaveFiles()
        {
            _fileDataManager.DeleteCashFile(CashFileConst.BindingsFileName);
            _fileDataManager.DeleteCashFile(CashFileConst.SceneComponentsFileName);
        }
        
        public void DeleteSignalsSaveFiles()
        {
            _fileDataManager.DeleteCashFile(CashFileConst.SignalsFileName);
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