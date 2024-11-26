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
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DiExplorer.Extensions
{
    public static class InstanceInfoEditorExtension
    {
        private const string InstanceNamePattern = "[^.]+$";
        
        public static void ShowInProjectFile(IReadOnlyList<GUIContent> content, int index)
        {
            
            
            Debug.Log("Project " + content[index].tooltip);

            var instanceTypeName = content[index].tooltip;
            var match = RegexExtension.FindMatch(InstanceNamePattern, instanceTypeName);
            
            if (!match.Success)
            {
                Debug.LogWarning($"[{nameof(InstanceInfoEditorExtension)}] {content[index].tooltip} " +
                                 $"Not found from Project!");
                return;
            }
            
            var guids = AssetDatabase.FindAssets($"{match.Value} t:Script");

            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids.First());
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(InstanceInfoEditorExtension)}] {content[index].tooltip} " +
                                     $"Asset not found! ");
                }
            }
            else
            {
                Debug.LogWarning($"[{nameof(InstanceInfoEditorExtension)}] {content[index].tooltip} " +
                                 $"No objects of this type were found in the project!");
            }
        }

        public static void ShowInHierarchyInstance(IReadOnlyList<GUIContent> content, int index)
        {
            var instanceTypeName = content[index].tooltip;
            var match = RegexExtension.FindMatch(InstanceNamePattern, instanceTypeName);
            
            if (!match.Success)
            {
                Debug.LogWarning($"[{nameof(InstanceInfoEditorExtension)}] {content[index].tooltip} " +
                                 $"Not found from Project!");
                return;
            }
            
            var type = FindTypeByName(instanceTypeName);
        
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogWarning($"[{nameof(InstanceInfoEditorExtension)}] {content[index].tooltip} " +
                                 $"Type not found or not a Component!");
                return;
            }
            
            var components = Object.FindObjectsOfType(type) as Component[];

            if (components != null && components.Length > 0)
            {
                var gameObjects = components.Select(component => component.gameObject).ToArray();

                Selection.objects = gameObjects; // select object
            }
            else
            {
                Debug.LogWarning($"[{nameof(InstanceInfoEditorExtension)}] {content[index].tooltip} " +
                                 $"Object not found in the scene!");
            }
        }
        
        private static Type FindTypeByName(string typeName)
        {
            // Find type in the uploaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                
                if (type != null)
                {
                    return type;
                }
            }
            
            return null;
        }
    }
}