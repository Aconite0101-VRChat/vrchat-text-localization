
using UnityEditor;

namespace VRCLocalization.Editor
{
    [CustomEditor(typeof(LocalizedInstantiateGameObject))]
    [CanEditMultipleObjects]
    internal class LocalizedInstantiateGameObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty keyProp;
        private SerializedProperty assignParentProp;
        private SerializedProperty setLocalTransformProp;
        private SerializedProperty languagesProp;
        private SerializedProperty valuesProp;

        private void OnEnable()
        {
            keyProp = serializedObject.FindProperty("key");
            assignParentProp = serializedObject.FindProperty("assignParent");
            setLocalTransformProp = serializedObject.FindProperty("setLocalTransform");
            languagesProp = serializedObject.FindProperty("languages");
            valuesProp = serializedObject.FindProperty("values");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(keyProp);
            EditorGUILayout.PropertyField(assignParentProp);
            if (assignParentProp.boolValue)
            {
                EditorGUILayout.PropertyField(setLocalTransformProp);
            }
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(languagesProp, true);
                EditorGUILayout.PropertyField(valuesProp, true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}