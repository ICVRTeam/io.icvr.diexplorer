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

using UnityEngine;

namespace DiContainerDebugger.Editor.Styles
{
    internal static class EditorElementStyle
    {
        public static readonly GUIStyle ListElementDefaultStyle = new GUIStyle(GUI.skin.box)
        {
            stretchWidth = true,
            fixedHeight = 20f,
            alignment = TextAnchor.MiddleLeft,
            imagePosition = ImagePosition.ImageLeft,
            hover = { textColor = new Color(0.42f, 0.78f, 1f) }, // при наведении курсора
            onNormal = { textColor = new Color(0.24f, 0.49f, 0.91f) }
        };
        
        public static readonly GUIStyle SelectedListElementStyle = new GUIStyle(ListElementDefaultStyle)
        {
            normal = { textColor = new Color(0.24f, 0.49f, 0.91f) }, // после нажатия
            onNormal = { textColor = new Color(0.24f, 0.49f, 0.91f) }
        };

        public static readonly GUIStyle BindingCountLabel = new GUIStyle(GUI.skin.box)
        {
            fixedHeight = 20f,
            fixedWidth = 50f
        };
        
        public static readonly GUIStyle ToggleDefaultStyle = new GUIStyle(GUI.skin.toggle)
        {
            stretchWidth = true,
            fixedHeight = 20f,
            alignment = TextAnchor.MiddleLeft,
            hover = { textColor = new Color(0.42f, 0.78f, 1f) }, // при наведении курсора
            onNormal = { textColor = new Color(0.24f, 0.49f, 0.91f) }
        };
    }
}