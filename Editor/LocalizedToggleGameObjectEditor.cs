using UnityEditor;
using UnityEngine;
using UdonSharpEditor;
using VRCLocalization;

namespace VRCLocalization.Editor
{
    [CustomEditor(typeof(LocalizedToggleGameObject))]
    public class LocalizedToggleGameObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();

            LocalizationSettings settings = GetSettings();
            if (settings != null)
            {
                SerializedProperty languagesProp = serializedObject.FindProperty("languages");
                if (languagesProp.arraySize != settings.languages.Count)
                {
                    languagesProp.arraySize = settings.languages.Count;
                }
                for (int i = 0; i < settings.languages.Count; i++)
                {
                    languagesProp.GetArrayElementAtIndex(i).stringValue = settings.languages[i];
                }

                SerializedProperty gameObjectsProp = serializedObject.FindProperty("gameObjects");
                if (gameObjectsProp.arraySize != settings.languages.Count)
                {
                    gameObjectsProp.arraySize = settings.languages.Count;
                }

                EditorGUILayout.LabelField("Localized GameObjects", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < settings.languages.Count; i++)
                {
                    EditorGUILayout.PropertyField(gameObjectsProp.GetArrayElementAtIndex(i), new GUIContent(settings.languages[i]));
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("LocalizationSettings not found.", MessageType.Warning);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gameObjects"), true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private LocalizationSettings GetSettings()
        {
            var descriptor = FindObjectOfType<VRCSceneLocalizationDescriptor>();
            return descriptor != null ? (LocalizationSettings)descriptor.settings : null;
        }
    }
}