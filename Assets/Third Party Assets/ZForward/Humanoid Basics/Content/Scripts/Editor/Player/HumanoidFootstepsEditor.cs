using Humanoid_Basics.Editor.Helpers;
using Humanoid_Basics.Player;
using UnityEditor;
using UnityEngine;

namespace Humanoid_Basics.Editor.Player
{
    [CustomEditor(typeof(HumanoidFootsteps)), CanEditMultipleObjects]
    public class HumanoidFootstepsEditor : UnityEditor.Editor
    {
        // References
        private SerializedProperty humanoidCore;
        private SerializedProperty leftFoot;
        private SerializedProperty rightFoot;
        private SerializedProperty groundType;
        
        // Audio
        private SerializedProperty audioClips;
        
        // Advanced
        
        private void OnEnable()
        {
            humanoidCore = serializedObject.FindProperty("humanoidCore");
            leftFoot = serializedObject.FindProperty("leftFoot");
            rightFoot = serializedObject.FindProperty("rightFoot");
            audioClips = serializedObject.FindProperty("audioClips");
            groundType = serializedObject.FindProperty("groundType");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var ground = groundType.stringValue;
            
            EditorGUILayout.LabelField("Humanoid Health", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current Ground: "+ground, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("References", "FixedJoint Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(humanoidCore);
                EditorGUILayout.PropertyField(leftFoot);
                EditorGUILayout.PropertyField(rightFoot);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Humanoid Main", EditorStyles.boldLabel);
            EditorUtilities.DropdownField("Main Audio", "d_SceneViewAudio@2x", audioClips);

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Version: " + HumanoidFootsteps.Version, EditorStyles.miniLabel);
            
            Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
