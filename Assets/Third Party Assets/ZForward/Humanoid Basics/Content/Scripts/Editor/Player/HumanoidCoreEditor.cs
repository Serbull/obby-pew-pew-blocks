using Humanoid_Basics.Editor.Helpers;
using Humanoid_Basics.Player;
using UnityEngine;
using UnityEditor;

namespace Humanoid_Basics.Editor.Player
{
    [CustomEditor(typeof(HumanoidCore)), CanEditMultipleObjects]
    public class HumanoidCoreEditor : UnityEditor.Editor
    {
        // Humanoid Settings
        private SerializedProperty humanoidType;
        private SerializedProperty humanoidStatus;
        
        // Layers
        private SerializedProperty groundLayers;
        private SerializedProperty frontLayers;
        
        // Detection/Sight
        private SerializedProperty raycastDownOffset;
        private SerializedProperty raycastDownDistance;
        private SerializedProperty raycastLayersDown;
        private SerializedProperty raycastLayersUp;

        // Player Settings
        private SerializedProperty playerSettings;

        // Player Features
        private SerializedProperty playerFeatures;

        // Weapon Settings
        private SerializedProperty advancedSettings;

        private void OnEnable()
        {
            // Core
            humanoidType = serializedObject.FindProperty("humanoidType");
            humanoidStatus = serializedObject.FindProperty("humanoidStatus");

            // Layers
            groundLayers = serializedObject.FindProperty("groundLayers");
            frontLayers = serializedObject.FindProperty("frontLayers");
            
            // Detection
            raycastDownOffset = serializedObject.FindProperty("heightFromGroundRaycast");
            raycastDownDistance = serializedObject.FindProperty("raycastDownDistance");
            raycastLayersDown = serializedObject.FindProperty("detectableLayers");
            raycastLayersUp = serializedObject.FindProperty("upwardsSensorLayerMask");

            // Basics
            playerSettings = serializedObject.FindProperty("playerSettings");
            
            // Features
            playerFeatures = serializedObject.FindProperty("playerFeatures");

            // Weapon Settings
            advancedSettings = serializedObject.FindProperty("advancedSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Humanoid Core", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Core Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("Core Settings", "HumanTemplate Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(humanoidType);
                EditorGUILayout.PropertyField(humanoidStatus);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Layers
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("Layers & Detection", "GUILayer Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(groundLayers);
                EditorGUILayout.PropertyField(frontLayers);
                EditorGUILayout.PropertyField(raycastDownOffset);
                EditorGUILayout.PropertyField(raycastDownDistance);
                EditorGUILayout.PropertyField(raycastLayersDown);
                EditorGUILayout.PropertyField(raycastLayersUp);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Humanoid Main", EditorStyles.boldLabel);
            EditorUtilities.DropdownField("Player Settings", "d_Avatar Icon", playerSettings);
            EditorUtilities.DropdownField("Controller Features", "d_Toggle Icon", playerFeatures);
            EditorUtilities.DropdownField("Advanced Settings", "AvatarMask Icon", advancedSettings);

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Version: " + HumanoidCore.Version, EditorStyles.miniLabel);
            
            Repaint();
            serializedObject.ApplyModifiedProperties();

        }
    }
    

}
