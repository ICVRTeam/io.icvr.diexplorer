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

using System.IO;
using UnityEngine;

namespace DiExplorer.Storages
{
    internal class DependenciesRepository
    {
        private const string FileName = "DiExplorerSaves.json";

        public void Save(string savedString)
        {
            File.WriteAllText(FileName, savedString);
        }
  
        public string Load()
        {
            if (File.Exists(FileName))
            {
                var jsonString = File.ReadAllText(FileName);
                return jsonString;
            }

            Debug.LogWarning
            (
                $"[{nameof(DependenciesRepository)}] " +
                $"An attempt to download data from a non-existent file! " +
                $"The file will be created when switching to Pay Mode."
            );
            
            return string.Empty;
        }

    }

}