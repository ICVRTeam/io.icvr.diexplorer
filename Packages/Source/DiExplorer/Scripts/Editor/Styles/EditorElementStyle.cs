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
        private static readonly Color HoverColor = new Color(0.42f, 0.78f, 1f);
        private static readonly Color SelectedColor = new Color(0.24f, 0.49f, 0.91f);
        private static readonly Color InfoColor = Color.yellow;
        
        public static readonly GUIStyle ListElementDefaultStyle = new GUIStyle(GUI.skin.box)
        {
            stretchWidth = true,
            fixedHeight = 20f,
            alignment = TextAnchor.MiddleLeft,
            imagePosition = ImagePosition.ImageLeft,
            hover = { textColor = HoverColor }, // When hovering over the cursor
            onNormal = { textColor = SelectedColor },
            onHover = { textColor = SelectedColor }
        };
        
        public static readonly GUIStyle SelectedListElementStyle = new GUIStyle(ListElementDefaultStyle)
        {
            hover = { textColor = SelectedColor },
            normal = { textColor = SelectedColor } // After clicking
        };
        
        public static readonly GUIStyle InfoListElementStyle = new GUIStyle(ListElementDefaultStyle)
        {
            hover = { textColor = InfoColor },
            normal = { textColor = InfoColor }
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
            hover = { textColor = HoverColor },
            onNormal = { textColor = SelectedColor },
            onHover = { textColor = SelectedColor }
        };
        
#if UNITY_2022_3_OR_NEWER
        public static readonly GUIStyle ToolbarSearchTextField = GUI.skin.FindStyle("ToolbarSearchTextField");
        public static readonly GUIStyle ToolbarSearchCancelButton = GUI.skin.FindStyle("ToolbarSearchCancelButton");
#else
        public static readonly GUIStyle ToolbarSearchTextField = GUI.skin.FindStyle("ToolbarSeachTextField");
        public static readonly GUIStyle ToolbarSearchCancelButton = GUI.skin.FindStyle("ToolbarSeachCancelButton");
#endif
    }
}