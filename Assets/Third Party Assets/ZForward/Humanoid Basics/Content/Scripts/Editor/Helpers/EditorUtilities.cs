/*
 * EditorUtilities.cs - ZForward
 * Helper Script for Custom Editors.
 * @version: 1.0.0
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Humanoid_Basics.Editor.Helpers
{
    public static class EditorUtilities
    {
        // Cache Colors
        private static readonly Color BaseColor = new Color(0.1f, 0.1f, 0.1f, 0f);
        private static readonly Color HoverColor = new Color(0.6f, 0.6f, 0.6f, 0.2f);

        // Render a basic icon + label field.
        public static void IconLabelField(string title, string iconName, GUIStyle style)
        {
            EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(" " + title, iconName),
                style);
        }

        // Render an image
        public static void ImageField(string image)
        {
            GUILayout.Label(Resources.Load(image) as Texture, EditorStyles.largeLabel);
        }

        // Render a custom dropdown menu.
        public static void DropdownField(string title, string iconName, SerializedProperty property)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                property.isExpanded = RenderDropdownHeader(GUILayoutUtility.GetRect(1, 20f),
                    EditorGUIUtility.TrTextContentWithIcon(" " + title, iconName),
                    true, property.isExpanded);

                if (property.isExpanded)
                {
                    EditorGUILayout.Space(3f);
                    RenderDropdownProperties(property, 20f);
                }
            }
            EditorGUILayout.EndVertical();
        }
        
        // Render the dropdown header label.
        public static bool RenderDropdownHeader(Rect rect, GUIContent content, bool hoverable, bool state)
        {
            // Set base color
            var color = BaseColor;

            // Icon rect
            var foldoutRect = rect;
            foldoutRect.y += 4f;
            foldoutRect.x += 2f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            // Label rect
            var labelRect = rect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;
            
            // Handle Mouse Events
            var e = Event.current;
            if (rect.Contains(e.mousePosition))
            {
                if (hoverable) color = HoverColor;

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    state = !state;
                    e.Use();
                }
            }

            // Draw Background
            EditorGUI.DrawRect(rect, color);

            // Render Title
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            EditorGUI.LabelField(labelRect, content);

            // Return State
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
            return state;
        }
        
        // Render all children in the dropdown menu.
        public static void RenderDropdownProperties(SerializedProperty root, float width)
        {
            foreach (var childProperty in root.GetVisibleChildren())
            {
                var rect = GUILayoutUtility.GetRect(1f, EditorGUI.GetPropertyHeight(childProperty, true));
                rect.xMin += width;
                EditorGUI.PropertyField(rect, childProperty, true);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
        }

        // Retrieve all children in a serialised property
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            var currentProperty = serializedProperty.Copy();
            var nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (!currentProperty.NextVisible(true)) yield break;
            
            do
            {
                if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                    break;

                yield return currentProperty;
            }
            while (currentProperty.NextVisible(false));
        }
        
    }
    
}
