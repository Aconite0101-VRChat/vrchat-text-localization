using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRCLocalization;

namespace VRCLocalization.Editor
{
    public class LocalizationEditorWindow : EditorWindow
    {
        private LocalizationSettings _settings;
        private Vector2 _scrollPos;
        private string _newKeyName = "";

        private bool _isDirty;
        private double _lastEditTime;
        private const double SAVE_DEBOUNCE_SECONDS = 1.0;
        private int _selectedTab = 0;
        private readonly string[] _tabNames = Enum.GetNames(typeof(LocalizationValueType));
        private string _searchQuery = "";

        [MenuItem("VRChat Localization/Localization Editor")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization Editor");
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            SaveIfDirtyImmediate();
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (!_isDirty || _settings == null) return;

            if (EditorApplication.timeSinceStartup - _lastEditTime >= SAVE_DEBOUNCE_SECONDS)
            {
                SaveIfDirtyImmediate();
            }
        }

        private void MarkDirty()
        {
            if (_settings == null) return;
            _isDirty = true;
            _lastEditTime = EditorApplication.timeSinceStartup;
            EditorUtility.SetDirty(_settings);
        }

        private void SaveIfDirtyImmediate()
        {
            if (!_isDirty || _settings == null) return;
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            _isDirty = false;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            var descriptor = FindObjectOfType<VRCSceneLocalizationDescriptor>();
            if (descriptor == null)
            {
                EditorGUILayout.HelpBox("VRCSceneLocalizationDescriptor not found in the scene.", MessageType.Warning);
                if (GUILayout.Button("Create Descriptor"))
                {
                    new GameObject("VRCSceneLocalizationDescriptor").AddComponent<VRCSceneLocalizationDescriptor>();
                }
                _settings = null;
                return;
            }

            EditorGUI.BeginChangeCheck();
            var newSettings = (LocalizationSettings)EditorGUILayout.ObjectField("Active Database", descriptor.settings, typeof(LocalizationSettings), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(descriptor, "Change Localization Settings");
                descriptor.settings = newSettings;
                EditorUtility.SetDirty(descriptor);
            }

            _settings = (LocalizationSettings)descriptor.settings;
            if (_settings == null)
            {
                DrawNoSettingsUI();
                return;
            }

            SerializedObject so = new SerializedObject(_settings);
            so.Update();

            DrawHeader(so);

            EditorGUILayout.Space();

            DrawTable(so);

            if (so.ApplyModifiedProperties())
            {
                ValidateDataIntegrity();
                MarkDirty();
            }
        }

        private void DrawNoSettingsUI()
        {
            EditorGUILayout.HelpBox("No Localization Settings file found in the project.", MessageType.Warning);
            if (GUILayout.Button("Create New Localization Database"))
            {
                CreateSettingsAsset();
            }
        }

        private void CreateSettingsAsset()
        {
            LocalizationSettings asset = ScriptableObject.CreateInstance<LocalizationSettings>();

            string path = EditorUtility.SaveFilePanelInProject("Save Localization Settings", "LocalizationSettings", "asset", "Please enter a file name to save the settings to");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                
                var descriptor = FindObjectOfType<VRCSceneLocalizationDescriptor>();
                if (descriptor != null)
                {
                    Undo.RecordObject(descriptor, "Assign Localization Settings");
                    descriptor.settings = asset;
                    EditorUtility.SetDirty(descriptor);
                }
            }
        }

        private void DrawHeader(SerializedObject so)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            SerializedProperty languagesProp = so.FindProperty("languages");
            EditorGUILayout.PropertyField(languagesProp, new GUIContent("Supported Languages"), true);

