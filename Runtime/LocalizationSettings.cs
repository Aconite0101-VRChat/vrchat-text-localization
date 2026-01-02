using UnityEngine;
using System;
using System.Collections.Generic;

namespace VRCLocalization
{
    public enum LocalizationValueType
    {
        String,
        AudioClip,
        Texture,
        Sprite,
        Prefab // GameObject
    }

    [CreateAssetMenu(fileName = "LocalizationSettings", menuName = "VRChat/Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        [Tooltip("List of language codes in RFC 5646 format (e.g., 'en', 'ja')")]
        public List<string> languages = new List<string> { "en", "ja" };

        [Tooltip("The database of keys and their translations")]
        public List<LocalizationKey> keys = new List<LocalizationKey>();
    }

    [Serializable]
    public class LocalizationKey
    {
        public string key = "NEW_KEY";
        public LocalizationValueType valueType = LocalizationValueType.String;
        public List<string> stringValues = new List<string>();
        public List<UnityEngine.Object> objectValues = new List<UnityEngine.Object>();
    }
}