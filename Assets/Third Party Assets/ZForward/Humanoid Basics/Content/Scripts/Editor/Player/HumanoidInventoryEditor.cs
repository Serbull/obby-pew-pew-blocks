using Humanoid_Basics.Editor.Helpers;
using Humanoid_Basics.Player;
using UnityEditor;
using UnityEngine;

namespace Humanoid_Basics.Editor.Player
{
    [CustomEditor(typeof(HumanoidInventory)), CanEditMultipleObjects]
    public class HumanoidInventoryEditor : UnityEditor.Editor
    {
        private SerializedProperty humanoidCore;
        
        private SerializedProperty weaponLimit;
        private SerializedProperty itemLimit;
        private SerializedProperty weapons;
        private SerializedProperty items;
        
        private void OnEnable()
        {
            humanoidCore = serializedObject.FindProperty("humanoidCore");
            weaponLimit = serializedObject.FindProperty("weaponLimit");
            itemLimit = serializedObject.FindProperty("itemLimit");
            weapons = serializedObject.FindProperty("weapons");
            items = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Humanoid Inventory", EditorStyles.boldLabel);
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
            EditorUtilities.IconLabelField("Inventory Settings", "Profiler.NetworkOperations@2x", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(weaponLimit);
                EditorGUILayout.PropertyField(itemLimit);
                EditorGUILayout.PropertyField(weapons);
                EditorGUILayout.PropertyField(items);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Version: " + HumanoidInventory.Version, EditorStyles.miniLabel);
            
            Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
