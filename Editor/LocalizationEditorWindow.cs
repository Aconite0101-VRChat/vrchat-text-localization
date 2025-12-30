using UnityEngine;
using UnityEditor;

namespace TextLocalization.Editor
{
    public class LocalizationEditorWindow : EditorWindow
    {
        private LocalizationSettings _settings;
        private Vector2 _scrollPos;
        private string _newKeyName = "";
        private const string PREFS_KEY = "TextLocalization.ActiveSettings";

        private bool _isDirty;
        private double _lastEditTime;
        private const double SAVE_DEBOUNCE_SECONDS = 1.0;

        [MenuItem("VRChat Localization/Localization Editor")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization Editor");
        }

        private void OnEnable()
        {
            string lastPath = EditorPrefs.GetString(PREFS_KEY, "");
            if (!string.IsNullOrEmpty(lastPath))
            {
                _settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(lastPath);
            }

            if (_settings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:LocalizationSettings");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(path);
                }
            }

            if (_settings != null)
            {
                EditorPrefs.SetString(PREFS_KEY, AssetDatabase.GetAssetPath(_settings));
            }

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
            EditorGUI.BeginChangeCheck();
            _settings = (LocalizationSettings)EditorGUILayout.ObjectField("Active Database", _settings, typeof(LocalizationSettings), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (_settings != null)
                {
                    EditorPrefs.SetString(PREFS_KEY, AssetDatabase.GetAssetPath(_settings));
                }
                else
                {
                    EditorPrefs.DeleteKey(PREFS_KEY);
                }
            }

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
                _settings = asset;
                EditorPrefs.SetString(PREFS_KEY, path);
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

        private void DrawTable(SerializedObject so)
        {
            EditorGUILayout.LabelField("Translations", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _newKeyName = EditorGUILayout.TextField("New Key Name", _newKeyName);
            if (GUILayout.Button("Add Key", GUILayout.Width(80)))
            {
                AddNewKey();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Key", GUILayout.Width(150));
            for (int i = 0; i < _settings.languages.Count; i++)
            {
                EditorGUILayout.LabelField(_settings.languages[i], GUILayout.MinWidth(100));
            }
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            SerializedProperty keysProp = so.FindProperty("keys");

            for (int i = 0; i < keysProp.arraySize; i++)
            {
                SerializedProperty keyItem = keysProp.GetArrayElementAtIndex(i);
                SerializedProperty keyName = keyItem.FindPropertyRelative("key");
                SerializedProperty values = keyItem.FindPropertyRelative("values");

                while (values.arraySize < _settings.languages.Count) values.InsertArrayElementAtIndex(values.arraySize);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.PropertyField(keyName, GUIContent.none, GUILayout.Width(150));

                for (int l = 0; l < _settings.languages.Count; l++)
                {
                    if (l < values.arraySize)
                    {
                        SerializedProperty valueProp = values.GetArrayElementAtIndex(l);
                        EditorGUILayout.PropertyField(valueProp, GUIContent.none, GUILayout.MinWidth(100));
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

            foreach (var lang in _settings.languages) newKey.values.Add("");

            _settings.keys.Add(newKey);
            _newKeyName = "";
            MarkDirty();
        }

        private void ValidateDataIntegrity()
        {
            // Logic to ensure all keys have the correct number of value entries matching languages count
        }
    }
}