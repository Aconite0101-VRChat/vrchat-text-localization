
using UnityEditor;
using UnityEngine;

namespace VRCLocalization
{
    [CustomPropertyDrawer(typeof(LocalizationSettingsAttribute))]
    public class VRCSceneLocalizationDescriptorAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(LocalizationSettings), false);
            EditorGUI.EndProperty();
        }
    }
}
