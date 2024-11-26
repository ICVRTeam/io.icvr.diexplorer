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
    internal class FileDataManager
    {
        public void Save(string filePath, string savedString)
        {
            File.WriteAllText(filePath, savedString);
        }
  
        public string Load(string filePath)
        {
            if (File.Exists(filePath))
            {
                var jsonString = File.ReadAllText(filePath);
                return jsonString;
            }

            Debug.LogWarning
            (
                $"[{nameof(FileDataManager)}] " +
                $"An attempt to download data from a non-existent file: {filePath}! " +
                $"The file will be created when switching to Play Mode."
            );
            
            return string.Empty;
        }

        public void DeleteCashFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }

}