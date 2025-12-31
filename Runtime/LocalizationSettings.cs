using UnityEngine;
using System.Collections.Generic;

namespace TextLocalization
{
    [CreateAssetMenu(fileName = "LocalizationSettings", menuName = "VRChat/Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        [Tooltip("List of language codes in RFC 5646 format (e.g., 'en', 'ja')")]
        public List<string> languages = new List<string> { "en", "ja" }; 

        [Tooltip("The database of keys and their translations")]
        public List<LocalizationKey> keys = new List<LocalizationKey>();
    }

    [System.Serializable]
    public class LocalizationKey
    {
        public string key = "NEW_KEY";
        public List<string> values = new List<string>();
    }
}                                   