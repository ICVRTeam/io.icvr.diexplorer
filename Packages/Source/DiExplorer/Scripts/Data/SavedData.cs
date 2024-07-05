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

namespace DiExplorer.Data
{
    internal struct SavedData
    {
        public Dictionary<string, BindingData[]> ContainerBindings;
        public Dictionary<string, InstanceData[]> ContainerInstances;
        public Dictionary<string, SignalData[]> ContainerSignals;
        public Dictionary<string, SubscriptionData[]> ContainerSubscriptions;
        public Dictionary<string, SignalCallData[]> SignalCalls;

        public SavedData(
            Dictionary<string, BindingData[]> containerBindings,
            Dictionary<string, InstanceData[]> containerInstances, 
            Dictionary<string, SignalData[]> containerSignals,
            Dictionary<string, SubscriptionData[]> containerSubscriptions,
            Dictionary<string, SignalCallData[]> signalCalls)
        {
            ContainerBindings = containerBindings;
            ContainerInstances = containerInstances;
            ContainerSignals = containerSignals;
            ContainerSubscriptions = containerSubscriptions;
            SignalCalls = signalCalls;
        }
    }
}