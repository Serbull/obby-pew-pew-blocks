using System;
using Humanoid_Basics.Editor.Helpers;
using Humanoid_Basics.Player;
using UnityEditor;
using UnityEngine;

namespace Humanoid_Basics.Editor.Player
{
    [CustomEditor(typeof(HumanoidHealth)), CanEditMultipleObjects]
    public class HumanoidHealthEditor : UnityEditor.Editor
    {
        // References
        private SerializedProperty humanoidCore;
        
        private SerializedProperty health;
        private SerializedProperty maxHealth;

        private void OnEnable()
        {
            humanoidCore = serializedObject.FindProperty("humanoidCore");
            health = serializedObject.FindProperty("health");
            maxHealth = serializedObject.FindProperty("maxHealth");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Humanoid Health", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("References", "FixedJoint Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(humanoidCore);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("Core Settings", "BodyPartPicker", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(health);
                EditorGUILayout.PropertyField(maxHealth);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Version: " + HumanoidHealth.Version, EditorStyles.miniLabel);
            
            Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
