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

using UnityEditor;
using UnityEngine;

namespace DiExplorer.Extensions
{
    public static class EditorIconExtension
    {
        private const string ScriptIconName = "cs Script Icon";
        private const string CameraIconName = "Camera Icon";
        private const string TestPassedIconName = "TestPassed";
        private const string ConsoleErrorIconName = "console.erroricon";
        private const string ZoomIconName = "d_ViewToolZoom";
        private const string WinCloseIconName = "d_winbtn_win_close";

        public static GUIContent ScriptIcon => EditorGUIUtility.IconContent(ScriptIconName);
        public static GUIContent CameraIcon => EditorGUIUtility.IconContent(CameraIconName);
        public static GUIContent TestPassedIcon => EditorGUIUtility.IconContent(TestPassedIconName);
        public static GUIContent ConsoleErrorIcon => EditorGUIUtility.IconContent(ConsoleErrorIconName);
        public static GUIContent ZoomIcon => EditorGUIUtility.IconContent(ZoomIconName);
        public static GUIContent WinCloseIcon => EditorGUIUtility.IconContent(WinCloseIconName);
    }
}