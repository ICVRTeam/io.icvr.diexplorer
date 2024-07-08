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
using DiContainerDebugger.Scripts;
using Unity.Plastic.Newtonsoft.Json;

namespace DiExplorer.Data
{
    [Serializable]
    public struct InstanceData
    {
        public string ParentContextName { get; }
        public string TypeName { get; }
        public InstanceType InstanceType { get; }
        public string[] InjectablesTypeName { get; }

        [JsonConstructor]
        public InstanceData(string parentContextName, string typeName, InstanceType instanceType, string[] injectablesTypeName)
        {
            ParentContextName = parentContextName;
            TypeName = typeName;
            InstanceType = instanceType;
            InjectablesTypeName = injectablesTypeName;
        }
    }
}