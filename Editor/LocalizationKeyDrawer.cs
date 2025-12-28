using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TextLocalization;

namespace TextLocalization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizationKeyPopupAttribute))]
    public class LocalizationKeyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);

            LocalizationSettings settings = GetSettings();
            if (settings != null && !string.IsNullOrEmpty(property.stringValue))
            {
                LocalizationKey keyData = GetKeyData(settings, property.stringValue);
                if (keyData != null)
                {
                    height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * settings.languages.Count;
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LocalizationSettings settings = GetSettings();

            if (settings == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            Rect popupRect = position;
            popupRect.height = EditorGUIUtility.singleLineHeight;

            List<string> keyList = new List<string>();
            foreach (var k in settings.keys) keyList.Add(k.key);

            int index = keyList.IndexOf(property.stringValue);

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(popupRect, label.text, index, keyList.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex >= 0 && newIndex < keyList.Count)
                {
                    property.stringValue = keyList[newIndex];
                    SyncData(property, settings, keyList[newIndex]);
                }
            }
            else if (index == -1 && !string.IsNullOrEmpty(property.stringValue))
            {
                EditorGUI.PropertyField(popupRect, property, label);
            }

            SerializedProperty languagesProp = property.serializedObject.FindProperty("languages");
            if (languagesProp != null && languagesProp.arraySize == 0 && !string.IsNullOrEmpty(property.stringValue))
            {
                SyncData(property, settings, property.stringValue);
            }

            if (!string.IsNullOrEmpty(property.stringValue))
            {
                LocalizationKey keyData = GetKeyData(settings, property.stringValue);
                if (keyData != null)
                {
                    EditorGUI.indentLevel++;
                    float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    for (int i = 0; i < settings.languages.Count; i++)
                    {
                        Rect langRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                        string lang = settings.languages[i];

                        while (keyData.values.Count <= i) keyData.values.Add("");

                        string currentVal = keyData.values[i];

                        EditorGUI.BeginChangeCheck();
                        string newVal = EditorGUI.TextField(langRect, lang, currentVal);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(settings, "Edit Localization Text");
                            keyData.values[i] = newVal;
                            EditorUtility.SetDirty(settings);
                            SyncData(property, settings, property.stringValue);
                        }

                        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void SyncData(SerializedProperty property, LocalizationSettings settings, string key)
        {
            SerializedProperty languagesProp = property.serializedObject.FindProperty("languages");
            SerializedProperty valuesProp = property.serializedObject.FindProperty("values");

            if (languagesProp == null || valuesProp == null) return;

            LocalizationKey keyData = GetKeyData(settings, key);

            if (keyData != null)
            {
                languagesProp.ClearArray();
                languagesProp.arraySize = settings.languages.Count;
                for (int i = 0; i < settings.languages.Count; i++)
                {
                    languagesProp.GetArrayElementAtIndex(i).stringValue = settings.languages[i];
                }

                valuesProp.ClearArray();
                valuesProp.arraySize = keyData.values.Count;
                for (int i = 0; i < keyData.values.Count; i++)
                {
                    valuesProp.GetArrayElementAtIndex(i).stringValue = keyData.values[i];
                }

            }
        }

        private LocalizationSettings GetSettings()
        {
            string lastPath = EditorPrefs.GetString("TextLocalization.ActiveSettings", "");
            if (!string.IsNullOrEmpty(lastPath))
            {
                var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(lastPath);
                if (settings != null) return settings;
            }

            string[] guids = AssetDatabase.FindAssets("t:LocalizationSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<LocalizationSettings>(path);
            }
            return null;
        }

        private LocalizationKey GetKeyData(LocalizationSettings settings, string key)
        {
            foreach (var k in settings.keys)
            {
                if (k.key == key)
                {
                    return k;
                }
            }
            return null;
        }
    }
}