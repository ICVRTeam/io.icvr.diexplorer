﻿// ICVR CONFIDENTIAL
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
using DiExplorer.Extensions;

namespace DiExplorer.Entities
{
    internal class DiExplorerModel
    {
        private readonly Dictionary<string, BindingData[]> _containerBindings = new Dictionary<string, BindingData[]>();
        private readonly Dictionary<string, InstanceData[]> _containerInstances = new Dictionary<string, InstanceData[]>();

        private readonly Dictionary<string, SignalData[]> _containerSignals = new Dictionary<string, SignalData[]>();
        private readonly Dictionary<string, SubscriptionData[]> _containerSubscriptions = new Dictionary<string, SubscriptionData[]>();
        private readonly Dictionary<string, SignalCallData[]> _signalCalls = new Dictionary<string, SignalCallData[]>();
        private readonly Dictionary<string, InheritorsData> _inheritors = new Dictionary<string, InheritorsData>();

        public string[] GetContainerNames()
        {
            return _containerBindings.Keys.ToArray();
        }
        
        public BindingData[] GetBindings(string containerName)
        {
            return DictionaryExtension.GetData(_containerBindings, containerName);
        }

        public InstanceData[] GetInstances(string containerName)
        {
            return DictionaryExtension.GetData(_containerInstances, containerName);
        }

        public SignalData[] GetSignals(string containerName)
        {
            return DictionaryExtension.GetData(_containerSignals, containerName);
        }
        
        public SubscriptionData[] GetSubscriptions(string containerName)
        {
            return DictionaryExtension.GetData(_containerSubscriptions, containerName);
        }

        public SignalCallData[] GetSignalCalls(string signalTypeName)
        {
            return DictionaryExtension.GetData(_signalCalls, signalTypeName);
        }

        public InheritorsData GetInheritors(string baseType)
        {
            if (_inheritors.TryGetValue(baseType, out var inheritorsData))
            {
                return inheritorsData;
            }

            return new InheritorsData(baseType, Array.Empty<Type>());
        }

        public BindingData[] GetAllBindings()
        {
            return DictionaryExtension.GetAllData(_containerBindings);
        }

        public InstanceData[] GetAllInstances()
        {
            return DictionaryExtension.GetAllData(_containerInstances);
        }

        public SignalData[] GetAllSignals()
        {
            return DictionaryExtension.GetAllData(_containerSignals);
        }
        
        public SubscriptionData[] GetAllSubscriptions()
        {
            return DictionaryExtension.GetAllData(_containerSubscriptions);
        }

        public SignalCallData[] GetAllSignalCalls()
        {
            return DictionaryExtension.GetAllData(_signalCalls);
        }

        public Dictionary<string, SignalCallData[]> GetSignalCallDictionary()
        {
            return new Dictionary<string, SignalCallData[]>(_signalCalls);
        }

        public string[] GetInjectableNamesFromInstance(string instanceTypeName)
        {
            var injectablesTypeName = _containerInstances
                .SelectMany(container => container.Value)
                .First(instanceData => instanceData.TypeName == instanceTypeName)
                .InjectablesTypeName;
            
;           return injectablesTypeName;
        }

        public string[] GetInstanceNamesFromBinding(string bindingTypeName)
        {
            var instances = _containerInstances
                .SelectMany(container => container.Value)
                .Where(instanceData => instanceData.InjectablesTypeName.Contains(bindingTypeName))
                .Select(data => data.TypeName);

            return instances.ToArray();
        }
        
        public string[] GetSignalNamesFromSubscription(string subscriptionTypeName)
        {
            var signalsTypeName = _containerSignals
                .SelectMany(container => container.Value)
                .Where(signalData => signalData.Subscriptions.Any(sub => sub.TypeName == subscriptionTypeName))
                .Select(data => data.TypeName);
            
            return signalsTypeName.ToArray();
        }
        
        public string[] GetSubscriptionNamesFromSignal(string signalTypeName)
        {
            var subscriptionsTypeName = _containerSubscriptions
                .SelectMany(container => container.Value)
                .Where(subscriptionData => subscriptionData.SignalTypeName == signalTypeName)
                .Select(data => data.TypeName);

            return subscriptionsTypeName.ToArray();
        }
        
        public int GetInheritorsCountByClass(string baseClass)
        {
            return _inheritors.TryGetValue(baseClass, out var inheritor) 
                ? inheritor.Inheritors.Length 
                : 0;
        }

        public void SetBindings(string containerName, BindingData[] bindings)
        {
            DictionaryExtension.SetDataArray(_containerBindings, containerName, bindings);
        }

        public void SetInstances(string containerName, InstanceData[] instances)
        {
            DictionaryExtension.SetDataArray(_containerInstances, containerName, instances);
        }

        public void SetSignals(string containerName, SignalData[] signals)
        {
            DictionaryExtension.SetDataArray(_containerSignals, containerName, signals);
        }

        public void SetSubscriptions(string containerName, SubscriptionData[] subscriptions)
        {
            if (containerName != "Project")
            {
                var result = _containerSubscriptions["Project"]
                    .Where(subscriptionData => subscriptions.All(data => subscriptionData.TypeName != data.TypeName))
                    .ToArray();
                _containerSubscriptions["Project"] = result;
            }
            
            DictionaryExtension.SetDataArray(_containerSubscriptions, containerName, subscriptions);
        }
        
        public void SetSignalCalls(Dictionary<string, SignalCallData[]> signalCalls)
        {
            _signalCalls.Clear();
            
            foreach (var signalCall in signalCalls)
            {
                _signalCalls.Add(signalCall.Key, signalCall.Value);
            }
        }
        
        public void SetInheritors(Dictionary<string, InheritorsData> inheritorsDictionary)
        {
            _inheritors.Clear();

            foreach (var pair in inheritorsDictionary)
            {
                _inheritors.Add(pair.Key, pair.Value);
            }
        }

        internal BindingsSavedData GetBindingsSavedData()
        {
            var savedData = new BindingsSavedData(_containerBindings, _containerInstances, _inheritors);

            return savedData;
        }
        
        internal SignalsSavedData GetSignalsSavedData()
        {
            var savedData = new SignalsSavedData(_containerSignals, _containerSubscriptions, _signalCalls);

            return savedData;
        }

        public void SetSavedData(BindingsSavedData bindingsSavedData, SignalsSavedData signalsSavedData)
        {
            _containerBindings.Clear();
            _containerInstances.Clear();
            _inheritors.Clear();
            
            _containerSignals.Clear();
            _containerSubscriptions.Clear();
            _signalCalls.Clear();

            foreach (var containerBinding in bindingsSavedData.ContainerBindings)
            {
                _containerBindings.Add(containerBinding.Key, containerBinding.Value);
            }

            foreach (var containerInstance in bindingsSavedData.ContainerInstances)
            {
                _containerInstances.Add(containerInstance.Key, containerInstance.Value);
            }

            foreach (var inheritors in bindingsSavedData.Inheritors)
            {
                _inheritors.Add(inheritors.Key, inheritors.Value);
            }

            foreach (var containerSignal in signalsSavedData.ContainerSignals)
            {
                _containerSignals.Add(containerSignal.Key, containerSignal.Value);
            }

            foreach (var containerSubscription in signalsSavedData.ContainerSubscriptions)
            {
                _containerSubscriptions.Add(containerSubscription.Key, containerSubscription.Value);   
            }

            foreach (var signalCall in signalsSavedData.SignalCalls)
            {
                _signalCalls.Add(signalCall.Key, signalCall.Value);
            }
        }
    }
}