            EditorGUILayout.EndVertical();
        }

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
                    return typeof(UnityEngine.Object);
            }
        }

        private void DrawTable(SerializedObject so)
        {
            EditorGUILayout.LabelField("Translations", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            if (EditorGUI.EndChangeCheck())
            {
                _scrollPos = Vector2.zero;
            }
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            _newKeyName = EditorGUILayout.TextField("New Key Name", _newKeyName);
            if (GUILayout.Button("Add Key", GUILayout.Width(80)))
            {
                AddNewKey();
            }
            EditorGUILayout.EndHorizontal();

            _searchQuery = EditorGUILayout.TextField("Search", _searchQuery);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Key", GUILayout.Width(200));
            for (int i = 0; i < _settings.languages.Count; i++)
            {
                EditorGUILayout.LabelField(_settings.languages[i], GUILayout.MinWidth(100));
            }
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            SerializedProperty keysProp = so.FindProperty("keys");

            List<int> indices = new List<int>();
            for (int i = 0; i < keysProp.arraySize; i++)
            {
                SerializedProperty keyItem = keysProp.GetArrayElementAtIndex(i);
                SerializedProperty keyName = keyItem.FindPropertyRelative("key");
                SerializedProperty valueTypeProp = keyItem.FindPropertyRelative("valueType");

                if (valueTypeProp.enumValueIndex != _selectedTab) continue;

                if (!string.IsNullOrEmpty(_searchQuery) && keyName.stringValue.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                indices.Add(i);
            }
            indices.Sort((a, b) => string.Compare(keysProp.GetArrayElementAtIndex(a).FindPropertyRelative("key").stringValue, keysProp.GetArrayElementAtIndex(b).FindPropertyRelative("key").stringValue, StringComparison.OrdinalIgnoreCase));


            foreach (int i in indices)
            {
                SerializedProperty keyItem = keysProp.GetArrayElementAtIndex(i);
                SerializedProperty keyName = keyItem.FindPropertyRelative("key");
                SerializedProperty valueTypeProp = keyItem.FindPropertyRelative("valueType");

                var valueType = (LocalizationValueType)valueTypeProp.enumValueIndex;

                SerializedProperty valuesProp;
                if (valueType == LocalizationValueType.String)
                {
                    valuesProp = keyItem.FindPropertyRelative("stringValues");
                }
                else
                {
                    valuesProp = keyItem.FindPropertyRelative("objectValues");
                }

                while (valuesProp.arraySize < _settings.languages.Count) valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.PropertyField(keyName, GUIContent.none, GUILayout.Width(200));

                for (int l = 0; l < _settings.languages.Count; l++)
                {
                    if (l < valuesProp.arraySize)
                    {
                        SerializedProperty valueProp = valuesProp.GetArrayElementAtIndex(l);
                        if (valueType == LocalizationValueType.String)
                        {
                            EditorGUILayout.PropertyField(valueProp, GUIContent.none, GUILayout.MinWidth(100));
                        }
                        else
                        {
                            Type objectType = GetTypeForLocalizationEnum(valueType);
                            valueProp.objectReferenceValue = EditorGUILayout.ObjectField(valueProp.objectReferenceValue, objectType, false, GUILayout.MinWidth(100));
                        }
                    }
                }

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    keysProp.DeleteArrayElementAtIndex(i);
                    MarkDirty();
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void AddNewKey()
        {
            if (string.IsNullOrEmpty(_newKeyName)) return;

            Undo.RecordObject(_settings, "Add Localization Key");

            LocalizationKey newKey = new LocalizationKey();
            newKey.key = _newKeyName.ToUpper().Replace(" ", "_");
            newKey.valueType = (LocalizationValueType)_selectedTab;

            for (int i = 0; i < _settings.languages.Count; i++)
            {
                newKey.stringValues.Add("");
                newKey.objectValues.Add(null);
            }

            _settings.keys.Add(newKey);
            _newKeyName = "";
            MarkDirty();
        }

        private void ValidateDataIntegrity()
        {
            if (_settings == null) return;

            foreach (var key in _settings.keys)
            {
                while (key.stringValues.Count < _settings.languages.Count) key.stringValues.Add("");
                while (key.stringValues.Count > _settings.languages.Count) key.stringValues.RemoveAt(key.stringValues.Count - 1);
                while (key.objectValues.Count < _settings.languages.Count) key.objectValues.Add(null);
                while (key.objectValues.Count > _settings.languages.Count) key.objectValues.RemoveAt(key.objectValues.Count - 1);
            }
        }
    }
}