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
using System.Reflection;
using DiContainerDebugger.Scripts;
using DiExplorer.Data;
using DiExplorer.Entities;
using DiExplorer.Interfaces;
using UnityEngine;
using Zenject;

namespace DiExplorer.Containers
{
    internal abstract class AbstractContainer : IInstancesProvider, IBindingsProvider, ISignalsProvider
    {
        public abstract string ContainerName { get; }
        protected SignalBus SignalBusInstance { get; }

        protected abstract string GetContainerName();
        protected abstract Context GetContext();

        protected AbstractContainer(SignalBus signalBusInstance)
        {
            SignalBusInstance = signalBusInstance;
        }
        
        public SignalBus GetSignalBus()
        {
            var context = GetContext();
            var signalBus = context.Container.Resolve<SignalBus>();

            return signalBus;
        }
        
        public virtual BindingData[] GetBindings()
        {
            var context = GetContext();
            var contextName = GetContainerName();
            var bindedClassTypes = context.Container.AllContracts.Select(id => id.Type).ToArray();
            var bindedClassDataList = new List<BindingData>();
            
            foreach (var classType in bindedClassTypes)
            {
                bindedClassDataList.Add(GetInjectables(contextName, classType));
            }

            return bindedClassDataList.ToArray();
        }

        public virtual InstanceData[] GetInstances()
        {
            var context = GetContext();
            var contextName = GetContainerName();
            var providers = context.Container.AllProviders;
            var instancesDataList = new List<InstanceData>();

            foreach (var provider in providers)
            {
                FieldInfo instanceField = provider.GetType().GetField("_instances", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (instanceField != null)
                {
                    var instances = (List<object>)instanceField.GetValue(provider);
                    if (instances != null)
                    {
                        foreach (var instance in instances)
                        {
                            var instanceType = instance.GetType();
                            var typeInfo = TypeAnalyzer.TryGetInfo(instanceType);
                            
                            if (typeInfo == null)
                            {
                                continue;
                            }
                            
                            var injectables = typeInfo.AllInjectables.ToArray();
                            var injectableTypes = new List<string>();
                            
                            if (injectables.Any())
                            {
                               injectableTypes.AddRange(injectables.Select(info => info.MemberType.ToString()));
                            }
                            
                            //This get binded MonoBehaviours class 
                            if (instanceType.BaseType == typeof(MonoBehaviour))
                            {
                                instancesDataList.Add(new InstanceData(contextName, instanceType.ToString(), InstanceType.DynamicMono, injectableTypes.ToArray()));
                            }
                            else
                            {
                                instancesDataList.Add(new InstanceData(contextName, instanceType.ToString(), InstanceType.NoMono, injectableTypes.ToArray()));
                            }
                        }
                    }
                }
            }

            return instancesDataList.ToArray();
        }

        private BindingData GetInjectables(string contextName, Type classType)
        {
            var injectablesTypeList = new List<Type>();
            var isMonoBehaviour = classType.BaseType == typeof(MonoBehaviour);
            var typeInfo = TypeAnalyzer.TryGetInfo(classType);
            
            if (typeInfo == null)
            {
                return new BindingData(contextName, classType.ToString(), injectablesTypeList.ToArray(), isMonoBehaviour);
            }
            
            var injectables = typeInfo.AllInjectables.ToArray();

            if (!injectables.Any())
            {
                return new BindingData(contextName, classType.ToString(), injectablesTypeList.ToArray(), isMonoBehaviour);
            }
            
            var injectableTypes = injectables.Select(info => info.MemberType).ToArray(); // get injectable types

            foreach (var injectableType in injectableTypes)
            {
                if (injectableType == classType)
                {
                    break;
                }
                
                injectablesTypeList.Add(injectableType);
            }

            return new BindingData(contextName, classType.ToString(), injectablesTypeList.ToArray(), isMonoBehaviour);
        }

        public virtual SubscriptionData[] GetSubscriptions()
        {
            var context = GetContext();
            var signalBus = context.Container.Resolve<SignalBus>();
            var subscriptionDataList = new List<SubscriptionData>();
            
            FieldInfo subscriptionMapField = signalBus.GetType().GetField("_subscriptionMap", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (subscriptionMapField != null)
            {
                var subscriptionsDictionary = (Dictionary<SignalSubscriptionId, SignalSubscription>)subscriptionMapField.GetValue(signalBus);
                
                foreach (var subscription in subscriptionsDictionary)
                {
                    Type targetType;
                    try
                    {
                        var callbackAction = (Action<object>)subscription.Key.Callback;
                        FieldInfo objectTypeField = callbackAction.Target.GetType().GetField("_objectType", BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        if (objectTypeField != null)
                        {
                            targetType = (Type)objectTypeField.GetValue(callbackAction.Target);
                        }
                        else
                        {
                            throw new Exception("ObjectTypeField not Found!");
                        }
                    }
                    catch (Exception e)
                    {
                        var exception = e;
                        var callbackDelegate = (Delegate)subscription.Key.Callback;
                        targetType = callbackDelegate.Target.GetType();
                    }

                    var signalType = subscription.Key.SignalId.Type;
                    var ignoredType = typeof(SignalCallsCollector); // Ignore Fake Subscriptions
                    
                    if (targetType.ToString() != ignoredType.ToString())
                    {
                        subscriptionDataList.Add(new SubscriptionData(ContainerName,
                            targetType.ToString(), signalType.ToString()));
                    }
                }
            }

            return subscriptionDataList.ToArray();
        }

        public virtual SignalData[] GetSignals(SubscriptionData[] subscriptionsData)
        {
            var context = GetContext();
            var signalBus = context.Container.Resolve<SignalBus>();
            
            var signalDataList = new List<SignalData>();
            
            FieldInfo declarationMapField = signalBus.GetType().GetField("_localDeclarationMap", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (declarationMapField != null)
            {
                var signalsDictionary = (Dictionary<BindingId, SignalDeclaration>)declarationMapField.GetValue(signalBus);
                foreach (var signal in signalsDictionary)
                {
                    var signalType = signal.Key.Type;
                    var signalSubscriptions = subscriptionsData
                        .Where(data => data.SignalTypeName == signalType.ToString())
                        .ToArray();
                    
                    signalDataList.Add(new SignalData(ContainerName, signalType.ToString(), signalType, signalSubscriptions));
                }
            }

            return signalDataList.ToArray();
        }
    }
}