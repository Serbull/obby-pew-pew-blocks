using Humanoid_Basics.Camera;
using Humanoid_Basics.Editor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Humanoid_Basics.Editor.Camera
{

    [CustomEditor(typeof(CameraCore))]
    public class CameraCoreEditor : UnityEditor.Editor
    {
        // References
        private SerializedProperty cameraObject;
        private SerializedProperty target;
        
        // Offset
        private SerializedProperty targetDistance;
        private SerializedProperty useTargetOffset;
        private SerializedProperty currentOffset;
        private SerializedProperty targetOffset;

        // Smoothing
        private SerializedProperty smoothCamera;
        private SerializedProperty smoothCameraRate;
        
        // Collider
        private SerializedProperty collisionLayers;
        
        private void OnEnable()
        {
            cameraObject = serializedObject.FindProperty("cameraObject");
            target = serializedObject.FindProperty("target");
            
            targetDistance = serializedObject.FindProperty("targetDistance");
            useTargetOffset = serializedObject.FindProperty("useTargetOffset");
            targetOffset = serializedObject.FindProperty("targetOffset");
            
            smoothCamera = serializedObject.FindProperty("smoothCamera");
            smoothCameraRate = serializedObject.FindProperty("smoothCameraRate");
            
            collisionLayers = serializedObject.FindProperty("collisionLayers");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Camera Core", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("References", "FixedJoint Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(cameraObject);
                EditorGUILayout.PropertyField(target);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("Settings", "d_SceneViewCamera@2x", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(targetDistance);
                EditorGUILayout.PropertyField(useTargetOffset);
                EditorGUILayout.PropertyField(targetOffset);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("Smoothing", "d_AnimationClip Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(smoothCamera);
                EditorGUILayout.PropertyField(smoothCameraRate);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtilities.IconLabelField("Collision", "d_BoxCollider Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(collisionLayers);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Version: " + CameraCore.Version, EditorStyles.miniLabel);
            
            Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
