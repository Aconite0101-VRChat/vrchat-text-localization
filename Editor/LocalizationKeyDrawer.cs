using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using VRCLocalization;

namespace VRCLocalization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizationKeyPopupAttribute))]
    public class LocalizationKeyDrawer : PropertyDrawer
    {
        private const double DebounceSeconds = 1.0;

        private Type GetTypeForLocalizationEnum(LocalizationValueType type)
        {
            switch (type)
            {
                case LocalizationValueType.AudioClip:
                    return typeof(AudioClip);
                case LocalizationValueType.Texture:
                    return typeof(Texture);
                case LocalizationValueType.Sprite:
                    return typeof(Sprite);
                case LocalizationValueType.Prefab:
                    return typeof(GameObject);
                default:
                    return typeof(Object);
            }
        }

        private string GetValuesPropName()
        {
            var attr = attribute as LocalizationKeyPopupAttribute;
            return attr?.ValuesFieldName ?? "values";
        }

        private LocalizationValueType? GetFilterType(SerializedProperty property)
        {
            var valuesProp = property.serializedObject.FindProperty(GetValuesPropName());
            if (valuesProp == null || !valuesProp.isArray) return null;

            string type = valuesProp.arrayElementType;
            if (type == "string") return LocalizationValueType.String;
            if (type.Contains("AudioClip")) return LocalizationValueType.AudioClip;
            if (type.Contains("Texture")) return LocalizationValueType.Texture;
            if (type.Contains("Sprite")) return LocalizationValueType.Sprite;
            if (type.Contains("GameObject")) return LocalizationValueType.Prefab;

            return null;
        }

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

            const float addButtonWidth = 70f;
            Rect buttonRect = new Rect(position.x + position.width - addButtonWidth, popupRect.y, addButtonWidth, popupRect.height);
            popupRect.width -= (addButtonWidth + EditorGUIUtility.standardVerticalSpacing);

            LocalizationValueType? filterType = GetFilterType(property);
            List<string> keyList = new List<string>();
            foreach (var k in settings.keys)
            {
                if (filterType.HasValue && k.valueType != filterType.Value) continue;
                keyList.Add(k.key);
            }

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

            if (GUI.Button(buttonRect, "Add Key"))
            {
                UnityEngine.Object targetObject = property.serializedObject.targetObject;
                string propertyPath = property.propertyPath;

                AddKeyWindow.Show(settings, (enteredName) =>
                {
                    if (string.IsNullOrEmpty(enteredName)) return;

                    string created = AddNewKey(settings, enteredName, filterType ?? LocalizationValueType.String);
                    if (string.IsNullOrEmpty(created)) return;

                    if (targetObject != null)
                    {
                        var so = new SerializedObject(targetObject);
                        var prop = so.FindProperty(propertyPath);
                        if (prop != null)
                        {
                            prop.stringValue = created;
                            so.ApplyModifiedProperties();
                            SyncData(prop, settings, created);
                        }
                    }
                });
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

                    SerializedProperty valuesProp = property.serializedObject.FindProperty(GetValuesPropName());
                    if (valuesProp != null)
                    {
                        while (valuesProp.arraySize < settings.languages.Count)
                        {
                            valuesProp.arraySize++;
                            valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1).stringValue = "";
                        }
                    }

                    var valuesPropOnComponent = property.serializedObject.FindProperty(GetValuesPropName());
                    bool isStringComponent = valuesPropOnComponent.arrayElementType == "string";
                    bool isStringInDb = keyData.valueType == LocalizationValueType.String;

                    if (isStringComponent != isStringInDb)
                    {
                        Rect helpBoxRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 2);
                        EditorGUI.HelpBox(helpBoxRect, "The selected key's type does not match the component's value type.", MessageType.Error);
                        y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else if (isStringComponent)
                    {
                        for (int i = 0; i < settings.languages.Count; i++)
                        {
                            Rect langRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                            string lang = settings.languages[i];

                            while (keyData.stringValues.Count <= i) keyData.stringValues.Add("");

                            string currentVal = GetSerializedStringValue(property, i, GetValuesPropName()) ?? keyData.stringValues[i];

                            EditorGUI.BeginChangeCheck();
                            string newVal = EditorGUI.TextField(langRect, lang, currentVal);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Object targetObject = property.serializedObject.targetObject;
                                Undo.RecordObject(targetObject, "Edit Localization Text");
                                SetSerializedStringValue(property, i, newVal, GetValuesPropName());

                                List<string> pendingValues = BuildStringValuesListFromSerialized(property, settings.languages.Count, GetValuesPropName());
                                DebouncedSaveManager.SetPending(settings, property.stringValue, pendingValues, DebounceSeconds);

                                property.serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(targetObject);
                            }
                            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                    else // Object type
                    {
                        for (int i = 0; i < settings.languages.Count; i++)
                        {
                            Rect langRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                            string lang = settings.languages[i];

                            while (keyData.objectValues.Count <= i) keyData.objectValues.Add(null);

                            Object currentVal = GetSerializedObjectValue(property, i, GetValuesPropName()) ?? keyData.objectValues[i];

                            EditorGUI.BeginChangeCheck();
                            Type objectType = GetTypeForLocalizationEnum(keyData.valueType);
                            Object newVal = EditorGUI.ObjectField(langRect, lang, currentVal, objectType, false);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Object targetObject = property.serializedObject.targetObject;
                                Undo.RecordObject(targetObject, "Edit Localization Value");
                                SetSerializedObjectValue(property, i, newVal, GetValuesPropName());

                                List<Object> pendingValues = BuildObjectValuesListFromSerialized(property, settings.languages.Count, GetValuesPropName());
                                DebouncedSaveManager.SetPending(settings, property.stringValue, pendingValues, DebounceSeconds);

                                property.serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(targetObject);
                            }
                            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void SyncData(SerializedProperty property, LocalizationSettings settings, string key)
        {
            SerializedProperty languagesProp = property.serializedObject.FindProperty("languages");
            SerializedProperty valuesProp = property.serializedObject.FindProperty(GetValuesPropName());

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

                bool isStringArray = valuesProp.arrayElementType == "string";
                if (isStringArray && keyData.valueType == LocalizationValueType.String)
                {
                    valuesProp.ClearArray();
                    valuesProp.arraySize = keyData.stringValues.Count;
                    for (int i = 0; i < keyData.stringValues.Count; i++)
                    {
                        valuesProp.GetArrayElementAtIndex(i).stringValue = keyData.stringValues[i];
                    }
                }
                else if (!isStringArray && keyData.valueType != LocalizationValueType.String)
                {
                    valuesProp.ClearArray();
                    valuesProp.arraySize = keyData.objectValues.Count;
                    for (int i = 0; i < keyData.objectValues.Count; i++)
                    {
                        valuesProp.GetArrayElementAtIndex(i).objectReferenceValue = keyData.objectValues[i];
                    }
                }

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private string AddNewKey(LocalizationSettings settings, string baseName, LocalizationValueType type)
        {
            if (settings == null) return null;

            string candidate = baseName;
            int suffix = 1;
            bool exists;
            do
            {
                exists = false;
                foreach (var k in settings.keys)
                {
                    if (k.key == candidate)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists) candidate = baseName + "_" + suffix++;
            } while (exists);

            var newKey = new LocalizationKey { key = candidate, valueType = type };
            for (int i = 0; i < settings.languages.Count; i++)
            {
                newKey.stringValues.Add(string.Empty);
                newKey.objectValues.Add(null);
            }

            Undo.RecordObject(settings, "Add Localization Key");
            settings.keys.Add(newKey);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            return candidate;
        }

        private LocalizationSettings GetSettings()
        {
            var descriptor = UnityEngine.Object.FindObjectOfType<VRCSceneLocalizationDescriptor>();
            return descriptor != null ? (LocalizationSettings)descriptor.settings : null;
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

        private static string GetSerializedStringValue(SerializedProperty property, int index, string valuesPropName)
        {
            var valuesProp = property.serializedObject.FindProperty(valuesPropName);
            if (valuesProp == null || !valuesProp.isArray) return null;
            if (index < 0 || index >= valuesProp.arraySize) return null;
            return valuesProp.GetArrayElementAtIndex(index).stringValue;
        }

        private static void SetSerializedStringValue(SerializedProperty property, int index, string value, string valuesPropName)
        {
            var valuesProp = property.serializedObject.FindProperty(valuesPropName);
            if (valuesProp == null || !valuesProp.isArray) return;
            if (index < 0) return;
            if (index >= valuesProp.arraySize)
            {
                valuesProp.arraySize = index + 1;
            }
            valuesProp.GetArrayElementAtIndex(index).stringValue = value;
        }

        private static Object GetSerializedObjectValue(SerializedProperty property, int index, string valuesPropName)
        {
            var valuesProp = property.serializedObject.FindProperty(valuesPropName);
            if (valuesProp == null || !valuesProp.isArray) return null;
            if (index < 0 || index >= valuesProp.arraySize) return null;
            return valuesProp.GetArrayElementAtIndex(index).objectReferenceValue;
        }

        private static void SetSerializedObjectValue(SerializedProperty property, int index, Object value, string valuesPropName)
        {
            var valuesProp = property.serializedObject.FindProperty(valuesPropName);
            if (valuesProp == null || !valuesProp.isArray) return;
            if (index < 0) return;
            if (index >= valuesProp.arraySize)
            {
                valuesProp.arraySize = index + 1;
            }
            valuesProp.GetArrayElementAtIndex(index).objectReferenceValue = value;
        }

        private static List<string> BuildStringValuesListFromSerialized(SerializedProperty property, int targetCount, string valuesPropName)
        {
            var result = new List<string>(targetCount);
            var valuesProp = property.serializedObject.FindProperty(valuesPropName);
            if (valuesProp != null && valuesProp.isArray)
            {
                for (int i = 0; i < targetCount; i++)
                {
                    if (i < valuesProp.arraySize)
                        result.Add(valuesProp.GetArrayElementAtIndex(i).stringValue);
                    else
                        result.Add(string.Empty);
                }
            }
            else
            {
                for (int i = 0; i < targetCount; i++) result.Add(string.Empty);
            }
            return result;
        }

        private static List<Object> BuildObjectValuesListFromSerialized(SerializedProperty property, int targetCount, string valuesPropName)
        {
            var result = new List<Object>(targetCount);
            var valuesProp = property.serializedObject.FindProperty(valuesPropName);
            if (valuesProp != null && valuesProp.isArray)
            {
                for (int i = 0; i < targetCount; i++)
                {
                    if (i < valuesProp.arraySize)
                        result.Add(valuesProp.GetArrayElementAtIndex(i).objectReferenceValue);
                    else
                        result.Add(null);
                }
            }
            else
            {
                for (int i = 0; i < targetCount; i++) result.Add(null);
            }
            return result;
        }

        private static class DebouncedSaveManager
        {
            private class Pending
            {
                public LocalizationSettings Settings;
                public string Key;
                public object Values;
                public double LastEditTime;
            }

            private static readonly Dictionary<string, Pending> s_pending = new Dictionary<string, Pending>();
            private static bool s_registered = false;

            private static string MakeId(LocalizationSettings settings, string key)
            {
                return settings.GetInstanceID() + ":" + key;
            }

            public static void SetPending(LocalizationSettings settings, string key, object values, double debounceSeconds)
            {
                if (settings == null || string.IsNullOrEmpty(key)) return;

                string id = MakeId(settings, key);
                Pending p;
                if (!s_pending.TryGetValue(id, out p))
                {
                    p = new Pending { Settings = settings, Key = key, Values = values, LastEditTime = EditorApplication.timeSinceStartup };
                    s_pending[id] = p;
                }
                else
                {
                    p.Values = values;
                    p.LastEditTime = EditorApplication.timeSinceStartup;
                }

                EnsureRegistered();
            }

            private static void EnsureRegistered()
            {
                if (s_registered) return;
                EditorApplication.update += OnUpdate;
                Selection.selectionChanged += OnSelectionChanged;
                EditorApplication.quitting += OnEditorQuitting;
                s_registered = true;
            }

            private static void UnregisterIfEmpty()
            {
                if (s_pending.Count == 0 && s_registered)
                {
                    EditorApplication.update -= OnUpdate;
                    Selection.selectionChanged -= OnSelectionChanged;
                    EditorApplication.quitting -= OnEditorQuitting;
                    s_registered = false;
                }
            }

            private static void OnUpdate()
            {
                double now = EditorApplication.timeSinceStartup;
                var toFlush = new List<string>();
                foreach (var kv in s_pending)
                {
                    if (now - kv.Value.LastEditTime >= DebounceSeconds)
                    {
                        toFlush.Add(kv.Key);
                    }
                }

                foreach (var id in toFlush)
                {
                    Flush(id);
                }

                UnregisterIfEmpty();
            }

            private static void OnSelectionChanged()
            {
                FlushAll();
            }

            private static void OnEditorQuitting()
            {
                FlushAll();
            }

            private static void FlushAll()
            {
                var keys = new List<string>(s_pending.Keys);
                foreach (var id in keys) Flush(id);
                UnregisterIfEmpty();
            }

            private static void Flush(string id)
            {
                if (!s_pending.TryGetValue(id, out var p)) return;

                var settings = p.Settings;
                var key = p.Key;
                var values = p.Values;

                if (settings != null)
                {
                    LocalizationKey keyData = null;
                    foreach (var k in settings.keys)
                    {
                        if (k.key == key)
                        {
                            keyData = k;
                            break;
                        }
                    }

                    if (keyData != null)
                    {
                        Undo.RecordObject(settings, "Edit Localization Text");
                        if (values is List<string> stringValues)
                        {
                            keyData.stringValues.Clear();
                            keyData.stringValues.AddRange(stringValues);
                            EditorUtility.SetDirty(settings);
                            AssetDatabase.SaveAssets();
                        }
                        else if (values is List<Object> objectValues)
                        {
                            keyData.objectValues.Clear();
                            keyData.objectValues.AddRange(objectValues);
                            EditorUtility.SetDirty(settings);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }

                s_pending.Remove(id);
            }
        }

        private class AddKeyWindow : EditorWindow
        {
            private string _name = "NEW_KEY";
            private LocalizationSettings _settings;
            private Action<string> _onConfirm;
            private string _validationMessage;

            public static void Show(LocalizationSettings settings, Action<string> onConfirm)
            {
                var wnd = CreateInstance<AddKeyWindow>();
                wnd.titleContent = new GUIContent("Add Localization Key");
                wnd._settings = settings;
                wnd._onConfirm = onConfirm;
                wnd.position = new Rect(Screen.width / 2f - 150f, Screen.height / 2f - 50f, 300f, 90f);
                wnd.ShowUtility();
            }

            private void OnGUI()
            {
                EditorGUILayout.LabelField("New key name", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                _name = EditorGUILayout.TextField(_name);
                if (EditorGUI.EndChangeCheck())
                {
                    _validationMessage = ValidateName(_name);
                }

                if (!string.IsNullOrEmpty(_validationMessage))
                {
                    EditorGUILayout.HelpBox(_validationMessage, MessageType.Warning);
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create"))
                {
                    _validationMessage = ValidateName(_name);
                    if (string.IsNullOrEmpty(_validationMessage))
                    {
                        _onConfirm?.Invoke(_name);
                        Close();
                    }
                }

                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }
                EditorGUILayout.EndHorizontal();
            }

            private string ValidateName(string name)
            {
                if (string.IsNullOrEmpty(name)) return "Name cannot be empty.";
                if (_settings != null)
                {
                    foreach (var k in _settings.keys)
                    {
                        if (k.key == name) return "A key with this name already exists.";
                    }
                }
                return null;
            }
        }
    }
}