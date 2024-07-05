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

namespace DiExplorer.Extensions
{
    internal static class DictionaryExtension
    {
        public static T[] GetData<T>(Dictionary<string, T[]> dictionary, string containerName)
        {
            var data = dictionary.TryGetValue(containerName, out var value)
                ? value
                : Array.Empty<T>();
            return data;
        }

        public static T[] GetAllData<T>(Dictionary<string, T[]> dictionary)
        {
            var dataList = new List<T>();

            foreach (var containerName in dictionary.Keys)
            {
                var data = GetData(dictionary, containerName);
                dataList.AddRange(data);
            }

            return dataList.ToArray();
        }

        public static void SetDataArray<T>(Dictionary<string, T[]> dictionary, string containerName, T[] data)
        {
            var isContains = dictionary.ContainsKey(containerName);

            if (isContains)
            {
                dictionary[containerName] = data;
            }
            else
            {
                dictionary.Add(containerName, data);
            }
        }

        public static void AddData<T>(Dictionary<string, List<T>> dictionary, string key, T data)
        {
            var isContains = dictionary.ContainsKey(key);

            if (isContains)
            {
                dictionary[key].Add(data);
            }
            else
            {
                dictionary.Add(key, new List<T> { data });
            }
        }
    }
}